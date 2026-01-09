using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Warehouse.DTOs;
using SmartWMS.API.Modules.Warehouse.Services;

namespace SmartWMS.API.Modules.Warehouse.Controllers;

/// <summary>
/// Zone management endpoints
/// </summary>
[ApiController]
[Route("api/v1/tenant/{tenantId:guid}/zones")]
[Authorize]
[Produces("application/json")]
public class ZonesController : ControllerBase
{
    private readonly IZonesService _zonesService;

    public ZonesController(IZonesService zonesService)
    {
        _zonesService = zonesService;
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
    /// Get paginated list of zones
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ZoneDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetZones(
        Guid tenantId,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _zonesService.GetZonesAsync(tenantId, warehouseId, page, pageSize, search, isActive);
        return Ok(result);
    }

    /// <summary>
    /// Get zone by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ZoneDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetZone(Guid tenantId, Guid id)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _zonesService.GetZoneByIdAsync(tenantId, id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create new zone
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ZoneDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateZone(Guid tenantId, [FromBody] CreateZoneRequest request)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _zonesService.CreateZoneAsync(tenantId, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetZone), new { tenantId, id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update zone
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ZoneDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateZone(Guid tenantId, Guid id, [FromBody] UpdateZoneRequest request)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _zonesService.UpdateZoneAsync(tenantId, id, request);
        if (!result.Success)
        {
            return result.Message == "Zone not found" ? NotFound(result) : BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete zone
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteZone(Guid tenantId, Guid id)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _zonesService.DeleteZoneAsync(tenantId, id);
        if (!result.Success)
        {
            if (result.Message == "Zone not found")
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }
        return Ok(result);
    }
}
