using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Users.DTOs;
using SmartWMS.API.Modules.Users.Services;

namespace SmartWMS.API.Modules.Users.Controllers;

/// <summary>
/// Roles management endpoints
/// </summary>
[ApiController]
[Route("api/v1/tenant/{tenantId:guid}/roles")]
[Authorize]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly IUsersService _usersService;

    public RolesController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    /// <summary>
    /// Validates that URL tenantId matches the token tenantId
    /// </summary>
    private bool ValidateTenantAccess(Guid urlTenantId, out Guid tokenTenantId)
    {
        var tenantClaim = User.FindFirst("tenant_id")?.Value;
        tokenTenantId = Guid.TryParse(tenantClaim, out var parsed) ? parsed : Guid.Empty;
        return urlTenantId == tokenTenantId;
    }

    /// <summary>
    /// Get all roles (system + tenant-specific)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<RoleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoles(Guid tenantId)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _usersService.GetRolesAsync(tenantId);
        return Ok(result);
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRole(Guid tenantId, Guid id)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _usersService.GetRoleByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create new role
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateRole(Guid tenantId, [FromBody] RoleRequest request)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _usersService.CreateRoleAsync(tenantId, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetRole), new { tenantId, id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update role
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(Guid tenantId, Guid id, [FromBody] RoleRequest request)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _usersService.UpdateRoleAsync(id, request);
        if (!result.Success)
        {
            return result.Message == "Role not found" ? NotFound(result) : BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete role
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(Guid tenantId, Guid id)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _usersService.DeleteRoleAsync(id);
        if (!result.Success)
        {
            if (result.Message == "Role not found")
                return NotFound(result);
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get available permissions
    /// </summary>
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public IActionResult GetPermissions(Guid tenantId)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var permissions = new List<string>
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

        return Ok(ApiResponse<List<string>>.Ok(permissions));
    }
}
