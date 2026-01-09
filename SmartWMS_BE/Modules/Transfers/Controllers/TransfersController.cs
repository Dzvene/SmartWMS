using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Transfers.DTOs;
using SmartWMS.API.Modules.Transfers.Models;
using SmartWMS.API.Modules.Transfers.Services;
using System.Security.Claims;

namespace SmartWMS.API.Modules.Transfers.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/transfers")]
[Authorize]
public class TransfersController : ControllerBase
{
    private readonly ITransfersService _transfersService;

    public TransfersController(ITransfersService transfersService)
    {
        _transfersService = transfersService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    #region Stock Transfers CRUD

    [HttpGet]
    public async Task<IActionResult> GetTransfers(
        Guid tenantId,
        [FromQuery] Guid? fromWarehouseId = null,
        [FromQuery] Guid? toWarehouseId = null,
        [FromQuery] TransferType? transferType = null,
        [FromQuery] TransferStatus? status = null,
        [FromQuery] TransferPriority? priority = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new TransferFilterRequest(
            fromWarehouseId, toWarehouseId, transferType, status, priority,
            assignedToUserId, dateFrom, dateTo, searchTerm, page, pageSize);

        var result = await _transfersService.GetTransfersAsync(tenantId, filter);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransferById(Guid tenantId, Guid id)
    {
        var result = await _transfersService.GetTransferByIdAsync(tenantId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("by-number/{transferNumber}")]
    public async Task<IActionResult> GetTransferByNumber(Guid tenantId, string transferNumber)
    {
        var result = await _transfersService.GetTransferByNumberAsync(tenantId, transferNumber);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransfer(
        Guid tenantId, [FromBody] CreateStockTransferRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _transfersService.CreateTransferAsync(tenantId, userId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetTransferById), new { tenantId, id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransfer(
        Guid tenantId, Guid id, [FromBody] UpdateStockTransferRequest request)
    {
        var result = await _transfersService.UpdateTransferAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransfer(Guid tenantId, Guid id)
    {
        var result = await _transfersService.DeleteTransferAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Transfer Lines

    [HttpPost("{transferId}/lines")]
    public async Task<IActionResult> AddLine(
        Guid tenantId, Guid transferId, [FromBody] AddTransferLineRequest request)
    {
        var result = await _transfersService.AddLineAsync(tenantId, transferId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{transferId}/lines/{lineId}")]
    public async Task<IActionResult> UpdateLine(
        Guid tenantId, Guid transferId, Guid lineId, [FromBody] UpdateTransferLineRequest request)
    {
        var result = await _transfersService.UpdateLineAsync(tenantId, transferId, lineId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{transferId}/lines/{lineId}")]
    public async Task<IActionResult> RemoveLine(Guid tenantId, Guid transferId, Guid lineId)
    {
        var result = await _transfersService.RemoveLineAsync(tenantId, transferId, lineId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Workflow Actions

    [HttpPost("{id}/release")]
    public async Task<IActionResult> Release(Guid tenantId, Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _transfersService.ReleaseAsync(tenantId, id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/assign")]
    public async Task<IActionResult> Assign(Guid tenantId, Guid id, [FromBody] AssignTransferRequest request)
    {
        var result = await _transfersService.AssignAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/start-picking")]
    public async Task<IActionResult> StartPicking(Guid tenantId, Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _transfersService.StartPickingAsync(tenantId, id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{transferId}/lines/{lineId}/pick")]
    public async Task<IActionResult> PickLine(
        Guid tenantId, Guid transferId, Guid lineId, [FromBody] PickLineRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _transfersService.PickLineAsync(tenantId, transferId, lineId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/complete-picking")]
    public async Task<IActionResult> CompletePicking(
        Guid tenantId, Guid id, [FromBody] CompletePickingRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _transfersService.CompletePickingAsync(tenantId, id, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/in-transit")]
    public async Task<IActionResult> MarkInTransit(Guid tenantId, Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _transfersService.MarkInTransitAsync(tenantId, id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{transferId}/lines/{lineId}/receive")]
    public async Task<IActionResult> ReceiveLine(
        Guid tenantId, Guid transferId, Guid lineId, [FromBody] ReceiveLineRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _transfersService.ReceiveLineAsync(tenantId, transferId, lineId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/complete-receiving")]
    public async Task<IActionResult> CompleteReceiving(
        Guid tenantId, Guid id, [FromBody] CompleteReceivingRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _transfersService.CompleteReceivingAsync(tenantId, id, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid tenantId, Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _transfersService.CancelAsync(tenantId, id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Replenishment & Quick Transfer

    [HttpPost("replenishment")]
    public async Task<IActionResult> CreateReplenishment(
        Guid tenantId, [FromBody] ReplenishmentRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _transfersService.CreateReplenishmentAsync(tenantId, userId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetTransferById), new { tenantId, id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPost("quick")]
    public async Task<IActionResult> QuickTransfer(
        Guid tenantId, [FromBody] CreateStockTransferRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _transfersService.QuickTransferAsync(tenantId, userId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetTransferById), new { tenantId, id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    #endregion
}
