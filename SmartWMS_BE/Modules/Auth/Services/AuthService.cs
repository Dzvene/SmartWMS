using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Infrastructure.Identity;
using SmartWMS.API.Modules.Auth.DTOs;

namespace SmartWMS.API.Modules.Auth.Services;

/// <summary>
/// Authentication service implementation with JWT token management
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IOptions<JwtSettings> jwtSettings,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        // Verify tenant exists
        var tenant = await _dbContext.Companies
            .FirstOrDefaultAsync(c => c.Code == request.TenantCode && c.IsActive);
        if (tenant == null)
        {
            return ApiResponse<LoginResponse>.Fail("Invalid tenant code");
        }

        // Find user by email within the tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.TenantId == tenant.Id);
        if (user == null || !user.IsActive)
        {
            return ApiResponse<LoginResponse>.Fail("Invalid credentials");
        }

        var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
        {
            return ApiResponse<LoginResponse>.Fail("Invalid credentials");
        }

        // Get user roles and permissions
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetUserPermissionsAsync(user.Id);

        // Generate tokens
        var (accessToken, expiresAt) = GenerateAccessToken(user, roles, permissions);
        var refreshToken = GenerateRefreshToken();

        // Save refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var response = new LoginResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                TenantId = user.TenantId,
                SiteId = user.DefaultSiteId,
                WarehouseId = user.DefaultWarehouseId,
                Roles = roles.ToList(),
                Permissions = permissions
            }
        };

        return ApiResponse<LoginResponse>.Ok(response);
    }

    public async Task<ApiResponse> LogoutAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponse.Fail("User not found");
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        await _userManager.UpdateAsync(user);

        return ApiResponse.Ok("Logged out successfully");
    }

    public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(RefreshRequest request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

        if (user == null ||
            user.RefreshTokenExpiresAt == null ||
            user.RefreshTokenExpiresAt < DateTime.UtcNow ||
            !user.IsActive)
        {
            return ApiResponse<LoginResponse>.Fail("Invalid or expired refresh token");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetUserPermissionsAsync(user.Id);

        var (accessToken, expiresAt) = GenerateAccessToken(user, roles, permissions);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        await _userManager.UpdateAsync(user);

        var response = new LoginResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                TenantId = user.TenantId,
                SiteId = user.DefaultSiteId,
                WarehouseId = user.DefaultWarehouseId,
                Roles = roles.ToList(),
                Permissions = permissions
            }
        };

        return ApiResponse<LoginResponse>.Ok(response);
    }

    public async Task<ApiResponse<UserInfo>> ValidateTokenAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return ApiResponse<UserInfo>.Fail("Invalid token");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            return ApiResponse<UserInfo>.Fail("User not found or inactive");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetUserPermissionsAsync(user.Id);

        var userInfo = new UserInfo
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            TenantId = user.TenantId,
            SiteId = user.DefaultSiteId,
            WarehouseId = user.DefaultWarehouseId,
            Roles = roles.ToList(),
            Permissions = permissions
        };

        return ApiResponse<UserInfo>.Ok(userInfo);
    }

    public async Task<ApiResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponse.Fail("User not found");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return ApiResponse.Fail("Password change failed", result.Errors.Select(e => e.Description).ToList());
        }

        return ApiResponse.Ok("Password changed successfully");
    }

    private (string token, DateTime expiresAt) GenerateAccessToken(
        ApplicationUser user,
        IList<string> roles,
        List<string> permissions)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new("tenant_id", user.TenantId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.DefaultSiteId.HasValue)
        {
            claims.Add(new Claim("site_id", user.DefaultSiteId.Value.ToString()));
        }

        if (user.DefaultWarehouseId.HasValue)
        {
            claims.Add(new Claim("warehouse_id", user.DefaultWarehouseId.Value.ToString()));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return new List<string>();

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = new HashSet<string>();

        // Define all available permissions
        var allPermissions = new List<string>
        {
            "users.view",
            "users.create",
            "users.edit",
            "users.delete",
            "roles.view",
            "roles.manage",
            "inventory.view",
            "inventory.manage",
            "orders.view",
            "orders.create",
            "orders.edit",
            "warehouse.view",
            "warehouse.manage",
            "reports.view",
            "settings.manage"
        };

        // Map roles to permissions
        foreach (var role in roles)
        {
            switch (role.ToLower())
            {
                case "admin":
                    // Admin gets all permissions
                    foreach (var p in allPermissions)
                        permissions.Add(p);
                    break;

                case "manager":
                    // Manager gets most permissions except settings
                    permissions.Add("users.view");
                    permissions.Add("roles.view");
                    permissions.Add("inventory.view");
                    permissions.Add("inventory.manage");
                    permissions.Add("orders.view");
                    permissions.Add("orders.create");
                    permissions.Add("orders.edit");
                    permissions.Add("warehouse.view");
                    permissions.Add("warehouse.manage");
                    permissions.Add("reports.view");
                    break;

                case "operator":
                    // Operator gets operational permissions
                    permissions.Add("inventory.view");
                    permissions.Add("inventory.manage");
                    permissions.Add("orders.view");
                    permissions.Add("orders.create");
                    permissions.Add("warehouse.view");
                    break;

                case "viewer":
                    // Viewer gets read-only permissions
                    permissions.Add("inventory.view");
                    permissions.Add("orders.view");
                    permissions.Add("warehouse.view");
                    permissions.Add("reports.view");
                    break;
            }
        }

        // Also get permissions from role claims (for custom roles)
        foreach (var roleName in roles)
        {
            var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role != null)
            {
                var roleClaims = await _dbContext.RoleClaims
                    .Where(rc => rc.RoleId == role.Id && rc.ClaimType == "permission")
                    .Select(rc => rc.ClaimValue)
                    .ToListAsync();

                foreach (var claim in roleClaims)
                {
                    if (!string.IsNullOrEmpty(claim))
                        permissions.Add(claim);
                }
            }
        }

        return permissions.ToList();
    }
}
