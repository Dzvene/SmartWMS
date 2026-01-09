using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Integrations.DTOs;
using SmartWMS.API.Modules.Integrations.Models;

namespace SmartWMS.API.Modules.Integrations.Services;

public interface IIntegrationsService
{
    // Integrations CRUD
    Task<ApiResponse<List<IntegrationSummaryDto>>> GetIntegrationsAsync(Guid tenantId);
    Task<ApiResponse<IntegrationDetailsDto>> GetIntegrationByIdAsync(Guid tenantId, Guid integrationId);
    Task<ApiResponse<IntegrationDto>> GetIntegrationByCodeAsync(Guid tenantId, string code);
    Task<ApiResponse<IntegrationDto>> CreateIntegrationAsync(Guid tenantId, CreateIntegrationRequest request);
    Task<ApiResponse<IntegrationDto>> UpdateIntegrationAsync(Guid tenantId, Guid integrationId, UpdateIntegrationRequest request);
    Task<ApiResponse<bool>> DeleteIntegrationAsync(Guid tenantId, Guid integrationId);

    // Connection & Status
    Task<ApiResponse<bool>> TestConnectionAsync(Guid tenantId, Guid integrationId);
    Task<ApiResponse<bool>> ActivateAsync(Guid tenantId, Guid integrationId);
    Task<ApiResponse<bool>> DeactivateAsync(Guid tenantId, Guid integrationId);
    Task<ApiResponse<bool>> UpdateTokenAsync(Guid tenantId, Guid integrationId, UpdateTokenRequest request);

    // Sync
    Task<ApiResponse<bool>> TriggerSyncAsync(Guid tenantId, Guid integrationId);
    Task<ApiResponse<bool>> ScheduleSyncAsync(Guid tenantId, Guid integrationId, DateTime scheduledAt);

    // Logs
    Task<ApiResponse<PaginatedResult<IntegrationLogDto>>> GetLogsAsync(
        Guid tenantId, Guid integrationId, IntegrationLogQueryParams query);
    Task<ApiResponse<IntegrationLogDetailDto>> GetLogByIdAsync(Guid tenantId, Guid logId);
    Task<ApiResponse<IntegrationStatsDto>> GetStatsAsync(Guid tenantId, Guid integrationId, int days = 30);
    Task<ApiResponse<int>> PurgeOldLogsAsync(Guid tenantId, Guid integrationId, int daysOld);

    // Mappings
    Task<ApiResponse<List<IntegrationMappingDto>>> GetMappingsAsync(Guid tenantId, Guid integrationId, string? entityType = null);
    Task<ApiResponse<IntegrationMappingDto>> GetMappingAsync(Guid tenantId, Guid mappingId);
    Task<ApiResponse<IntegrationMappingDto>> CreateMappingAsync(Guid tenantId, Guid integrationId, CreateMappingRequest request);
    Task<ApiResponse<bool>> DeleteMappingAsync(Guid tenantId, Guid mappingId);
    Task<ApiResponse<IntegrationMappingDto>> SyncMappingAsync(Guid tenantId, Guid mappingId, SyncMappingRequest request);

    // Webhooks
    Task<ApiResponse<List<WebhookEndpointDto>>> GetWebhooksAsync(Guid tenantId, Guid integrationId);
    Task<ApiResponse<WebhookEndpointDto>> GetWebhookByIdAsync(Guid tenantId, Guid webhookId);
    Task<ApiResponse<WebhookEndpointDto>> CreateWebhookAsync(Guid tenantId, Guid integrationId, CreateWebhookRequest request);
    Task<ApiResponse<WebhookEndpointDto>> UpdateWebhookAsync(Guid tenantId, Guid webhookId, UpdateWebhookRequest request);
    Task<ApiResponse<bool>> DeleteWebhookAsync(Guid tenantId, Guid webhookId);
    Task<ApiResponse<bool>> TestWebhookAsync(Guid tenantId, Guid webhookId);
}
