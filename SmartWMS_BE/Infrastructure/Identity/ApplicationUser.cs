using Microsoft.AspNetCore.Identity;

namespace SmartWMS.API.Infrastructure.Identity;

/// <summary>
/// Custom user entity extending ASP.NET Identity.
/// Users are scoped to a specific tenant (Company) and optionally a warehouse.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";

    // Tenant isolation
    public Guid TenantId { get; set; }

    // Default site and warehouse assignment
    public Guid? DefaultSiteId { get; set; }
    public Guid? DefaultWarehouseId { get; set; }

    // Status and audit
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Refresh token for JWT authentication
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
}
