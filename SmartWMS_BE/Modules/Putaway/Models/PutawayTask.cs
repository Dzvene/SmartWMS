using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Inventory.Models;
using SmartWMS.API.Modules.Receiving.Models;
using SmartWMS.API.Modules.Warehouse.Models;

namespace SmartWMS.API.Modules.Putaway.Models;

/// <summary>
/// Putaway task - moves received goods from receiving dock to storage location
/// </summary>
public class PutawayTask : TenantEntity
{
    public required string TaskNumber { get; set; }

    // Source - from goods receipt
    public Guid? GoodsReceiptId { get; set; }
    public GoodsReceipt? GoodsReceipt { get; set; }
    public Guid? GoodsReceiptLineId { get; set; }
    public GoodsReceiptLine? GoodsReceiptLine { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public required string Sku { get; set; }

    // Quantity
    public decimal QuantityToPutaway { get; set; }
    public decimal QuantityPutaway { get; set; }

    // Locations
    public Guid FromLocationId { get; set; }
    public Location? FromLocation { get; set; } // Receiving dock

    public Guid? SuggestedLocationId { get; set; }
    public Location? SuggestedLocation { get; set; } // System suggested

    public Guid? ActualLocationId { get; set; }
    public Location? ActualLocation { get; set; } // Where actually put

    // Batch/Serial tracking
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // Assignment
    public Guid? AssignedToUserId { get; set; }
    public DateTime? AssignedAt { get; set; }

    // Status
    public PutawayTaskStatus Status { get; set; } = PutawayTaskStatus.Pending;

    // Timing
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Priority (1 = highest)
    public int Priority { get; set; } = 5;

    public string? Notes { get; set; }
}

/// <summary>
/// Status of a putaway task
/// </summary>
public enum PutawayTaskStatus
{
    Pending,
    Assigned,
    InProgress,
    Complete,
    Cancelled
}
