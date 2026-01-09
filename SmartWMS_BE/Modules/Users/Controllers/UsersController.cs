using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Users.DTOs;
using SmartWMS.API.Modules.Users.Services;

namespace SmartWMS.API.Modules.Users.Controllers;

/// <summary>
/// Users management endpoints
/// </summary>
[ApiController]
[Route("api/v1/tenant/{tenantId:guid}/users")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
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
    /// Get paginated list of users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers(
        Guid tenantId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _usersService.GetUsersAsync(tenantId, page, pageSize, search, isActive);
        return Ok(result);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(Guid tenantId, Guid id)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _usersService.GetUserByIdAsync(tenantId, id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCurrentUser(Guid tenantId)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var result = await _usersService.GetUserByIdAsync(tenantId, userId);
        return Ok(result);
    }

    /// <summary>
    /// Create new user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateUser(Guid tenantId, [FromBody] CreateUserRequest request)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _usersService.CreateUserAsync(tenantId, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetUser), new { tenantId, id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update user
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid tenantId, Guid id, [FromBody] UpdateUserRequest request)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _usersService.UpdateUserAsync(tenantId, id, request);
        if (!result.Success)
        {
            return result.Message == "User not found" ? NotFound(result) : BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Deactivate user
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateUser(Guid tenantId, Guid id)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _usersService.DeactivateUserAsync(tenantId, id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Activate user
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateUser(Guid tenantId, Guid id)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _usersService.ActivateUserAsync(tenantId, id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Reset user password
    /// </summary>
    [HttpPost("{id:guid}/reset-password")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(Guid tenantId, Guid id, [FromBody] ResetPasswordRequest request)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _usersService.ResetPasswordAsync(tenantId, id, request);
        if (!result.Success)
        {
            return result.Message == "User not found" ? NotFound(result) : BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete user
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid tenantId, Guid id)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        // Prevent self-deletion
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == id.ToString())
        {
            return BadRequest(ApiResponse.Fail("Cannot delete your own account"));
        }

        var result = await _usersService.DeleteUserAsync(tenantId, id);
        if (!result.Success)
        {
            return result.Message == "User not found" ? NotFound(result) : BadRequest(result);
        }
        return Ok(result);
    }
}
