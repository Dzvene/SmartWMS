using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Returns.DTOs;
using SmartWMS.API.Modules.Returns.Models;
using SmartWMS.API.Modules.Returns.Services;

namespace SmartWMS.API.Modules.Returns.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/returns")]
[Authorize]
public class ReturnsController : ControllerBase
{
    private readonly IReturnsService _returnsService;

    public ReturnsController(IReturnsService returnsService)
    {
        _returnsService = returnsService;
    }

    #region Queries

    /// <summary>
    /// Get paginated return orders with optional filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetReturnOrders(
        Guid tenantId,
        [FromQuery] ReturnOrderStatus? status = null,
        [FromQuery] ReturnType? returnType = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? originalSalesOrderId = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] string? rmaNumber = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filters = new ReturnOrderFilters
        {
            Status = status,
            ReturnType = returnType,
            CustomerId = customerId,
            OriginalSalesOrderId = originalSalesOrderId,
            AssignedToUserId = assignedToUserId,
            RmaNumber = rmaNumber,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _returnsService.GetReturnOrdersAsync(tenantId, filters, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get return order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReturnOrderById(Guid tenantId, Guid id)
    {
        var result = await _returnsService.GetReturnOrderByIdAsync(tenantId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get return order by number
    /// </summary>
    [HttpGet("by-number/{returnNumber}")]
    public async Task<IActionResult> GetReturnOrderByNumber(Guid tenantId, string returnNumber)
    {
        var result = await _returnsService.GetReturnOrderByNumberAsync(tenantId, returnNumber);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get my assigned returns
    /// </summary>
    [HttpGet("my-returns/{userId}")]
    public async Task<IActionResult> GetMyReturns(Guid tenantId, Guid userId)
    {
        var result = await _returnsService.GetMyReturnsAsync(tenantId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region CRUD

    /// <summary>
    /// Create new return order
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateReturnOrder(
        Guid tenantId, [FromBody] CreateReturnOrderRequest request)
    {
        var result = await _returnsService.CreateReturnOrderAsync(tenantId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetReturnOrderById), new { tenantId, id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update return order
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReturnOrder(
        Guid tenantId, Guid id, [FromBody] UpdateReturnOrderRequest request)
    {
        var result = await _returnsService.UpdateReturnOrderAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete return order
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReturnOrder(Guid tenantId, Guid id)
    {
        var result = await _returnsService.DeleteReturnOrderAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Lines

    /// <summary>
    /// Add line to return order
    /// </summary>
    [HttpPost("{returnOrderId}/lines")]
    public async Task<IActionResult> AddLine(
        Guid tenantId, Guid returnOrderId, [FromBody] AddReturnLineRequest request)
    {
        var result = await _returnsService.AddLineAsync(tenantId, returnOrderId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Remove line from return order
    /// </summary>
    [HttpDelete("{returnOrderId}/lines/{lineId}")]
    public async Task<IActionResult> RemoveLine(Guid tenantId, Guid returnOrderId, Guid lineId)
    {
        var result = await _returnsService.RemoveLineAsync(tenantId, returnOrderId, lineId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Workflow

    /// <summary>
    /// Assign return to user
    /// </summary>
    [HttpPost("{id}/assign")]
    public async Task<IActionResult> AssignReturn(
        Guid tenantId, Guid id, [FromBody] AssignReturnRequest request)
    {
        var result = await _returnsService.AssignReturnAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Mark return as in transit
    /// </summary>
    [HttpPost("{id}/in-transit")]
    public async Task<IActionResult> MarkInTransit(
        Guid tenantId, Guid id, [FromBody] SetReturnShippingRequest request)
    {
        var result = await _returnsService.MarkInTransitAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Start receiving return
    /// </summary>
    [HttpPost("{id}/start-receiving")]
    public async Task<IActionResult> StartReceiving(Guid tenantId, Guid id)
    {
        var result = await _returnsService.StartReceivingAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Receive line
    /// </summary>
    [HttpPost("{returnOrderId}/lines/{lineId}/receive")]
    public async Task<IActionResult> ReceiveLine(
        Guid tenantId, Guid returnOrderId, Guid lineId, [FromBody] ReceiveReturnLineRequest request)
    {
        var result = await _returnsService.ReceiveLineAsync(tenantId, returnOrderId, lineId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Process line (inspect and disposition)
    /// </summary>
    [HttpPost("{returnOrderId}/lines/{lineId}/process")]
    public async Task<IActionResult> ProcessLine(
        Guid tenantId, Guid returnOrderId, Guid lineId, [FromBody] ProcessReturnLineRequest request)
    {
        var result = await _returnsService.ProcessLineAsync(tenantId, returnOrderId, lineId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Complete return
    /// </summary>
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteReturn(Guid tenantId, Guid id)
    {
        var result = await _returnsService.CompleteReturnAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Cancel return
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelReturn(
        Guid tenantId, Guid id, [FromBody] string? reason = null)
    {
        var result = await _returnsService.CancelReturnAsync(tenantId, id, reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
