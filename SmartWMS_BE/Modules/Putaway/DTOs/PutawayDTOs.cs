using SmartWMS.API.Modules.Putaway.Models;

namespace SmartWMS.API.Modules.Putaway.DTOs;

/// <summary>
/// Putaway task DTO for API responses
/// </summary>
public class PutawayTaskDto
{
    public Guid Id { get; set; }
    public string? TaskNumber { get; set; }

    // Source
    public Guid? GoodsReceiptId { get; set; }
    public string? GoodsReceiptNumber { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public string? Sku { get; set; }
    public string? ProductName { get; set; }

    // Quantity
    public decimal QuantityToPutaway { get; set; }
    public decimal QuantityPutaway { get; set; }
    public decimal QuantityRemaining => QuantityToPutaway - QuantityPutaway;

    // Locations
    public Guid FromLocationId { get; set; }
    public string? FromLocationCode { get; set; }

    public Guid? SuggestedLocationId { get; set; }
    public string? SuggestedLocationCode { get; set; }

    public Guid? ActualLocationId { get; set; }
    public string? ActualLocationCode { get; set; }

    // Batch/Serial
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // Assignment
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public DateTime? AssignedAt { get; set; }

    // Status
    public PutawayTaskStatus Status { get; set; }

    // Timing
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int Priority { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request to create putaway tasks from goods receipt
/// </summary>
public class CreatePutawayFromReceiptRequest
{
    public Guid GoodsReceiptId { get; set; }
    public Guid? DefaultFromLocationId { get; set; } // Override receiving dock
}

/// <summary>
/// Request to create a manual putaway task
/// </summary>
public class CreatePutawayTaskRequest
{
    public Guid ProductId { get; set; }
    public Guid FromLocationId { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public Guid? SuggestedLocationId { get; set; }
    public int Priority { get; set; } = 5;
    public string? Notes { get; set; }
}

/// <summary>
/// Request to assign task to user
/// </summary>
public class AssignPutawayTaskRequest
{
    public Guid UserId { get; set; }
}

/// <summary>
/// Request to start a putaway task
/// </summary>
public class StartPutawayTaskRequest
{
    // Can be empty - just starts the task
}

/// <summary>
/// Request to complete a putaway task
/// </summary>
public class CompletePutawayTaskRequest
{
    public Guid ActualLocationId { get; set; }
    public decimal QuantityPutaway { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to get suggested location
/// </summary>
public class SuggestLocationRequest
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public Guid? PreferredZoneId { get; set; }
}

/// <summary>
/// Location suggestion result
/// </summary>
public class LocationSuggestionDto
{
    public Guid LocationId { get; set; }
    public string? LocationCode { get; set; }
    public string? ZoneName { get; set; }
    public string? WarehouseName { get; set; }
    public decimal Score { get; set; } // Higher = better
    public string? Reason { get; set; } // Why suggested
}

/// <summary>
/// Filter options for putaway tasks
/// </summary>
public class PutawayTaskFilters
{
    public PutawayTaskStatus? Status { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? FromLocationId { get; set; }
    public Guid? GoodsReceiptId { get; set; }
    public int? Priority { get; set; }
    public bool? Unassigned { get; set; }
}
