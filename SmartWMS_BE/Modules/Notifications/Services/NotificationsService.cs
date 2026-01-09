using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Notifications.DTOs;
using SmartWMS.API.Modules.Notifications.Models;

namespace SmartWMS.API.Modules.Notifications.Services;

public class NotificationsService : INotificationsService
{
    private readonly ApplicationDbContext _context;

    public NotificationsService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Notifications CRUD

    public async Task<ApiResponse<PaginatedResult<NotificationSummaryDto>>> GetNotificationsAsync(
        Guid tenantId, Guid userId, NotificationQueryParams query)
    {
        var queryable = _context.Notifications
            .Where(n => n.TenantId == tenantId && n.UserId == userId);

        // Apply filters
        if (query.IsRead.HasValue)
            queryable = queryable.Where(n => n.IsRead == query.IsRead.Value);

        if (query.IsArchived.HasValue)
            queryable = queryable.Where(n => n.IsArchived == query.IsArchived.Value);
        else
            queryable = queryable.Where(n => !n.IsArchived); // Default: not archived

        if (query.Type.HasValue)
            queryable = queryable.Where(n => n.Type == query.Type.Value);

        if (query.MinPriority.HasValue)
            queryable = queryable.Where(n => n.Priority >= query.MinPriority.Value);

        if (!string.IsNullOrEmpty(query.Category))
            queryable = queryable.Where(n => n.Category == query.Category);

        if (query.FromDate.HasValue)
            queryable = queryable.Where(n => n.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            queryable = queryable.Where(n => n.CreatedAt <= query.ToDate.Value);

        // Exclude expired
        queryable = queryable.Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow);

        var totalCount = await queryable.CountAsync();

        var items = await queryable
            .OrderByDescending(n => n.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(n => new NotificationSummaryDto(
                n.Id,
                n.Type,
                n.Priority,
                n.Title,
                n.Category,
                n.IsRead,
                n.CreatedAt
            ))
            .ToListAsync();

        var result = new PaginatedResult<NotificationSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };

        return ApiResponse<PaginatedResult<NotificationSummaryDto>>.Ok(result);
    }

    public async Task<ApiResponse<NotificationDto>> GetNotificationByIdAsync(Guid tenantId, Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.TenantId == tenantId && n.Id == notificationId);

        if (notification == null)
            return ApiResponse<NotificationDto>.Fail("Notification not found");

