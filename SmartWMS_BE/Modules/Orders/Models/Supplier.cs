using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Orders.Models;

/// <summary>
/// Supplier - represents a supplier/vendor for purchase orders.
/// </summary>
public class Supplier : TenantEntity
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

    // Navigation properties
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
