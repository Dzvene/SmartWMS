using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Equipment.DTOs;
using SmartWMS.API.Modules.Equipment.Models;
using SmartWMS.API.Modules.Equipment.Services;

namespace SmartWMS.API.Modules.Equipment.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/equipment")]
[Authorize]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _equipmentService;

    public EquipmentController(IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    /// <summary>
    /// Get all equipment with optional filters and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<EquipmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEquipment(
        Guid tenantId,
        [FromQuery] string? search = null,
        [FromQuery] EquipmentType? type = null,
        [FromQuery] EquipmentStatus? status = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? zoneId = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] bool? isAssigned = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filters = new EquipmentFilters
        {
            Search = search,
            Type = type,
            Status = status,
            WarehouseId = warehouseId,
            ZoneId = zoneId,
            AssignedToUserId = assignedToUserId,
            IsAssigned = isAssigned,
            IsActive = isActive
        };

        var result = await _equipmentService.GetEquipmentAsync(tenantId, filters, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get available equipment (not assigned, status = Available)
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EquipmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableEquipment(
        Guid tenantId,
        [FromQuery] EquipmentType? type = null,
        [FromQuery] Guid? warehouseId = null)
    {
        var result = await _equipmentService.GetAvailableEquipmentAsync(tenantId, type, warehouseId);
        return Ok(result);
    }

    /// <summary>
    /// Get equipment by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEquipmentById(Guid tenantId, Guid id)
    {
        var result = await _equipmentService.GetEquipmentByIdAsync(tenantId, id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get equipment by code
    /// </summary>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEquipmentByCode(Guid tenantId, string code)
    {
        var result = await _equipmentService.GetEquipmentByCodeAsync(tenantId, code);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create new equipment
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEquipment(Guid tenantId, [FromBody] CreateEquipmentRequest request)
    {
        var result = await _equipmentService.CreateEquipmentAsync(tenantId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(
            nameof(GetEquipmentById),
            new { tenantId, id = result.Data!.Id },
            result);
    }

    /// <summary>
    /// Update equipment
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEquipment(Guid tenantId, Guid id, [FromBody] UpdateEquipmentRequest request)
    {
        var result = await _equipmentService.UpdateEquipmentAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete equipment
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEquipment(Guid tenantId, Guid id)
    {
        var result = await _equipmentService.DeleteEquipmentAsync(tenantId, id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Assign equipment to a user or unassign (set userId to null)
    /// </summary>
    [HttpPut("{id:guid}/assign")]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignEquipment(Guid tenantId, Guid id, [FromBody] AssignEquipmentRequest request)
    {
        var result = await _equipmentService.AssignEquipmentAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update equipment status
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEquipmentStatus(Guid tenantId, Guid id, [FromBody] UpdateEquipmentStatusRequest request)
    {
        var result = await _equipmentService.UpdateEquipmentStatusAsync(tenantId, id, request);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
