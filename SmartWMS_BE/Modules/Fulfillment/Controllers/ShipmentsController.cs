using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Fulfillment.DTOs;
using SmartWMS.API.Modules.Fulfillment.Services;

namespace SmartWMS.API.Modules.Fulfillment.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/shipments")]
[Authorize]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentsService _shipmentsService;

    public ShipmentsController(IShipmentsService shipmentsService)
    {
        _shipmentsService = shipmentsService;
    }

    /// <summary>
    /// Get all shipments with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ShipmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShipments(
        Guid tenantId,
        [FromQuery] ShipmentFilters? filters,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = await _shipmentsService.GetShipmentsAsync(tenantId, filters, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get a shipment by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShipment(Guid tenantId, Guid id)
    {
        var result = await _shipmentsService.GetShipmentByIdAsync(tenantId, id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get a shipment by shipment number
    /// </summary>
    [HttpGet("by-number/{shipmentNumber}")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShipmentByNumber(Guid tenantId, string shipmentNumber)
    {
        var result = await _shipmentsService.GetShipmentByNumberAsync(tenantId, shipmentNumber);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new shipment
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateShipment(Guid tenantId, [FromBody] CreateShipmentRequest request)
    {
        var result = await _shipmentsService.CreateShipmentAsync(tenantId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(
            nameof(GetShipment),
            new { tenantId, id = result.Data!.Id },
            result);
    }

    /// <summary>
    /// Update an existing shipment
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateShipment(Guid tenantId, Guid id, [FromBody] UpdateShipmentRequest request)
    {
        var result = await _shipmentsService.UpdateShipmentAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a shipment
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteShipment(Guid tenantId, Guid id)
    {
        var result = await _shipmentsService.DeleteShipmentAsync(tenantId, id);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Add tracking information to a shipment
    /// </summary>
    [HttpPost("{id:guid}/tracking")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTracking(Guid tenantId, Guid id, [FromBody] AddTrackingRequest request)
    {
        var result = await _shipmentsService.AddTrackingAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update the shipping label
    /// </summary>
    [HttpPost("{id:guid}/label")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLabel(Guid tenantId, Guid id, [FromBody] UpdateLabelRequest request)
    {
        var result = await _shipmentsService.UpdateLabelAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Mark the shipment as packed
    /// </summary>
    [HttpPost("{id:guid}/pack")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsPacked(Guid tenantId, Guid id)
    {
        var result = await _shipmentsService.MarkAsPackedAsync(tenantId, id);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Ship the shipment (mark as in transit)
    /// </summary>
    [HttpPost("{id:guid}/ship")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ship(Guid tenantId, Guid id, [FromBody] ShipShipmentRequest? request = null)
    {
        var result = await _shipmentsService.ShipAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Mark the shipment as delivered
    /// </summary>
    [HttpPost("{id:guid}/deliver")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsDelivered(Guid tenantId, Guid id)
    {
        var result = await _shipmentsService.MarkAsDeliveredAsync(tenantId, id);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel a shipment
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelShipment(Guid tenantId, Guid id, [FromQuery] string? reason = null)
    {
        var result = await _shipmentsService.CancelShipmentAsync(tenantId, id, reason);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }
}
