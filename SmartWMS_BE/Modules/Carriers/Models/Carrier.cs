using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Carriers.Models;

/// <summary>
/// Shipping carrier definition
/// </summary>
public class Carrier : TenantEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Contact
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }

    // Account
    public string? AccountNumber { get; set; }

    // Integration
    public CarrierIntegrationType IntegrationType { get; set; } = CarrierIntegrationType.Manual;
    public string? ApiEndpoint { get; set; }
    public string? ApiKey { get; set; }

    // Status
    public bool IsActive { get; set; } = true;

    // Default settings
    public string? DefaultServiceCode { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Services
    public List<CarrierService> Services { get; set; } = new();
}

/// <summary>
/// Carrier service/shipping method
/// </summary>
public class CarrierService : TenantEntity
{
    public Guid CarrierId { get; set; }
    public Carrier? Carrier { get; set; }

    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Estimated transit days
    public int? MinTransitDays { get; set; }
    public int? MaxTransitDays { get; set; }

    // Service type
    public ServiceType ServiceType { get; set; } = ServiceType.Ground;

    // Tracking
    public bool HasTracking { get; set; } = true;
    public string? TrackingUrlTemplate { get; set; } // e.g., "https://carrier.com/track/{tracking}"

    // Constraints
    public decimal? MaxWeightKg { get; set; }
    public decimal? MaxLengthCm { get; set; }
    public decimal? MaxWidthCm { get; set; }
    public decimal? MaxHeightCm { get; set; }

    // Status
    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }
}

/// <summary>
/// Integration type for carrier
/// </summary>
public enum CarrierIntegrationType
{
    Manual,         // No integration, manual label/tracking
    Api,            // Direct API integration
    Plugin,         // Third-party plugin
    Edi             // EDI integration
}

/// <summary>
/// Service type classification
/// </summary>
public enum ServiceType
{
    Ground,
    Express,
    NextDay,
    SameDay,
    Economy,
    International,
    Freight,
    Parcel
}
