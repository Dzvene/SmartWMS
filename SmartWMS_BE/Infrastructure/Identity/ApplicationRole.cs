using Microsoft.AspNetCore.Identity;

namespace SmartWMS.API.Infrastructure.Identity;

/// <summary>
/// Custom role entity extending ASP.NET Identity.
/// Roles are tenant-scoped, allowing each Company to define their own roles.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base() { }

    public ApplicationRole(string roleName) : base(roleName) { }

    // Tenant isolation (null = system-wide role)
    public Guid? TenantId { get; set; }

    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
