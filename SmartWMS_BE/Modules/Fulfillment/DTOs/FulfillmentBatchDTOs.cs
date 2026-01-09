using SmartWMS.API.Common.Enums;

namespace SmartWMS.API.Modules.Fulfillment.DTOs;

/// <summary>
/// Fulfillment batch DTO for API responses
/// </summary>
public class FulfillmentBatchDto
{
    public Guid Id { get; set; }
    public required string BatchNumber { get; set; }
    public string? Name { get; set; }

    // Warehouse
    public Guid WarehouseId { get; set; }
    public string? WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }

    // Status and type
    public FulfillmentStatus Status { get; set; }
    public BatchType BatchType { get; set; }

    // Counts
    public int OrderCount { get; set; }
    public int LineCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal PickedQuantity { get; set; }

    // Progress
    public decimal ProgressPercent => TotalQuantity > 0
        ? Math.Round(PickedQuantity / TotalQuantity * 100, 1)
        : 0;

    // Priority
    public int Priority { get; set; }

    // Assignment
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }

    // Zone
    public Guid? ZoneId { get; set; }
    public string? ZoneName { get; set; }

    // Timestamps
    public DateTime? ReleasedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Orders in batch (optional)
    public List<FulfillmentOrderDto>? Orders { get; set; }

    // Pick tasks (optional)
    public List<PickTaskDto>? PickTasks { get; set; }
}

/// <summary>
/// Fulfillment order (link between batch and sales order)
/// </summary>
public class FulfillmentOrderDto
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public Guid OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public string? CustomerName { get; set; }
    public int Sequence { get; set; }
}

/// <summary>
/// Request to create a fulfillment batch
/// </summary>
public class CreateFulfillmentBatchRequest
{
    public string? BatchNumber { get; set; } // Auto-generated if not provided
    public string? Name { get; set; }
    public Guid WarehouseId { get; set; }
    public BatchType BatchType { get; set; } = BatchType.Multi;
    public int Priority { get; set; } = 0;
    public Guid? ZoneId { get; set; }
    public string? Notes { get; set; }

    // Orders to include in the batch
    public List<Guid>? OrderIds { get; set; }
}

/// <summary>
/// Request to update a fulfillment batch
/// </summary>
public class UpdateFulfillmentBatchRequest
{
    public string? Name { get; set; }
    public int? Priority { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? ZoneId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to add orders to a batch
/// </summary>
public class AddOrdersToBatchRequest
{
    public List<Guid> OrderIds { get; set; } = new();
}

/// <summary>
/// Request to release a batch for picking
/// </summary>
public class ReleaseBatchRequest
{
    public Guid? AssignedToUserId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Filter options for batch queries
/// </summary>
public class FulfillmentBatchFilters
{
    public string? Search { get; set; }
    public FulfillmentStatus? Status { get; set; }
    public BatchType? BatchType { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? ZoneId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}
