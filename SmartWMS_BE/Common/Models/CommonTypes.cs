namespace SmartWMS.API.Common.Models;

/// <summary>
/// Physical dimensions in millimeters (matching SmartWMS_UI Dimensions type)
/// </summary>
public class Dimensions
{
    public int WidthMm { get; set; }
    public int HeightMm { get; set; }
    public int DepthMm { get; set; }
}

/// <summary>
/// Weight in kilograms (matching SmartWMS_UI Weight type)
/// </summary>
public class Weight
{
    public decimal GrossKg { get; set; }
    public decimal? NetKg { get; set; }
}

/// <summary>
/// Postal address for shipping (matching SmartWMS_UI PostalAddress type)
/// </summary>
public class PostalAddress
{
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string City { get; set; }
    public required string PostalCode { get; set; }
    public required string CountryCode { get; set; }
    public string? Region { get; set; }
}

/// <summary>
/// Reference to a user (for assignments, audit trails)
/// </summary>
public class UserRef
{
    public int UserId { get; set; }
    public required string Username { get; set; }
    public string? DisplayName { get; set; }
}
