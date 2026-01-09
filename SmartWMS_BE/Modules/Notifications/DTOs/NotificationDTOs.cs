using SmartWMS.API.Modules.Notifications.Models;

namespace SmartWMS.API.Modules.Notifications.DTOs;

#region Notification DTOs

public record NotificationDto(
    Guid Id,
    Guid UserId,
    NotificationType Type,
    NotificationPriority Priority,
    string Title,
    string Message,
    string? Category,
    string? EntityType,
    Guid? EntityId,
    string? ActionUrl,
    bool IsRead,
    DateTime? ReadAt,
    bool IsArchived,
    DateTime CreatedAt,
    DateTime? ExpiresAt
);

public record NotificationSummaryDto(
    Guid Id,
    NotificationType Type,
    NotificationPriority Priority,
    string Title,
    string? Category,
    bool IsRead,
    DateTime CreatedAt
);

public record NotificationCountDto(
    int Total,
    int Unread,
    int High,
    int Urgent
);

#endregion

#region Create/Update Requests

public record CreateNotificationRequest(
    Guid UserId,
    NotificationType Type,
    NotificationPriority Priority,
    string Title,
    string Message,
    string? Category,
    string? EntityType,
    Guid? EntityId,
    string? ActionUrl,
    DateTime? ExpiresAt,
    bool SendEmail,
    bool SendPush
);

public record CreateBulkNotificationRequest(
    List<Guid> UserIds,
    NotificationType Type,
    NotificationPriority Priority,
    string Title,
    string Message,
    string? Category,
    string? EntityType,
    Guid? EntityId,
    string? ActionUrl,
    DateTime? ExpiresAt,
    bool SendEmail,
    bool SendPush
);

public record CreateNotificationFromTemplateRequest(
    Guid UserId,
    string TemplateCode,
    Dictionary<string, string> Parameters,
    string? EntityType,
    Guid? EntityId,
    string? ActionUrl,
    bool SendEmail,
    bool SendPush
);

#endregion

#region Preference DTOs

public record NotificationPreferenceDto(
    Guid Id,
    Guid UserId,
    string Category,
    bool InAppEnabled,
    bool EmailEnabled,
    bool PushEnabled,
    NotificationPriority MinimumPriority,
    bool MuteAll,
    DateTime? MutedUntil
);

public record UpdatePreferenceRequest(
    bool? InAppEnabled,
    bool? EmailEnabled,
    bool? PushEnabled,
    NotificationPriority? MinimumPriority,
    bool? MuteAll,
    DateTime? MutedUntil
);

public record UserNotificationSettingsDto(
    bool GlobalMute,
    DateTime? MutedUntil,
    List<NotificationPreferenceDto> CategoryPreferences
);

public record UpdateGlobalSettingsRequest(
    bool? GlobalMute,
    DateTime? MutedUntil
);

#endregion

#region Template DTOs

public record NotificationTemplateDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    NotificationType Type,
    string Category,
    string TitleTemplate,
    string MessageTemplate,
    string? EmailSubjectTemplate,
    string? EmailBodyTemplate,
    bool IsActive
);

public record CreateTemplateRequest(
    string Code,
    string Name,
    string? Description,
    NotificationType Type,
    string Category,
    string TitleTemplate,
    string MessageTemplate,
    string? EmailSubjectTemplate,
    string? EmailBodyTemplate
);

public record UpdateTemplateRequest(
    string? Name,
    string? Description,
    NotificationType? Type,
    string? Category,
    string? TitleTemplate,
    string? MessageTemplate,
    string? EmailSubjectTemplate,
    string? EmailBodyTemplate,
    bool? IsActive
);

#endregion

#region Query Parameters

public record NotificationQueryParams(
    int Page = 1,
    int PageSize = 20,
    bool? IsRead = null,
    bool? IsArchived = null,
    NotificationType? Type = null,
    NotificationPriority? MinPriority = null,
    string? Category = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
);

#endregion
