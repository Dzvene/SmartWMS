using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Notifications.Models;

public class Notification : TenantEntity
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Category { get; set; }

    // Link to related entity
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? ActionUrl { get; set; }

    // Status
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }

    // Delivery
    public bool EmailSent { get; set; }
    public DateTime? EmailSentAt { get; set; }
    public bool PushSent { get; set; }
    public DateTime? PushSentAt { get; set; }

    // Expiration
    public DateTime? ExpiresAt { get; set; }
}

public class NotificationPreference : TenantEntity
{
    public Guid UserId { get; set; }
    public string Category { get; set; } = string.Empty;

    // Channels
    public bool InAppEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; }
    public bool PushEnabled { get; set; }

    // Settings
    public NotificationPriority MinimumPriority { get; set; } = NotificationPriority.Normal;
    public bool MuteAll { get; set; }
    public DateTime? MutedUntil { get; set; }
}

public class NotificationTemplate : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NotificationType Type { get; set; }
    public string Category { get; set; } = string.Empty;

    // Templates
    public string TitleTemplate { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public string? EmailSubjectTemplate { get; set; }
    public string? EmailBodyTemplate { get; set; }
    public string? PushTitleTemplate { get; set; }
    public string? PushBodyTemplate { get; set; }

    public bool IsActive { get; set; } = true;
}

#region Enums

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    Alert,
    Task,
    Reminder
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}

#endregion
