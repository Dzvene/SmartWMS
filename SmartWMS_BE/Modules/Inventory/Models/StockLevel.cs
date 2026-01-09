using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Inventory.Models;

/// <summary>
/// StockLevel - inventory quantity at a specific location.
/// (Matches SmartWMS_UI: inventory/stock.types.ts - StockLevel type)
/// </summary>
public class StockLevel : TenantEntity
{
    // Product reference
    public Guid ProductId { get; set; }
    public required string Sku { get; set; }

    // Location reference
    public Guid LocationId { get; set; }

    // Quantities
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;

    // Batch/Serial tracking (if applicable)
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // Audit
    public DateTime? LastMovementAt { get; set; }
    public DateTime? LastCountAt { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual Warehouse.Models.Location Location { get; set; } = null!;
}
