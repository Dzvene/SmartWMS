using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Users.DTOs;

namespace SmartWMS.API.Modules.Users.Services;

/// <summary>
/// Users management service interface
/// </summary>
public interface IUsersService
{
    // Users
    Task<ApiResponse<PaginatedResult<UserDto>>> GetUsersAsync(Guid tenantId, int page, int pageSize, string? search = null, bool? isActive = null);
    Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid tenantId, Guid userId);
    Task<ApiResponse<UserDto>> CreateUserAsync(Guid tenantId, CreateUserRequest request);
    Task<ApiResponse<UserDto>> UpdateUserAsync(Guid tenantId, Guid userId, UpdateUserRequest request);
    Task<ApiResponse> DeactivateUserAsync(Guid tenantId, Guid userId);
    Task<ApiResponse> ActivateUserAsync(Guid tenantId, Guid userId);
    Task<ApiResponse> ResetPasswordAsync(Guid tenantId, Guid userId, ResetPasswordRequest request);
    Task<ApiResponse> DeleteUserAsync(Guid tenantId, Guid userId);

    // Roles
    Task<ApiResponse<List<RoleDto>>> GetRolesAsync(Guid? tenantId = null);
    Task<ApiResponse<RoleDto>> GetRoleByIdAsync(Guid roleId);
    Task<ApiResponse<RoleDto>> CreateRoleAsync(Guid? tenantId, RoleRequest request);
    Task<ApiResponse<RoleDto>> UpdateRoleAsync(Guid roleId, RoleRequest request);
    Task<ApiResponse> DeleteRoleAsync(Guid roleId);
}
