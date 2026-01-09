namespace SmartWMS.API.Modules.Sites.DTOs;

/// <summary>
/// Site response DTO
/// </summary>
public class SiteDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }
    public string? Timezone { get; set; }
    public bool IsActive { get; set; }
    public bool IsPrimary { get; set; }
    public int WarehousesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Create site request
/// </summary>
public class CreateSiteRequest
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
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
/// Update site request
/// </summary>
public class UpdateSiteRequest
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
