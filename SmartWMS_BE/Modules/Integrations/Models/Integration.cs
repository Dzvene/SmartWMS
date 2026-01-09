using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Integrations.Models;

public class Integration : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public IntegrationType Type { get; set; }
    public string? Description { get; set; }
    public string? Provider { get; set; }

    // Connection settings
    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? AccessToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public string? RefreshToken { get; set; }

    // Additional configuration (JSON)
    public string? ConfigurationJson { get; set; }

    // Status
    public bool IsActive { get; set; }
    public IntegrationStatus Status { get; set; } = IntegrationStatus.Inactive;
    public DateTime? LastConnectedAt { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public string? LastError { get; set; }

    // Sync settings
    public bool AutoSyncEnabled { get; set; }
    public int SyncIntervalMinutes { get; set; } = 60;
    public DateTime? NextSyncAt { get; set; }

    // Navigation
    public ICollection<IntegrationLog> Logs { get; set; } = new List<IntegrationLog>();
    public ICollection<IntegrationMapping> Mappings { get; set; } = new List<IntegrationMapping>();
}

public class IntegrationLog : TenantEntity
{
    public Guid IntegrationId { get; set; }
    public Integration Integration { get; set; } = null!;

    public IntegrationLogType LogType { get; set; }
    public string? Direction { get; set; } // "In" or "Out"
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? ExternalId { get; set; }

    public string? RequestData { get; set; }
    public string? ResponseData { get; set; }
    public int? HttpStatusCode { get; set; }

    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int? DurationMs { get; set; }
}

public class IntegrationMapping : TenantEntity
{
    public Guid IntegrationId { get; set; }
    public Integration Integration { get; set; } = null!;

    public string LocalEntityType { get; set; } = string.Empty;
    public Guid LocalEntityId { get; set; }
    public string ExternalEntityType { get; set; } = string.Empty;
    public string ExternalEntityId { get; set; } = string.Empty;

    public DateTime? LastSyncAt { get; set; }
    public string? LastSyncDirection { get; set; }
    public string? SyncStatus { get; set; }
}

public class WebhookEndpoint : TenantEntity
{
    public Guid IntegrationId { get; set; }
    public Integration Integration { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Secret { get; set; }
    public List<string> Events { get; set; } = new();

    public bool IsActive { get; set; } = true;
    public int RetryCount { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;

    public DateTime? LastTriggeredAt { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

#region Enums

public enum IntegrationType
{
    ERP,
    Ecommerce,
    Marketplace,
    Shipping,
    Accounting,
    CRM,
    WMS,
    Custom,
    API
}

public enum IntegrationStatus
{
    Inactive,
    Connecting,
    Connected,
    Error,
    Suspended
}

public enum IntegrationLogType
{
    Connection,
    Authentication,
    Sync,
    Webhook,
    Error,
    Info
}

#endregion
