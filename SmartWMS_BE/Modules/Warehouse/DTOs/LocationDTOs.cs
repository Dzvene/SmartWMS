using SmartWMS.API.Common.Enums;

namespace SmartWMS.API.Modules.Warehouse.DTOs;

/// <summary>
/// Location data transfer object
/// </summary>
public class LocationDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public string? Name { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public Guid? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public string? Aisle { get; set; }
    public string? Rack { get; set; }
    public string? Level { get; set; }
    public string? Position { get; set; }
    public LocationType LocationType { get; set; }
    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }
    public int? DepthMm { get; set; }
    public decimal? MaxWeight { get; set; }
    public decimal? MaxVolume { get; set; }
    public bool IsActive { get; set; }
    public bool IsPickLocation { get; set; }
    public bool IsPutawayLocation { get; set; }
    public bool IsReceivingDock { get; set; }
    public bool IsShippingDock { get; set; }
    public int? PickSequence { get; set; }
    public int? PutawaySequence { get; set; }
    public int StockLevelCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request to create a new location
/// </summary>
public class CreateLocationRequest
{
    public required string Code { get; set; }
    public string? Name { get; set; }
    public required Guid WarehouseId { get; set; }
    public Guid? ZoneId { get; set; }
    public string? Aisle { get; set; }
    public string? Rack { get; set; }
    public string? Level { get; set; }
    public string? Position { get; set; }
    public LocationType LocationType { get; set; } = LocationType.Bulk;
    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }
    public int? DepthMm { get; set; }
    public decimal? MaxWeight { get; set; }
    public decimal? MaxVolume { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPickLocation { get; set; }
    public bool IsPutawayLocation { get; set; }
    public bool IsReceivingDock { get; set; }
    public bool IsShippingDock { get; set; }
    public int? PickSequence { get; set; }
    public int? PutawaySequence { get; set; }
}

/// <summary>
/// Request to update a location
/// </summary>
public class UpdateLocationRequest
{
    public string? Name { get; set; }
    public Guid? ZoneId { get; set; }
    public string? Aisle { get; set; }
    public string? Rack { get; set; }
    public string? Level { get; set; }
    public string? Position { get; set; }
    public LocationType? LocationType { get; set; }
    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }
    public int? DepthMm { get; set; }
    public decimal? MaxWeight { get; set; }
    public decimal? MaxVolume { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsPickLocation { get; set; }
    public bool? IsPutawayLocation { get; set; }
    public bool? IsReceivingDock { get; set; }
    public bool? IsShippingDock { get; set; }
    public int? PickSequence { get; set; }
    public int? PutawaySequence { get; set; }
}
