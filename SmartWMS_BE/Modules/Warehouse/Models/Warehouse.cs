using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Warehouse.Models;

/// <summary>
/// Warehouse - represents a physical warehouse facility.
/// (Matches SmartWMS_UI: warehouse/location.types.ts - Warehouse type)
/// </summary>
public class Warehouse : TenantEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Site association
    public Guid SiteId { get; set; }

    // Address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }

    // Configuration
    public string? Timezone { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Zone> Zones { get; set; } = new List<Zone>();
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}
