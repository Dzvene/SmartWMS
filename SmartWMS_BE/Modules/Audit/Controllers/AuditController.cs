using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Audit.DTOs;
using SmartWMS.API.Modules.Audit.Models;
using SmartWMS.API.Modules.Audit.Services;

namespace SmartWMS.API.Modules.Audit.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/audit")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    #region Audit Logs

    [HttpGet("logs")]
    public async Task<IActionResult> GetAuditLogs(
        Guid tenantId,
        [FromQuery] string? eventType = null,
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] AuditSeverity? severity = null,
        [FromQuery] string? module = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] bool? isSuccess = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var filter = new AuditLogFilterRequest(
            eventType, entityType, entityId, userId, severity, module,
            dateFrom, dateTo, isSuccess, searchTerm, page, pageSize);

        var result = await _auditService.GetAuditLogsAsync(tenantId, filter);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("logs/{id}")]
    public async Task<IActionResult> GetAuditLogById(Guid tenantId, Guid id)
    {
        var result = await _auditService.GetAuditLogByIdAsync(tenantId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("logs/entity/{entityType}/{entityId}")]
    public async Task<IActionResult> GetEntityAuditHistory(Guid tenantId, string entityType, Guid entityId)
    {
        var result = await _auditService.GetEntityAuditHistoryAsync(tenantId, entityType, entityId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Activity Logs

    [HttpGet("activities")]
    public async Task<IActionResult> GetActivityLogs(
        Guid tenantId,
        [FromQuery] string? activityType = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? module = null,
        [FromQuery] Guid? relatedEntityId = null,
        [FromQuery] string? relatedEntityType = null,
        [FromQuery] string? deviceType = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var filter = new ActivityLogFilterRequest(
            activityType, userId, module, relatedEntityId, relatedEntityType,
            deviceType, dateFrom, dateTo, searchTerm, page, pageSize);

        var result = await _auditService.GetActivityLogsAsync(tenantId, filter);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("activities/user/{userId}")]
    public async Task<IActionResult> GetUserActivity(
        Guid tenantId, Guid userId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _auditService.GetUserActivityAsync(tenantId, userId, from, to);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region System Events

    [HttpGet("system-events")]
    public async Task<IActionResult> GetSystemEvents(
        Guid tenantId,
        [FromQuery] string? eventType = null,
        [FromQuery] SystemEventCategory? category = null,
        [FromQuery] SystemEventSeverity? minSeverity = null,
        [FromQuery] string? source = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] bool? hasException = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var filter = new SystemEventFilterRequest(
            eventType, category, minSeverity, source,
            dateFrom, dateTo, hasException, searchTerm, page, pageSize);

        var result = await _auditService.GetSystemEventsAsync(tenantId, filter);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("system-events/{id}")]
    public async Task<IActionResult> GetSystemEventById(Guid tenantId, Guid id)
    {
        var result = await _auditService.GetSystemEventByIdAsync(tenantId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    #endregion

    #region Statistics

    [HttpGet("statistics")]
    public async Task<IActionResult> GetAuditStatistics(
        Guid tenantId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _auditService.GetAuditStatisticsAsync(tenantId, from, to);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("statistics/user/{userId}")]
    public async Task<IActionResult> GetUserActivitySummary(
        Guid tenantId, Guid userId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _auditService.GetUserActivitySummaryAsync(tenantId, userId, from, to);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Cleanup

    [HttpDelete("purge")]
    public async Task<IActionResult> PurgeOldLogs(
        Guid tenantId, [FromQuery] int daysToKeep = 90)
    {
        if (daysToKeep < 7)
            return BadRequest("Cannot purge logs newer than 7 days");

        var result = await _auditService.PurgeOldLogsAsync(tenantId, daysToKeep);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
