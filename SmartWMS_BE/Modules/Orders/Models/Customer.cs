using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Orders.Models;

/// <summary>
/// Customer - represents a customer for sales orders.
/// </summary>
public class Customer : TenantEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // Default shipping address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }

    // Business info
    public string? TaxId { get; set; }
    public string? PaymentTerms { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
}
