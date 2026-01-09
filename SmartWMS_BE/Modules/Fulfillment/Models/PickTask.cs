using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Fulfillment.Models;

/// <summary>
/// PickTask - individual picking assignment for a warehouse worker.
/// (Matches SmartWMS_UI: orders/fulfillment.types.ts - PickTask type)
/// </summary>
public class PickTask : TenantEntity
{
    public required string TaskNumber { get; set; }

    // Batch reference (optional - can be standalone task)
    public Guid? BatchId { get; set; }

    // Order/Line reference
    public Guid OrderId { get; set; }
    public Guid OrderLineId { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public required string Sku { get; set; }

    // Location
    public Guid FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; } // Staging/packing location

    // Quantities
    public decimal QuantityRequired { get; set; }
    public decimal QuantityPicked { get; set; }
    public decimal QuantityShortPicked { get; set; }

    // Batch/Serial picked
    public string? PickedBatchNumber { get; set; }
    public string? PickedSerialNumber { get; set; }

    // Status
    public PickTaskStatus Status { get; set; } = PickTaskStatus.Pending;

    // Priority and sequence
    public int Priority { get; set; }
    public int Sequence { get; set; }

    // Assignment
    public Guid? AssignedToUserId { get; set; }

    // Timestamps
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Short pick reason
    public string? ShortPickReason { get; set; }

    // Navigation properties
    public virtual FulfillmentBatch? Batch { get; set; }
    public virtual Orders.Models.SalesOrder Order { get; set; } = null!;
    public virtual Orders.Models.SalesOrderLine OrderLine { get; set; } = null!;
    public virtual Inventory.Models.Product Product { get; set; } = null!;
    public virtual Warehouse.Models.Location FromLocation { get; set; } = null!;
    public virtual Warehouse.Models.Location? ToLocation { get; set; }
}
