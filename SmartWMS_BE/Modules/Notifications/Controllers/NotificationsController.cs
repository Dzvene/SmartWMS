using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Notifications.DTOs;
using SmartWMS.API.Modules.Notifications.Services;

namespace SmartWMS.API.Modules.Notifications.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationsService _notificationsService;

    public NotificationsController(INotificationsService notificationsService)
    {
        _notificationsService = notificationsService;
    }

    #region Notifications

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetNotifications(
        Guid tenantId, Guid userId, [FromQuery] NotificationQueryParams query)
    {
        var result = await _notificationsService.GetNotificationsAsync(tenantId, userId, query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{notificationId}")]
    public async Task<IActionResult> GetNotificationById(Guid tenantId, Guid notificationId)
    {
        var result = await _notificationsService.GetNotificationByIdAsync(tenantId, notificationId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("user/{userId}/count")]
    public async Task<IActionResult> GetNotificationCount(Guid tenantId, Guid userId)
    {
        var result = await _notificationsService.GetNotificationCountAsync(tenantId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification(
        Guid tenantId, [FromBody] CreateNotificationRequest request)
    {
        var result = await _notificationsService.CreateNotificationAsync(tenantId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetNotificationById), new { tenantId, notificationId = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulkNotifications(
        Guid tenantId, [FromBody] CreateBulkNotificationRequest request)
    {
        var result = await _notificationsService.CreateBulkNotificationsAsync(tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("from-template")]
    public async Task<IActionResult> CreateFromTemplate(
        Guid tenantId, [FromBody] CreateNotificationFromTemplateRequest request)
    {
        var result = await _notificationsService.CreateFromTemplateAsync(tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotification(Guid tenantId, Guid notificationId)
    {
        var result = await _notificationsService.DeleteNotificationAsync(tenantId, notificationId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Notification Actions

    [HttpPost("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid tenantId, Guid notificationId)
    {
        var result = await _notificationsService.MarkAsReadAsync(tenantId, notificationId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{notificationId}/unread")]
    public async Task<IActionResult> MarkAsUnread(Guid tenantId, Guid notificationId)
    {
        var result = await _notificationsService.MarkAsUnreadAsync(tenantId, notificationId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("user/{userId}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(Guid tenantId, Guid userId)
    {
        var result = await _notificationsService.MarkAllAsReadAsync(tenantId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{notificationId}/archive")]
    public async Task<IActionResult> ArchiveNotification(Guid tenantId, Guid notificationId)
    {
        var result = await _notificationsService.ArchiveNotificationAsync(tenantId, notificationId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("user/{userId}/archive-read")]
    public async Task<IActionResult> ArchiveAllRead(Guid tenantId, Guid userId)
    {
        var result = await _notificationsService.ArchiveAllReadAsync(tenantId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Preferences

    [HttpGet("user/{userId}/settings")]
    public async Task<IActionResult> GetUserSettings(Guid tenantId, Guid userId)
    {
        var result = await _notificationsService.GetUserSettingsAsync(tenantId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("user/{userId}/settings")]
    public async Task<IActionResult> UpdateGlobalSettings(
        Guid tenantId, Guid userId, [FromBody] UpdateGlobalSettingsRequest request)
    {
        var result = await _notificationsService.UpdateGlobalSettingsAsync(tenantId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("user/{userId}/preferences/{category}")]
    public async Task<IActionResult> GetPreference(Guid tenantId, Guid userId, string category)
    {
        var result = await _notificationsService.GetPreferenceAsync(tenantId, userId, category);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("user/{userId}/preferences/{category}")]
    public async Task<IActionResult> UpdatePreference(
        Guid tenantId, Guid userId, string category, [FromBody] UpdatePreferenceRequest request)
    {
        var result = await _notificationsService.UpdatePreferenceAsync(tenantId, userId, category, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Templates (Admin)

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
    {
        var result = await _notificationsService.GetTemplatesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("templates/{code}")]
    public async Task<IActionResult> GetTemplateByCode(string code)
    {
        var result = await _notificationsService.GetTemplateByCodeAsync(code);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("templates")]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateRequest request)
    {
        var result = await _notificationsService.CreateTemplateAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("templates/{templateId}")]
    public async Task<IActionResult> UpdateTemplate(Guid templateId, [FromBody] UpdateTemplateRequest request)
    {
        var result = await _notificationsService.UpdateTemplateAsync(templateId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("templates/{templateId}")]
    public async Task<IActionResult> DeleteTemplate(Guid templateId)
    {
        var result = await _notificationsService.DeleteTemplateAsync(templateId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Cleanup

    [HttpPost("purge-expired")]
    public async Task<IActionResult> PurgeExpiredNotifications(Guid tenantId)
    {
        var result = await _notificationsService.PurgeExpiredNotificationsAsync(tenantId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("purge-archived")]
    public async Task<IActionResult> PurgeOldArchived(Guid tenantId, [FromQuery] int daysOld = 30)
    {
        var result = await _notificationsService.PurgeOldArchivedAsync(tenantId, daysOld);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
