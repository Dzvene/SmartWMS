using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Automation.DTOs;
using SmartWMS.API.Modules.Automation.Models;

namespace SmartWMS.API.Modules.Automation.Services;

/// <summary>
/// Main Automation Service - orchestrates rule execution and provides CRUD operations
/// </summary>
public interface IAutomationService
{
    // CRUD
    Task<PaginatedResult<AutomationRuleDto>> GetRulesAsync(Guid tenantId, AutomationRuleFilters filters, CancellationToken cancellationToken = default);
    Task<AutomationRuleDetailDto?> GetRuleByIdAsync(Guid tenantId, Guid ruleId, CancellationToken cancellationToken = default);
    Task<AutomationRuleDto> CreateRuleAsync(Guid tenantId, CreateAutomationRuleRequest request, string? userId, CancellationToken cancellationToken = default);
    Task<AutomationRuleDto?> UpdateRuleAsync(Guid tenantId, Guid ruleId, UpdateAutomationRuleRequest request, string? userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteRuleAsync(Guid tenantId, Guid ruleId, CancellationToken cancellationToken = default);

    // Execution
    Task<RuleExecution> TriggerRuleAsync(Guid tenantId, Guid ruleId, TriggerRuleRequest? request, CancellationToken cancellationToken = default);
    Task<TestRuleResponse> TestRuleAsync(Guid tenantId, Guid ruleId, TestRuleRequest request, CancellationToken cancellationToken = default);
    Task ProcessEventAsync(Guid tenantId, TriggerType triggerType, string entityType, string eventName, Guid? entityId, object? eventData, CancellationToken cancellationToken = default);

    // Executions history
    Task<PaginatedResult<RuleExecutionDto>> GetExecutionsAsync(Guid tenantId, RuleExecutionFilters filters, CancellationToken cancellationToken = default);

    // Stats
    Task<AutomationStatsDto> GetStatsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public class AutomationService : IAutomationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRuleEngine _ruleEngine;
    private readonly IActionExecutor _actionExecutor;
    private readonly ILogger<AutomationService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AutomationService(
        ApplicationDbContext dbContext,
        IRuleEngine ruleEngine,
        IActionExecutor actionExecutor,
        ILogger<AutomationService> logger)
    {
        _dbContext = dbContext;
        _ruleEngine = ruleEngine;
        _actionExecutor = actionExecutor;
        _logger = logger;
    }

    #region CRUD

    public async Task<PaginatedResult<AutomationRuleDto>> GetRulesAsync(
        Guid tenantId,
        AutomationRuleFilters filters,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AutomationRules
            .Where(r => r.TenantId == tenantId);

        if (filters.TriggerType.HasValue)
            query = query.Where(r => r.TriggerType == filters.TriggerType.Value);

        if (filters.ActionType.HasValue)
            query = query.Where(r => r.ActionType == filters.ActionType.Value);

        if (filters.IsActive.HasValue)
            query = query.Where(r => r.IsActive == filters.IsActive.Value);

        if (!string.IsNullOrEmpty(filters.SearchTerm))
        {
            var term = filters.SearchTerm.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(term) ||
                                     (r.Description != null && r.Description.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(r => MapToDto(r))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AutomationRuleDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filters.Page,
            PageSize = filters.PageSize
        };
    }

    public async Task<AutomationRuleDetailDto?> GetRuleByIdAsync(
        Guid tenantId,
        Guid ruleId,
        CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.AutomationRules
            .FirstOrDefaultAsync(r => r.Id == ruleId && r.TenantId == tenantId, cancellationToken);

        if (rule == null)
            return null;

        var conditions = await _dbContext.RuleConditions
            .Where(c => c.RuleId == ruleId)
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);

        var recentExecutions = await _dbContext.RuleExecutions
            .Where(e => e.RuleId == ruleId)
            .OrderByDescending(e => e.StartedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        return new AutomationRuleDetailDto
        {
            Id = rule.Id,
            Name = rule.Name,
            Description = rule.Description,
            TriggerType = rule.TriggerType,
            TriggerEntityType = rule.TriggerEntityType,
            TriggerEvent = rule.TriggerEvent,
            CronExpression = rule.CronExpression,
            Timezone = rule.Timezone,
            ActionType = rule.ActionType,
            ConditionsJson = rule.ConditionsJson,
            ActionConfigJson = rule.ActionConfigJson,
            IsActive = rule.IsActive,
            Priority = rule.Priority,
            MaxExecutionsPerDay = rule.MaxExecutionsPerDay,
            CooldownSeconds = rule.CooldownSeconds,
            TotalExecutions = rule.TotalExecutions,
            SuccessfulExecutions = rule.SuccessfulExecutions,
            FailedExecutions = rule.FailedExecutions,
            LastExecutedAt = rule.LastExecutedAt,
            NextScheduledAt = rule.NextScheduledAt,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt,
            Conditions = conditions.Select(c => new RuleConditionDto
            {
                Id = c.Id,
                Order = c.Order,
                Logic = c.Logic,
                Field = c.Field,
                Operator = c.Operator,
                Value = c.Value,
                ValueType = c.ValueType
            }).ToList(),
            RecentExecutions = recentExecutions.Select(e => new RuleExecutionSummaryDto
            {
                Id = e.Id,
                StartedAt = e.StartedAt,
                Status = e.Status,
                DurationMs = e.DurationMs,
                ConditionsMet = e.ConditionsMet,
                ErrorMessage = e.ErrorMessage
            }).ToList()
        };
    }

    public async Task<AutomationRuleDto> CreateRuleAsync(
        Guid tenantId,
        CreateAutomationRuleRequest request,
        string? userId,
        CancellationToken cancellationToken = default)
    {
        var rule = new AutomationRule
        {
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            TriggerType = request.TriggerType,
            TriggerEntityType = request.TriggerEntityType,
            TriggerEvent = request.TriggerEvent,
            CronExpression = request.CronExpression,
            Timezone = request.Timezone,
            ActionType = request.ActionType,
            ActionConfigJson = request.ActionConfig != null ? JsonSerializer.Serialize(request.ActionConfig, JsonOptions) : null,
            ConditionsJson = request.Conditions != null ? JsonSerializer.Serialize(request.Conditions, JsonOptions) : null,
            IsActive = request.IsActive,
            Priority = request.Priority,
            MaxExecutionsPerDay = request.MaxExecutionsPerDay,
            CooldownSeconds = request.CooldownSeconds,
            CreatedBy = userId
        };

        // Calculate next scheduled time for schedule triggers
        if (rule.TriggerType == TriggerType.Schedule && !string.IsNullOrEmpty(rule.CronExpression))
        {
            rule.NextScheduledAt = CalculateNextSchedule(rule.CronExpression, rule.Timezone);
        }

        _dbContext.AutomationRules.Add(rule);

        // Add conditions as separate entities if needed
        if (request.Conditions?.Any() == true)
        {
            foreach (var conditionReq in request.Conditions)
            {
                var condition = new RuleCondition
                {
                    TenantId = tenantId,
                    RuleId = rule.Id,
                    Order = conditionReq.Order,
                    Logic = conditionReq.Logic,
                    Field = conditionReq.Field,
                    Operator = conditionReq.Operator,
                    Value = conditionReq.Value,
                    ValueType = conditionReq.ValueType,
                    CreatedBy = userId
                };
                _dbContext.RuleConditions.Add(condition);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(rule);
    }

    public async Task<AutomationRuleDto?> UpdateRuleAsync(
        Guid tenantId,
        Guid ruleId,
        UpdateAutomationRuleRequest request,
        string? userId,
        CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.AutomationRules
            .FirstOrDefaultAsync(r => r.Id == ruleId && r.TenantId == tenantId, cancellationToken);

        if (rule == null)
            return null;

        if (request.Name != null) rule.Name = request.Name;
        if (request.Description != null) rule.Description = request.Description;
        if (request.TriggerType.HasValue) rule.TriggerType = request.TriggerType.Value;
        if (request.TriggerEntityType != null) rule.TriggerEntityType = request.TriggerEntityType;
        if (request.TriggerEvent != null) rule.TriggerEvent = request.TriggerEvent;
        if (request.CronExpression != null) rule.CronExpression = request.CronExpression;
        if (request.Timezone != null) rule.Timezone = request.Timezone;
        if (request.ActionType.HasValue) rule.ActionType = request.ActionType.Value;
        if (request.ActionConfig != null) rule.ActionConfigJson = JsonSerializer.Serialize(request.ActionConfig, JsonOptions);
        if (request.IsActive.HasValue) rule.IsActive = request.IsActive.Value;
        if (request.Priority.HasValue) rule.Priority = request.Priority.Value;
        if (request.MaxExecutionsPerDay.HasValue) rule.MaxExecutionsPerDay = request.MaxExecutionsPerDay;
        if (request.CooldownSeconds.HasValue) rule.CooldownSeconds = request.CooldownSeconds;

        rule.UpdatedBy = userId;
        rule.UpdatedAt = DateTime.UtcNow;

        // Update conditions
        if (request.Conditions != null)
        {
            // Remove old conditions
            var oldConditions = await _dbContext.RuleConditions
                .Where(c => c.RuleId == ruleId)
                .ToListAsync(cancellationToken);
            _dbContext.RuleConditions.RemoveRange(oldConditions);

            // Add new conditions
            rule.ConditionsJson = JsonSerializer.Serialize(request.Conditions, JsonOptions);
            foreach (var conditionReq in request.Conditions)
            {
                var condition = new RuleCondition
                {
                    TenantId = tenantId,
                    RuleId = rule.Id,
                    Order = conditionReq.Order,
                    Logic = conditionReq.Logic,
                    Field = conditionReq.Field,
                    Operator = conditionReq.Operator,
                    Value = conditionReq.Value,
                    ValueType = conditionReq.ValueType,
                    CreatedBy = userId
                };
                _dbContext.RuleConditions.Add(condition);
            }
        }

        // Recalculate next scheduled time
        if (rule.TriggerType == TriggerType.Schedule && !string.IsNullOrEmpty(rule.CronExpression))
        {
            rule.NextScheduledAt = CalculateNextSchedule(rule.CronExpression, rule.Timezone);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(rule);
    }

    public async Task<bool> DeleteRuleAsync(Guid tenantId, Guid ruleId, CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.AutomationRules
            .FirstOrDefaultAsync(r => r.Id == ruleId && r.TenantId == tenantId, cancellationToken);

        if (rule == null)
            return false;

        _dbContext.AutomationRules.Remove(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    #endregion

    #region Execution

    public async Task<RuleExecution> TriggerRuleAsync(
        Guid tenantId,
        Guid ruleId,
        TriggerRuleRequest? request,
        CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.AutomationRules
            .FirstOrDefaultAsync(r => r.Id == ruleId && r.TenantId == tenantId, cancellationToken);

        if (rule == null)
            throw new InvalidOperationException($"Rule {ruleId} not found");

        var execution = new RuleExecution
        {
            TenantId = tenantId,
            RuleId = ruleId,
            TriggerEntityType = request?.EntityType,
            TriggerEntityId = request?.EntityId,
            TriggerEventData = request?.EventData != null ? JsonSerializer.Serialize(request.EventData, JsonOptions) : null,
            Status = ExecutionStatus.Running
        };

        _dbContext.RuleExecutions.Add(execution);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var startTime = DateTime.UtcNow;

        try
        {
            // Evaluate conditions
            execution.ConditionsMet = await _ruleEngine.EvaluateConditionsAsync(rule, request?.EventData, cancellationToken);

            if (!execution.ConditionsMet)
            {
                execution.Status = ExecutionStatus.Skipped;
                execution.ResultData = JsonSerializer.Serialize(new { reason = "Conditions not met" }, JsonOptions);
            }
            else
            {
                // Execute action
                var result = await _actionExecutor.ExecuteAsync(rule, execution, request?.EventData, cancellationToken);

                execution.Status = result.IsSuccess ? ExecutionStatus.Completed : ExecutionStatus.Failed;
                execution.ResultData = JsonSerializer.Serialize(new { message = result.Message }, JsonOptions);
                execution.CreatedEntityType = result.CreatedEntityType;
                execution.CreatedEntityId = result.CreatedEntityId;

                if (!result.IsSuccess)
                {
                    execution.ErrorMessage = result.Message;
                }
            }
        }
        catch (Exception ex)
        {
            execution.Status = ExecutionStatus.Failed;
            execution.ErrorMessage = ex.Message;
            execution.ErrorStackTrace = ex.StackTrace;
            _logger.LogError(ex, "Error executing rule {RuleId}", ruleId);
        }

        execution.CompletedAt = DateTime.UtcNow;
        execution.DurationMs = (int)(execution.CompletedAt.Value - startTime).TotalMilliseconds;

        // Update rule stats
        rule.TotalExecutions++;
        rule.LastExecutedAt = DateTime.UtcNow;
        if (execution.Status == ExecutionStatus.Completed)
            rule.SuccessfulExecutions++;
        else if (execution.Status == ExecutionStatus.Failed)
            rule.FailedExecutions++;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return execution;
    }

    public async Task<TestRuleResponse> TestRuleAsync(
        Guid tenantId,
        Guid ruleId,
        TestRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.AutomationRules
            .FirstOrDefaultAsync(r => r.Id == ruleId && r.TenantId == tenantId, cancellationToken);

        if (rule == null)
            throw new InvalidOperationException($"Rule {ruleId} not found");

        var conditionResults = await _ruleEngine.EvaluateConditionsDetailedAsync(rule, request.TestData, cancellationToken);
        var conditionsMet = conditionResults.All(r => r.Passed);

        return new TestRuleResponse
        {
            WouldTrigger = rule.IsActive,
            ConditionsMet = conditionsMet,
            ConditionResults = conditionResults.Select(r => new DTOs.ConditionEvaluationResult
            {
                Field = r.Field,
                Operator = r.Operator,
                ExpectedValue = r.ExpectedValue,
                ActualValue = r.ActualValue,
                Passed = r.Passed
            }).ToList(),
            ActionPreview = $"Would execute: {rule.ActionType}"
        };
    }

    public async Task ProcessEventAsync(
        Guid tenantId,
        TriggerType triggerType,
        string entityType,
        string eventName,
        Guid? entityId,
        object? eventData,
        CancellationToken cancellationToken = default)
    {
        var matchingRules = await _ruleEngine.GetMatchingRulesAsync(
            tenantId, triggerType, entityType, eventName, cancellationToken);

        _logger.LogDebug("Found {Count} matching rules for event {EntityType}.{EventName}",
            matchingRules.Count, entityType, eventName);

        foreach (var rule in matchingRules)
        {
            try
            {
                await TriggerRuleAsync(tenantId, rule.Id, new TriggerRuleRequest
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    EventData = eventData
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing rule {RuleId} for event {EntityType}.{EventName}",
                    rule.Id, entityType, eventName);
            }
        }
    }

    #endregion

    #region Executions History

    public async Task<PaginatedResult<RuleExecutionDto>> GetExecutionsAsync(
        Guid tenantId,
        RuleExecutionFilters filters,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.RuleExecutions
            .Include(e => e.Rule)
            .Where(e => e.TenantId == tenantId);

        if (filters.RuleId.HasValue)
            query = query.Where(e => e.RuleId == filters.RuleId.Value);

        if (filters.Status.HasValue)
            query = query.Where(e => e.Status == filters.Status.Value);

        if (filters.DateFrom.HasValue)
            query = query.Where(e => e.StartedAt >= filters.DateFrom.Value);

        if (filters.DateTo.HasValue)
            query = query.Where(e => e.StartedAt <= filters.DateTo.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.StartedAt)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(e => new RuleExecutionDto
            {
                Id = e.Id,
                RuleId = e.RuleId,
                RuleName = e.Rule.Name,
                TriggerEntityType = e.TriggerEntityType,
                TriggerEntityId = e.TriggerEntityId,
                TriggerEventData = e.TriggerEventData,
                StartedAt = e.StartedAt,
                CompletedAt = e.CompletedAt,
                DurationMs = e.DurationMs,
                Status = e.Status,
                ConditionsMet = e.ConditionsMet,
                ResultData = e.ResultData,
                ErrorMessage = e.ErrorMessage,
                CreatedEntityType = e.CreatedEntityType,
                CreatedEntityId = e.CreatedEntityId
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<RuleExecutionDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filters.Page,
            PageSize = filters.PageSize
        };
    }

    #endregion

    #region Stats

    public async Task<AutomationStatsDto> GetStatsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        var totalRules = await _dbContext.AutomationRules
            .CountAsync(r => r.TenantId == tenantId, cancellationToken);

        var activeRules = await _dbContext.AutomationRules
            .CountAsync(r => r.TenantId == tenantId && r.IsActive, cancellationToken);

        var todayExecutions = await _dbContext.RuleExecutions
            .Where(e => e.TenantId == tenantId && e.StartedAt >= today)
            .GroupBy(e => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Successful = g.Count(e => e.Status == ExecutionStatus.Completed),
                Failed = g.Count(e => e.Status == ExecutionStatus.Failed)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var pendingJobs = await _dbContext.ScheduledJobs
            .CountAsync(j => j.Rule.TenantId == tenantId && j.Status == ScheduledJobStatus.Pending, cancellationToken);

        // Get execution trend for last 7 days
        var sevenDaysAgo = today.AddDays(-6);
        var trend = await _dbContext.RuleExecutions
            .Where(e => e.TenantId == tenantId && e.StartedAt >= sevenDaysAgo)
            .GroupBy(e => e.StartedAt.Date)
            .Select(g => new RuleExecutionTrendDto
            {
                Date = g.Key,
                Total = g.Count(),
                Successful = g.Count(e => e.Status == ExecutionStatus.Completed),
                Failed = g.Count(e => e.Status == ExecutionStatus.Failed)
            })
            .OrderBy(t => t.Date)
            .ToListAsync(cancellationToken);

        // Top rules by executions
        var topRules = await _dbContext.AutomationRules
            .Where(r => r.TenantId == tenantId && r.TotalExecutions > 0)
            .OrderByDescending(r => r.TotalExecutions)
            .Take(5)
            .Select(r => new TopRuleDto
            {
                RuleId = r.Id,
                RuleName = r.Name,
                Executions = r.TotalExecutions,
                SuccessRate = r.TotalExecutions > 0
                    ? (decimal)r.SuccessfulExecutions / r.TotalExecutions * 100
                    : 0
            })
            .ToListAsync(cancellationToken);

        return new AutomationStatsDto
        {
            TotalRules = totalRules,
            ActiveRules = activeRules,
            TotalExecutionsToday = todayExecutions?.Total ?? 0,
            SuccessfulExecutionsToday = todayExecutions?.Successful ?? 0,
            FailedExecutionsToday = todayExecutions?.Failed ?? 0,
            ScheduledJobsPending = pendingJobs,
            ExecutionTrend = trend,
            TopRulesByExecutions = topRules
        };
    }

    #endregion

    #region Helpers

    private static AutomationRuleDto MapToDto(AutomationRule rule) => new()
    {
        Id = rule.Id,
        Name = rule.Name,
        Description = rule.Description,
        TriggerType = rule.TriggerType,
        TriggerEntityType = rule.TriggerEntityType,
        TriggerEvent = rule.TriggerEvent,
        CronExpression = rule.CronExpression,
        ActionType = rule.ActionType,
        IsActive = rule.IsActive,
        Priority = rule.Priority,
        TotalExecutions = rule.TotalExecutions,
        SuccessfulExecutions = rule.SuccessfulExecutions,
        FailedExecutions = rule.FailedExecutions,
        LastExecutedAt = rule.LastExecutedAt,
        NextScheduledAt = rule.NextScheduledAt,
        CreatedAt = rule.CreatedAt,
        UpdatedAt = rule.UpdatedAt
    };

    private DateTime? CalculateNextSchedule(string cronExpression, string? timezone)
    {
        // Simple cron parser - in production use a library like Cronos
        // For now, just return next hour
        return DateTime.UtcNow.AddHours(1);
    }

    #endregion
}
