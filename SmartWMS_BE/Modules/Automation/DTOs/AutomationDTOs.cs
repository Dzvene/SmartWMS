using SmartWMS.API.Modules.Automation.Models;

namespace SmartWMS.API.Modules.Automation.DTOs;

#region Rule DTOs

public record AutomationRuleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }

    public TriggerType TriggerType { get; init; }
    public string TriggerTypeDisplay => TriggerType.ToString();
    public string? TriggerEntityType { get; init; }
    public string? TriggerEvent { get; init; }
    public string? CronExpression { get; init; }

    public ActionType ActionType { get; init; }
    public string ActionTypeDisplay => ActionType.ToString();

    public bool IsActive { get; init; }
    public int Priority { get; init; }

    public int TotalExecutions { get; init; }
    public int SuccessfulExecutions { get; init; }
    public int FailedExecutions { get; init; }
    public DateTime? LastExecutedAt { get; init; }
    public DateTime? NextScheduledAt { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record AutomationRuleDetailDto : AutomationRuleDto
{
    public string? Timezone { get; init; }
    public string? ConditionsJson { get; init; }
    public string? ActionConfigJson { get; init; }
    public int? MaxExecutionsPerDay { get; init; }
    public int? CooldownSeconds { get; init; }

    public List<RuleConditionDto> Conditions { get; init; } = new();
    public List<RuleExecutionSummaryDto> RecentExecutions { get; init; } = new();
}

public record CreateAutomationRuleRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }

    public TriggerType TriggerType { get; init; }
    public string? TriggerEntityType { get; init; }
    public string? TriggerEvent { get; init; }
    public string? CronExpression { get; init; }
    public string? Timezone { get; init; }

    public ActionType ActionType { get; init; }
    public object? ActionConfig { get; init; } // Will be serialized to JSON

    public List<CreateRuleConditionRequest>? Conditions { get; init; }

    public bool IsActive { get; init; } = true;
    public int Priority { get; init; } = 100;
    public int? MaxExecutionsPerDay { get; init; }
    public int? CooldownSeconds { get; init; }
}

public record UpdateAutomationRuleRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }

    public TriggerType? TriggerType { get; init; }
    public string? TriggerEntityType { get; init; }
    public string? TriggerEvent { get; init; }
    public string? CronExpression { get; init; }
    public string? Timezone { get; init; }

    public ActionType? ActionType { get; init; }
    public object? ActionConfig { get; init; }

    public List<CreateRuleConditionRequest>? Conditions { get; init; }

    public bool? IsActive { get; init; }
    public int? Priority { get; init; }
    public int? MaxExecutionsPerDay { get; init; }
    public int? CooldownSeconds { get; init; }
}

#endregion

#region Condition DTOs

public record RuleConditionDto
{
    public Guid Id { get; init; }
    public int Order { get; init; }
    public ConditionLogic Logic { get; init; }
    public string Field { get; init; } = string.Empty;
    public ConditionOperator Operator { get; init; }
    public string OperatorDisplay => Operator.ToString();
    public string Value { get; init; } = string.Empty;
    public string? ValueType { get; init; }
}

public record CreateRuleConditionRequest
{
    public int Order { get; init; }
    public ConditionLogic Logic { get; init; } = ConditionLogic.And;
    public string Field { get; init; } = string.Empty;
    public ConditionOperator Operator { get; init; }
    public string Value { get; init; } = string.Empty;
    public string? ValueType { get; init; }
}

#endregion

#region Execution DTOs

public record RuleExecutionDto
{
    public Guid Id { get; init; }
    public Guid RuleId { get; init; }
    public string? RuleName { get; init; }

    public string? TriggerEntityType { get; init; }
    public Guid? TriggerEntityId { get; init; }
    public string? TriggerEventData { get; init; }

    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int DurationMs { get; init; }

    public ExecutionStatus Status { get; init; }
    public string StatusDisplay => Status.ToString();
    public bool ConditionsMet { get; init; }
    public string? ResultData { get; init; }
    public string? ErrorMessage { get; init; }

    public string? CreatedEntityType { get; init; }
    public Guid? CreatedEntityId { get; init; }
}

public record RuleExecutionSummaryDto
{
    public Guid Id { get; init; }
    public DateTime StartedAt { get; init; }
    public ExecutionStatus Status { get; init; }
    public string StatusDisplay => Status.ToString();
    public int DurationMs { get; init; }
    public bool ConditionsMet { get; init; }
    public string? ErrorMessage { get; init; }
}

#endregion

#region Template DTOs

public record ActionTemplateDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ActionType ActionType { get; init; }
    public string ActionTypeDisplay => ActionType.ToString();
    public string ConfigJson { get; init; } = "{}";
    public bool IsSystem { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateActionTemplateRequest
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ActionType ActionType { get; init; }
    public object Config { get; init; } = new { };
    public bool IsActive { get; init; } = true;
}

public record UpdateActionTemplateRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public object? Config { get; init; }
    public bool? IsActive { get; init; }
}

#endregion

#region Execution Request DTOs

public record TriggerRuleRequest
{
    public string? EntityType { get; init; }
    public Guid? EntityId { get; init; }
    public object? EventData { get; init; }
}

public record TestRuleRequest
{
    public string? EntityType { get; init; }
    public Guid? EntityId { get; init; }
    public object? TestData { get; init; }
}

public record TestRuleResponse
{
    public bool WouldTrigger { get; init; }
    public bool ConditionsMet { get; init; }
    public List<ConditionEvaluationResult> ConditionResults { get; init; } = new();
    public string? ActionPreview { get; init; }
}

public record ConditionEvaluationResult
{
    public string Field { get; init; } = string.Empty;
    public string Operator { get; init; } = string.Empty;
    public string ExpectedValue { get; init; } = string.Empty;
    public string? ActualValue { get; init; }
    public bool Passed { get; init; }
}

#endregion

#region Query/Filter DTOs

public record AutomationRuleFilters
{
    public TriggerType? TriggerType { get; init; }
    public ActionType? ActionType { get; init; }
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
}

public record RuleExecutionFilters
{
    public Guid? RuleId { get; init; }
    public ExecutionStatus? Status { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
}

#endregion

#region Stats DTOs

public record AutomationStatsDto
{
    public int TotalRules { get; init; }
    public int ActiveRules { get; init; }
    public int TotalExecutionsToday { get; init; }
    public int SuccessfulExecutionsToday { get; init; }
    public int FailedExecutionsToday { get; init; }
    public int ScheduledJobsPending { get; init; }
    public List<RuleExecutionTrendDto> ExecutionTrend { get; init; } = new();
    public List<TopRuleDto> TopRulesByExecutions { get; init; } = new();
}

public record RuleExecutionTrendDto
{
    public DateTime Date { get; init; }
    public int Total { get; init; }
    public int Successful { get; init; }
    public int Failed { get; init; }
}

public record TopRuleDto
{
    public Guid RuleId { get; init; }
    public string RuleName { get; init; } = string.Empty;
    public int Executions { get; init; }
    public decimal SuccessRate { get; init; }
}

#endregion
