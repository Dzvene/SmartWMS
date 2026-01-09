using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Automation.Models;

/// <summary>
/// Automation Rule - defines what triggers an action and under what conditions
/// </summary>
public class AutomationRule : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Trigger
    public TriggerType TriggerType { get; set; }
    public string? TriggerEntityType { get; set; } // Product, Order, StockLevel, etc.
    public string? TriggerEvent { get; set; } // Created, Updated, StatusChanged, etc.

    // Schedule trigger (for scheduled rules)
    public string? CronExpression { get; set; } // "0 6 * * *" = every day at 6:00
    public string? Timezone { get; set; }

    // Conditions (JSON)
    public string? ConditionsJson { get; set; }

    // Action
    public ActionType ActionType { get; set; }
    public string? ActionConfigJson { get; set; } // JSON with action-specific config

    // Status
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 100; // Lower = higher priority

    // Limits
    public int? MaxExecutionsPerDay { get; set; }
    public int? CooldownSeconds { get; set; } // Minimum time between executions

    // Stats
    public int TotalExecutions { get; set; }
    public int SuccessfulExecutions { get; set; }
    public int FailedExecutions { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public DateTime? NextScheduledAt { get; set; }

    // Navigation
    public ICollection<RuleExecution> Executions { get; set; } = new List<RuleExecution>();
}

/// <summary>
/// Rule Condition - a single condition in a rule
/// </summary>
public class RuleCondition : TenantEntity
{
    public Guid RuleId { get; set; }
    public AutomationRule Rule { get; set; } = null!;

    public int Order { get; set; }
    public ConditionLogic Logic { get; set; } = ConditionLogic.And; // AND/OR with previous

    public string Field { get; set; } = string.Empty; // e.g. "quantity", "status", "priority"
    public ConditionOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? ValueType { get; set; } // String, Number, Boolean, Date
}

/// <summary>
/// Rule Execution - log of each time a rule was executed
/// </summary>
public class RuleExecution : TenantEntity
{
    public Guid RuleId { get; set; }
    public AutomationRule Rule { get; set; } = null!;

    // Trigger info
    public string? TriggerEntityType { get; set; }
    public Guid? TriggerEntityId { get; set; }
    public string? TriggerEventData { get; set; } // JSON snapshot of trigger data

    // Execution
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int DurationMs { get; set; }

    // Result
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
    public bool ConditionsMet { get; set; }
    public string? ResultData { get; set; } // JSON with action result
    public string? ErrorMessage { get; set; }
    public string? ErrorStackTrace { get; set; }

    // Created entities
    public string? CreatedEntityType { get; set; }
    public Guid? CreatedEntityId { get; set; }
}

/// <summary>
/// Action Template - predefined action configurations
/// </summary>
public class ActionTemplate : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ActionType ActionType { get; set; }
    public string ConfigJson { get; set; } = "{}";

    public bool IsSystem { get; set; } // System templates can't be deleted
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Scheduled Job - for tracking scheduled rule executions
/// </summary>
public class ScheduledJob : TenantEntity
{
    public Guid RuleId { get; set; }
    public AutomationRule Rule { get; set; } = null!;

    public DateTime ScheduledFor { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ScheduledJobStatus Status { get; set; } = ScheduledJobStatus.Pending;
    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;
}

#region Enums

public enum TriggerType
{
    // Event-based
    EntityCreated,      // When entity is created
    EntityUpdated,      // When entity is updated
    EntityDeleted,      // When entity is deleted
    StatusChanged,      // When entity status changes

    // Threshold-based
    ThresholdCrossed,   // When value crosses threshold (e.g. stock < min)

    // Time-based
    Schedule,           // Cron-based schedule

    // Manual
    Manual,             // Triggered manually via API

    // Webhook
    WebhookReceived     // External webhook trigger
}

public enum ActionType
{
    // Tasks
    CreateTask,         // Create a warehouse task (pick, pack, putaway)
    AssignTask,         // Assign task to user/group

    // Notifications
    SendNotification,   // Send in-app notification
    SendEmail,          // Send email
    SendWebhook,        // Call external webhook

    // Status updates
    UpdateEntityStatus, // Update status of an entity
    UpdateEntityField,  // Update specific field

    // Reports
    GenerateReport,     // Generate and optionally email report

    // Integration
    TriggerSync,        // Trigger integration sync

    // Stock
    CreateAdjustment,   // Create stock adjustment
    CreateTransfer,     // Create stock transfer

    // Custom
    ExecuteScript       // Execute custom script/logic
}

public enum ConditionOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEquals,
    LessThan,
    LessThanOrEquals,
    Contains,
    NotContains,
    StartsWith,
    EndsWith,
    IsNull,
    IsNotNull,
    In,
    NotIn
}

public enum ConditionLogic
{
    And,
    Or
}

public enum ExecutionStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Skipped,        // Conditions not met
    Cancelled
}

public enum ScheduledJobStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
    Retrying
}

#endregion

#region Action Configs (for JSON serialization)

public class CreateTaskConfig
{
    public string TaskType { get; set; } = string.Empty; // Pick, Pack, Putaway, CycleCount
    public int? Priority { get; set; }
    public Guid? AssignToUserId { get; set; }
    public Guid? AssignToRoleId { get; set; }
    public string? Notes { get; set; }
}

public class SendNotificationConfig
{
    public string? TemplateCode { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string Priority { get; set; } = "Normal";
    public List<Guid>? UserIds { get; set; }
    public List<Guid>? RoleIds { get; set; }
    public bool NotifyCreator { get; set; }
    public bool NotifyAssignee { get; set; }
    public bool? SendToAllAdmins { get; set; }
    public string? NotificationType { get; set; } // Alert, Info, Warning, Success
    public string? ActionUrl { get; set; }
    public string? ActionLabel { get; set; }
}

public class SendEmailConfig
{
    public string? TemplateCode { get; set; }
    public List<string>? ToAddresses { get; set; }
    public List<Guid>? ToUserIds { get; set; }
    public List<Guid>? ToRoleIds { get; set; }
    public List<string>? CcAddresses { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? BodyTemplate { get; set; }
    public bool IsHtml { get; set; } = true;
}

public class SendWebhookConfig
{
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "POST";
    public Dictionary<string, string>? Headers { get; set; }
    public string? PayloadTemplate { get; set; } // JSON template with placeholders
    public int? TimeoutSeconds { get; set; }
    public string? AuthType { get; set; } // bearer, basic, apikey
    public string? AuthToken { get; set; }
    public string? AuthUsername { get; set; }
    public string? AuthPassword { get; set; }
    public string? ApiKeyHeader { get; set; }
}

public class UpdateEntityConfig
{
    public string EntityType { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class GenerateReportConfig
{
    public string ReportType { get; set; } = string.Empty;
    public Dictionary<string, string>? Parameters { get; set; }
    public bool EmailReport { get; set; }
    public List<string>? EmailAddresses { get; set; }
}

public class TriggerSyncConfig
{
    public Guid IntegrationId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Direction { get; set; } = "Outbound"; // Inbound, Outbound, Bidirectional
}

public class CreateAdjustmentConfig
{
    public string? ReasonCode { get; set; }
    public decimal? Quantity { get; set; }
    public string? Notes { get; set; }
}

public class CreateTransferConfig
{
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public decimal? Quantity { get; set; }
    public string? Notes { get; set; }
}

#endregion
