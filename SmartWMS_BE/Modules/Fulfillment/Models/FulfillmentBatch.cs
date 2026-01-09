using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Fulfillment.Models;

/// <summary>
/// FulfillmentBatch - groups orders for efficient picking and processing.
/// This is the KEY module that differentiates modern WMS from simple inventory systems.
/// (Matches SmartWMS_UI: orders/fulfillment.types.ts - FulfillmentBatch type)
/// </summary>
public class FulfillmentBatch : TenantEntity
{
    public required string BatchNumber { get; set; }
    public string? Name { get; set; }

    // Warehouse
    public Guid WarehouseId { get; set; }

    // Status and type
    public FulfillmentStatus Status { get; set; } = FulfillmentStatus.Created;
    public BatchType BatchType { get; set; } = BatchType.Multi;

    // Counts and quantities
    public int OrderCount { get; set; }
    public int LineCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal PickedQuantity { get; set; }

    // Priority
    public int Priority { get; set; }

    // Assignment
    public Guid? AssignedToUserId { get; set; }

    // Timestamps
    public DateTime? ReleasedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Zone restriction (for zone-based picking)
    public Guid? ZoneId { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Warehouse.Models.Warehouse Warehouse { get; set; } = null!;
    public virtual Warehouse.Models.Zone? Zone { get; set; }
    public virtual ICollection<FulfillmentOrder> FulfillmentOrders { get; set; } = new List<FulfillmentOrder>();
    public virtual ICollection<PickTask> PickTasks { get; set; } = new List<PickTask>();
}
