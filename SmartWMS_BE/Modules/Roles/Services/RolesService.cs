using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Infrastructure.Identity;
using SmartWMS.API.Modules.Roles.DTOs;

namespace SmartWMS.API.Modules.Roles.Services;

public class RolesService : IRolesService
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RolesService(
        ApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    #region Roles CRUD

    public async Task<ApiResponse<List<RoleSummaryDto>>> GetRolesAsync(Guid tenantId)
    {
        var roles = await _context.Roles
            .Where(r => r.TenantId == tenantId || r.TenantId == null)
            .ToListAsync();

        var roleDtos = new List<RoleSummaryDto>();

        foreach (var role in roles)
        {
            var userCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
            roleDtos.Add(new RoleSummaryDto(
                role.Id,
                role.Name ?? "",
                role.Description,
                role.IsSystemRole,
                userCount
            ));
        }

        return ApiResponse<List<RoleSummaryDto>>.Ok(roleDtos);
    }

    public async Task<ApiResponse<RoleDto>> GetRoleByIdAsync(Guid tenantId, Guid roleId)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId && (r.TenantId == tenantId || r.TenantId == null));

        if (role == null)
            return ApiResponse<RoleDto>.Fail("Role not found");

        var userCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
        var permissions = await GetRoleClaimsAsync(role.Id);

        return ApiResponse<RoleDto>.Ok(new RoleDto(
            role.Id,
            role.Name ?? "",
            role.NormalizedName,
            role.Description,
            role.IsSystemRole,
            role.TenantId,
            role.CreatedAt,
            userCount,
            permissions
        ));
    }

    public async Task<ApiResponse<RoleDto>> GetRoleByNameAsync(Guid tenantId, string roleName)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == roleName && (r.TenantId == tenantId || r.TenantId == null));

        if (role == null)
            return ApiResponse<RoleDto>.Fail("Role not found");

        return await GetRoleByIdAsync(tenantId, role.Id);
    }

    public async Task<ApiResponse<RoleDto>> CreateRoleAsync(Guid tenantId, CreateRoleRequest request)
    {
        // Check if role name exists
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == request.Name && (r.TenantId == tenantId || r.TenantId == null));

        if (existingRole != null)
            return ApiResponse<RoleDto>.Fail("Role with this name already exists");

        var role = new ApplicationRole(request.Name)
        {
            TenantId = tenantId,
            Description = request.Description,
            IsSystemRole = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return ApiResponse<RoleDto>.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

        // Add permissions
        if (request.Permissions?.Any() == true)
        {
            foreach (var permission in request.Permissions)
            {
                await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("permission", permission));
            }
        }

        return await GetRoleByIdAsync(tenantId, role.Id);
    }

    public async Task<ApiResponse<RoleDto>> UpdateRoleAsync(Guid tenantId, Guid roleId, UpdateRoleRequest request)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId);

        if (role == null)
            return ApiResponse<RoleDto>.Fail("Role not found");

        if (role.IsSystemRole)
            return ApiResponse<RoleDto>.Fail("Cannot modify system roles");

        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != role.Name)
        {
            // Check if new name exists
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == request.Name && r.TenantId == tenantId && r.Id != roleId);

            if (existingRole != null)
                return ApiResponse<RoleDto>.Fail("Role with this name already exists");

            role.Name = request.Name;
            role.NormalizedName = request.Name.ToUpperInvariant();
        }

        if (request.Description != null)
            role.Description = request.Description;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            return ApiResponse<RoleDto>.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

        // Update permissions if provided
        if (request.Permissions != null)
        {
            // Remove existing permissions
            var existingClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in existingClaims.Where(c => c.Type == "permission"))
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            // Add new permissions
            foreach (var permission in request.Permissions)
            {
                await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("permission", permission));
            }
        }

        return await GetRoleByIdAsync(tenantId, roleId);
    }

    public async Task<ApiResponse<bool>> DeleteRoleAsync(Guid tenantId, Guid roleId)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId);

        if (role == null)
            return ApiResponse<bool>.Fail("Role not found");

        if (role.IsSystemRole)
            return ApiResponse<bool>.Fail("Cannot delete system roles");

        // Check if role has users
        var userCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == roleId);
        if (userCount > 0)
            return ApiResponse<bool>.Fail($"Cannot delete role with {userCount} assigned users");

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return ApiResponse<bool>.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

        return ApiResponse<bool>.Ok(true);
    }

    #endregion

    #region Role-User Assignments

    public async Task<ApiResponse<RoleWithUsersDto>> GetRoleWithUsersAsync(Guid tenantId, Guid roleId)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId && (r.TenantId == tenantId || r.TenantId == null));

        if (role == null)
            return ApiResponse<RoleWithUsersDto>.Fail("Role not found");

        var userIds = await _context.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync();

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new RoleUserDto(
                u.Id,
                u.UserName,
                u.Email,
                u.FullName
            ))
            .ToListAsync();

        return ApiResponse<RoleWithUsersDto>.Ok(new RoleWithUsersDto(
            role.Id,
            role.Name ?? "",
            role.Description,
            role.IsSystemRole,
            users
        ));
    }

    public async Task<ApiResponse<bool>> AssignUsersToRoleAsync(Guid tenantId, Guid roleId, AssignUsersToRoleRequest request)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId && (r.TenantId == tenantId || r.TenantId == null));

        if (role == null)
            return ApiResponse<bool>.Fail("Role not found");

        foreach (var userId in request.UserIds)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null && !await _userManager.IsInRoleAsync(user, role.Name!))
            {
                await _userManager.AddToRoleAsync(user, role.Name!);
            }
        }

        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<bool>> RemoveUserFromRoleAsync(Guid tenantId, Guid roleId, Guid userId)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId && (r.TenantId == tenantId || r.TenantId == null));

        if (role == null)
            return ApiResponse<bool>.Fail("Role not found");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ApiResponse<bool>.Fail("User not found");

        var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);
        if (!result.Succeeded)
            return ApiResponse<bool>.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<UserRolesDto>> GetUserRolesAsync(Guid tenantId, Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ApiResponse<UserRolesDto>.Fail("User not found");

        var roleNames = await _userManager.GetRolesAsync(user);
        var roles = await _context.Roles
            .Where(r => roleNames.Contains(r.Name!) && (r.TenantId == tenantId || r.TenantId == null))
            .ToListAsync();

        var roleDtos = new List<RoleSummaryDto>();
        foreach (var role in roles)
        {
            var userCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
            roleDtos.Add(new RoleSummaryDto(
                role.Id,
                role.Name ?? "",
                role.Description,
                role.IsSystemRole,
                userCount
            ));
        }

        return ApiResponse<UserRolesDto>.Ok(new UserRolesDto(
            user.Id,
            user.UserName,
            roleDtos
        ));
    }

    public async Task<ApiResponse<bool>> AssignRolesToUserAsync(Guid tenantId, Guid userId, AssignRolesToUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ApiResponse<bool>.Fail("User not found");

        // Get current roles
        var currentRoles = await _userManager.GetRolesAsync(user);

        // Get requested role names
        var requestedRoles = await _context.Roles
            .Where(r => request.RoleIds.Contains(r.Id) && (r.TenantId == tenantId || r.TenantId == null))
            .Select(r => r.Name!)
            .ToListAsync();

        // Remove roles not in request
        var rolesToRemove = currentRoles.Except(requestedRoles).ToList();
        if (rolesToRemove.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
        }

        // Add new roles
        var rolesToAdd = requestedRoles.Except(currentRoles).ToList();
        if (rolesToAdd.Any())
        {
            await _userManager.AddToRolesAsync(user, rolesToAdd);
        }

        return ApiResponse<bool>.Ok(true);
    }

    #endregion

    #region Permissions

    public Task<ApiResponse<List<PermissionGroupDto>>> GetAllPermissionsAsync()
    {
        var permissions = GetDefinedPermissions();
        return Task.FromResult(ApiResponse<List<PermissionGroupDto>>.Ok(permissions));
    }

    public async Task<ApiResponse<List<string>>> GetRolePermissionsAsync(Guid tenantId, Guid roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
            return ApiResponse<List<string>>.Fail("Role not found");

        var permissions = await GetRoleClaimsAsync(roleId);
        return ApiResponse<List<string>>.Ok(permissions);
    }

    public async Task<ApiResponse<bool>> AssignPermissionsToRoleAsync(Guid tenantId, Guid roleId, AssignPermissionsRequest request)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId);

        if (role == null)
            return ApiResponse<bool>.Fail("Role not found");

        if (role.IsSystemRole)
            return ApiResponse<bool>.Fail("Cannot modify permissions of system roles");

        // Remove existing permissions
        var existingClaims = await _roleManager.GetClaimsAsync(role);
        foreach (var claim in existingClaims.Where(c => c.Type == "permission"))
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        // Add new permissions
        foreach (var permission in request.Permissions)
        {
            await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("permission", permission));
        }

        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<List<string>>> GetUserPermissionsAsync(Guid tenantId, Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ApiResponse<List<string>>.Fail("User not found");

        var permissions = new HashSet<string>();

        // Get user's direct claims
        var userClaims = await _userManager.GetClaimsAsync(user);
        foreach (var claim in userClaims.Where(c => c.Type == "permission"))
        {
            permissions.Add(claim.Value);
        }

        // Get user's role-based permissions
        var roleNames = await _userManager.GetRolesAsync(user);
        foreach (var roleName in roleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in roleClaims.Where(c => c.Type == "permission"))
                {
                    permissions.Add(claim.Value);
                }
            }
        }

        return ApiResponse<List<string>>.Ok(permissions.ToList());
    }

    public async Task<ApiResponse<bool>> UserHasPermissionAsync(Guid tenantId, Guid userId, string permission)
    {
        var permissionsResult = await GetUserPermissionsAsync(tenantId, userId);
        if (!permissionsResult.Success)
            return ApiResponse<bool>.Fail(permissionsResult.Message ?? "Error getting permissions");

        var hasPermission = permissionsResult.Data?.Contains(permission) ?? false;
        return ApiResponse<bool>.Ok(hasPermission);
    }

    #endregion

    #region Private Helpers

    private async Task<List<string>> GetRoleClaimsAsync(Guid roleId)
    {
        var claims = await _context.RoleClaims
            .Where(rc => rc.RoleId == roleId && rc.ClaimType == "permission")
            .Select(rc => rc.ClaimValue!)
            .ToListAsync();

        return claims;
    }

    private List<PermissionGroupDto> GetDefinedPermissions()
    {
        return new List<PermissionGroupDto>
        {
            new("inventory", "Inventory", new List<PermissionCategoryDto>
            {
                new("products", "Products", new List<PermissionDto>
                {
                    new("inventory.products.view", "View Products", "View product catalog", "inventory", "products"),
                    new("inventory.products.create", "Create Products", "Add new products", "inventory", "products"),
                    new("inventory.products.edit", "Edit Products", "Modify product details", "inventory", "products"),
                    new("inventory.products.delete", "Delete Products", "Remove products", "inventory", "products"),
                }),
                new("stock", "Stock Levels", new List<PermissionDto>
                {
                    new("inventory.stock.view", "View Stock", "View stock levels", "inventory", "stock"),
                    new("inventory.stock.adjust", "Adjust Stock", "Make stock adjustments", "inventory", "stock"),
                    new("inventory.stock.transfer", "Transfer Stock", "Create stock transfers", "inventory", "stock"),
                }),
                new("cyclecount", "Cycle Count", new List<PermissionDto>
                {
                    new("inventory.cyclecount.view", "View Cycle Counts", "View cycle count sessions", "inventory", "cyclecount"),
                    new("inventory.cyclecount.create", "Create Cycle Count", "Start new cycle counts", "inventory", "cyclecount"),
                    new("inventory.cyclecount.count", "Perform Count", "Enter count results", "inventory", "cyclecount"),
                    new("inventory.cyclecount.approve", "Approve Variances", "Approve count variances", "inventory", "cyclecount"),
                }),
            }),
            new("orders", "Orders", new List<PermissionCategoryDto>
            {
                new("sales", "Sales Orders", new List<PermissionDto>
                {
                    new("orders.sales.view", "View Sales Orders", "View sales orders", "orders", "sales"),
                    new("orders.sales.create", "Create Sales Orders", "Create new sales orders", "orders", "sales"),
                    new("orders.sales.edit", "Edit Sales Orders", "Modify sales orders", "orders", "sales"),
                    new("orders.sales.cancel", "Cancel Sales Orders", "Cancel sales orders", "orders", "sales"),
                }),
                new("purchase", "Purchase Orders", new List<PermissionDto>
                {
                    new("orders.purchase.view", "View Purchase Orders", "View purchase orders", "orders", "purchase"),
                    new("orders.purchase.create", "Create Purchase Orders", "Create new purchase orders", "orders", "purchase"),
                    new("orders.purchase.edit", "Edit Purchase Orders", "Modify purchase orders", "orders", "purchase"),
                    new("orders.purchase.approve", "Approve Purchase Orders", "Approve purchase orders", "orders", "purchase"),
                }),
            }),
            new("warehouse", "Warehouse", new List<PermissionCategoryDto>
            {
                new("receiving", "Receiving", new List<PermissionDto>
                {
                    new("warehouse.receiving.view", "View Receipts", "View goods receipts", "warehouse", "receiving"),
                    new("warehouse.receiving.create", "Receive Goods", "Create goods receipts", "warehouse", "receiving"),
                    new("warehouse.receiving.complete", "Complete Receipts", "Complete receiving", "warehouse", "receiving"),
                }),
                new("picking", "Picking", new List<PermissionDto>
                {
                    new("warehouse.picking.view", "View Pick Tasks", "View pick tasks", "warehouse", "picking"),
                    new("warehouse.picking.pick", "Perform Picking", "Execute pick tasks", "warehouse", "picking"),
                    new("warehouse.picking.assign", "Assign Pick Tasks", "Assign tasks to pickers", "warehouse", "picking"),
                }),
                new("packing", "Packing", new List<PermissionDto>
                {
                    new("warehouse.packing.view", "View Packing", "View packing tasks", "warehouse", "packing"),
                    new("warehouse.packing.pack", "Perform Packing", "Execute packing", "warehouse", "packing"),
                }),
                new("shipping", "Shipping", new List<PermissionDto>
                {
                    new("warehouse.shipping.view", "View Shipments", "View shipments", "warehouse", "shipping"),
                    new("warehouse.shipping.create", "Create Shipments", "Create shipments", "warehouse", "shipping"),
                    new("warehouse.shipping.ship", "Ship Orders", "Mark as shipped", "warehouse", "shipping"),
                }),
            }),
            new("configuration", "Configuration", new List<PermissionCategoryDto>
            {
                new("users", "Users", new List<PermissionDto>
                {
                    new("config.users.view", "View Users", "View user list", "configuration", "users"),
                    new("config.users.create", "Create Users", "Add new users", "configuration", "users"),
                    new("config.users.edit", "Edit Users", "Modify user details", "configuration", "users"),
                    new("config.users.delete", "Delete Users", "Remove users", "configuration", "users"),
                }),
                new("roles", "Roles", new List<PermissionDto>
                {
                    new("config.roles.view", "View Roles", "View roles", "configuration", "roles"),
                    new("config.roles.create", "Create Roles", "Add new roles", "configuration", "roles"),
                    new("config.roles.edit", "Edit Roles", "Modify roles", "configuration", "roles"),
                    new("config.roles.delete", "Delete Roles", "Remove roles", "configuration", "roles"),
                    new("config.roles.assign", "Assign Roles", "Assign roles to users", "configuration", "roles"),
                }),
                new("warehouse", "Warehouse Setup", new List<PermissionDto>
                {
                    new("config.warehouse.view", "View Warehouse Config", "View warehouse configuration", "configuration", "warehouse"),
                    new("config.warehouse.edit", "Edit Warehouse Config", "Modify warehouse settings", "configuration", "warehouse"),
                }),
            }),
            new("reports", "Reports", new List<PermissionCategoryDto>
            {
                new("reports", "Reports", new List<PermissionDto>
                {
                    new("reports.inventory", "Inventory Reports", "View inventory reports", "reports", "reports"),
                    new("reports.orders", "Order Reports", "View order reports", "reports", "reports"),
                    new("reports.warehouse", "Warehouse Reports", "View warehouse reports", "reports", "reports"),
                    new("reports.audit", "Audit Reports", "View audit reports", "reports", "reports"),
                }),
            }),
        };
    }

    #endregion
}
