using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Receiving.DTOs;
using SmartWMS.API.Modules.Receiving.Services;

namespace SmartWMS.API.Modules.Receiving.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/goods-receipts")]
[Authorize]
public class GoodsReceiptsController : ControllerBase
{
    private readonly IGoodsReceiptService _goodsReceiptService;

    public GoodsReceiptsController(IGoodsReceiptService goodsReceiptService)
    {
        _goodsReceiptService = goodsReceiptService;
    }

    /// <summary>
    /// Get all goods receipts with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<GoodsReceiptDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReceipts(
        Guid tenantId,
        [FromQuery] GoodsReceiptFilters? filters,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = await _goodsReceiptService.GetReceiptsAsync(tenantId, filters, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get a goods receipt by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GoodsReceiptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReceipt(Guid tenantId, Guid id)
    {
        var result = await _goodsReceiptService.GetReceiptByIdAsync(tenantId, id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get a goods receipt by receipt number
    /// </summary>
    [HttpGet("by-number/{receiptNumber}")]
    [ProducesResponseType(typeof(ApiResponse<GoodsReceiptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReceiptByNumber(Guid tenantId, string receiptNumber)
    {
        var result = await _goodsReceiptService.GetReceiptByNumberAsync(tenantId, receiptNumber);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new goods receipt
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GoodsReceiptDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReceipt(Guid tenantId, [FromBody] CreateGoodsReceiptRequest request)
    {
        var result = await _goodsReceiptService.CreateReceiptAsync(tenantId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(
            nameof(GetReceipt),
            new { tenantId, id = result.Data!.Id },
            result);
    }

    /// <summary>
    /// Create a goods receipt from a purchase order
    /// </summary>
    [HttpPost("from-purchase-order/{purchaseOrderId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GoodsReceiptDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFromPurchaseOrder(
        Guid tenantId,
        Guid purchaseOrderId,
        [FromQuery] Guid warehouseId,
        [FromQuery] Guid? receivingLocationId = null)
    {
        var result = await _goodsReceiptService.CreateFromPurchaseOrderAsync(tenantId, purchaseOrderId, warehouseId, receivingLocationId);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(
            nameof(GetReceipt),
            new { tenantId, id = result.Data!.Id },
            result);
    }

    /// <summary>
    /// Update an existing goods receipt
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GoodsReceiptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReceipt(Guid tenantId, Guid id, [FromBody] UpdateGoodsReceiptRequest request)
    {
        var result = await _goodsReceiptService.UpdateReceiptAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a goods receipt
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReceipt(Guid tenantId, Guid id)
    {
        var result = await _goodsReceiptService.DeleteReceiptAsync(tenantId, id);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Add a line to an existing goods receipt
    /// </summary>
    [HttpPost("{id:guid}/lines")]
    [ProducesResponseType(typeof(ApiResponse<GoodsReceiptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddLine(Guid tenantId, Guid id, [FromBody] AddGoodsReceiptLineRequest request)
    {
        var result = await _goodsReceiptService.AddLineAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Remove a line from a goods receipt
    /// </summary>
    [HttpDelete("{id:guid}/lines/{lineId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveLine(Guid tenantId, Guid id, Guid lineId)
    {
        var result = await _goodsReceiptService.RemoveLineAsync(tenantId, id, lineId);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Start the receiving process
    /// </summary>
    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(typeof(ApiResponse<GoodsReceiptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartReceiving(Guid tenantId, Guid id)
    {
        var result = await _goodsReceiptService.StartReceivingAsync(tenantId, id);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Receive a line (record actual quantities received)
    /// </summary>
    [HttpPost("{id:guid}/lines/{lineId:guid}/receive")]
    [ProducesResponseType(typeof(ApiResponse<GoodsReceiptLineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReceiveLine(Guid tenantId, Guid id, Guid lineId, [FromBody] ReceiveLineRequest request)
    {
        var result = await _goodsReceiptService.ReceiveLineAsync(tenantId, id, lineId, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Complete the goods receipt
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse<GoodsReceiptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteReceipt(Guid tenantId, Guid id)
    {
        var result = await _goodsReceiptService.CompleteReceiptAsync(tenantId, id);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel a goods receipt
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<GoodsReceiptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelReceipt(Guid tenantId, Guid id, [FromQuery] string? reason = null)
    {
        var result = await _goodsReceiptService.CancelReceiptAsync(tenantId, id, reason);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }
}
