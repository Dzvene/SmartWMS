using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Orders.Models;

/// <summary>
/// PurchaseOrder - supplier order for inbound receipt.
/// (Matches SmartWMS_UI: orders/purchase-order.types.ts - PurchaseOrder type)
/// </summary>
public class PurchaseOrder : TenantEntity
{
    public required string OrderNumber { get; set; }
    public string? ExternalReference { get; set; }

    // Supplier
    public Guid SupplierId { get; set; }

    // Warehouse
    public Guid WarehouseId { get; set; }

    // Status
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    // Dates
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }

    // Receiving dock assignment
    public Guid? ReceivingDockId { get; set; }

    // Computed totals
    public int TotalLines { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    // Navigation properties
    public virtual Supplier Supplier { get; set; } = null!;
    public virtual Warehouse.Models.Warehouse Warehouse { get; set; } = null!;
    public virtual Warehouse.Models.Location? ReceivingDock { get; set; }
    public virtual ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}
