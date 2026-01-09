using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Carriers.DTOs;
using SmartWMS.API.Modules.Carriers.Models;
using SmartWMS.API.Modules.Carriers.Services;

namespace SmartWMS.API.Modules.Carriers.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/carriers")]
[Authorize]
public class CarriersController : ControllerBase
{
    private readonly ICarriersService _carriersService;

    public CarriersController(ICarriersService carriersService)
    {
        _carriersService = carriersService;
    }

    #region Carriers

    /// <summary>
    /// Get paginated carriers
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCarriers(
        Guid tenantId,
        [FromQuery] CarrierIntegrationType? integrationType = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filters = new CarrierFilters
        {
            IntegrationType = integrationType,
            IsActive = isActive,
            Search = search
        };

        var result = await _carriersService.GetCarriersAsync(tenantId, filters, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get all active carriers (for dropdowns)
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveCarriers(Guid tenantId)
    {
        var result = await _carriersService.GetActiveCarriersAsync(tenantId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get carrier by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCarrierById(Guid tenantId, Guid id)
    {
        var result = await _carriersService.GetCarrierByIdAsync(tenantId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get carrier by code
    /// </summary>
    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetCarrierByCode(Guid tenantId, string code)
    {
        var result = await _carriersService.GetCarrierByCodeAsync(tenantId, code);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create carrier
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCarrier(Guid tenantId, [FromBody] CreateCarrierRequest request)
    {
        var result = await _carriersService.CreateCarrierAsync(tenantId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetCarrierById), new { tenantId, id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update carrier
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCarrier(
        Guid tenantId, Guid id, [FromBody] UpdateCarrierRequest request)
    {
        var result = await _carriersService.UpdateCarrierAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete carrier
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCarrier(Guid tenantId, Guid id)
    {
        var result = await _carriersService.DeleteCarrierAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Carrier Services

    /// <summary>
    /// Get carrier services
    /// </summary>
    [HttpGet("{carrierId}/services")]
    public async Task<IActionResult> GetCarrierServices(Guid tenantId, Guid carrierId)
    {
        var result = await _carriersService.GetCarrierServicesAsync(tenantId, carrierId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get carrier service by ID
    /// </summary>
    [HttpGet("{carrierId}/services/{serviceId}")]
    public async Task<IActionResult> GetCarrierServiceById(Guid tenantId, Guid carrierId, Guid serviceId)
    {
        var result = await _carriersService.GetCarrierServiceByIdAsync(tenantId, carrierId, serviceId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create carrier service
    /// </summary>
    [HttpPost("{carrierId}/services")]
    public async Task<IActionResult> CreateCarrierService(
        Guid tenantId, Guid carrierId, [FromBody] CreateCarrierServiceRequest request)
    {
        var result = await _carriersService.CreateCarrierServiceAsync(tenantId, carrierId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetCarrierServiceById),
                new { tenantId, carrierId, serviceId = result.Data!.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update carrier service
    /// </summary>
    [HttpPut("{carrierId}/services/{serviceId}")]
    public async Task<IActionResult> UpdateCarrierService(
        Guid tenantId, Guid carrierId, Guid serviceId, [FromBody] UpdateCarrierServiceRequest request)
    {
        var result = await _carriersService.UpdateCarrierServiceAsync(tenantId, carrierId, serviceId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete carrier service
    /// </summary>
    [HttpDelete("{carrierId}/services/{serviceId}")]
    public async Task<IActionResult> DeleteCarrierService(Guid tenantId, Guid carrierId, Guid serviceId)
    {
        var result = await _carriersService.DeleteCarrierServiceAsync(tenantId, carrierId, serviceId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
