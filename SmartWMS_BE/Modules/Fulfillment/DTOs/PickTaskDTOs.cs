using SmartWMS.API.Common.Enums;

namespace SmartWMS.API.Modules.Fulfillment.DTOs;

/// <summary>
/// Pick task DTO for API responses
/// </summary>
public class PickTaskDto
{
    public Guid Id { get; set; }
    public required string TaskNumber { get; set; }

    // Batch
    public Guid? BatchId { get; set; }
    public string? BatchNumber { get; set; }

    // Order
    public Guid OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public Guid OrderLineId { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public required string Sku { get; set; }
    public string? ProductName { get; set; }

    // Location
    public Guid FromLocationId { get; set; }
    public string? FromLocationCode { get; set; }
    public Guid? ToLocationId { get; set; }
    public string? ToLocationCode { get; set; }

    // Quantities
    public decimal QuantityRequired { get; set; }
    public decimal QuantityPicked { get; set; }
    public decimal QuantityShortPicked { get; set; }
    public decimal QuantityOutstanding => QuantityRequired - QuantityPicked - QuantityShortPicked;

    // Batch/Serial
    public string? PickedBatchNumber { get; set; }
    public string? PickedSerialNumber { get; set; }

    // Status
    public PickTaskStatus Status { get; set; }

    // Priority and sequence
    public int Priority { get; set; }
    public int Sequence { get; set; }

    // Assignment
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }

    // Timestamps
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Short pick
    public string? ShortPickReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request to create a pick task
/// </summary>
public class CreatePickTaskRequest
{
    public Guid? BatchId { get; set; }
    public Guid OrderId { get; set; }
    public Guid OrderLineId { get; set; }
    public Guid ProductId { get; set; }
    public Guid FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public decimal QuantityRequired { get; set; }
    public int Priority { get; set; } = 0;
    public Guid? AssignedToUserId { get; set; }
}

/// <summary>
/// Request to assign a pick task
/// </summary>
public class AssignPickTaskRequest
{
    public Guid? UserId { get; set; }
}

/// <summary>
/// Request to start picking
/// </summary>
public class StartPickTaskRequest
{
    public string? Notes { get; set; }
}

/// <summary>
/// Request to confirm a pick
/// </summary>
public class ConfirmPickRequest
{
    public decimal QuantityPicked { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public Guid? ToLocationId { get; set; }
}

/// <summary>
/// Request to record a short pick
/// </summary>
public class ShortPickRequest
{
    public decimal QuantityPicked { get; set; }
    public decimal QuantityShortPicked { get; set; }
    public required string Reason { get; set; }
}

/// <summary>
/// Filter options for pick task queries
/// </summary>
public class PickTaskFilters
{
    public string? Search { get; set; }
    public PickTaskStatus? Status { get; set; }
    public Guid? BatchId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? FromLocationId { get; set; }
    public Guid? WarehouseId { get; set; }
    public bool? IsAssigned { get; set; }
}
