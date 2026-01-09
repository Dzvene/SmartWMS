using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Automation.Models;

namespace SmartWMS.API.Modules.Automation.Services;

/// <summary>
/// Rule Engine - evaluates automation rules and their conditions
/// </summary>
public interface IRuleEngine
{
    Task<List<AutomationRule>> GetMatchingRulesAsync(
        Guid tenantId,
        TriggerType triggerType,
        string? entityType = null,
        string? eventName = null,
        CancellationToken cancellationToken = default);

    Task<bool> EvaluateConditionsAsync(
        AutomationRule rule,
        object? entityData,
        CancellationToken cancellationToken = default);

    Task<ConditionEvaluationResult[]> EvaluateConditionsDetailedAsync(
        AutomationRule rule,
        object? entityData,
        CancellationToken cancellationToken = default);
}

public class RuleEngine : IRuleEngine
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<RuleEngine> _logger;

    public RuleEngine(ApplicationDbContext dbContext, ILogger<RuleEngine> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<AutomationRule>> GetMatchingRulesAsync(
        Guid tenantId,
        TriggerType triggerType,
        string? entityType = null,
        string? eventName = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AutomationRules
            .Where(r => r.TenantId == tenantId && r.IsActive && r.TriggerType == triggerType);

        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(r => r.TriggerEntityType == null || r.TriggerEntityType == entityType);
        }

        if (!string.IsNullOrEmpty(eventName))
        {
            query = query.Where(r => r.TriggerEvent == null || r.TriggerEvent == eventName);
        }

        return await query
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EvaluateConditionsAsync(
        AutomationRule rule,
        object? entityData,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rule.ConditionsJson))
            return true;

        try
        {
            var conditions = JsonSerializer.Deserialize<List<ConditionDefinition>>(rule.ConditionsJson);
            if (conditions == null || conditions.Count == 0)
                return true;

            var dataDict = ConvertToDict(entityData);
            return EvaluateConditionGroup(conditions, dataDict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating conditions for rule {RuleId}", rule.Id);
            return false;
        }
    }

    public Task<ConditionEvaluationResult[]> EvaluateConditionsDetailedAsync(
        AutomationRule rule,
        object? entityData,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ConditionEvaluationResult>();

        if (string.IsNullOrEmpty(rule.ConditionsJson))
            return Task.FromResult(results.ToArray());

        try
        {
            var conditions = JsonSerializer.Deserialize<List<ConditionDefinition>>(rule.ConditionsJson);
            if (conditions == null)
                return Task.FromResult(results.ToArray());

            var dataDict = ConvertToDict(entityData);

            foreach (var condition in conditions)
            {
                var actualValue = GetFieldValue(dataDict, condition.Field);
                var passed = EvaluateCondition(condition, actualValue);

                results.Add(new ConditionEvaluationResult
                {
                    Field = condition.Field,
                    Operator = condition.Operator.ToString(),
                    ExpectedValue = condition.Value,
                    ActualValue = actualValue?.ToString(),
                    Passed = passed
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating conditions for rule {RuleId}", rule.Id);
        }

        return Task.FromResult(results.ToArray());
    }

    private bool EvaluateConditionGroup(List<ConditionDefinition> conditions, Dictionary<string, object?> data)
    {
        if (conditions.Count == 0)
            return true;

        bool result = EvaluateCondition(conditions[0], GetFieldValue(data, conditions[0].Field));

        for (int i = 1; i < conditions.Count; i++)
        {
            var condition = conditions[i];
            var conditionResult = EvaluateCondition(condition, GetFieldValue(data, condition.Field));

            result = condition.Logic == ConditionLogic.And
                ? result && conditionResult
                : result || conditionResult;
        }

        return result;
    }

    private bool EvaluateCondition(ConditionDefinition condition, object? actualValue)
    {
        var expectedValue = condition.Value;
        var actualStr = actualValue?.ToString() ?? "";

        return condition.Operator switch
        {
            ConditionOperator.Equals => string.Equals(actualStr, expectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.NotEquals => !string.Equals(actualStr, expectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.Contains => actualStr.Contains(expectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.NotContains => !actualStr.Contains(expectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.StartsWith => actualStr.StartsWith(expectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.EndsWith => actualStr.EndsWith(expectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.IsNull => actualValue == null || string.IsNullOrEmpty(actualStr),
            ConditionOperator.IsNotNull => actualValue != null && !string.IsNullOrEmpty(actualStr),
            ConditionOperator.GreaterThan => CompareNumeric(actualValue, expectedValue) > 0,
            ConditionOperator.GreaterThanOrEquals => CompareNumeric(actualValue, expectedValue) >= 0,
            ConditionOperator.LessThan => CompareNumeric(actualValue, expectedValue) < 0,
            ConditionOperator.LessThanOrEquals => CompareNumeric(actualValue, expectedValue) <= 0,
            ConditionOperator.In => expectedValue.Split(',').Select(s => s.Trim()).Contains(actualStr, StringComparer.OrdinalIgnoreCase),
            ConditionOperator.NotIn => !expectedValue.Split(',').Select(s => s.Trim()).Contains(actualStr, StringComparer.OrdinalIgnoreCase),
            _ => false
        };
    }

    private int CompareNumeric(object? actualValue, string expectedValue)
    {
        if (decimal.TryParse(actualValue?.ToString(), out var actual) &&
            decimal.TryParse(expectedValue, out var expected))
        {
            return actual.CompareTo(expected);
        }

        return string.Compare(actualValue?.ToString() ?? "", expectedValue, StringComparison.OrdinalIgnoreCase);
    }

    private object? GetFieldValue(Dictionary<string, object?> data, string fieldPath)
    {
        var parts = fieldPath.Split('.');
        object? current = data;

        foreach (var part in parts)
        {
            if (current is Dictionary<string, object?> dict)
            {
                if (dict.TryGetValue(part, out var value))
                {
                    current = value;
                }
                else
                {
                    // Try case-insensitive
                    var key = dict.Keys.FirstOrDefault(k => k.Equals(part, StringComparison.OrdinalIgnoreCase));
                    current = key != null ? dict[key] : null;
                }
            }
            else if (current is JsonElement jsonElement)
            {
                if (jsonElement.TryGetProperty(part, out var prop))
                {
                    current = GetJsonElementValue(prop);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        return current;
    }

    private object? GetJsonElementValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element
        };
    }

    private Dictionary<string, object?> ConvertToDict(object? obj)
    {
        if (obj == null)
            return new Dictionary<string, object?>();

        if (obj is Dictionary<string, object?> dict)
            return dict;

        if (obj is JsonElement jsonElement)
        {
            var result = new Dictionary<string, object?>();
            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in jsonElement.EnumerateObject())
                {
                    result[prop.Name] = GetJsonElementValue(prop.Value);
                }
            }
            return result;
        }

        // Convert object to dictionary using reflection
        var type = obj.GetType();
        return type.GetProperties()
            .ToDictionary(
                p => p.Name,
                p => p.GetValue(obj)
            );
    }
}

// Internal class for JSON deserialization
internal class ConditionDefinition
{
    public int Order { get; set; }
    public ConditionLogic Logic { get; set; } = ConditionLogic.And;
    public string Field { get; set; } = string.Empty;
    public ConditionOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? ValueType { get; set; }
}

public class ConditionEvaluationResult
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string ExpectedValue { get; set; } = string.Empty;
    public string? ActualValue { get; set; }
    public bool Passed { get; set; }
}
