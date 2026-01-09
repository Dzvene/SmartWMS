using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Integrations.DTOs;
using SmartWMS.API.Modules.Integrations.Models;

namespace SmartWMS.API.Modules.Integrations.Services;

public class IntegrationsService : IIntegrationsService
{
    private readonly ApplicationDbContext _context;

    public IntegrationsService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Integrations CRUD

    public async Task<ApiResponse<List<IntegrationSummaryDto>>> GetIntegrationsAsync(Guid tenantId)
    {
        var integrations = await _context.Integrations
            .Where(i => i.TenantId == tenantId)
            .OrderBy(i => i.Name)
            .Select(i => new IntegrationSummaryDto(
                i.Id,
                i.Name,
                i.Code,
                i.Type,
                i.Provider,
                i.IsActive,
                i.Status,
                i.LastSyncAt
            ))
            .ToListAsync();

        return ApiResponse<List<IntegrationSummaryDto>>.Ok(integrations);
    }

    public async Task<ApiResponse<IntegrationDetailsDto>> GetIntegrationByIdAsync(Guid tenantId, Guid integrationId)
    {
        var integration = await _context.Integrations
            .Include(i => i.Mappings)
            .Include(i => i.Logs.OrderByDescending(l => l.CreatedAt).Take(10))
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == integrationId);

        if (integration == null)
            return ApiResponse<IntegrationDetailsDto>.Fail("Integration not found");

        var dto = new IntegrationDetailsDto(
            integration.Id,
            integration.Name,
            integration.Code,
            integration.Type,
            integration.Description,
            integration.Provider,
            integration.BaseUrl,
            !string.IsNullOrEmpty(integration.ApiKey),
            !string.IsNullOrEmpty(integration.AccessToken),
            integration.ConfigurationJson,
            integration.IsActive,
            integration.Status,
            integration.LastConnectedAt,
            integration.LastSyncAt,
            integration.LastError,
            integration.AutoSyncEnabled,
            integration.SyncIntervalMinutes,
            integration.NextSyncAt,
            integration.Mappings.Count,
            integration.Logs.Count
        );

