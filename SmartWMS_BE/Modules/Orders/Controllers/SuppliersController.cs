using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Orders.DTOs;
using SmartWMS.API.Modules.Orders.Services;

namespace SmartWMS.API.Modules.Orders.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISuppliersService _suppliersService;

    public SuppliersController(ISuppliersService suppliersService)
    {
        _suppliersService = suppliersService;
    }

    /// <summary>
    /// Get all suppliers with optional filters and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<SupplierDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuppliers(
        Guid tenantId,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? countryCode = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filters = new SupplierFilters
        {
            Search = search,
            IsActive = isActive,
            CountryCode = countryCode
        };

        var result = await _suppliersService.GetSuppliersAsync(tenantId, filters, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get supplier by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplierById(Guid tenantId, Guid id)
    {
        var result = await _suppliersService.GetSupplierByIdAsync(tenantId, id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get supplier by code
    /// </summary>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplierByCode(Guid tenantId, string code)
    {
        var result = await _suppliersService.GetSupplierByCodeAsync(tenantId, code);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create new supplier
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSupplier(Guid tenantId, [FromBody] CreateSupplierRequest request)
    {
        var result = await _suppliersService.CreateSupplierAsync(tenantId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(
            nameof(GetSupplierById),
            new { tenantId, id = result.Data!.Id },
            result);
    }

    /// <summary>
    /// Update supplier
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplier(Guid tenantId, Guid id, [FromBody] UpdateSupplierRequest request)
    {
        var result = await _suppliersService.UpdateSupplierAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete supplier
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSupplier(Guid tenantId, Guid id)
    {
        var result = await _suppliersService.DeleteSupplierAsync(tenantId, id);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }
}
