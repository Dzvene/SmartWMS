using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Transfers.Models;

/// <summary>
/// Stock Transfer - document for moving inventory between locations/warehouses
/// </summary>
public class StockTransfer : TenantEntity
{
    public required string TransferNumber { get; set; }

    // Transfer type
    public TransferType TransferType { get; set; } = TransferType.Internal;

    // Source
    public Guid FromWarehouseId { get; set; }
    public Guid? FromZoneId { get; set; }

    // Destination
    public Guid ToWarehouseId { get; set; }
    public Guid? ToZoneId { get; set; }

    // Status
    public TransferStatus Status { get; set; } = TransferStatus.Draft;

    // Priority and scheduling
    public TransferPriority Priority { get; set; } = TransferPriority.Normal;
    public DateTime? ScheduledDate { get; set; }
    public DateTime? RequiredByDate { get; set; }

    // Reason
    public Guid? ReasonCodeId { get; set; }
    public string? ReasonNotes { get; set; }

    // Source reference (e.g., Replenishment request)
    public string? SourceDocumentType { get; set; }
    public Guid? SourceDocumentId { get; set; }
    public string? SourceDocumentNumber { get; set; }

    // User tracking
    public Guid CreatedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? PickedByUserId { get; set; }
    public DateTime? PickedAt { get; set; }
    public Guid? ReceivedByUserId { get; set; }
    public DateTime? ReceivedAt { get; set; }

    // Summary
    public string? Notes { get; set; }
    public int TotalLines { get; set; }
    public decimal TotalQuantity { get; set; }
    public int PickedLines { get; set; }
    public int ReceivedLines { get; set; }

    // Navigation
    public virtual Warehouse.Models.Warehouse FromWarehouse { get; set; } = null!;
    public virtual Warehouse.Models.Zone? FromZone { get; set; }
    public virtual Warehouse.Models.Warehouse ToWarehouse { get; set; } = null!;
    public virtual Warehouse.Models.Zone? ToZone { get; set; }
    public virtual Configuration.Models.ReasonCode? ReasonCode { get; set; }
    public virtual List<StockTransferLine> Lines { get; set; } = new();
}

/// <summary>
/// Stock Transfer Line - individual item to transfer
/// </summary>
public class StockTransferLine : TenantEntity
{
    public Guid TransferId { get; set; }
    public int LineNumber { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public required string Sku { get; set; }

    // Source location
    public Guid FromLocationId { get; set; }

    // Destination location
    public Guid ToLocationId { get; set; }

    // Batch/Serial (if applicable)
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }

    // Quantities
    public decimal QuantityRequested { get; set; }
    public decimal QuantityPicked { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QuantityVariance => QuantityPicked - QuantityReceived;

    // Status
    public TransferLineStatus Status { get; set; } = TransferLineStatus.Pending;

    // Timestamps
    public DateTime? PickedAt { get; set; }
    public Guid? PickedByUserId { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public Guid? ReceivedByUserId { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Navigation
    public virtual StockTransfer Transfer { get; set; } = null!;
    public virtual Inventory.Models.Product Product { get; set; } = null!;
    public virtual Warehouse.Models.Location FromLocation { get; set; } = null!;
    public virtual Warehouse.Models.Location ToLocation { get; set; } = null!;
}

#region Enums

public enum TransferType
{
    Internal,           // Within same warehouse
    InterWarehouse,     // Between warehouses
    Replenishment,      // Replenishment from reserve to picking
    Return,             // Return to storage
    Consolidation,      // Consolidate partial pallets
    Relocation          // Move for optimization
}

public enum TransferStatus
{
    Draft,
    Requested,
    Approved,
    Released,
    InProgress,
    Picked,
    InTransit,
    Received,
    Complete,
    Cancelled
}

public enum TransferLineStatus
{
    Pending,
    Allocated,
    PartiallyPicked,
    Picked,
    InTransit,
    PartiallyReceived,
    Received,
    Cancelled
}

public enum TransferPriority
{
    Low,
    Normal,
    High,
    Urgent
}

#endregion
