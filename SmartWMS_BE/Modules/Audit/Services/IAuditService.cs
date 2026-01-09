using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Audit.DTOs;
using SmartWMS.API.Modules.Audit.Models;

namespace SmartWMS.API.Modules.Audit.Services;

public interface IAuditService
{
    // Audit Logs
    Task<ApiResponse<PaginatedResult<AuditLogSummaryDto>>> GetAuditLogsAsync(
        Guid tenantId, AuditLogFilterRequest filter);
    Task<ApiResponse<AuditLogDto>> GetAuditLogByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<List<AuditLogDto>>> GetEntityAuditHistoryAsync(
        Guid tenantId, string entityType, Guid entityId);
    Task LogAuditAsync(Guid tenantId, Guid? userId, CreateAuditLogRequest request);
    Task LogAuditAsync(Guid tenantId, Guid? userId, string eventType, string entityType,
        Guid? entityId, string action, object? oldValues = null, object? newValues = null);

    // Activity Logs
    Task<ApiResponse<PaginatedResult<ActivityLogDto>>> GetActivityLogsAsync(
        Guid tenantId, ActivityLogFilterRequest filter);
    Task<ApiResponse<List<ActivityLogDto>>> GetUserActivityAsync(
        Guid tenantId, Guid userId, DateTime? from = null, DateTime? to = null);
    Task LogActivityAsync(Guid tenantId, Guid userId, CreateActivityLogRequest request);
    Task LogActivityAsync(Guid tenantId, Guid userId, string activityType, string description,
        string? module = null, Guid? relatedEntityId = null, string? relatedEntityType = null);

    // System Events
    Task<ApiResponse<PaginatedResult<SystemEventLogDto>>> GetSystemEventsAsync(
        Guid tenantId, SystemEventFilterRequest filter);
    Task<ApiResponse<SystemEventLogDto>> GetSystemEventByIdAsync(Guid tenantId, Guid id);
    Task LogSystemEventAsync(Guid tenantId, CreateSystemEventLogRequest request);
    Task LogErrorAsync(Guid tenantId, string source, Exception exception, string? correlationId = null);

    // Statistics
    Task<ApiResponse<AuditStatisticsDto>> GetAuditStatisticsAsync(
        Guid tenantId, DateTime? from = null, DateTime? to = null);
    Task<ApiResponse<UserActivitySummaryDto>> GetUserActivitySummaryAsync(
        Guid tenantId, Guid userId, DateTime? from = null, DateTime? to = null);

    // Cleanup
    Task<ApiResponse<int>> PurgeOldLogsAsync(Guid tenantId, int daysToKeep);
}
