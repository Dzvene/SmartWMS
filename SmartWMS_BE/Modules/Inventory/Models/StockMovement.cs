using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Inventory.Models;

/// <summary>
/// StockMovement - records inventory transactions for audit and traceability.
/// (Matches SmartWMS_UI: inventory/stock.types.ts - StockMovement type)
/// </summary>
public class StockMovement : TenantEntity
{
    // Movement identification
    public required string MovementNumber { get; set; }
    public MovementType MovementType { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public required string Sku { get; set; }

    // Location (from/to)
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }

    // Quantity
    public decimal Quantity { get; set; }

    // Batch/Serial tracking
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }

    // Reference to source document
    public string? ReferenceType { get; set; } // PurchaseOrder, SalesOrder, etc.
    public Guid? ReferenceId { get; set; }
    public string? ReferenceNumber { get; set; }

    // Reason and notes
    public string? ReasonCode { get; set; }
    public string? Notes { get; set; }

    // Movement timestamp
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual Warehouse.Models.Location? FromLocation { get; set; }
    public virtual Warehouse.Models.Location? ToLocation { get; set; }
}
