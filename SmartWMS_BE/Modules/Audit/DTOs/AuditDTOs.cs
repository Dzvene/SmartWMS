using SmartWMS.API.Modules.Audit.Models;

namespace SmartWMS.API.Modules.Audit.DTOs;

#region Audit Log DTOs

public record AuditLogDto(
    Guid Id,
    string EventType,
    string EntityType,
    Guid? EntityId,
    string? EntityNumber,
    string Action,
    AuditSeverity Severity,
    Guid? UserId,
    string? UserName,
    string? UserEmail,
    DateTime EventTime,
    string? IpAddress,
    string? OldValues,
    string? NewValues,
    string? ChangedFields,
    string? Module,
    string? SubModule,
    string? CorrelationId,
    string? Notes,
    bool IsSuccess,
    string? ErrorMessage
);

public record AuditLogSummaryDto(
    Guid Id,
    string EventType,
    string EntityType,
    string? EntityNumber,
    string Action,
    AuditSeverity Severity,
    string? UserName,
    DateTime EventTime,
    string? Module,
    bool IsSuccess
);

public record ActivityLogDto(
    Guid Id,
    string ActivityType,
    string Description,
    Guid UserId,
    string? UserName,
    DateTime ActivityTime,
    string? Module,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    string? RelatedEntityNumber,
    string? DeviceType,
    string? Notes
);

public record SystemEventLogDto(
    Guid Id,
    string EventType,
    SystemEventCategory Category,
    SystemEventSeverity Severity,
    string Message,
    string? Details,
    DateTime EventTime,
    string? Source,
    string? ExceptionType,
    string? ExceptionMessage,
    string? CorrelationId
);

#endregion

#region Create Requests

public record CreateAuditLogRequest(
    string EventType,
    string EntityType,
    Guid? EntityId,
    string? EntityNumber,
    string Action,
    AuditSeverity Severity = AuditSeverity.Info,
    string? OldValues = null,
    string? NewValues = null,
    string? ChangedFields = null,
    string? Module = null,
    string? SubModule = null,
    string? CorrelationId = null,
    string? Notes = null,
    bool IsSuccess = true,
    string? ErrorMessage = null
);

public record CreateActivityLogRequest(
    string ActivityType,
    string Description,
    string? Module = null,
    Guid? RelatedEntityId = null,
    string? RelatedEntityType = null,
    string? RelatedEntityNumber = null,
    string? DeviceType = null,
    string? DeviceId = null,
    string? Notes = null
);

public record CreateSystemEventLogRequest(
    string EventType,
    SystemEventCategory Category,
    SystemEventSeverity Severity,
    string Message,
    string? Details = null,
    string? Source = null,
    string? ExceptionType = null,
    string? ExceptionMessage = null,
    string? StackTrace = null,
    string? CorrelationId = null,
    string? RequestId = null
);

#endregion

#region Query/Filter

public record AuditLogFilterRequest(
    string? EventType = null,
    string? EntityType = null,
    Guid? EntityId = null,
    Guid? UserId = null,
    AuditSeverity? Severity = null,
    string? Module = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    bool? IsSuccess = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 50
);

public record ActivityLogFilterRequest(
    string? ActivityType = null,
    Guid? UserId = null,
    string? Module = null,
    Guid? RelatedEntityId = null,
    string? RelatedEntityType = null,
    string? DeviceType = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 50
);

public record SystemEventFilterRequest(
    string? EventType = null,
    SystemEventCategory? Category = null,
    SystemEventSeverity? MinSeverity = null,
    string? Source = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    bool? HasException = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 50
);

#endregion

#region Statistics

public record AuditStatisticsDto(
    int TotalEvents,
    int TodayEvents,
    int ErrorCount,
    int WarningCount,
    Dictionary<string, int> EventsByModule,
    Dictionary<string, int> EventsByType,
    Dictionary<string, int> EventsByUser,
    DateTime? LastEventTime
);

public record UserActivitySummaryDto(
    Guid UserId,
    string? UserName,
    int TotalActivities,
    DateTime? LastActivityTime,
    Dictionary<string, int> ActivitiesByType,
    Dictionary<string, int> ActivitiesByModule
);

#endregion
