namespace SmartWMS.API.Modules.Roles.DTOs;

#region Role DTOs

public record RoleDto(
    Guid Id,
    string Name,
    string? NormalizedName,
    string? Description,
    bool IsSystemRole,
    Guid? TenantId,
    DateTime CreatedAt,
    int UserCount,
    List<string>? Permissions
);

public record RoleSummaryDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystemRole,
    int UserCount
);

public record RoleWithUsersDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystemRole,
    List<RoleUserDto> Users
);

public record RoleUserDto(
    Guid UserId,
    string? UserName,
    string? Email,
    string? FullName
);

#endregion

#region Create/Update Requests

public record CreateRoleRequest(
    string Name,
    string? Description,
    List<string>? Permissions
);

public record UpdateRoleRequest(
    string? Name,
    string? Description,
    List<string>? Permissions
);

#endregion

#region Permission DTOs

public record PermissionDto(
    string Code,
    string Name,
    string? Description,
    string Module,
    string Category
);

public record PermissionGroupDto(
    string Module,
    string ModuleName,
    List<PermissionCategoryDto> Categories
);

public record PermissionCategoryDto(
    string Category,
    string CategoryName,
    List<PermissionDto> Permissions
);

public record AssignPermissionsRequest(
    List<string> Permissions
);

#endregion

#region User-Role Assignment

public record AssignUsersToRoleRequest(
    List<Guid> UserIds
);

public record AssignRolesToUserRequest(
    List<Guid> RoleIds
);

public record UserRolesDto(
    Guid UserId,
    string? UserName,
    List<RoleSummaryDto> Roles
);

#endregion
