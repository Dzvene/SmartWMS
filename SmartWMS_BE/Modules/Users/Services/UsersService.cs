using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Infrastructure.Identity;
using SmartWMS.API.Modules.Users.DTOs;

namespace SmartWMS.API.Modules.Users.Services;

/// <summary>
/// Users management service implementation
/// </summary>
public class UsersService : IUsersService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;

    public UsersService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
    }

    #region Users

    public async Task<ApiResponse<PaginatedResult<UserDto>>> GetUsersAsync(
        Guid tenantId, int page, int pageSize, string? search = null, bool? isActive = null)
    {
        var query = _dbContext.Users
            .Where(u => u.TenantId == tenantId);

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u =>
                u.Email!.ToLower().Contains(searchLower) ||
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault();

            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Username = user.UserName ?? user.Email!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RoleName = roleName,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt,
                DefaultSiteId = user.DefaultSiteId,
                DefaultWarehouseId = user.DefaultWarehouseId
            });
        }

        var result = new PaginatedResult<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return ApiResponse<PaginatedResult<UserDto>>.Ok(result);
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid tenantId, Guid userId)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
        {
            return ApiResponse<UserDto>.Fail("User not found");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var roleName = roles.FirstOrDefault();

        var dto = new UserDto
        {
            Id = user.Id,
            Username = user.UserName ?? user.Email!,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleName = roleName,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            DefaultSiteId = user.DefaultSiteId,
            DefaultWarehouseId = user.DefaultWarehouseId
        };

        return ApiResponse<UserDto>.Ok(dto);
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(Guid tenantId, CreateUserRequest request)
    {
        // Check if email already exists in tenant
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.TenantId == tenantId);

        if (existingUser != null)
        {
            return ApiResponse<UserDto>.Fail("User with this email already exists");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            FirstName = request.FirstName,
            LastName = request.LastName,
            TenantId = tenantId,
            DefaultSiteId = request.DefaultSiteId,
            DefaultWarehouseId = request.DefaultWarehouseId,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return ApiResponse<UserDto>.Fail("Failed to create user", result.Errors.Select(e => e.Description).ToList());
        }

        // Assign role if provided
        if (!string.IsNullOrWhiteSpace(request.RoleName))
        {
            if (await _roleManager.RoleExistsAsync(request.RoleName))
            {
                await _userManager.AddToRoleAsync(user, request.RoleName);
            }
        }

        var dto = new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleName = request.RoleName,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            DefaultSiteId = user.DefaultSiteId,
            DefaultWarehouseId = user.DefaultWarehouseId
        };

        return ApiResponse<UserDto>.Ok(dto, "User created successfully");
    }

    public async Task<ApiResponse<UserDto>> UpdateUserAsync(Guid tenantId, Guid userId, UpdateUserRequest request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
        {
            return ApiResponse<UserDto>.Fail("User not found");
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            // Check if new email is unique within tenant
            var emailExists = await _dbContext.Users
                .AnyAsync(u => u.Email == request.Email && u.TenantId == tenantId && u.Id != userId);

            if (emailExists)
            {
                return ApiResponse<UserDto>.Fail("Email is already in use");
            }

            user.Email = request.Email;
            user.UserName = request.Email;
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        if (request.DefaultSiteId.HasValue)
            user.DefaultSiteId = request.DefaultSiteId;

        if (request.DefaultWarehouseId.HasValue)
            user.DefaultWarehouseId = request.DefaultWarehouseId;

        await _userManager.UpdateAsync(user);

        // Update role if provided
        if (!string.IsNullOrWhiteSpace(request.RoleName))
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (await _roleManager.RoleExistsAsync(request.RoleName))
            {
                await _userManager.AddToRoleAsync(user, request.RoleName);
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        var dto = new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleName = roles.FirstOrDefault(),
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            DefaultSiteId = user.DefaultSiteId,
            DefaultWarehouseId = user.DefaultWarehouseId
        };

        return ApiResponse<UserDto>.Ok(dto, "User updated successfully");
    }

    public async Task<ApiResponse> DeactivateUserAsync(Guid tenantId, Guid userId)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
        {
            return ApiResponse.Fail("User not found");
        }

        user.IsActive = false;
        await _userManager.UpdateAsync(user);

        return ApiResponse.Ok("User deactivated successfully");
    }

    public async Task<ApiResponse> ActivateUserAsync(Guid tenantId, Guid userId)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
        {
            return ApiResponse.Fail("User not found");
        }

        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        return ApiResponse.Ok("User activated successfully");
    }

    public async Task<ApiResponse> ResetPasswordAsync(Guid tenantId, Guid userId, ResetPasswordRequest request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
        {
            return ApiResponse.Fail("User not found");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
        {
            return ApiResponse.Fail("Failed to reset password", result.Errors.Select(e => e.Description).ToList());
        }

        return ApiResponse.Ok("Password reset successfully");
    }

    public async Task<ApiResponse> DeleteUserAsync(Guid tenantId, Guid userId)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
        {
            return ApiResponse.Fail("User not found");
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            return ApiResponse.Fail("Failed to delete user", result.Errors.Select(e => e.Description).ToList());
        }

        return ApiResponse.Ok("User deleted successfully");
    }

    #endregion

    #region Roles

    public async Task<ApiResponse<List<RoleDto>>> GetRolesAsync(Guid? tenantId = null)
    {
        var query = _roleManager.Roles.AsQueryable();

        // Filter by tenant or get system roles
        if (tenantId.HasValue)
        {
            query = query.Where(r => r.TenantId == null || r.TenantId == tenantId);
        }
        else
        {
            query = query.Where(r => r.TenantId == null);
        }

        var roles = await query.OrderBy(r => r.Name).ToListAsync();

        var roleDtos = new List<RoleDto>();
        foreach (var role in roles)
        {
            var userCount = await _userManager.GetUsersInRoleAsync(role.Name!);

            // Get permissions from role claims
            var permissions = await _dbContext.RoleClaims
                .Where(rc => rc.RoleId == role.Id && rc.ClaimType == "permission")
                .Select(rc => rc.ClaimValue!)
                .ToListAsync();

            roleDtos.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name!,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                UserCount = userCount.Count,
                Permissions = permissions
            });
        }

        return ApiResponse<List<RoleDto>>.Ok(roleDtos);
    }

    public async Task<ApiResponse<RoleDto>> GetRoleByIdAsync(Guid roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());

        if (role == null)
        {
            return ApiResponse<RoleDto>.Fail("Role not found");
        }

        var userCount = await _userManager.GetUsersInRoleAsync(role.Name!);

        // Get permissions from role claims
        var permissions = await _dbContext.RoleClaims
            .Where(rc => rc.RoleId == role.Id && rc.ClaimType == "permission")
            .Select(rc => rc.ClaimValue!)
            .ToListAsync();

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            UserCount = userCount.Count,
            Permissions = permissions
        };

        return ApiResponse<RoleDto>.Ok(dto);
    }

    public async Task<ApiResponse<RoleDto>> CreateRoleAsync(Guid? tenantId, RoleRequest request)
    {
        // Check if role already exists
        if (await _roleManager.RoleExistsAsync(request.Name))
        {
            return ApiResponse<RoleDto>.Fail("Role with this name already exists");
        }

        var role = new ApplicationRole(request.Name)
        {
            TenantId = tenantId,
            Description = request.Description,
            IsSystemRole = false
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            return ApiResponse<RoleDto>.Fail("Failed to create role", result.Errors.Select(e => e.Description).ToList());
        }

        // Add permissions as role claims
        if (request.Permissions != null && request.Permissions.Count > 0)
        {
            foreach (var permission in request.Permissions)
            {
                await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("permission", permission));
            }
        }

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            UserCount = 0,
            Permissions = request.Permissions ?? new List<string>()
        };

        return ApiResponse<RoleDto>.Ok(dto, "Role created successfully");
    }

    public async Task<ApiResponse<RoleDto>> UpdateRoleAsync(Guid roleId, RoleRequest request)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());

        if (role == null)
        {
            return ApiResponse<RoleDto>.Fail("Role not found");
        }

        if (role.IsSystemRole)
        {
            return ApiResponse<RoleDto>.Fail("System roles cannot be modified");
        }

        role.Name = request.Name;
        role.Description = request.Description;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            return ApiResponse<RoleDto>.Fail("Failed to update role", result.Errors.Select(e => e.Description).ToList());
        }

        // Update permissions: remove existing claims and add new ones
        var existingClaims = await _roleManager.GetClaimsAsync(role);
        var permissionClaims = existingClaims.Where(c => c.Type == "permission").ToList();

        foreach (var claim in permissionClaims)
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        if (request.Permissions != null && request.Permissions.Count > 0)
        {
            foreach (var permission in request.Permissions)
            {
                await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("permission", permission));
            }
        }

        var userCount = await _userManager.GetUsersInRoleAsync(role.Name!);

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            UserCount = userCount.Count,
            Permissions = request.Permissions ?? new List<string>()
        };

        return ApiResponse<RoleDto>.Ok(dto, "Role updated successfully");
    }

    public async Task<ApiResponse> DeleteRoleAsync(Guid roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());

        if (role == null)
        {
            return ApiResponse.Fail("Role not found");
        }

        if (role.IsSystemRole)
        {
            return ApiResponse.Fail("System roles cannot be deleted");
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Count > 0)
        {
            return ApiResponse.Fail("Cannot delete role that has assigned users");
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            return ApiResponse.Fail("Failed to delete role", result.Errors.Select(e => e.Description).ToList());
        }

        return ApiResponse.Ok("Role deleted successfully");
    }

    #endregion
}
