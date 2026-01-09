using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Orders.DTOs;
using SmartWMS.API.Modules.Orders.Services;

namespace SmartWMS.API.Modules.Orders.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/sales-orders")]
[Authorize]
public class SalesOrdersController : ControllerBase
{
    private readonly ISalesOrdersService _salesOrdersService;

    public SalesOrdersController(ISalesOrdersService salesOrdersService)
    {
        _salesOrdersService = salesOrdersService;
    }

    #region Orders

    /// <summary>
    /// Get all sales orders with optional filters and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<SalesOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        Guid tenantId,
        [FromQuery] string? search = null,
        [FromQuery] SalesOrderStatus? status = null,
        [FromQuery] OrderPriority? priority = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTime? orderDateFrom = null,
        [FromQuery] DateTime? orderDateTo = null,
        [FromQuery] DateTime? requiredDateFrom = null,
        [FromQuery] DateTime? requiredDateTo = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filters = new SalesOrderFilters
        {
            Search = search,
            Status = status,
            Priority = priority,
            CustomerId = customerId,
            WarehouseId = warehouseId,
            OrderDateFrom = orderDateFrom,
            OrderDateTo = orderDateTo,
            RequiredDateFrom = requiredDateFrom,
            RequiredDateTo = requiredDateTo
        };

        var result = await _salesOrdersService.GetOrdersAsync(tenantId, filters, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get sales order by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid tenantId, Guid id, [FromQuery] bool includeLines = true)
    {
        var result = await _salesOrdersService.GetOrderByIdAsync(tenantId, id, includeLines);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get sales order by order number
    /// </summary>
    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderByNumber(Guid tenantId, string orderNumber, [FromQuery] bool includeLines = true)
    {
        var result = await _salesOrdersService.GetOrderByNumberAsync(tenantId, orderNumber, includeLines);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create new sales order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(Guid tenantId, [FromBody] CreateSalesOrderRequest request)
    {
        var result = await _salesOrdersService.CreateOrderAsync(tenantId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(
            nameof(GetOrderById),
            new { tenantId, id = result.Data!.Id },
            result);
    }

    /// <summary>
    /// Update sales order
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrder(Guid tenantId, Guid id, [FromBody] UpdateSalesOrderRequest request)
    {
        var result = await _salesOrdersService.UpdateOrderAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update sales order status
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(Guid tenantId, Guid id, [FromBody] UpdateSalesOrderStatusRequest request)
    {
        var result = await _salesOrdersService.UpdateOrderStatusAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete sales order (only Draft orders)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(Guid tenantId, Guid id)
    {
        var result = await _salesOrdersService.DeleteOrderAsync(tenantId, id);

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
    /// Add line to sales order
    /// </summary>
    [HttpPost("{id:guid}/lines")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderLineDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderLineDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderLineDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddLine(Guid tenantId, Guid id, [FromBody] AddSalesOrderLineRequest request)
    {
        var result = await _salesOrdersService.AddLineAsync(tenantId, id, request);

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
    /// Update sales order line
    /// </summary>
    [HttpPut("{id:guid}/lines/{lineId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderLineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderLineDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderLineDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLine(Guid tenantId, Guid id, Guid lineId, [FromBody] UpdateSalesOrderLineRequest request)
    {
        var result = await _salesOrdersService.UpdateLineAsync(tenantId, id, lineId, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Remove line from sales order
    /// </summary>
    [HttpDelete("{id:guid}/lines/{lineId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveLine(Guid tenantId, Guid id, Guid lineId)
    {
        var result = await _salesOrdersService.RemoveLineAsync(tenantId, id, lineId);

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
