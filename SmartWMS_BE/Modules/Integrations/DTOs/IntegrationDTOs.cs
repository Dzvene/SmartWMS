using SmartWMS.API.Modules.Integrations.Models;

namespace SmartWMS.API.Modules.Integrations.DTOs;

#region Integration DTOs

public record IntegrationDto(
    Guid Id,
    string Name,
    string Code,
    IntegrationType Type,
    string? Description,
    string? Provider,
    string? BaseUrl,
    bool IsActive,
    IntegrationStatus Status,
    DateTime? LastConnectedAt,
    DateTime? LastSyncAt,
    string? LastError,
    bool AutoSyncEnabled,
    int SyncIntervalMinutes,
    DateTime? NextSyncAt,
    DateTime CreatedAt
);

public record IntegrationSummaryDto(
    Guid Id,
    string Name,
    string Code,
    IntegrationType Type,
    string? Provider,
    bool IsActive,
    IntegrationStatus Status,
    DateTime? LastSyncAt
);

public record IntegrationDetailsDto(
    Guid Id,
    string Name,
    string Code,
    IntegrationType Type,
    string? Description,
    string? Provider,
    string? BaseUrl,
    bool HasApiKey,
    bool HasAccessToken,
    string? ConfigurationJson,
    bool IsActive,
    IntegrationStatus Status,
    DateTime? LastConnectedAt,
    DateTime? LastSyncAt,
    string? LastError,
    bool AutoSyncEnabled,
    int SyncIntervalMinutes,
    DateTime? NextSyncAt,
    int MappingsCount,
    int RecentLogsCount
);

#endregion

#region Create/Update Requests

public record CreateIntegrationRequest(
    string Name,
    string Code,
    IntegrationType Type,
    string? Description,
    string? Provider,
    string? BaseUrl,
    string? ApiKey,
    string? ApiSecret,
    string? Username,
    string? Password,
    string? ConfigurationJson,
    bool AutoSyncEnabled,
    int SyncIntervalMinutes
);

public record UpdateIntegrationRequest(
    string? Name,
    string? Description,
    string? BaseUrl,
    string? ApiKey,
    string? ApiSecret,
    string? Username,
    string? Password,
    string? ConfigurationJson,
    bool? IsActive,
    bool? AutoSyncEnabled,
    int? SyncIntervalMinutes
);

public record UpdateTokenRequest(
    string AccessToken,
    DateTime? TokenExpiresAt,
    string? RefreshToken
);

#endregion

#region Log DTOs

public record IntegrationLogDto(
    Guid Id,
    Guid IntegrationId,
    IntegrationLogType LogType,
    string? Direction,
    string? EntityType,
    Guid? EntityId,
    string? ExternalId,
    bool Success,
    string? ErrorMessage,
    int? HttpStatusCode,
    int? DurationMs,
    DateTime CreatedAt
);

public record IntegrationLogDetailDto(
    Guid Id,
    Guid IntegrationId,
    IntegrationLogType LogType,
    string? Direction,
    string? EntityType,
    Guid? EntityId,
    string? ExternalId,
    string? RequestData,
    string? ResponseData,
    int? HttpStatusCode,
    bool Success,
    string? ErrorMessage,
    int? DurationMs,
    DateTime CreatedAt
);

public record IntegrationStatsDto(
    int TotalRequests,
    int SuccessfulRequests,
    int FailedRequests,
    double SuccessRate,
    int AverageDurationMs,
    DateTime? LastRequestAt
);

#endregion

#region Mapping DTOs

public record IntegrationMappingDto(
    Guid Id,
    Guid IntegrationId,
    string LocalEntityType,
    Guid LocalEntityId,
    string ExternalEntityType,
    string ExternalEntityId,
    DateTime? LastSyncAt,
    string? LastSyncDirection,
    string? SyncStatus
);

public record CreateMappingRequest(
    string LocalEntityType,
    Guid LocalEntityId,
    string ExternalEntityType,
    string ExternalEntityId
);

public record SyncMappingRequest(
    string Direction,
    bool Force
);

#endregion

#region Webhook DTOs

public record WebhookEndpointDto(
    Guid Id,
    Guid IntegrationId,
    string Name,
    string Url,
    bool HasSecret,
    List<string> Events,
    bool IsActive,
    int RetryCount,
    int TimeoutSeconds,
    DateTime? LastTriggeredAt,
    int SuccessCount,
    int FailureCount
);

public record CreateWebhookRequest(
    string Name,
    string Url,
    string? Secret,
    List<string> Events,
    int RetryCount,
    int TimeoutSeconds
);

public record UpdateWebhookRequest(
    string? Name,
    string? Url,
    string? Secret,
    List<string>? Events,
    bool? IsActive,
    int? RetryCount,
    int? TimeoutSeconds
);

#endregion

#region Query Parameters

public record IntegrationLogQueryParams(
    int Page = 1,
    int PageSize = 50,
    IntegrationLogType? LogType = null,
    string? Direction = null,
    bool? Success = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
);

public record SyncJobQueryParams(
    int Page = 1,
    int PageSize = 25,
    Guid? IntegrationId = null,
    string? EntityType = null,
    SyncDirection? Direction = null,
    SyncJobStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
);

#endregion

#region Sync Job DTOs

public record SyncJobDto(
    Guid Id,
    Guid IntegrationId,
    string? IntegrationName,
    string EntityType,
    SyncDirection Direction,
    SyncJobStatus Status,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    int TotalRecords,
    int ProcessedRecords,
    int FailedRecords,
    string? ErrorMessage,
    DateTime CreatedAt
);

#endregion
