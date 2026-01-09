using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Integrations.DTOs;
using SmartWMS.API.Modules.Integrations.Services;

namespace SmartWMS.API.Modules.Integrations.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/integrations")]
[Authorize]
public class IntegrationsController : ControllerBase
{
    private readonly IIntegrationsService _integrationsService;

    public IntegrationsController(IIntegrationsService integrationsService)
    {
        _integrationsService = integrationsService;
    }

    #region Integrations CRUD

    [HttpGet]
    public async Task<IActionResult> GetIntegrations(Guid tenantId)
    {
        var result = await _integrationsService.GetIntegrationsAsync(tenantId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{integrationId}")]
    public async Task<IActionResult> GetIntegrationById(Guid tenantId, Guid integrationId)
    {
        var result = await _integrationsService.GetIntegrationByIdAsync(tenantId, integrationId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetIntegrationByCode(Guid tenantId, string code)
    {
        var result = await _integrationsService.GetIntegrationByCodeAsync(tenantId, code);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateIntegration(
        Guid tenantId, [FromBody] CreateIntegrationRequest request)
    {
        var result = await _integrationsService.CreateIntegrationAsync(tenantId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetIntegrationById), new { tenantId, integrationId = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("{integrationId}")]
    public async Task<IActionResult> UpdateIntegration(
        Guid tenantId, Guid integrationId, [FromBody] UpdateIntegrationRequest request)
    {
        var result = await _integrationsService.UpdateIntegrationAsync(tenantId, integrationId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{integrationId}")]
    public async Task<IActionResult> DeleteIntegration(Guid tenantId, Guid integrationId)
    {
        var result = await _integrationsService.DeleteIntegrationAsync(tenantId, integrationId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Connection & Status

    [HttpPost("{integrationId}/test")]
    public async Task<IActionResult> TestConnection(Guid tenantId, Guid integrationId)
    {
        var result = await _integrationsService.TestConnectionAsync(tenantId, integrationId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{integrationId}/activate")]
    public async Task<IActionResult> Activate(Guid tenantId, Guid integrationId)
    {
        var result = await _integrationsService.ActivateAsync(tenantId, integrationId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{integrationId}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid tenantId, Guid integrationId)
    {
        var result = await _integrationsService.DeactivateAsync(tenantId, integrationId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{integrationId}/token")]
    public async Task<IActionResult> UpdateToken(
        Guid tenantId, Guid integrationId, [FromBody] UpdateTokenRequest request)
    {
        var result = await _integrationsService.UpdateTokenAsync(tenantId, integrationId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Sync

    [HttpPost("{integrationId}/sync")]
    public async Task<IActionResult> TriggerSync(Guid tenantId, Guid integrationId)
    {
        var result = await _integrationsService.TriggerSyncAsync(tenantId, integrationId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{integrationId}/schedule-sync")]
    public async Task<IActionResult> ScheduleSync(
        Guid tenantId, Guid integrationId, [FromQuery] DateTime scheduledAt)
    {
        var result = await _integrationsService.ScheduleSyncAsync(tenantId, integrationId, scheduledAt);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Logs

    [HttpGet("{integrationId}/logs")]
    public async Task<IActionResult> GetLogs(
        Guid tenantId, Guid integrationId, [FromQuery] IntegrationLogQueryParams query)
    {
        var result = await _integrationsService.GetLogsAsync(tenantId, integrationId, query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("logs/{logId}")]
    public async Task<IActionResult> GetLogById(Guid tenantId, Guid logId)
    {
        var result = await _integrationsService.GetLogByIdAsync(tenantId, logId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("{integrationId}/stats")]
    public async Task<IActionResult> GetStats(Guid tenantId, Guid integrationId, [FromQuery] int days = 30)
    {
        var result = await _integrationsService.GetStatsAsync(tenantId, integrationId, days);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{integrationId}/logs")]
    public async Task<IActionResult> PurgeOldLogs(
        Guid tenantId, Guid integrationId, [FromQuery] int daysOld = 30)
    {
        var result = await _integrationsService.PurgeOldLogsAsync(tenantId, integrationId, daysOld);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Mappings

    [HttpGet("{integrationId}/mappings")]
    public async Task<IActionResult> GetMappings(
        Guid tenantId, Guid integrationId, [FromQuery] string? entityType = null)
    {
        var result = await _integrationsService.GetMappingsAsync(tenantId, integrationId, entityType);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("mappings/{mappingId}")]
    public async Task<IActionResult> GetMapping(Guid tenantId, Guid mappingId)
    {
        var result = await _integrationsService.GetMappingAsync(tenantId, mappingId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{integrationId}/mappings")]
    public async Task<IActionResult> CreateMapping(
        Guid tenantId, Guid integrationId, [FromBody] CreateMappingRequest request)
    {
        var result = await _integrationsService.CreateMappingAsync(tenantId, integrationId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("mappings/{mappingId}")]
    public async Task<IActionResult> DeleteMapping(Guid tenantId, Guid mappingId)
    {
        var result = await _integrationsService.DeleteMappingAsync(tenantId, mappingId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("mappings/{mappingId}/sync")]
    public async Task<IActionResult> SyncMapping(
        Guid tenantId, Guid mappingId, [FromBody] SyncMappingRequest request)
    {
        var result = await _integrationsService.SyncMappingAsync(tenantId, mappingId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Webhooks

    [HttpGet("{integrationId}/webhooks")]
    public async Task<IActionResult> GetWebhooks(Guid tenantId, Guid integrationId)
    {
        var result = await _integrationsService.GetWebhooksAsync(tenantId, integrationId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("webhooks/{webhookId}")]
    public async Task<IActionResult> GetWebhookById(Guid tenantId, Guid webhookId)
    {
        var result = await _integrationsService.GetWebhookByIdAsync(tenantId, webhookId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{integrationId}/webhooks")]
    public async Task<IActionResult> CreateWebhook(
        Guid tenantId, Guid integrationId, [FromBody] CreateWebhookRequest request)
    {
        var result = await _integrationsService.CreateWebhookAsync(tenantId, integrationId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("webhooks/{webhookId}")]
    public async Task<IActionResult> UpdateWebhook(
        Guid tenantId, Guid webhookId, [FromBody] UpdateWebhookRequest request)
    {
        var result = await _integrationsService.UpdateWebhookAsync(tenantId, webhookId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("webhooks/{webhookId}")]
    public async Task<IActionResult> DeleteWebhook(Guid tenantId, Guid webhookId)
    {
        var result = await _integrationsService.DeleteWebhookAsync(tenantId, webhookId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("webhooks/{webhookId}/test")]
    public async Task<IActionResult> TestWebhook(Guid tenantId, Guid webhookId)
    {
        var result = await _integrationsService.TestWebhookAsync(tenantId, webhookId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
