using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Companies.Models;

namespace SmartWMS.API.Modules.Sites.Models;

/// <summary>
/// Site - represents a physical location or branch of a Company.
/// A Company can have multiple Sites, each with their own warehouses.
/// </summary>
public class Site : TenantEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }
    public string? Timezone { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPrimary { get; set; }

    // Foreign key to Company (TenantId serves as CompanyId in this case)
    public Guid CompanyId { get; set; }

    // Navigation properties
    public virtual Company Company { get; set; } = null!;
    public virtual ICollection<Warehouse.Models.Warehouse> Warehouses { get; set; } = new List<Warehouse.Models.Warehouse>();
}
