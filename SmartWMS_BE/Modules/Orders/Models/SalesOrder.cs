using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Orders.Models;

/// <summary>
/// SalesOrder - customer order for outbound shipment.
/// (Matches SmartWMS_UI: orders/sales-order.types.ts - SalesOrder type)
/// </summary>
public class SalesOrder : TenantEntity
{
    public required string OrderNumber { get; set; }
    public string? ExternalReference { get; set; }

    // Customer
    public Guid CustomerId { get; set; }

    // Warehouse
    public Guid WarehouseId { get; set; }

    // Status
    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    public OrderPriority Priority { get; set; } = OrderPriority.Normal;

    // Dates
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? RequiredDate { get; set; }
    public DateTime? ShippedDate { get; set; }

    // Shipping address (can differ from customer default)
    public string? ShipToName { get; set; }
    public string? ShipToAddressLine1 { get; set; }
    public string? ShipToAddressLine2 { get; set; }
    public string? ShipToCity { get; set; }
    public string? ShipToRegion { get; set; }
    public string? ShipToPostalCode { get; set; }
    public string? ShipToCountryCode { get; set; }

    // Shipping info
    public string? CarrierCode { get; set; }
    public string? ServiceLevel { get; set; }
    public string? ShippingInstructions { get; set; }

    // Computed totals (maintained by application logic)
    public int TotalLines { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal AllocatedQuantity { get; set; }
    public decimal PickedQuantity { get; set; }
    public decimal ShippedQuantity { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual Warehouse.Models.Warehouse Warehouse { get; set; } = null!;
    public virtual ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
}
