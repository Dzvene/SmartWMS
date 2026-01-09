using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Orders.DTOs;
using SmartWMS.API.Modules.Orders.Services;

namespace SmartWMS.API.Modules.Orders.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/purchase-orders")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrdersService _purchaseOrdersService;

    public PurchaseOrdersController(IPurchaseOrdersService purchaseOrdersService)
    {
        _purchaseOrdersService = purchaseOrdersService;
    }

    #region Orders

    /// <summary>
    /// Get all purchase orders with optional filters and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PurchaseOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        Guid tenantId,
        [FromQuery] string? search = null,
        [FromQuery] PurchaseOrderStatus? status = null,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTime? orderDateFrom = null,
        [FromQuery] DateTime? orderDateTo = null,
        [FromQuery] DateTime? expectedDateFrom = null,
        [FromQuery] DateTime? expectedDateTo = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filters = new PurchaseOrderFilters
        {
            Search = search,
            Status = status,
            SupplierId = supplierId,
            WarehouseId = warehouseId,
            OrderDateFrom = orderDateFrom,
            OrderDateTo = orderDateTo,
            ExpectedDateFrom = expectedDateFrom,
            ExpectedDateTo = expectedDateTo
        };

        var result = await _purchaseOrdersService.GetOrdersAsync(tenantId, filters, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get purchase order by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid tenantId, Guid id, [FromQuery] bool includeLines = true)
    {
        var result = await _purchaseOrdersService.GetOrderByIdAsync(tenantId, id, includeLines);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get purchase order by order number
    /// </summary>
    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderByNumber(Guid tenantId, string orderNumber, [FromQuery] bool includeLines = true)
    {
        var result = await _purchaseOrdersService.GetOrderByNumberAsync(tenantId, orderNumber, includeLines);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create new purchase order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(Guid tenantId, [FromBody] CreatePurchaseOrderRequest request)
    {
        var result = await _purchaseOrdersService.CreateOrderAsync(tenantId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(
            nameof(GetOrderById),
            new { tenantId, id = result.Data!.Id },
            result);
    }

    /// <summary>
    /// Update purchase order
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrder(Guid tenantId, Guid id, [FromBody] UpdatePurchaseOrderRequest request)
    {
        var result = await _purchaseOrdersService.UpdateOrderAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update purchase order status
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(Guid tenantId, Guid id, [FromBody] UpdatePurchaseOrderStatusRequest request)
    {
        var result = await _purchaseOrdersService.UpdateOrderStatusAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete purchase order (only Draft orders)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(Guid tenantId, Guid id)
    {
        var result = await _purchaseOrdersService.DeleteOrderAsync(tenantId, id);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    #endregion

    #region Order Lines

    /// <summary>
    /// Add line to purchase order
    /// </summary>
    [HttpPost("{id:guid}/lines")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderLineDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderLineDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderLineDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddLine(Guid tenantId, Guid id, [FromBody] AddPurchaseOrderLineRequest request)
    {
        var result = await _purchaseOrdersService.AddLineAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetOrderById),
            new { tenantId, id },
            result);
    }

    /// <summary>
    /// Update purchase order line
    /// </summary>
    [HttpPut("{id:guid}/lines/{lineId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderLineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderLineDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderLineDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLine(Guid tenantId, Guid id, Guid lineId, [FromBody] UpdatePurchaseOrderLineRequest request)
    {
        var result = await _purchaseOrdersService.UpdateLineAsync(tenantId, id, lineId, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Remove line from purchase order
    /// </summary>
    [HttpDelete("{id:guid}/lines/{lineId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveLine(Guid tenantId, Guid id, Guid lineId)
    {
        var result = await _purchaseOrdersService.RemoveLineAsync(tenantId, id, lineId);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Receive items on a purchase order line
    /// </summary>
    [HttpPost("{id:guid}/lines/{lineId:guid}/receive")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderLineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderLineDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderLineDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReceiveLine(Guid tenantId, Guid id, Guid lineId, [FromBody] ReceivePurchaseOrderLineRequest request)
    {
        var result = await _purchaseOrdersService.ReceiveLineAsync(tenantId, id, lineId, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    #endregion
}
