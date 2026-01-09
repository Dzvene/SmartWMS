namespace SmartWMS.API.Modules.Orders.DTOs;

/// <summary>
/// Supplier DTO for API responses
/// </summary>
public class SupplierDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // Address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }

    // Business info
    public string? TaxId { get; set; }
    public string? PaymentTerms { get; set; }
    public int? LeadTimeDays { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Statistics
    public int OrderCount { get; set; }
}

/// <summary>
/// Request to create a new supplier
/// </summary>
public class CreateSupplierRequest
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // Address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }

    // Business info
    public string? TaxId { get; set; }
    public string? PaymentTerms { get; set; }
    public int? LeadTimeDays { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request to update an existing supplier
/// </summary>
public class UpdateSupplierRequest
{
    public string? Name { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // Address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }

    // Business info
    public string? TaxId { get; set; }
    public string? PaymentTerms { get; set; }
    public int? LeadTimeDays { get; set; }

    public bool? IsActive { get; set; }
}

/// <summary>
/// Filter options for supplier queries
/// </summary>
public class SupplierFilters
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string? CountryCode { get; set; }
}
