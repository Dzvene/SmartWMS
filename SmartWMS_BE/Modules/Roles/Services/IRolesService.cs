using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Roles.DTOs;

namespace SmartWMS.API.Modules.Roles.Services;

public interface IRolesService
{
    // Roles CRUD
    Task<ApiResponse<List<RoleSummaryDto>>> GetRolesAsync(Guid tenantId);
    Task<ApiResponse<RoleDto>> GetRoleByIdAsync(Guid tenantId, Guid roleId);
    Task<ApiResponse<RoleDto>> GetRoleByNameAsync(Guid tenantId, string roleName);
    Task<ApiResponse<RoleDto>> CreateRoleAsync(Guid tenantId, CreateRoleRequest request);
    Task<ApiResponse<RoleDto>> UpdateRoleAsync(Guid tenantId, Guid roleId, UpdateRoleRequest request);
    Task<ApiResponse<bool>> DeleteRoleAsync(Guid tenantId, Guid roleId);

    // Role-User assignments
    Task<ApiResponse<RoleWithUsersDto>> GetRoleWithUsersAsync(Guid tenantId, Guid roleId);
    Task<ApiResponse<bool>> AssignUsersToRoleAsync(Guid tenantId, Guid roleId, AssignUsersToRoleRequest request);
    Task<ApiResponse<bool>> RemoveUserFromRoleAsync(Guid tenantId, Guid roleId, Guid userId);
    Task<ApiResponse<UserRolesDto>> GetUserRolesAsync(Guid tenantId, Guid userId);
    Task<ApiResponse<bool>> AssignRolesToUserAsync(Guid tenantId, Guid userId, AssignRolesToUserRequest request);

    // Permissions
    Task<ApiResponse<List<PermissionGroupDto>>> GetAllPermissionsAsync();
    Task<ApiResponse<List<string>>> GetRolePermissionsAsync(Guid tenantId, Guid roleId);
    Task<ApiResponse<bool>> AssignPermissionsToRoleAsync(Guid tenantId, Guid roleId, AssignPermissionsRequest request);
    Task<ApiResponse<List<string>>> GetUserPermissionsAsync(Guid tenantId, Guid userId);
    Task<ApiResponse<bool>> UserHasPermissionAsync(Guid tenantId, Guid userId, string permission);
}
