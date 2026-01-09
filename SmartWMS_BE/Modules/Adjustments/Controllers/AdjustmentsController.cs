using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Adjustments.DTOs;
using SmartWMS.API.Modules.Adjustments.Models;
using SmartWMS.API.Modules.Adjustments.Services;
using System.Security.Claims;

namespace SmartWMS.API.Modules.Adjustments.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/adjustments")]
[Authorize]
public class AdjustmentsController : ControllerBase
{
    private readonly IAdjustmentsService _adjustmentsService;

    public AdjustmentsController(IAdjustmentsService adjustmentsService)
    {
        _adjustmentsService = adjustmentsService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    #region Stock Adjustments CRUD

    [HttpGet]
    public async Task<IActionResult> GetAdjustments(
        Guid tenantId,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] AdjustmentStatus? status = null,
        [FromQuery] AdjustmentType? adjustmentType = null,
        [FromQuery] Guid? reasonCodeId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] Guid? createdByUserId = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new AdjustmentFilterRequest(
            warehouseId, status, adjustmentType, reasonCodeId,
            dateFrom, dateTo, createdByUserId, searchTerm, page, pageSize);

        var result = await _adjustmentsService.GetAdjustmentsAsync(tenantId, filter);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAdjustmentById(Guid tenantId, Guid id)
    {
        var result = await _adjustmentsService.GetAdjustmentByIdAsync(tenantId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("by-number/{adjustmentNumber}")]
    public async Task<IActionResult> GetAdjustmentByNumber(Guid tenantId, string adjustmentNumber)
    {
        var result = await _adjustmentsService.GetAdjustmentByNumberAsync(tenantId, adjustmentNumber);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdjustment(
        Guid tenantId, [FromBody] CreateStockAdjustmentRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _adjustmentsService.CreateAdjustmentAsync(tenantId, userId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetAdjustmentById), new { tenantId, id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAdjustment(
        Guid tenantId, Guid id, [FromBody] UpdateStockAdjustmentRequest request)
    {
        var result = await _adjustmentsService.UpdateAdjustmentAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAdjustment(Guid tenantId, Guid id)
    {
        var result = await _adjustmentsService.DeleteAdjustmentAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Adjustment Lines

    [HttpPost("{adjustmentId}/lines")]
    public async Task<IActionResult> AddLine(
        Guid tenantId, Guid adjustmentId, [FromBody] AddAdjustmentLineRequest request)
    {
        var result = await _adjustmentsService.AddLineAsync(tenantId, adjustmentId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{adjustmentId}/lines/{lineId}")]
    public async Task<IActionResult> UpdateLine(
        Guid tenantId, Guid adjustmentId, Guid lineId, [FromBody] UpdateAdjustmentLineRequest request)
    {
        var result = await _adjustmentsService.UpdateLineAsync(tenantId, adjustmentId, lineId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{adjustmentId}/lines/{lineId}")]
    public async Task<IActionResult> RemoveLine(Guid tenantId, Guid adjustmentId, Guid lineId)
    {
        var result = await _adjustmentsService.RemoveLineAsync(tenantId, adjustmentId, lineId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Workflow Actions

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> SubmitForApproval(
        Guid tenantId, Guid id, [FromBody] SubmitForApprovalRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _adjustmentsService.SubmitForApprovalAsync(tenantId, id, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(
        Guid tenantId, Guid id, [FromBody] ApproveAdjustmentRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _adjustmentsService.ApproveAsync(tenantId, id, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(
        Guid tenantId, Guid id, [FromBody] RejectAdjustmentRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _adjustmentsService.RejectAsync(tenantId, id, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(
        Guid tenantId, Guid id, [FromBody] PostAdjustmentRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _adjustmentsService.PostAsync(tenantId, id, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid tenantId, Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _adjustmentsService.CancelAsync(tenantId, id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Quick & Auto Adjustments

    [HttpPost("quick")]
    public async Task<IActionResult> QuickAdjust(
        Guid tenantId, [FromBody] CreateStockAdjustmentRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _adjustmentsService.QuickAdjustAsync(tenantId, userId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetAdjustmentById), new { tenantId, id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPost("from-cycle-count/{cycleCountSessionId}")]
    public async Task<IActionResult> CreateFromCycleCount(Guid tenantId, Guid cycleCountSessionId)
    {
        var userId = GetCurrentUserId();
        var result = await _adjustmentsService.CreateFromCycleCountAsync(tenantId, userId, cycleCountSessionId);
        return result.Success
            ? CreatedAtAction(nameof(GetAdjustmentById), new { tenantId, id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    #endregion
}
