using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Orders.Models;

/// <summary>
/// SalesOrderLine - line item on a sales order.
/// (Matches SmartWMS_UI: orders/sales-order.types.ts - SalesOrderLine type)
/// </summary>
public class SalesOrderLine : TenantEntity
{
    // Parent order
    public Guid OrderId { get; set; }
    public int LineNumber { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public required string Sku { get; set; }

    // Quantities
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityAllocated { get; set; }
    public decimal QuantityPicked { get; set; }
    public decimal QuantityShipped { get; set; }
    public decimal QuantityCancelled { get; set; }

    public decimal QuantityOutstanding => QuantityOrdered - QuantityShipped - QuantityCancelled;

    // Batch/Serial requirements
    public string? RequiredBatchNumber { get; set; }
    public DateTime? RequiredExpiryDate { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Navigation properties
    public virtual SalesOrder Order { get; set; } = null!;
    public virtual Inventory.Models.Product Product { get; set; } = null!;
}
