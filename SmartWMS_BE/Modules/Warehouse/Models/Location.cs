using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Warehouse.Models;

/// <summary>
/// Location - specific storage position in the warehouse.
/// (Matches SmartWMS_UI: warehouse/location.types.ts - Location type)
/// </summary>
public class Location : TenantEntity
{
    public required string Code { get; set; }
    public string? Name { get; set; }

    // Parent associations
    public Guid WarehouseId { get; set; }
    public Guid? ZoneId { get; set; }

    // Location coordinates (for organization and pick path optimization)
    public string? Aisle { get; set; }
    public string? Rack { get; set; }
    public string? Level { get; set; }
    public string? Position { get; set; }

    // Location type and capabilities
    public LocationType LocationType { get; set; }

    // Capacity constraints
    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }
    public int? DepthMm { get; set; }
    public decimal? MaxWeight { get; set; }
    public decimal? MaxVolume { get; set; }

    // Operational flags
    public bool IsActive { get; set; } = true;
    public bool IsPickLocation { get; set; }
    public bool IsPutawayLocation { get; set; }
    public bool IsReceivingDock { get; set; }
    public bool IsShippingDock { get; set; }

    // Sequence numbers for optimized operations
    public int? PickSequence { get; set; }
    public int? PutawaySequence { get; set; }

    // Navigation properties
    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual Zone? Zone { get; set; }
    public virtual ICollection<Inventory.Models.StockLevel> StockLevels { get; set; } = new List<Inventory.Models.StockLevel>();
}
