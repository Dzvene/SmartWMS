using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Sites.Models;

namespace SmartWMS.API.Modules.Companies.Models;

/// <summary>
/// Company (Tenant) - represents a customer organization using the WMS.
/// This is the root entity for multi-tenancy isolation.
/// </summary>
public class Company : BaseEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? TaxId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    // Subscription/plan info for future SaaS features
    public string? PlanCode { get; set; }
    public DateTime? PlanExpiresAt { get; set; }

    // Navigation properties
    public virtual ICollection<Site> Sites { get; set; } = new List<Site>();
}
