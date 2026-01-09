using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Orders.Models;

/// <summary>
/// PurchaseOrderLine - line item on a purchase order.
/// (Matches SmartWMS_UI: orders/purchase-order.types.ts - PurchaseOrderLine type)
/// </summary>
public class PurchaseOrderLine : TenantEntity
{
    // Parent order
    public Guid OrderId { get; set; }
    public int LineNumber { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public required string Sku { get; set; }

    // Quantities
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QuantityCancelled { get; set; }

    public decimal QuantityOutstanding => QuantityOrdered - QuantityReceived - QuantityCancelled;

    // Expected batch/serial
    public string? ExpectedBatchNumber { get; set; }
    public DateTime? ExpectedExpiryDate { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Navigation properties
    public virtual PurchaseOrder Order { get; set; } = null!;
    public virtual Inventory.Models.Product Product { get; set; } = null!;
}
