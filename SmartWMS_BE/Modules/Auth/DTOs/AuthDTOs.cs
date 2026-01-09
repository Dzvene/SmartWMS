namespace SmartWMS.API.Modules.Auth.DTOs;

/// <summary>
/// Login request (matches SmartWMS_UI LoginRequest)
/// </summary>
public class LoginRequest
{
    public required string TenantCode { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

/// <summary>
/// Login response (matches SmartWMS_UI LoginResponse)
/// </summary>
public class LoginResponse
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public required DateTime ExpiresAt { get; set; }
    public required UserInfo User { get; set; }
}

/// <summary>
/// User info included in login response (matches SmartWMS_UI UserInfo)
/// </summary>
public class UserInfo
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public Guid TenantId { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? WarehouseId { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// Refresh token request
/// </summary>
public class RefreshRequest
{
    public required string RefreshToken { get; set; }
}

/// <summary>
/// Change password request
/// </summary>
public class ChangePasswordRequest
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}