        return ApiResponse<NotificationDto>.Ok(MapToDto(notification));
    }

    public async Task<ApiResponse<NotificationCountDto>> GetNotificationCountAsync(Guid tenantId, Guid userId)
    {
        var now = DateTime.UtcNow;

        var notifications = await _context.Notifications
            .Where(n => n.TenantId == tenantId && n.UserId == userId && !n.IsArchived)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > now)
            .ToListAsync();

        var count = new NotificationCountDto(
            Total: notifications.Count,
            Unread: notifications.Count(n => !n.IsRead),
            High: notifications.Count(n => !n.IsRead && n.Priority == NotificationPriority.High),
            Urgent: notifications.Count(n => !n.IsRead && n.Priority == NotificationPriority.Urgent)
        );

        return ApiResponse<NotificationCountDto>.Ok(count);
    }

    public async Task<ApiResponse<NotificationDto>> CreateNotificationAsync(Guid tenantId, CreateNotificationRequest request)
    {
        // Check user preferences
        var preference = await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.TenantId == tenantId &&
                                      p.UserId == request.UserId &&
                                      (p.Category == request.Category || p.Category == "*"));

        if (preference != null)
        {
            if (preference.MuteAll && (preference.MutedUntil == null || preference.MutedUntil > DateTime.UtcNow))
                return ApiResponse<NotificationDto>.Fail("User has muted notifications for this category");

            if (request.Priority < preference.MinimumPriority)
                return ApiResponse<NotificationDto>.Fail("Notification priority below user threshold");

            if (!preference.InAppEnabled)
                return ApiResponse<NotificationDto>.Fail("User has disabled in-app notifications for this category");
        }

        var notification = new Notification
        {
            TenantId = tenantId,
            UserId = request.UserId,
            Type = request.Type,
            Priority = request.Priority,
            Title = request.Title,
            Message = request.Message,
            Category = request.Category,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            ActionUrl = request.ActionUrl,
            ExpiresAt = request.ExpiresAt,
            IsRead = false,
            IsArchived = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // TODO: Send email/push if requested and enabled in preferences

        return ApiResponse<NotificationDto>.Ok(MapToDto(notification));
    }

    public async Task<ApiResponse<List<NotificationDto>>> CreateBulkNotificationsAsync(
        Guid tenantId, CreateBulkNotificationRequest request)
    {
        var notifications = new List<Notification>();

        foreach (var userId in request.UserIds)
        {
            var notification = new Notification
            {
                TenantId = tenantId,
                UserId = userId,
                Type = request.Type,
                Priority = request.Priority,
                Title = request.Title,
                Message = request.Message,
                Category = request.Category,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                ActionUrl = request.ActionUrl,
                ExpiresAt = request.ExpiresAt,
                IsRead = false,
                IsArchived = false
            };

            notifications.Add(notification);
        }

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();

        return ApiResponse<List<NotificationDto>>.Ok(
            notifications.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<NotificationDto>> CreateFromTemplateAsync(
        Guid tenantId, CreateNotificationFromTemplateRequest request)
    {
        var template = await _context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Code == request.TemplateCode && t.IsActive);

        if (template == null)
            return ApiResponse<NotificationDto>.Fail("Template not found or inactive");

        var title = ReplaceParameters(template.TitleTemplate, request.Parameters);
        var message = ReplaceParameters(template.MessageTemplate, request.Parameters);

        var notification = new Notification
        {
            TenantId = tenantId,
            UserId = request.UserId,
            Type = template.Type,
            Priority = NotificationPriority.Normal,
            Title = title,
            Message = message,
            Category = template.Category,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            ActionUrl = request.ActionUrl,
            IsRead = false,
            IsArchived = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return ApiResponse<NotificationDto>.Ok(MapToDto(notification));
    }

    public async Task<ApiResponse<bool>> DeleteNotificationAsync(Guid tenantId, Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.TenantId == tenantId && n.Id == notificationId);

        if (notification == null)
            return ApiResponse<bool>.Fail("Notification not found");

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Notification deleted");
    }

    #endregion

    #region Notification Actions

    public async Task<ApiResponse<bool>> MarkAsReadAsync(Guid tenantId, Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.TenantId == tenantId && n.Id == notificationId);

        if (notification == null)
            return ApiResponse<bool>.Fail("Notification not found");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Marked as read");
    }

    public async Task<ApiResponse<bool>> MarkAsUnreadAsync(Guid tenantId, Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.TenantId == tenantId && n.Id == notificationId);

        if (notification == null)
            return ApiResponse<bool>.Fail("Notification not found");

        notification.IsRead = false;
        notification.ReadAt = null;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Marked as unread");
    }

    public async Task<ApiResponse<int>> MarkAllAsReadAsync(Guid tenantId, Guid userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.TenantId == tenantId && n.UserId == userId && !n.IsRead)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _context.SaveChangesAsync();

        return ApiResponse<int>.Ok(notifications.Count, $"{notifications.Count} notifications marked as read");
    }

    public async Task<ApiResponse<bool>> ArchiveNotificationAsync(Guid tenantId, Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.TenantId == tenantId && n.Id == notificationId);

        if (notification == null)
            return ApiResponse<bool>.Fail("Notification not found");

        notification.IsArchived = true;
        notification.ArchivedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Notification archived");
    }

    public async Task<ApiResponse<int>> ArchiveAllReadAsync(Guid tenantId, Guid userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.TenantId == tenantId && n.UserId == userId && n.IsRead && !n.IsArchived)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var notification in notifications)
        {
            notification.IsArchived = true;
            notification.ArchivedAt = now;
        }

        await _context.SaveChangesAsync();

        return ApiResponse<int>.Ok(notifications.Count, $"{notifications.Count} notifications archived");
    }

    #endregion

    #region Preferences

    public async Task<ApiResponse<UserNotificationSettingsDto>> GetUserSettingsAsync(Guid tenantId, Guid userId)
    {
        var preferences = await _context.NotificationPreferences
            .Where(p => p.TenantId == tenantId && p.UserId == userId)
            .ToListAsync();

        var globalSetting = preferences.FirstOrDefault(p => p.Category == "*");

        var settings = new UserNotificationSettingsDto(
            GlobalMute: globalSetting?.MuteAll ?? false,
            MutedUntil: globalSetting?.MutedUntil,
            CategoryPreferences: preferences
                .Where(p => p.Category != "*")
                .Select(p => new NotificationPreferenceDto(
                    p.Id,
                    p.UserId,
                    p.Category,
                    p.InAppEnabled,
                    p.EmailEnabled,
                    p.PushEnabled,
                    p.MinimumPriority,
                    p.MuteAll,
                    p.MutedUntil
                ))
                .ToList()
        );

        return ApiResponse<UserNotificationSettingsDto>.Ok(settings);
    }

    public async Task<ApiResponse<bool>> UpdateGlobalSettingsAsync(
        Guid tenantId, Guid userId, UpdateGlobalSettingsRequest request)
    {
        var preference = await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.UserId == userId && p.Category == "*");

        if (preference == null)
        {
            preference = new NotificationPreference
            {
                TenantId = tenantId,
                UserId = userId,
                Category = "*"
            };
            _context.NotificationPreferences.Add(preference);
        }

        if (request.GlobalMute.HasValue)
            preference.MuteAll = request.GlobalMute.Value;

        preference.MutedUntil = request.MutedUntil;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Global settings updated");
    }

    public async Task<ApiResponse<NotificationPreferenceDto>> GetPreferenceAsync(
        Guid tenantId, Guid userId, string category)
    {
        var preference = await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.UserId == userId && p.Category == category);

        if (preference == null)
            return ApiResponse<NotificationPreferenceDto>.Fail("Preference not found");

        return ApiResponse<NotificationPreferenceDto>.Ok(new NotificationPreferenceDto(
            preference.Id,
            preference.UserId,
            preference.Category,
            preference.InAppEnabled,
            preference.EmailEnabled,
            preference.PushEnabled,
            preference.MinimumPriority,
            preference.MuteAll,
            preference.MutedUntil
        ));
    }

    public async Task<ApiResponse<NotificationPreferenceDto>> UpdatePreferenceAsync(
        Guid tenantId, Guid userId, string category, UpdatePreferenceRequest request)
    {
        var preference = await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.UserId == userId && p.Category == category);

        if (preference == null)
        {
            preference = new NotificationPreference
            {
                TenantId = tenantId,
                UserId = userId,
                Category = category
            };
            _context.NotificationPreferences.Add(preference);
        }

        if (request.InAppEnabled.HasValue)
            preference.InAppEnabled = request.InAppEnabled.Value;

        if (request.EmailEnabled.HasValue)
            preference.EmailEnabled = request.EmailEnabled.Value;

        if (request.PushEnabled.HasValue)
            preference.PushEnabled = request.PushEnabled.Value;

        if (request.MinimumPriority.HasValue)
            preference.MinimumPriority = request.MinimumPriority.Value;

        if (request.MuteAll.HasValue)
            preference.MuteAll = request.MuteAll.Value;

        preference.MutedUntil = request.MutedUntil;

        await _context.SaveChangesAsync();

        return ApiResponse<NotificationPreferenceDto>.Ok(new NotificationPreferenceDto(
            preference.Id,
            preference.UserId,
            preference.Category,
            preference.InAppEnabled,
            preference.EmailEnabled,
            preference.PushEnabled,
            preference.MinimumPriority,
            preference.MuteAll,
            preference.MutedUntil
        ));
    }

    #endregion

    #region Templates

    public async Task<ApiResponse<List<NotificationTemplateDto>>> GetTemplatesAsync()
    {
        var templates = await _context.NotificationTemplates
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .Select(t => new NotificationTemplateDto(
                t.Id,
                t.Code,
                t.Name,
                t.Description,
                t.Type,
                t.Category,
                t.TitleTemplate,
                t.MessageTemplate,
                t.EmailSubjectTemplate,
                t.EmailBodyTemplate,
                t.IsActive
            ))
            .ToListAsync();

        return ApiResponse<List<NotificationTemplateDto>>.Ok(templates);
    }

    public async Task<ApiResponse<NotificationTemplateDto>> GetTemplateByCodeAsync(string code)
    {
        var template = await _context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Code == code);

        if (template == null)
            return ApiResponse<NotificationTemplateDto>.Fail("Template not found");

        return ApiResponse<NotificationTemplateDto>.Ok(new NotificationTemplateDto(
            template.Id,
            template.Code,
            template.Name,
            template.Description,
            template.Type,
            template.Category,
            template.TitleTemplate,
            template.MessageTemplate,
            template.EmailSubjectTemplate,
            template.EmailBodyTemplate,
            template.IsActive
        ));
    }

    public async Task<ApiResponse<NotificationTemplateDto>> CreateTemplateAsync(CreateTemplateRequest request)
    {
        if (await _context.NotificationTemplates.AnyAsync(t => t.Code == request.Code))
            return ApiResponse<NotificationTemplateDto>.Fail("Template code already exists");

        var template = new NotificationTemplate
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Category = request.Category,
            TitleTemplate = request.TitleTemplate,
            MessageTemplate = request.MessageTemplate,
            EmailSubjectTemplate = request.EmailSubjectTemplate,
            EmailBodyTemplate = request.EmailBodyTemplate,
            IsActive = true
        };

        _context.NotificationTemplates.Add(template);
        await _context.SaveChangesAsync();

        return ApiResponse<NotificationTemplateDto>.Ok(new NotificationTemplateDto(
            template.Id,
            template.Code,
            template.Name,
            template.Description,
            template.Type,
            template.Category,
            template.TitleTemplate,
            template.MessageTemplate,
            template.EmailSubjectTemplate,
            template.EmailBodyTemplate,
            template.IsActive
        ));
    }

    public async Task<ApiResponse<NotificationTemplateDto>> UpdateTemplateAsync(
        Guid templateId, UpdateTemplateRequest request)
    {
        var template = await _context.NotificationTemplates.FindAsync(templateId);

        if (template == null)
            return ApiResponse<NotificationTemplateDto>.Fail("Template not found");

        if (!string.IsNullOrEmpty(request.Name))
            template.Name = request.Name;

        if (request.Description != null)
            template.Description = request.Description;

        if (request.Type.HasValue)
            template.Type = request.Type.Value;

        if (!string.IsNullOrEmpty(request.Category))
            template.Category = request.Category;

        if (!string.IsNullOrEmpty(request.TitleTemplate))
            template.TitleTemplate = request.TitleTemplate;

        if (!string.IsNullOrEmpty(request.MessageTemplate))
            template.MessageTemplate = request.MessageTemplate;

        if (request.EmailSubjectTemplate != null)
            template.EmailSubjectTemplate = request.EmailSubjectTemplate;

        if (request.EmailBodyTemplate != null)
            template.EmailBodyTemplate = request.EmailBodyTemplate;

        if (request.IsActive.HasValue)
            template.IsActive = request.IsActive.Value;

        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<NotificationTemplateDto>.Ok(new NotificationTemplateDto(
            template.Id,
            template.Code,
            template.Name,
            template.Description,
            template.Type,
            template.Category,
            template.TitleTemplate,
            template.MessageTemplate,
            template.EmailSubjectTemplate,
            template.EmailBodyTemplate,
            template.IsActive
        ));
    }

    public async Task<ApiResponse<bool>> DeleteTemplateAsync(Guid templateId)
    {
        var template = await _context.NotificationTemplates.FindAsync(templateId);

        if (template == null)
            return ApiResponse<bool>.Fail("Template not found");

        _context.NotificationTemplates.Remove(template);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Template deleted");
    }

    #endregion

    #region Cleanup

    public async Task<ApiResponse<int>> PurgeExpiredNotificationsAsync(Guid tenantId)
    {
        var now = DateTime.UtcNow;

        var expired = await _context.Notifications
            .Where(n => n.TenantId == tenantId && n.ExpiresAt != null && n.ExpiresAt <= now)
            .ToListAsync();

        _context.Notifications.RemoveRange(expired);
        await _context.SaveChangesAsync();

        return ApiResponse<int>.Ok(expired.Count, $"{expired.Count} expired notifications purged");
    }

    public async Task<ApiResponse<int>> PurgeOldArchivedAsync(Guid tenantId, int daysOld)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysOld);

        var oldArchived = await _context.Notifications
            .Where(n => n.TenantId == tenantId && n.IsArchived && n.ArchivedAt != null && n.ArchivedAt <= cutoff)
            .ToListAsync();

        _context.Notifications.RemoveRange(oldArchived);
        await _context.SaveChangesAsync();

        return ApiResponse<int>.Ok(oldArchived.Count, $"{oldArchived.Count} old archived notifications purged");
    }

    #endregion

    #region Private Helpers

    private static NotificationDto MapToDto(Notification n) => new(
        n.Id,
        n.UserId,
        n.Type,
        n.Priority,
        n.Title,
        n.Message,
        n.Category,
        n.EntityType,
        n.EntityId,
        n.ActionUrl,
        n.IsRead,
        n.ReadAt,
        n.IsArchived,
        n.CreatedAt,
        n.ExpiresAt
    );

    private static string ReplaceParameters(string template, Dictionary<string, string> parameters)
    {
        var result = template;
        foreach (var param in parameters)
        {
            result = result.Replace($"{{{{{param.Key}}}}}", param.Value);
        }
        return result;
    }

    #endregion
}
