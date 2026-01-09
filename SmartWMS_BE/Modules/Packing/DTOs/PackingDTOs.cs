using SmartWMS.API.Modules.Packing.Models;

namespace SmartWMS.API.Modules.Packing.DTOs;

#region Packing Task DTOs

public class PackingTaskDto
{
    public Guid Id { get; set; }
    public required string TaskNumber { get; set; }

    public Guid SalesOrderId { get; set; }
    public string? SalesOrderNumber { get; set; }
    public string? CustomerName { get; set; }

    public Guid? FulfillmentBatchId { get; set; }
    public string? FulfillmentBatchNumber { get; set; }

    public Guid? PackingStationId { get; set; }
    public string? PackingStationCode { get; set; }
    public string? PackingStationName { get; set; }

    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public DateTime? AssignedAt { get; set; }

    public PackingTaskStatus Status { get; set; }

    public int TotalItems { get; set; }
    public int PackedItems { get; set; }
    public decimal PackingProgress => TotalItems > 0 ? (decimal)PackedItems / TotalItems * 100 : 0;

    public int BoxCount { get; set; }
    public decimal TotalWeightKg { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration => CompletedAt.HasValue && StartedAt.HasValue
        ? CompletedAt.Value - StartedAt.Value
        : null;

    public int Priority { get; set; }
    public string? Notes { get; set; }

    public List<PackageDto> Packages { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PackingTaskListDto
{
    public Guid Id { get; set; }
    public required string TaskNumber { get; set; }
    public string? SalesOrderNumber { get; set; }
    public string? CustomerName { get; set; }
    public string? PackingStationCode { get; set; }
    public string? AssignedToUserName { get; set; }
    public PackingTaskStatus Status { get; set; }
    public int TotalItems { get; set; }
    public int PackedItems { get; set; }
    public int BoxCount { get; set; }
    public int Priority { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion

#region Package DTOs

public class PackageDto
{
    public Guid Id { get; set; }
    public Guid PackingTaskId { get; set; }
    public required string PackageNumber { get; set; }
    public int SequenceNumber { get; set; }

    public int? LengthMm { get; set; }
    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }
    public decimal? VolumeCm3 => LengthMm.HasValue && WidthMm.HasValue && HeightMm.HasValue
        ? (decimal)LengthMm.Value * WidthMm.Value * HeightMm.Value / 1000
        : null;

    public decimal WeightKg { get; set; }
    public string? PackagingType { get; set; }

    public string? TrackingNumber { get; set; }
    public string? LabelUrl { get; set; }

    public List<PackageItemDto> Items { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}

public class PackageItemDto
{
    public Guid Id { get; set; }
    public Guid PackageId { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public required string Sku { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
}

#endregion

#region Packing Station DTOs

public class PackingStationDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public bool IsActive { get; set; }
    public bool CanPrintLabels { get; set; }
    public bool HasScale { get; set; }
    public bool HasDimensioner { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

#endregion

#region Request DTOs

public class CreatePackingTaskRequest
{
    public Guid SalesOrderId { get; set; }
    public Guid? FulfillmentBatchId { get; set; }
    public Guid? PackingStationId { get; set; }
    public Guid? AssignToUserId { get; set; }
    public int Priority { get; set; } = 5;
    public string? Notes { get; set; }
}

public class CreatePackingFromFulfillmentRequest
{
    public Guid FulfillmentBatchId { get; set; }
    public Guid? PackingStationId { get; set; }
    public bool AutoAssign { get; set; } = false;
}

public class AssignPackingTaskRequest
{
    public Guid UserId { get; set; }
}

public class CreatePackageRequest
{
    public int? LengthMm { get; set; }
    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }
    public decimal WeightKg { get; set; }
    public string? PackagingType { get; set; }
}

public class AddItemToPackageRequest
{
    public Guid ProductId { get; set; }
    public required string Sku { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
}

public class CompletePackingTaskRequest
{
    public string? Notes { get; set; }
}

public class SetTrackingRequest
{
    public required string TrackingNumber { get; set; }
    public string? LabelUrl { get; set; }
}

public class CreatePackingStationRequest
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public Guid WarehouseId { get; set; }
    public bool CanPrintLabels { get; set; } = true;
    public bool HasScale { get; set; } = true;
    public bool HasDimensioner { get; set; } = false;
    public string? Notes { get; set; }
}

public class UpdatePackingStationRequest
{
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public bool CanPrintLabels { get; set; }
    public bool HasScale { get; set; }
    public bool HasDimensioner { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Filter DTOs

public class PackingTaskFilters
{
    public PackingTaskStatus? Status { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? SalesOrderId { get; set; }
    public Guid? FulfillmentBatchId { get; set; }
    public Guid? PackingStationId { get; set; }
    public int? Priority { get; set; }
    public bool? Unassigned { get; set; }
}

#endregion
