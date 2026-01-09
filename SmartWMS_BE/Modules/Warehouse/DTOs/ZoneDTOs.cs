using SmartWMS.API.Common.Enums;

namespace SmartWMS.API.Modules.Warehouse.DTOs;

/// <summary>
/// Zone data transfer object
/// </summary>
public class ZoneDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public ZoneType ZoneType { get; set; }
    public int? PickSequence { get; set; }
    public bool IsActive { get; set; }
    public int LocationCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request to create a new zone
/// </summary>
public class CreateZoneRequest
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required Guid WarehouseId { get; set; }
    public ZoneType ZoneType { get; set; } = ZoneType.Storage;
    public int? PickSequence { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request to update a zone
/// </summary>
public class UpdateZoneRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ZoneType? ZoneType { get; set; }
    public int? PickSequence { get; set; }
    public bool? IsActive { get; set; }
}