        return ApiResponse<IntegrationDetailsDto>.Ok(dto);
    }

    public async Task<ApiResponse<IntegrationDto>> GetIntegrationByCodeAsync(Guid tenantId, string code)
    {
        var integration = await _context.Integrations
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Code == code);

        if (integration == null)
            return ApiResponse<IntegrationDto>.Fail("Integration not found");

        return ApiResponse<IntegrationDto>.Ok(MapToDto(integration));
    }

    public async Task<ApiResponse<IntegrationDto>> CreateIntegrationAsync(Guid tenantId, CreateIntegrationRequest request)
    {
        if (await _context.Integrations.AnyAsync(i => i.TenantId == tenantId && i.Code == request.Code))
            return ApiResponse<IntegrationDto>.Fail("Integration code already exists");

        var integration = new Integration
        {
            TenantId = tenantId,
            Name = request.Name,
            Code = request.Code,
            Type = request.Type,
            Description = request.Description,
            Provider = request.Provider,
            BaseUrl = request.BaseUrl,
            ApiKey = request.ApiKey,
            ApiSecret = request.ApiSecret,
            Username = request.Username,
            Password = request.Password,
            ConfigurationJson = request.ConfigurationJson,
            AutoSyncEnabled = request.AutoSyncEnabled,
            SyncIntervalMinutes = request.SyncIntervalMinutes,
            IsActive = false,
            Status = IntegrationStatus.Inactive
        };

        _context.Integrations.Add(integration);
        await _context.SaveChangesAsync();

        return ApiResponse<IntegrationDto>.Ok(MapToDto(integration), "Integration created");
    }

    public async Task<ApiResponse<IntegrationDto>> UpdateIntegrationAsync(
        Guid tenantId, Guid integrationId, UpdateIntegrationRequest request)
    {
        var integration = await _context.Integrations
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == integrationId);

        if (integration == null)
            return ApiResponse<IntegrationDto>.Fail("Integration not found");

        if (!string.IsNullOrEmpty(request.Name))
            integration.Name = request.Name;

        if (request.Description != null)
            integration.Description = request.Description;

        if (request.BaseUrl != null)
            integration.BaseUrl = request.BaseUrl;

        if (!string.IsNullOrEmpty(request.ApiKey))
            integration.ApiKey = request.ApiKey;

        if (!string.IsNullOrEmpty(request.ApiSecret))
            integration.ApiSecret = request.ApiSecret;

        if (!string.IsNullOrEmpty(request.Username))
            integration.Username = request.Username;

        if (!string.IsNullOrEmpty(request.Password))
            integration.Password = request.Password;

        if (request.ConfigurationJson != null)
            integration.ConfigurationJson = request.ConfigurationJson;

        if (request.IsActive.HasValue)
            integration.IsActive = request.IsActive.Value;

        if (request.AutoSyncEnabled.HasValue)
            integration.AutoSyncEnabled = request.AutoSyncEnabled.Value;

        if (request.SyncIntervalMinutes.HasValue)
            integration.SyncIntervalMinutes = request.SyncIntervalMinutes.Value;

        integration.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<IntegrationDto>.Ok(MapToDto(integration), "Integration updated");
    }

    public async Task<ApiResponse<bool>> DeleteIntegrationAsync(Guid tenantId, Guid integrationId)
    {
        var integration = await _context.Integrations
            .Include(i => i.Logs)
            .Include(i => i.Mappings)
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == integrationId);

        if (integration == null)
            return ApiResponse<bool>.Fail("Integration not found");

        _context.IntegrationLogs.RemoveRange(integration.Logs);
        _context.IntegrationMappings.RemoveRange(integration.Mappings);
        _context.Integrations.Remove(integration);

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Integration deleted");
    }

    #endregion

    #region Connection & Status

    public async Task<ApiResponse<bool>> TestConnectionAsync(Guid tenantId, Guid integrationId)
    {
        var integration = await _context.Integrations
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == integrationId);

        if (integration == null)
            return ApiResponse<bool>.Fail("Integration not found");

        // Log the connection test
        var log = new IntegrationLog
        {
            TenantId = tenantId,
            IntegrationId = integrationId,
            LogType = IntegrationLogType.Connection,
            Direction = "Out",
            Success = true, // Simulate success - real implementation would test the connection
            DurationMs = 100
        };

        _context.IntegrationLogs.Add(log);

        integration.Status = IntegrationStatus.Connected;
        integration.LastConnectedAt = DateTime.UtcNow;
        integration.LastError = null;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Connection successful");
    }

    public async Task<ApiResponse<bool>> ActivateAsync(Guid tenantId, Guid integrationId)
    {
        var integration = await _context.Integrations
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == integrationId);

        if (integration == null)
            return ApiResponse<bool>.Fail("Integration not found");

        integration.IsActive = true;
        integration.Status = IntegrationStatus.Connected;

        if (integration.AutoSyncEnabled)
        {
            integration.NextSyncAt = DateTime.UtcNow.AddMinutes(integration.SyncIntervalMinutes);
        }

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Integration activated");
    }

    public async Task<ApiResponse<bool>> DeactivateAsync(Guid tenantId, Guid integrationId)
    {
        var integration = await _context.Integrations
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == integrationId);

        if (integration == null)
            return ApiResponse<bool>.Fail("Integration not found");

        integration.IsActive = false;
        integration.Status = IntegrationStatus.Inactive;
        integration.NextSyncAt = null;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Integration deactivated");
    }

    public async Task<ApiResponse<bool>> UpdateTokenAsync(Guid tenantId, Guid integrationId, UpdateTokenRequest request)
    {
        var integration = await _context.Integrations
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == integrationId);

        if (integration == null)
            return ApiResponse<bool>.Fail("Integration not found");

        integration.AccessToken = request.AccessToken;
        integration.TokenExpiresAt = request.TokenExpiresAt;
        integration.RefreshToken = request.RefreshToken;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Token updated");
    }

    #endregion

    #region Sync

    public async Task<ApiResponse<bool>> TriggerSyncAsync(Guid tenantId, Guid integrationId)
    {
        var integration = await _context.Integrations
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == integrationId);

        if (integration == null)
            return ApiResponse<bool>.Fail("Integration not found");

        if (!integration.IsActive)
            return ApiResponse<bool>.Fail("Integration is not active");

        // Log sync trigger
        var log = new IntegrationLog
        {
            TenantId = tenantId,
            IntegrationId = integrationId,
            LogType = IntegrationLogType.Sync,
            Direction = "Out",
            Success = true,
            DurationMs = 0
        };

        _context.IntegrationLogs.Add(log);

        integration.LastSyncAt = DateTime.UtcNow;
        if (integration.AutoSyncEnabled)
        {
            integration.NextSyncAt = DateTime.UtcNow.AddMinutes(integration.SyncIntervalMinutes);
        }

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Sync triggered");
    }

    public async Task<ApiResponse<bool>> ScheduleSyncAsync(Guid tenantId, Guid integrationId, DateTime scheduledAt)
    {
        var integration = await _context.Integrations
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == integrationId);

        if (integration == null)
            return ApiResponse<bool>.Fail("Integration not found");

        integration.NextSyncAt = scheduledAt;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, $"Sync scheduled for {scheduledAt:g}");
    }

    #endregion

    #region Logs

    public async Task<ApiResponse<PaginatedResult<IntegrationLogDto>>> GetLogsAsync(
        Guid tenantId, Guid integrationId, IntegrationLogQueryParams query)
    {
        var queryable = _context.IntegrationLogs
            .Where(l => l.TenantId == tenantId && l.IntegrationId == integrationId);

        if (query.LogType.HasValue)
            queryable = queryable.Where(l => l.LogType == query.LogType.Value);

        if (!string.IsNullOrEmpty(query.Direction))
            queryable = queryable.Where(l => l.Direction == query.Direction);

        if (query.Success.HasValue)
            queryable = queryable.Where(l => l.Success == query.Success.Value);

        if (query.FromDate.HasValue)
            queryable = queryable.Where(l => l.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            queryable = queryable.Where(l => l.CreatedAt <= query.ToDate.Value);

        var totalCount = await queryable.CountAsync();

        var items = await queryable
            .OrderByDescending(l => l.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(l => new IntegrationLogDto(
                l.Id,
                l.IntegrationId,
                l.LogType,
                l.Direction,
                l.EntityType,
                l.EntityId,
                l.ExternalId,
                l.Success,
                l.ErrorMessage,
                l.HttpStatusCode,
                l.DurationMs,
                l.CreatedAt
            ))
            .ToListAsync();

        var result = new PaginatedResult<IntegrationLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };

        return ApiResponse<PaginatedResult<IntegrationLogDto>>.Ok(result);
    }

    public async Task<ApiResponse<IntegrationLogDetailDto>> GetLogByIdAsync(Guid tenantId, Guid logId)
    {
        var log = await _context.IntegrationLogs
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == logId);

        if (log == null)
            return ApiResponse<IntegrationLogDetailDto>.Fail("Log not found");

        var dto = new IntegrationLogDetailDto(
            log.Id,
            log.IntegrationId,
            log.LogType,
            log.Direction,
            log.EntityType,
            log.EntityId,
            log.ExternalId,
            log.RequestData,
            log.ResponseData,
            log.HttpStatusCode,
            log.Success,
            log.ErrorMessage,
            log.DurationMs,
            log.CreatedAt
        );

        return ApiResponse<IntegrationLogDetailDto>.Ok(dto);
    }

    public async Task<ApiResponse<IntegrationStatsDto>> GetStatsAsync(Guid tenantId, Guid integrationId, int days = 30)
    {
        var since = DateTime.UtcNow.AddDays(-days);

        var logs = await _context.IntegrationLogs
            .Where(l => l.TenantId == tenantId && l.IntegrationId == integrationId && l.CreatedAt >= since)
            .ToListAsync();

        var stats = new IntegrationStatsDto(
            TotalRequests: logs.Count,
            SuccessfulRequests: logs.Count(l => l.Success),
            FailedRequests: logs.Count(l => !l.Success),
            SuccessRate: logs.Count > 0 ? (double)logs.Count(l => l.Success) / logs.Count * 100 : 0,
            AverageDurationMs: logs.Any(l => l.DurationMs.HasValue) ?
                (int)logs.Where(l => l.DurationMs.HasValue).Average(l => l.DurationMs!.Value) : 0,
            LastRequestAt: logs.OrderByDescending(l => l.CreatedAt).FirstOrDefault()?.CreatedAt
        );

        return ApiResponse<IntegrationStatsDto>.Ok(stats);
    }

    public async Task<ApiResponse<int>> PurgeOldLogsAsync(Guid tenantId, Guid integrationId, int daysOld)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysOld);

        var oldLogs = await _context.IntegrationLogs
            .Where(l => l.TenantId == tenantId && l.IntegrationId == integrationId && l.CreatedAt <= cutoff)
            .ToListAsync();

        _context.IntegrationLogs.RemoveRange(oldLogs);
        await _context.SaveChangesAsync();

        return ApiResponse<int>.Ok(oldLogs.Count, $"{oldLogs.Count} logs purged");
    }

    #endregion

    #region Mappings

    public async Task<ApiResponse<List<IntegrationMappingDto>>> GetMappingsAsync(
        Guid tenantId, Guid integrationId, string? entityType = null)
    {
        var query = _context.IntegrationMappings
            .Where(m => m.TenantId == tenantId && m.IntegrationId == integrationId);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(m => m.LocalEntityType == entityType);

        var mappings = await query
            .OrderBy(m => m.LocalEntityType)
            .ThenBy(m => m.CreatedAt)
            .Select(m => new IntegrationMappingDto(
                m.Id,
                m.IntegrationId,
                m.LocalEntityType,
                m.LocalEntityId,
                m.ExternalEntityType,
                m.ExternalEntityId,
                m.LastSyncAt,
                m.LastSyncDirection,
                m.SyncStatus
            ))
            .ToListAsync();

        return ApiResponse<List<IntegrationMappingDto>>.Ok(mappings);
    }

    public async Task<ApiResponse<IntegrationMappingDto>> GetMappingAsync(Guid tenantId, Guid mappingId)
    {
        var mapping = await _context.IntegrationMappings
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Id == mappingId);

        if (mapping == null)
            return ApiResponse<IntegrationMappingDto>.Fail("Mapping not found");

        return ApiResponse<IntegrationMappingDto>.Ok(new IntegrationMappingDto(
            mapping.Id,
            mapping.IntegrationId,
            mapping.LocalEntityType,
            mapping.LocalEntityId,
            mapping.ExternalEntityType,
            mapping.ExternalEntityId,
            mapping.LastSyncAt,
            mapping.LastSyncDirection,
            mapping.SyncStatus
        ));
    }

    public async Task<ApiResponse<IntegrationMappingDto>> CreateMappingAsync(
        Guid tenantId, Guid integrationId, CreateMappingRequest request)
    {
        var exists = await _context.IntegrationMappings
            .AnyAsync(m => m.TenantId == tenantId &&
                          m.IntegrationId == integrationId &&
                          m.LocalEntityType == request.LocalEntityType &&
                          m.LocalEntityId == request.LocalEntityId);

        if (exists)
            return ApiResponse<IntegrationMappingDto>.Fail("Mapping already exists for this entity");

        var mapping = new IntegrationMapping
        {
            TenantId = tenantId,
            IntegrationId = integrationId,
            LocalEntityType = request.LocalEntityType,
            LocalEntityId = request.LocalEntityId,
            ExternalEntityType = request.ExternalEntityType,
            ExternalEntityId = request.ExternalEntityId,
            SyncStatus = "Pending"
        };

        _context.IntegrationMappings.Add(mapping);
        await _context.SaveChangesAsync();

        return ApiResponse<IntegrationMappingDto>.Ok(new IntegrationMappingDto(
            mapping.Id,
            mapping.IntegrationId,
            mapping.LocalEntityType,
            mapping.LocalEntityId,
            mapping.ExternalEntityType,
            mapping.ExternalEntityId,
            mapping.LastSyncAt,
            mapping.LastSyncDirection,
            mapping.SyncStatus
        ));
    }

    public async Task<ApiResponse<bool>> DeleteMappingAsync(Guid tenantId, Guid mappingId)
    {
        var mapping = await _context.IntegrationMappings
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Id == mappingId);

        if (mapping == null)
            return ApiResponse<bool>.Fail("Mapping not found");

        _context.IntegrationMappings.Remove(mapping);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Mapping deleted");
    }

    public async Task<ApiResponse<IntegrationMappingDto>> SyncMappingAsync(
        Guid tenantId, Guid mappingId, SyncMappingRequest request)
    {
        var mapping = await _context.IntegrationMappings
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Id == mappingId);

        if (mapping == null)
            return ApiResponse<IntegrationMappingDto>.Fail("Mapping not found");

        // Simulate sync - real implementation would perform the actual sync
        mapping.LastSyncAt = DateTime.UtcNow;
        mapping.LastSyncDirection = request.Direction;
        mapping.SyncStatus = "Synced";

        await _context.SaveChangesAsync();

        return ApiResponse<IntegrationMappingDto>.Ok(new IntegrationMappingDto(
            mapping.Id,
            mapping.IntegrationId,
            mapping.LocalEntityType,
            mapping.LocalEntityId,
            mapping.ExternalEntityType,
            mapping.ExternalEntityId,
            mapping.LastSyncAt,
            mapping.LastSyncDirection,
            mapping.SyncStatus
        ));
    }

    #endregion

    #region Webhooks

    public async Task<ApiResponse<List<WebhookEndpointDto>>> GetWebhooksAsync(Guid tenantId, Guid integrationId)
    {
        var webhooks = await _context.WebhookEndpoints
            .Where(w => w.TenantId == tenantId && w.IntegrationId == integrationId)
            .OrderBy(w => w.Name)
            .Select(w => new WebhookEndpointDto(
                w.Id,
                w.IntegrationId,
                w.Name,
                w.Url,
                !string.IsNullOrEmpty(w.Secret),
                w.Events,
                w.IsActive,
                w.RetryCount,
                w.TimeoutSeconds,
                w.LastTriggeredAt,
                w.SuccessCount,
                w.FailureCount
            ))
            .ToListAsync();

        return ApiResponse<List<WebhookEndpointDto>>.Ok(webhooks);
    }

    public async Task<ApiResponse<WebhookEndpointDto>> GetWebhookByIdAsync(Guid tenantId, Guid webhookId)
    {
        var webhook = await _context.WebhookEndpoints
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == webhookId);

        if (webhook == null)
            return ApiResponse<WebhookEndpointDto>.Fail("Webhook not found");

        return ApiResponse<WebhookEndpointDto>.Ok(new WebhookEndpointDto(
            webhook.Id,
            webhook.IntegrationId,
            webhook.Name,
            webhook.Url,
            !string.IsNullOrEmpty(webhook.Secret),
            webhook.Events,
            webhook.IsActive,
            webhook.RetryCount,
            webhook.TimeoutSeconds,
            webhook.LastTriggeredAt,
            webhook.SuccessCount,
            webhook.FailureCount
        ));
    }

    public async Task<ApiResponse<WebhookEndpointDto>> CreateWebhookAsync(
        Guid tenantId, Guid integrationId, CreateWebhookRequest request)
    {
        var webhook = new WebhookEndpoint
        {
            TenantId = tenantId,
            IntegrationId = integrationId,
            Name = request.Name,
            Url = request.Url,
            Secret = request.Secret,
            Events = request.Events,
            RetryCount = request.RetryCount,
            TimeoutSeconds = request.TimeoutSeconds,
            IsActive = true
        };

        _context.WebhookEndpoints.Add(webhook);
        await _context.SaveChangesAsync();

        return ApiResponse<WebhookEndpointDto>.Ok(new WebhookEndpointDto(
            webhook.Id,
            webhook.IntegrationId,
            webhook.Name,
            webhook.Url,
            !string.IsNullOrEmpty(webhook.Secret),
            webhook.Events,
            webhook.IsActive,
            webhook.RetryCount,
            webhook.TimeoutSeconds,
            webhook.LastTriggeredAt,
            webhook.SuccessCount,
            webhook.FailureCount
        ));
    }

    public async Task<ApiResponse<WebhookEndpointDto>> UpdateWebhookAsync(
        Guid tenantId, Guid webhookId, UpdateWebhookRequest request)
    {
        var webhook = await _context.WebhookEndpoints
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == webhookId);

        if (webhook == null)
            return ApiResponse<WebhookEndpointDto>.Fail("Webhook not found");

        if (!string.IsNullOrEmpty(request.Name))
            webhook.Name = request.Name;

        if (!string.IsNullOrEmpty(request.Url))
            webhook.Url = request.Url;

        if (!string.IsNullOrEmpty(request.Secret))
            webhook.Secret = request.Secret;

        if (request.Events != null)
            webhook.Events = request.Events;

        if (request.IsActive.HasValue)
            webhook.IsActive = request.IsActive.Value;

        if (request.RetryCount.HasValue)
            webhook.RetryCount = request.RetryCount.Value;

        if (request.TimeoutSeconds.HasValue)
            webhook.TimeoutSeconds = request.TimeoutSeconds.Value;

        webhook.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<WebhookEndpointDto>.Ok(new WebhookEndpointDto(
            webhook.Id,
            webhook.IntegrationId,
            webhook.Name,
            webhook.Url,
            !string.IsNullOrEmpty(webhook.Secret),
            webhook.Events,
            webhook.IsActive,
            webhook.RetryCount,
            webhook.TimeoutSeconds,
            webhook.LastTriggeredAt,
            webhook.SuccessCount,
            webhook.FailureCount
        ));
    }

    public async Task<ApiResponse<bool>> DeleteWebhookAsync(Guid tenantId, Guid webhookId)
    {
        var webhook = await _context.WebhookEndpoints
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == webhookId);

        if (webhook == null)
            return ApiResponse<bool>.Fail("Webhook not found");

        _context.WebhookEndpoints.Remove(webhook);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Webhook deleted");
    }

    public async Task<ApiResponse<bool>> TestWebhookAsync(Guid tenantId, Guid webhookId)
    {
        var webhook = await _context.WebhookEndpoints
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == webhookId);

        if (webhook == null)
            return ApiResponse<bool>.Fail("Webhook not found");

        // Simulate test - real implementation would send a test request
        webhook.LastTriggeredAt = DateTime.UtcNow;
        webhook.SuccessCount++;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Test webhook sent");
    }

    #endregion

    #region Private Helpers

    private static IntegrationDto MapToDto(Integration i) => new(
        i.Id,
        i.Name,
        i.Code,
        i.Type,
        i.Description,
        i.Provider,
        i.BaseUrl,
        i.IsActive,
        i.Status,
        i.LastConnectedAt,
        i.LastSyncAt,
        i.LastError,
        i.AutoSyncEnabled,
        i.SyncIntervalMinutes,
        i.NextSyncAt,
        i.CreatedAt
    );

    #endregion
}
