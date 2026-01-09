using SmartWMS.API.Modules.CycleCount.Models;

namespace SmartWMS.API.Modules.CycleCount.DTOs;

#region Cycle Count Session DTOs

public class CycleCountSessionDto
{
    public Guid Id { get; set; }
    public required string CountNumber { get; set; }
    public string? Description { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public Guid? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public CountType CountType { get; set; }
    public CountScope CountScope { get; set; }
    public CycleCountStatus Status { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public int TotalLocations { get; set; }
    public int CountedLocations { get; set; }
    public int VarianceCount { get; set; }
    public decimal CountProgress => TotalLocations > 0
        ? (decimal)CountedLocations / TotalLocations * 100
        : 0;
    public bool RequireBlindCount { get; set; }
    public bool AllowRecounts { get; set; }
    public int MaxRecounts { get; set; }
    public string? Notes { get; set; }
    public List<CycleCountItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CycleCountSessionListDto
{
    public Guid Id { get; set; }
    public required string CountNumber { get; set; }
    public string? Description { get; set; }
    public string? WarehouseName { get; set; }
    public string? ZoneName { get; set; }
    public CountType CountType { get; set; }
    public CycleCountStatus Status { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public int TotalLocations { get; set; }
    public int CountedLocations { get; set; }
    public int VarianceCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion

#region Cycle Count Item DTOs

public class CycleCountItemDto
{
    public Guid Id { get; set; }
    public Guid CycleCountSessionId { get; set; }
    public Guid LocationId { get; set; }
    public string? LocationCode { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public required string Sku { get; set; }
    public decimal ExpectedQuantity { get; set; }
    public string? ExpectedBatchNumber { get; set; }
    public decimal? CountedQuantity { get; set; }
    public string? CountedBatchNumber { get; set; }
    public DateTime? CountedAt { get; set; }
    public Guid? CountedByUserId { get; set; }
    public string? CountedByUserName { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercent { get; set; }
    public CountItemStatus Status { get; set; }
    public int RecountNumber { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsApproved { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Request DTOs

public class CreateCycleCountRequest
{
    public string? Description { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? ZoneId { get; set; }
    public CountType CountType { get; set; } = CountType.Scheduled;
    public CountScope CountScope { get; set; } = CountScope.Location;
    public DateTime? ScheduledDate { get; set; }
    public bool RequireBlindCount { get; set; } = false;
    public bool AllowRecounts { get; set; } = true;
    public int MaxRecounts { get; set; } = 3;
    public string? Notes { get; set; }
    public List<Guid>? LocationIds { get; set; }
    public List<Guid>? ProductIds { get; set; }
}

public class UpdateCycleCountRequest
{
    public string? Description { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public bool RequireBlindCount { get; set; }
    public bool AllowRecounts { get; set; }
    public int MaxRecounts { get; set; }
    public string? Notes { get; set; }
}

public class AssignCycleCountRequest
{
    public Guid UserId { get; set; }
}

public class RecordCountRequest
{
    public decimal CountedQuantity { get; set; }
    public string? CountedBatchNumber { get; set; }
    public Guid CountedByUserId { get; set; }
    public string? Notes { get; set; }
}

public class ApproveVarianceRequest
{
    public Guid ApprovedByUserId { get; set; }
    public string? Notes { get; set; }
}

public class AdjustStockRequest
{
    public string? Reason { get; set; }
}

#endregion

#region Filter DTOs

public class CycleCountFilters
{
    public CycleCountStatus? Status { get; set; }
    public CountType? CountType { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? ZoneId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

#endregion
