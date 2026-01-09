using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Inventory.Models;
using SmartWMS.API.Modules.Orders.Models;

namespace SmartWMS.API.Modules.Returns.Models;

/// <summary>
/// Return order - customer returning items
/// </summary>
public class ReturnOrder : TenantEntity
{
    public required string ReturnNumber { get; set; }

    // Original order reference
    public Guid? OriginalSalesOrderId { get; set; }
    public SalesOrder? OriginalSalesOrder { get; set; }

    // Customer
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    // Status
    public ReturnOrderStatus Status { get; set; } = ReturnOrderStatus.Pending;

    // Return type
    public ReturnType ReturnType { get; set; } = ReturnType.CustomerReturn;

    // Reason
    public Guid? ReasonCodeId { get; set; }
    public string? ReasonDescription { get; set; }

    // Return location (where items will be received)
    public Guid? ReceivingLocationId { get; set; }
    public Warehouse.Models.Location? ReceivingLocation { get; set; }

    // Dates
    public DateTime? RequestedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? ProcessedDate { get; set; }

    // RMA (Return Merchandise Authorization)
    public string? RmaNumber { get; set; }
    public DateTime? RmaExpiryDate { get; set; }

    // Carrier/Tracking
    public string? CarrierCode { get; set; }
    public string? TrackingNumber { get; set; }

    // Assignment
    public Guid? AssignedToUserId { get; set; }

    // Totals
    public int TotalLines { get; set; }
    public decimal TotalQuantityExpected { get; set; }
    public decimal TotalQuantityReceived { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    // Lines
    public List<ReturnOrderLine> Lines { get; set; } = new();
}

/// <summary>
/// Return order line item
/// </summary>
public class ReturnOrderLine : TenantEntity
{
    public Guid ReturnOrderId { get; set; }
    public ReturnOrder? ReturnOrder { get; set; }

    public int LineNumber { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public required string Sku { get; set; }

    // Quantities
    public decimal QuantityExpected { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QuantityAccepted { get; set; }
    public decimal QuantityRejected { get; set; }

    // Quality
    public ReturnCondition Condition { get; set; } = ReturnCondition.Unknown;
    public string? ConditionNotes { get; set; }

    // Disposition
    public ReturnDisposition Disposition { get; set; } = ReturnDisposition.Pending;
    public Guid? DispositionLocationId { get; set; }
    public Warehouse.Models.Location? DispositionLocation { get; set; }

    // Batch/Serial
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }

    // Original order line reference
    public Guid? OriginalOrderLineId { get; set; }

    // Reason (can override order-level reason)
    public Guid? ReasonCodeId { get; set; }
    public string? ReasonDescription { get; set; }

    // Notes
    public string? Notes { get; set; }
}

/// <summary>
/// Status of a return order
/// </summary>
public enum ReturnOrderStatus
{
    Pending,        // Return requested, awaiting receipt
    InTransit,      // Customer shipped, awaiting arrival
    Received,       // Items received, awaiting inspection
    InProgress,     // Being processed/inspected
    Complete,       // All lines processed
    Cancelled       // Return cancelled
}

/// <summary>
/// Type of return
/// </summary>
public enum ReturnType
{
    CustomerReturn,     // Customer returning items
    SupplierReturn,     // Returning to supplier
    InternalTransfer,   // Internal warehouse transfer/correction
    Damaged,            // Damaged goods return
    Recall              // Product recall
}

/// <summary>
/// Condition of returned item
/// </summary>
public enum ReturnCondition
{
    Unknown,        // Not yet inspected
    Good,           // Sellable as new
    Refurbished,    // Can be refurbished and resold
    Damaged,        // Damaged but repairable
    Defective,      // Defective, cannot repair
    Destroyed       // Cannot be used
}

/// <summary>
/// Disposition of returned item
/// </summary>
public enum ReturnDisposition
{
    Pending,        // Not yet decided
    ReturnToStock,  // Return to sellable inventory
    Quarantine,     // Hold for inspection
    Scrap,          // Write off and dispose
    ReturnToSupplier, // Return to vendor
    Donate,         // Donate
    Repair          // Send for repair
}
