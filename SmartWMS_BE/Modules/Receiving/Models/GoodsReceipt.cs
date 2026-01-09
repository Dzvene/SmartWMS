using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Orders.Models;

namespace SmartWMS.API.Modules.Receiving.Models;

/// <summary>
/// Goods receipt document for receiving inventory from purchase orders
/// </summary>
public class GoodsReceipt : TenantEntity
{
    public required string ReceiptNumber { get; set; }

    // Source
    public Guid? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }

    // Supplier
    public Guid? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    // Warehouse
    public Guid WarehouseId { get; set; }
    public Modules.Warehouse.Models.Warehouse? Warehouse { get; set; }

    // Receiving dock/location
    public Guid? ReceivingLocationId { get; set; }
    public Modules.Warehouse.Models.Location? ReceivingLocation { get; set; }

    // Status
    public GoodsReceiptStatus Status { get; set; } = GoodsReceiptStatus.Draft;

    // Dates
    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Carrier info
    public string? CarrierName { get; set; }
    public string? TrackingNumber { get; set; }
    public string? DeliveryNote { get; set; }

    // Totals
    public int TotalLines { get; set; }
    public decimal TotalQuantityExpected { get; set; }
    public decimal TotalQuantityReceived { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    // Lines
    public List<GoodsReceiptLine> Lines { get; set; } = new();
}

/// <summary>
/// Line item in a goods receipt
/// </summary>
public class GoodsReceiptLine : TenantEntity
{
    public Guid ReceiptId { get; set; }
    public GoodsReceipt? Receipt { get; set; }

    public int LineNumber { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public Modules.Inventory.Models.Product? Product { get; set; }
    public required string Sku { get; set; }

    // Quantities
    public decimal QuantityExpected { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QuantityRejected { get; set; }

    // Lot/Batch tracking
    public string? BatchNumber { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime? ManufactureDate { get; set; }

    // Put-away location
    public Guid? PutawayLocationId { get; set; }
    public Modules.Warehouse.Models.Location? PutawayLocation { get; set; }

    // Status
    public GoodsReceiptLineStatus Status { get; set; } = GoodsReceiptLineStatus.Pending;

    // Quality
    public string? QualityStatus { get; set; } // Good, Damaged, Quarantine
    public string? RejectionReason { get; set; }

    // Source PO line reference
    public Guid? PurchaseOrderLineId { get; set; }
    public PurchaseOrderLine? PurchaseOrderLine { get; set; }

    public string? Notes { get; set; }
}

/// <summary>
/// Status of a goods receipt
/// </summary>
public enum GoodsReceiptStatus
{
    Draft,
    InProgress,
    Complete,
    PartiallyComplete,
    Cancelled
}

/// <summary>
/// Status of a goods receipt line
/// </summary>
public enum GoodsReceiptLineStatus
{
    Pending,
    Received,
    PartiallyReceived,
    Rejected,
    PutAway
}
