using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Audit.DTOs;
using SmartWMS.API.Modules.Audit.Models;
using System.Text.Json;

namespace SmartWMS.API.Modules.Audit.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    #region Audit Logs

    public async Task<ApiResponse<PaginatedResult<AuditLogSummaryDto>>> GetAuditLogsAsync(
        Guid tenantId, AuditLogFilterRequest filter)
    {
        var query = _context.AuditLogs
            .Where(a => a.TenantId == tenantId);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.EventType))
            query = query.Where(a => a.EventType == filter.EventType);

        if (!string.IsNullOrWhiteSpace(filter.EntityType))
            query = query.Where(a => a.EntityType == filter.EntityType);

        if (filter.EntityId.HasValue)
            query = query.Where(a => a.EntityId == filter.EntityId.Value);

        if (filter.UserId.HasValue)
            query = query.Where(a => a.UserId == filter.UserId.Value);

        if (filter.Severity.HasValue)
            query = query.Where(a => a.Severity == filter.Severity.Value);

        if (!string.IsNullOrWhiteSpace(filter.Module))
            query = query.Where(a => a.Module == filter.Module);

        if (filter.DateFrom.HasValue)
            query = query.Where(a => a.EventTime >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(a => a.EventTime <= filter.DateTo.Value);

        if (filter.IsSuccess.HasValue)
            query = query.Where(a => a.IsSuccess == filter.IsSuccess.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(a =>
                a.Action.ToLower().Contains(term) ||
                (a.EntityNumber != null && a.EntityNumber.ToLower().Contains(term)) ||
                (a.UserName != null && a.UserName.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.EventTime)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new AuditLogSummaryDto(
                a.Id,
                a.EventType,
                a.EntityType,
                a.EntityNumber,
                a.Action,
                a.Severity,
                a.UserName,
                a.EventTime,
                a.Module,
                a.IsSuccess
            ))
            .ToListAsync();

        var result = new PaginatedResult<AuditLogSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        return ApiResponse<PaginatedResult<AuditLogSummaryDto>>.Ok(result);
    }

    public async Task<ApiResponse<AuditLogDto>> GetAuditLogByIdAsync(Guid tenantId, Guid id)
    {
        var log = await _context.AuditLogs
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id);

        if (log == null)
            return ApiResponse<AuditLogDto>.Fail("Audit log not found");

        return ApiResponse<AuditLogDto>.Ok(MapToDto(log));
    }

    public async Task<ApiResponse<List<AuditLogDto>>> GetEntityAuditHistoryAsync(
        Guid tenantId, string entityType, Guid entityId)
    {
        var logs = await _context.AuditLogs
            .Where(a => a.TenantId == tenantId &&
                       a.EntityType == entityType &&
                       a.EntityId == entityId)
            .OrderByDescending(a => a.EventTime)
            .Take(100)
            .Select(a => MapToDto(a))
            .ToListAsync();

        return ApiResponse<List<AuditLogDto>>.Ok(logs);
    }

    public async Task LogAuditAsync(Guid tenantId, Guid? userId, CreateAuditLogRequest request)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var user = httpContext?.User;

        var log = new AuditLog
        {
            TenantId = tenantId,
            EventType = request.EventType,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            EntityNumber = request.EntityNumber,
            Action = request.Action,
            Severity = request.Severity,
            UserId = userId,
            UserName = user?.Identity?.Name,
            IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
            OldValues = request.OldValues,
            NewValues = request.NewValues,
            ChangedFields = request.ChangedFields,
            Module = request.Module,
            SubModule = request.SubModule,
            CorrelationId = request.CorrelationId,
            Notes = request.Notes,
            IsSuccess = request.IsSuccess,
            ErrorMessage = request.ErrorMessage,
            EventTime = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task LogAuditAsync(Guid tenantId, Guid? userId, string eventType, string entityType,
        Guid? entityId, string action, object? oldValues = null, object? newValues = null)
    {
        var request = new CreateAuditLogRequest(
            eventType,
            entityType,
            entityId,
            null,
            action,
            AuditSeverity.Info,
            oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            newValues != null ? JsonSerializer.Serialize(newValues) : null
        );

        await LogAuditAsync(tenantId, userId, request);
    }

    #endregion

    #region Activity Logs

    public async Task<ApiResponse<PaginatedResult<ActivityLogDto>>> GetActivityLogsAsync(
        Guid tenantId, ActivityLogFilterRequest filter)
    {
        var query = _context.ActivityLogs
            .Where(a => a.TenantId == tenantId);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.ActivityType))
            query = query.Where(a => a.ActivityType == filter.ActivityType);

        if (filter.UserId.HasValue)
            query = query.Where(a => a.UserId == filter.UserId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Module))
            query = query.Where(a => a.Module == filter.Module);

        if (filter.RelatedEntityId.HasValue)
            query = query.Where(a => a.RelatedEntityId == filter.RelatedEntityId.Value);

        if (!string.IsNullOrWhiteSpace(filter.RelatedEntityType))
            query = query.Where(a => a.RelatedEntityType == filter.RelatedEntityType);

        if (!string.IsNullOrWhiteSpace(filter.DeviceType))
            query = query.Where(a => a.DeviceType == filter.DeviceType);

        if (filter.DateFrom.HasValue)
            query = query.Where(a => a.ActivityTime >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(a => a.ActivityTime <= filter.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(a =>
                a.Description.ToLower().Contains(term) ||
                (a.UserName != null && a.UserName.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.ActivityTime)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new ActivityLogDto(
                a.Id,
                a.ActivityType,
                a.Description,
                a.UserId,
                a.UserName,
                a.ActivityTime,
                a.Module,
                a.RelatedEntityId,
                a.RelatedEntityType,
                a.RelatedEntityNumber,
                a.DeviceType,
                a.Notes
            ))
            .ToListAsync();

        var result = new PaginatedResult<ActivityLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        return ApiResponse<PaginatedResult<ActivityLogDto>>.Ok(result);
    }

    public async Task<ApiResponse<List<ActivityLogDto>>> GetUserActivityAsync(
        Guid tenantId, Guid userId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.ActivityLogs
            .Where(a => a.TenantId == tenantId && a.UserId == userId);

        if (from.HasValue)
            query = query.Where(a => a.ActivityTime >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.ActivityTime <= to.Value);

        var activities = await query
            .OrderByDescending(a => a.ActivityTime)
            .Take(100)
            .Select(a => new ActivityLogDto(
                a.Id,
                a.ActivityType,
                a.Description,
                a.UserId,
                a.UserName,
                a.ActivityTime,
                a.Module,
                a.RelatedEntityId,
                a.RelatedEntityType,
                a.RelatedEntityNumber,
                a.DeviceType,
                a.Notes
            ))
            .ToListAsync();

        return ApiResponse<List<ActivityLogDto>>.Ok(activities);
    }

    public async Task LogActivityAsync(Guid tenantId, Guid userId, CreateActivityLogRequest request)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var user = httpContext?.User;

        var log = new ActivityLog
        {
            TenantId = tenantId,
            ActivityType = request.ActivityType,
            Description = request.Description,
            UserId = userId,
            UserName = user?.Identity?.Name,
            ActivityTime = DateTime.UtcNow,
            Module = request.Module,
            RelatedEntityId = request.RelatedEntityId,
            RelatedEntityType = request.RelatedEntityType,
            RelatedEntityNumber = request.RelatedEntityNumber,
            DeviceType = request.DeviceType,
            DeviceId = request.DeviceId,
            Notes = request.Notes
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task LogActivityAsync(Guid tenantId, Guid userId, string activityType, string description,
        string? module = null, Guid? relatedEntityId = null, string? relatedEntityType = null)
    {
        var request = new CreateActivityLogRequest(
            activityType,
            description,
            module,
            relatedEntityId,
            relatedEntityType
        );

        await LogActivityAsync(tenantId, userId, request);
    }

    #endregion

    #region System Events

    public async Task<ApiResponse<PaginatedResult<SystemEventLogDto>>> GetSystemEventsAsync(
        Guid tenantId, SystemEventFilterRequest filter)
    {
        var query = _context.SystemEventLogs
            .Where(e => e.TenantId == tenantId);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.EventType))
            query = query.Where(e => e.EventType == filter.EventType);

        if (filter.Category.HasValue)
            query = query.Where(e => e.Category == filter.Category.Value);

        if (filter.MinSeverity.HasValue)
            query = query.Where(e => e.Severity >= filter.MinSeverity.Value);

        if (!string.IsNullOrWhiteSpace(filter.Source))
            query = query.Where(e => e.Source == filter.Source);

        if (filter.DateFrom.HasValue)
            query = query.Where(e => e.EventTime >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(e => e.EventTime <= filter.DateTo.Value);

        if (filter.HasException.HasValue)
        {
            if (filter.HasException.Value)
                query = query.Where(e => e.ExceptionType != null);
            else
                query = query.Where(e => e.ExceptionType == null);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(e =>
                e.Message.ToLower().Contains(term) ||
                (e.ExceptionMessage != null && e.ExceptionMessage.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.EventTime)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(e => new SystemEventLogDto(
                e.Id,
                e.EventType,
                e.Category,
                e.Severity,
                e.Message,
                e.Details,
                e.EventTime,
                e.Source,
                e.ExceptionType,
                e.ExceptionMessage,
                e.CorrelationId
            ))
            .ToListAsync();

        var result = new PaginatedResult<SystemEventLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        return ApiResponse<PaginatedResult<SystemEventLogDto>>.Ok(result);
    }

    public async Task<ApiResponse<SystemEventLogDto>> GetSystemEventByIdAsync(Guid tenantId, Guid id)
    {
        var log = await _context.SystemEventLogs
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id);

        if (log == null)
            return ApiResponse<SystemEventLogDto>.Fail("System event not found");

        return ApiResponse<SystemEventLogDto>.Ok(new SystemEventLogDto(
            log.Id,
            log.EventType,
            log.Category,
            log.Severity,
            log.Message,
            log.Details,
            log.EventTime,
            log.Source,
            log.ExceptionType,
            log.ExceptionMessage,
            log.CorrelationId
        ));
    }

    public async Task LogSystemEventAsync(Guid tenantId, CreateSystemEventLogRequest request)
    {
        var log = new SystemEventLog
        {
            TenantId = tenantId,
            EventType = request.EventType,
            Category = request.Category,
            Severity = request.Severity,
            Message = request.Message,
            Details = request.Details,
            Source = request.Source,
            SourceVersion = GetType().Assembly.GetName().Version?.ToString(),
            MachineName = Environment.MachineName,
            ExceptionType = request.ExceptionType,
            ExceptionMessage = request.ExceptionMessage,
            StackTrace = request.StackTrace,
            CorrelationId = request.CorrelationId,
            RequestId = request.RequestId,
            EventTime = DateTime.UtcNow
        };

        _context.SystemEventLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task LogErrorAsync(Guid tenantId, string source, Exception exception, string? correlationId = null)
    {
        var request = new CreateSystemEventLogRequest(
            "Error",
            SystemEventCategory.Application,
            SystemEventSeverity.Error,
            exception.Message,
            null,
            source,
            exception.GetType().FullName,
            exception.Message,
            exception.StackTrace,
            correlationId
        );

        await LogSystemEventAsync(tenantId, request);
    }

    #endregion

    #region Statistics

    public async Task<ApiResponse<AuditStatisticsDto>> GetAuditStatisticsAsync(
        Guid tenantId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.AuditLogs.Where(a => a.TenantId == tenantId);

        if (from.HasValue)
            query = query.Where(a => a.EventTime >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.EventTime <= to.Value);

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var totalEvents = await query.CountAsync();
        var todayEvents = await query.CountAsync(a => a.EventTime >= today && a.EventTime < tomorrow);
        var errorCount = await query.CountAsync(a => a.Severity == AuditSeverity.Error || a.Severity == AuditSeverity.Critical);
        var warningCount = await query.CountAsync(a => a.Severity == AuditSeverity.Warning);

        var eventsByModule = await query
            .Where(a => a.Module != null)
            .GroupBy(a => a.Module!)
            .Select(g => new { Module = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Module, x => x.Count);

        var eventsByType = await query
            .GroupBy(a => a.EventType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

        var eventsByUser = await query
            .Where(a => a.UserName != null)
            .GroupBy(a => a.UserName!)
            .Select(g => new { User = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToDictionaryAsync(x => x.User, x => x.Count);

        var lastEventTime = await query.MaxAsync(a => (DateTime?)a.EventTime);

        var stats = new AuditStatisticsDto(
            totalEvents,
            todayEvents,
            errorCount,
            warningCount,
            eventsByModule,
            eventsByType,
            eventsByUser,
            lastEventTime
        );

        return ApiResponse<AuditStatisticsDto>.Ok(stats);
    }

    public async Task<ApiResponse<UserActivitySummaryDto>> GetUserActivitySummaryAsync(
        Guid tenantId, Guid userId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.ActivityLogs
            .Where(a => a.TenantId == tenantId && a.UserId == userId);

        if (from.HasValue)
            query = query.Where(a => a.ActivityTime >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.ActivityTime <= to.Value);

        var totalActivities = await query.CountAsync();
        var lastActivityTime = await query.MaxAsync(a => (DateTime?)a.ActivityTime);

        var userName = await query.Select(a => a.UserName).FirstOrDefaultAsync();

        var activitiesByType = await query
            .GroupBy(a => a.ActivityType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

        var activitiesByModule = await query
            .Where(a => a.Module != null)
            .GroupBy(a => a.Module!)
            .Select(g => new { Module = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Module, x => x.Count);

        var summary = new UserActivitySummaryDto(
            userId,
            userName,
            totalActivities,
            lastActivityTime,
            activitiesByType,
            activitiesByModule
        );

        return ApiResponse<UserActivitySummaryDto>.Ok(summary);
    }

    #endregion

    #region Cleanup

    public async Task<ApiResponse<int>> PurgeOldLogsAsync(Guid tenantId, int daysToKeep)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        // Delete old audit logs
        var auditDeleted = await _context.AuditLogs
            .Where(a => a.TenantId == tenantId && a.EventTime < cutoffDate)
            .ExecuteDeleteAsync();

        // Delete old activity logs
        var activityDeleted = await _context.ActivityLogs
            .Where(a => a.TenantId == tenantId && a.ActivityTime < cutoffDate)
            .ExecuteDeleteAsync();

        // Delete old system events
        var systemDeleted = await _context.SystemEventLogs
            .Where(e => e.TenantId == tenantId && e.EventTime < cutoffDate)
            .ExecuteDeleteAsync();

        var totalDeleted = auditDeleted + activityDeleted + systemDeleted;

        return ApiResponse<int>.Ok(totalDeleted);
    }

    #endregion

    #region Private Helpers

    private static AuditLogDto MapToDto(AuditLog log)
    {
        return new AuditLogDto(
            log.Id,
            log.EventType,
            log.EntityType,
            log.EntityId,
            log.EntityNumber,
            log.Action,
            log.Severity,
            log.UserId,
            log.UserName,
            log.UserEmail,
            log.EventTime,
            log.IpAddress,
            log.OldValues,
            log.NewValues,
            log.ChangedFields,
            log.Module,
            log.SubModule,
            log.CorrelationId,
            log.Notes,
            log.IsSuccess,
            log.ErrorMessage
        );
    }

    #endregion
}
