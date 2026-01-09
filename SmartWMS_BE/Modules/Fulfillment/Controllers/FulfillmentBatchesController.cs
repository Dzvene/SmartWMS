using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Fulfillment.DTOs;
using SmartWMS.API.Modules.Fulfillment.Services;

namespace SmartWMS.API.Modules.Fulfillment.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/fulfillment-batches")]
[Authorize]
public class FulfillmentBatchesController : ControllerBase
{
    private readonly IFulfillmentBatchesService _batchesService;

    public FulfillmentBatchesController(IFulfillmentBatchesService batchesService)
    {
        _batchesService = batchesService;
    }

    /// <summary>
    /// Get all fulfillment batches with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<FulfillmentBatchDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBatches(
        Guid tenantId,
        [FromQuery] FulfillmentBatchFilters? filters,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = await _batchesService.GetBatchesAsync(tenantId, filters, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get a fulfillment batch by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FulfillmentBatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatch(Guid tenantId, Guid id)
    {
        var result = await _batchesService.GetBatchByIdAsync(tenantId, id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get a fulfillment batch by batch number
    /// </summary>
    [HttpGet("by-number/{batchNumber}")]
    [ProducesResponseType(typeof(ApiResponse<FulfillmentBatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatchByNumber(Guid tenantId, string batchNumber)
    {
        var result = await _batchesService.GetBatchByNumberAsync(tenantId, batchNumber);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new fulfillment batch
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FulfillmentBatchDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBatch(Guid tenantId, [FromBody] CreateFulfillmentBatchRequest request)
    {
        var result = await _batchesService.CreateBatchAsync(tenantId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(
            nameof(GetBatch),
            new { tenantId, id = result.Data!.Id },
            result);
    }

    /// <summary>
    /// Update an existing fulfillment batch
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FulfillmentBatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBatch(Guid tenantId, Guid id, [FromBody] UpdateFulfillmentBatchRequest request)
    {
        var result = await _batchesService.UpdateBatchAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a fulfillment batch
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBatch(Guid tenantId, Guid id)
    {
        var result = await _batchesService.DeleteBatchAsync(tenantId, id);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Add orders to the fulfillment batch
    /// </summary>
    [HttpPost("{id:guid}/orders")]
    [ProducesResponseType(typeof(ApiResponse<FulfillmentBatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddOrders(Guid tenantId, Guid id, [FromBody] AddOrdersToBatchRequest request)
    {
        var result = await _batchesService.AddOrdersToBatchAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Remove an order from the fulfillment batch
    /// </summary>
    [HttpDelete("{id:guid}/orders/{orderId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveOrder(Guid tenantId, Guid id, Guid orderId)
    {
        var result = await _batchesService.RemoveOrderFromBatchAsync(tenantId, id, orderId);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Release the batch for fulfillment (creates pick tasks)
    /// </summary>
    [HttpPost("{id:guid}/release")]
    [ProducesResponseType(typeof(ApiResponse<FulfillmentBatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReleaseBatch(Guid tenantId, Guid id, [FromBody] ReleaseBatchRequest? request = null)
    {
        var result = await _batchesService.ReleaseBatchAsync(tenantId, id, request ?? new ReleaseBatchRequest());

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Complete the fulfillment batch
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse<FulfillmentBatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteBatch(Guid tenantId, Guid id)
    {
        var result = await _batchesService.CompleteBatchAsync(tenantId, id);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel the fulfillment batch
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<FulfillmentBatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelBatch(Guid tenantId, Guid id, [FromQuery] string? reason = null)
    {
        var result = await _batchesService.CancelBatchAsync(tenantId, id, reason);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }
}
