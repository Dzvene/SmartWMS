using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Roles.DTOs;
using SmartWMS.API.Modules.Roles.Services;

namespace SmartWMS.API.Modules.Roles.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRolesService _rolesService;

    public RolesController(IRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    #region Roles CRUD

    [HttpGet]
    public async Task<IActionResult> GetRoles(Guid tenantId)
    {
        var result = await _rolesService.GetRolesAsync(tenantId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{roleId}")]
    public async Task<IActionResult> GetRoleById(Guid tenantId, Guid roleId)
    {
        var result = await _rolesService.GetRoleByIdAsync(tenantId, roleId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("by-name/{roleName}")]
    public async Task<IActionResult> GetRoleByName(Guid tenantId, string roleName)
    {
        var result = await _rolesService.GetRoleByNameAsync(tenantId, roleName);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(Guid tenantId, [FromBody] CreateRoleRequest request)
    {
        var result = await _rolesService.CreateRoleAsync(tenantId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetRoleById), new { tenantId, roleId = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("{roleId}")]
    public async Task<IActionResult> UpdateRole(Guid tenantId, Guid roleId, [FromBody] UpdateRoleRequest request)
    {
        var result = await _rolesService.UpdateRoleAsync(tenantId, roleId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{roleId}")]
    public async Task<IActionResult> DeleteRole(Guid tenantId, Guid roleId)
    {
        var result = await _rolesService.DeleteRoleAsync(tenantId, roleId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Role-User Assignments

    [HttpGet("{roleId}/users")]
    public async Task<IActionResult> GetRoleWithUsers(Guid tenantId, Guid roleId)
    {
        var result = await _rolesService.GetRoleWithUsersAsync(tenantId, roleId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{roleId}/users")]
    public async Task<IActionResult> AssignUsersToRole(
        Guid tenantId, Guid roleId, [FromBody] AssignUsersToRoleRequest request)
    {
        var result = await _rolesService.AssignUsersToRoleAsync(tenantId, roleId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{roleId}/users/{userId}")]
    public async Task<IActionResult> RemoveUserFromRole(Guid tenantId, Guid roleId, Guid userId)
    {
        var result = await _rolesService.RemoveUserFromRoleAsync(tenantId, roleId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserRoles(Guid tenantId, Guid userId)
    {
        var result = await _rolesService.GetUserRolesAsync(tenantId, userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("user/{userId}")]
    public async Task<IActionResult> AssignRolesToUser(
        Guid tenantId, Guid userId, [FromBody] AssignRolesToUserRequest request)
    {
        var result = await _rolesService.AssignRolesToUserAsync(tenantId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Permissions

    [HttpGet("permissions")]
    public async Task<IActionResult> GetAllPermissions()
    {
        var result = await _rolesService.GetAllPermissionsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{roleId}/permissions")]
    public async Task<IActionResult> GetRolePermissions(Guid tenantId, Guid roleId)
    {
        var result = await _rolesService.GetRolePermissionsAsync(tenantId, roleId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("{roleId}/permissions")]
    public async Task<IActionResult> AssignPermissionsToRole(
        Guid tenantId, Guid roleId, [FromBody] AssignPermissionsRequest request)
    {
        var result = await _rolesService.AssignPermissionsToRoleAsync(tenantId, roleId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("user/{userId}/permissions")]
    public async Task<IActionResult> GetUserPermissions(Guid tenantId, Guid userId)
    {
        var result = await _rolesService.GetUserPermissionsAsync(tenantId, userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("user/{userId}/permissions/{permission}")]
    public async Task<IActionResult> CheckUserPermission(Guid tenantId, Guid userId, string permission)
    {
        var result = await _rolesService.UserHasPermissionAsync(tenantId, userId, permission);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
