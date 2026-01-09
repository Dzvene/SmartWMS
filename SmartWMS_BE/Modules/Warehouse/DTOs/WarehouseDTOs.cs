namespace SmartWMS.API.Modules.Warehouse.DTOs;

/// <summary>
/// Warehouse data transfer object
/// </summary>
public class WarehouseDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid SiteId { get; set; }
    public string? SiteName { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }
    public string? Timezone { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public int ZoneCount { get; set; }
    public int LocationCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Simplified warehouse for dropdowns
/// </summary>
public class WarehouseOptionDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
}

/// <summary>
/// Request to create a new warehouse
/// </summary>
public class CreateWarehouseRequest
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required Guid SiteId { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }
    public string? Timezone { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request to update a warehouse
/// </summary>
public class UpdateWarehouseRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }
    public string? Timezone { get; set; }
    public bool? IsPrimary { get; set; }
    public bool? IsActive { get; set; }
}
