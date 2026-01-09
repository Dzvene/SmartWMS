using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Audit.Models;

/// <summary>
/// Audit Log - records all significant system events and changes
/// </summary>
public class AuditLog : TenantEntity
{
    // Event identification
    public required string EventType { get; set; } // Create, Update, Delete, Login, etc.
    public required string EntityType { get; set; } // Product, Order, User, etc.
    public Guid? EntityId { get; set; }
    public string? EntityNumber { get; set; } // Human-readable reference (e.g., SO-001)

    // What happened
    public required string Action { get; set; } // Detailed action description
    public AuditSeverity Severity { get; set; } = AuditSeverity.Info;

    // Who did it
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }

    // When and where
    public DateTime EventTime { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Change details (JSON)
    public string? OldValues { get; set; } // JSON of old values
    public string? NewValues { get; set; } // JSON of new values
    public string? ChangedFields { get; set; } // Comma-separated list of changed fields

    // Additional context
    public string? Module { get; set; } // Inventory, Orders, Users, etc.
    public string? SubModule { get; set; } // Products, StockLevels, etc.
    public string? CorrelationId { get; set; } // For tracking related events
    public string? SessionId { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? ErrorMessage { get; set; } // If action failed

    // Result
    public bool IsSuccess { get; set; } = true;
}

/// <summary>
/// Activity Log - simplified event log for user activities
/// </summary>
public class ActivityLog : TenantEntity
{
    // Activity info
    public required string ActivityType { get; set; }
    public required string Description { get; set; }

    // Who
    public Guid UserId { get; set; }
    public string? UserName { get; set; }

    // When
    public DateTime ActivityTime { get; set; } = DateTime.UtcNow;

    // Context
    public string? Module { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityNumber { get; set; }

    // Device/Session
    public string? DeviceType { get; set; } // Web, Mobile, Scanner, API
    public string? DeviceId { get; set; }

    // Notes
    public string? Notes { get; set; }
}

/// <summary>
/// System Event Log - for system-level events
/// </summary>
public class SystemEventLog : TenantEntity
{
    // Event info
    public required string EventType { get; set; }
    public SystemEventCategory Category { get; set; }
    public SystemEventSeverity Severity { get; set; } = SystemEventSeverity.Information;

    // Message
    public required string Message { get; set; }
    public string? Details { get; set; } // Additional JSON details

    // Timestamps
    public DateTime EventTime { get; set; } = DateTime.UtcNow;

    // Source
    public string? Source { get; set; } // Service/Module name
    public string? SourceVersion { get; set; }
    public string? MachineName { get; set; }

    // Error details
    public string? ExceptionType { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? StackTrace { get; set; }

    // Correlation
    public string? CorrelationId { get; set; }
    public string? RequestId { get; set; }
}

#region Enums

public enum AuditSeverity
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

public enum SystemEventCategory
{
    Application,
    Security,
    Performance,
    Integration,
    Scheduler,
    Database,
    FileSystem,
    Network,
    Other
}

public enum SystemEventSeverity
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical,
    Fatal
}

#endregion
