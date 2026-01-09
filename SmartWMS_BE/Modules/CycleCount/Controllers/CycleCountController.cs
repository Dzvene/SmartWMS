using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.CycleCount.DTOs;
using SmartWMS.API.Modules.CycleCount.Models;
using SmartWMS.API.Modules.CycleCount.Services;

namespace SmartWMS.API.Modules.CycleCount.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/cycle-counts")]
[Authorize]
public class CycleCountController : ControllerBase
{
    private readonly ICycleCountService _cycleCountService;

    public CycleCountController(ICycleCountService cycleCountService)
    {
        _cycleCountService = cycleCountService;
    }

    #region Queries

    /// <summary>
    /// Get paginated cycle counts
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCycleCounts(
        Guid tenantId,
        [FromQuery] CycleCountStatus? status = null,
        [FromQuery] CountType? countType = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? zoneId = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filters = new CycleCountFilters
        {
            Status = status,
            CountType = countType,
            WarehouseId = warehouseId,
            ZoneId = zoneId,
            AssignedToUserId = assignedToUserId,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _cycleCountService.GetCycleCountsAsync(tenantId, filters, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get cycle count by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCycleCountById(Guid tenantId, Guid id)
    {
        var result = await _cycleCountService.GetCycleCountByIdAsync(tenantId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get my assigned cycle counts
    /// </summary>
    [HttpGet("my-counts/{userId}")]
    public async Task<IActionResult> GetMyCycleCounts(Guid tenantId, Guid userId)
    {
        var result = await _cycleCountService.GetMyCycleCountsAsync(tenantId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region CRUD

    /// <summary>
    /// Create cycle count
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCycleCount(
        Guid tenantId, [FromBody] CreateCycleCountRequest request)
    {
        var result = await _cycleCountService.CreateCycleCountAsync(tenantId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetCycleCountById), new { tenantId, id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update cycle count
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCycleCount(
        Guid tenantId, Guid id, [FromBody] UpdateCycleCountRequest request)
    {
        var result = await _cycleCountService.UpdateCycleCountAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete cycle count
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCycleCount(Guid tenantId, Guid id)
    {
        var result = await _cycleCountService.DeleteCycleCountAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Workflow

    /// <summary>
    /// Assign cycle count to user
    /// </summary>
    [HttpPost("{id}/assign")]
    public async Task<IActionResult> AssignCycleCount(
        Guid tenantId, Guid id, [FromBody] AssignCycleCountRequest request)
    {
        var result = await _cycleCountService.AssignCycleCountAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Schedule cycle count
    /// </summary>
    [HttpPost("{id}/schedule")]
    public async Task<IActionResult> ScheduleCycleCount(
        Guid tenantId, Guid id, [FromBody] DateTime scheduledDate)
    {
        var result = await _cycleCountService.ScheduleCycleCountAsync(tenantId, id, scheduledDate);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Start cycle count
    /// </summary>
    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartCycleCount(Guid tenantId, Guid id)
    {
        var result = await _cycleCountService.StartCycleCountAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Record count for item
    /// </summary>
    [HttpPost("{sessionId}/items/{itemId}/count")]
    public async Task<IActionResult> RecordCount(
        Guid tenantId, Guid sessionId, Guid itemId, [FromBody] RecordCountRequest request)
    {
        var result = await _cycleCountService.RecordCountAsync(tenantId, sessionId, itemId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Request recount for item
    /// </summary>
    [HttpPost("{sessionId}/items/{itemId}/recount")]
    public async Task<IActionResult> RequestRecount(
        Guid tenantId, Guid sessionId, Guid itemId, [FromBody] string? reason = null)
    {
        var result = await _cycleCountService.RequestRecountAsync(tenantId, sessionId, itemId, reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Approve variance for item
    /// </summary>
    [HttpPost("{sessionId}/items/{itemId}/approve")]
    public async Task<IActionResult> ApproveVariance(
        Guid tenantId, Guid sessionId, Guid itemId, [FromBody] ApproveVarianceRequest request)
    {
        var result = await _cycleCountService.ApproveVarianceAsync(tenantId, sessionId, itemId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Adjust stock for item
    /// </summary>
    [HttpPost("{sessionId}/items/{itemId}/adjust")]
    public async Task<IActionResult> AdjustStock(
        Guid tenantId, Guid sessionId, Guid itemId, [FromBody] AdjustStockRequest request)
    {
        var result = await _cycleCountService.AdjustStockAsync(tenantId, sessionId, itemId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Complete cycle count
    /// </summary>
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteCycleCount(Guid tenantId, Guid id)
    {
        var result = await _cycleCountService.CompleteCycleCountAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Cancel cycle count
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelCycleCount(
        Guid tenantId, Guid id, [FromBody] string? reason = null)
    {
        var result = await _cycleCountService.CancelCycleCountAsync(tenantId, id, reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
