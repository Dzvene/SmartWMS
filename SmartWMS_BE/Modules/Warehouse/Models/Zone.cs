using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Warehouse.Models;

/// <summary>
/// Zone - logical grouping of locations within a warehouse.
/// (Matches SmartWMS_UI: warehouse/location.types.ts - Zone type)
/// </summary>
public class Zone : TenantEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Parent warehouse
    public Guid WarehouseId { get; set; }

    // Zone configuration
    public ZoneType ZoneType { get; set; }
    public bool IsActive { get; set; } = true;

    // Pick sequence for optimized picking
    public int? PickSequence { get; set; }

    // Computed property (maintained by triggers or application logic)
    public int LocationCount { get; set; }

    // Navigation properties
    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}
