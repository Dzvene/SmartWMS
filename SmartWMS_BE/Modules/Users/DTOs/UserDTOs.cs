namespace SmartWMS.API.Modules.Users.DTOs;

/// <summary>
/// User response DTO
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public Guid? RoleId { get; set; }
    public string? RoleName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? DefaultSiteId { get; set; }
    public Guid? DefaultWarehouseId { get; set; }
}

/// <summary>
/// Create user request
/// </summary>
public class CreateUserRequest
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Password { get; set; }
    public string? RoleName { get; set; }
    public Guid? DefaultSiteId { get; set; }
    public Guid? DefaultWarehouseId { get; set; }
}

/// <summary>
/// Update user request
/// </summary>
public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? RoleName { get; set; }
    public bool? IsActive { get; set; }
    public Guid? DefaultSiteId { get; set; }
    public Guid? DefaultWarehouseId { get; set; }
}

/// <summary>
/// Reset password request
/// </summary>
public class ResetPasswordRequest
{
    public required string NewPassword { get; set; }
}

/// <summary>
/// Role response DTO
/// </summary>
public class RoleDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public int UserCount { get; set; }
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// Create/Update role request
/// </summary>
public class RoleRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
}
