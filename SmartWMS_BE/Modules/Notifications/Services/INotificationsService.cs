using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Notifications.DTOs;

namespace SmartWMS.API.Modules.Notifications.Services;

public interface INotificationsService
{
    // Notifications CRUD
    Task<ApiResponse<PaginatedResult<NotificationSummaryDto>>> GetNotificationsAsync(
        Guid tenantId, Guid userId, NotificationQueryParams query);
    Task<ApiResponse<NotificationDto>> GetNotificationByIdAsync(Guid tenantId, Guid notificationId);
    Task<ApiResponse<NotificationCountDto>> GetNotificationCountAsync(Guid tenantId, Guid userId);
    Task<ApiResponse<NotificationDto>> CreateNotificationAsync(Guid tenantId, CreateNotificationRequest request);
    Task<ApiResponse<List<NotificationDto>>> CreateBulkNotificationsAsync(Guid tenantId, CreateBulkNotificationRequest request);
    Task<ApiResponse<NotificationDto>> CreateFromTemplateAsync(Guid tenantId, CreateNotificationFromTemplateRequest request);
    Task<ApiResponse<bool>> DeleteNotificationAsync(Guid tenantId, Guid notificationId);

    // Notification Actions
    Task<ApiResponse<bool>> MarkAsReadAsync(Guid tenantId, Guid notificationId);
    Task<ApiResponse<bool>> MarkAsUnreadAsync(Guid tenantId, Guid notificationId);
    Task<ApiResponse<int>> MarkAllAsReadAsync(Guid tenantId, Guid userId);
    Task<ApiResponse<bool>> ArchiveNotificationAsync(Guid tenantId, Guid notificationId);
    Task<ApiResponse<int>> ArchiveAllReadAsync(Guid tenantId, Guid userId);

    // Preferences
    Task<ApiResponse<UserNotificationSettingsDto>> GetUserSettingsAsync(Guid tenantId, Guid userId);
    Task<ApiResponse<bool>> UpdateGlobalSettingsAsync(Guid tenantId, Guid userId, UpdateGlobalSettingsRequest request);
    Task<ApiResponse<NotificationPreferenceDto>> GetPreferenceAsync(Guid tenantId, Guid userId, string category);
    Task<ApiResponse<NotificationPreferenceDto>> UpdatePreferenceAsync(
        Guid tenantId, Guid userId, string category, UpdatePreferenceRequest request);

    // Templates (admin)
    Task<ApiResponse<List<NotificationTemplateDto>>> GetTemplatesAsync();
    Task<ApiResponse<NotificationTemplateDto>> GetTemplateByCodeAsync(string code);
    Task<ApiResponse<NotificationTemplateDto>> CreateTemplateAsync(CreateTemplateRequest request);
    Task<ApiResponse<NotificationTemplateDto>> UpdateTemplateAsync(Guid templateId, UpdateTemplateRequest request);
    Task<ApiResponse<bool>> DeleteTemplateAsync(Guid templateId);

    // Cleanup
    Task<ApiResponse<int>> PurgeExpiredNotificationsAsync(Guid tenantId);
    Task<ApiResponse<int>> PurgeOldArchivedAsync(Guid tenantId, int daysOld);
}
