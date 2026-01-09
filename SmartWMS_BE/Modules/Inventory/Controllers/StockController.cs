using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Inventory.DTOs;
using SmartWMS.API.Modules.Inventory.Services;
using SmartWMS.API.Common.Enums;

namespace SmartWMS.API.Modules.Inventory.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/stock")]
[Authorize]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    #region Stock Levels

    /// <summary>
    /// Get paginated stock levels with optional filters
    /// </summary>
    [HttpGet("levels")]
    public async Task<IActionResult> GetStockLevels(
        Guid tenantId,
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? locationId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? zoneId = null,
        [FromQuery] string? sku = null,
        [FromQuery] string? batchNumber = null,
        [FromQuery] bool? hasAvailableStock = null,
        [FromQuery] bool? isExpiringSoon = null,
        [FromQuery] int? expiringWithinDays = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filters = new StockLevelFilters
        {
            ProductId = productId,
            LocationId = locationId,
            WarehouseId = warehouseId,
            ZoneId = zoneId,
            Sku = sku,
            BatchNumber = batchNumber,
            HasAvailableStock = hasAvailableStock,
            IsExpiringSoon = isExpiringSoon,
            ExpiringWithinDays = expiringWithinDays
        };

        var result = await _stockService.GetStockLevelsAsync(tenantId, filters, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get stock level by ID
    /// </summary>
    [HttpGet("levels/{id}")]
    public async Task<IActionResult> GetStockLevelById(Guid tenantId, Guid id)
    {
        var result = await _stockService.GetStockLevelByIdAsync(tenantId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get aggregated stock summary for a product
    /// </summary>
    [HttpGet("products/{productId}/summary")]
    public async Task<IActionResult> GetProductStockSummary(Guid tenantId, Guid productId)
    {
        var result = await _stockService.GetProductStockSummaryAsync(tenantId, productId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get products with low stock
    /// </summary>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockProducts(Guid tenantId, [FromQuery] Guid? warehouseId = null)
    {
        var result = await _stockService.GetLowStockProductsAsync(tenantId, warehouseId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get available quantity for a product
    /// </summary>
    [HttpGet("products/{productId}/available")]
    public async Task<IActionResult> GetAvailableQuantity(
        Guid tenantId,
        Guid productId,
        [FromQuery] Guid? locationId = null,
        [FromQuery] string? batchNumber = null)
    {
        var result = await _stockService.GetAvailableQuantityAsync(tenantId, productId, locationId, batchNumber);
        return result.Success ? Ok(result) : NotFound(result);
    }

    #endregion

    #region Stock Movements

    /// <summary>
    /// Get paginated stock movements with optional filters
    /// </summary>
    [HttpGet("movements")]
    public async Task<IActionResult> GetStockMovements(
        Guid tenantId,
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? locationId = null,
        [FromQuery] MovementType? movementType = null,
        [FromQuery] string? referenceType = null,
        [FromQuery] Guid? referenceId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filters = new StockMovementFilters
        {
            ProductId = productId,
            LocationId = locationId,
            MovementType = movementType,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        var result = await _stockService.GetStockMovementsAsync(tenantId, filters, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get movement history for a product
    /// </summary>
    [HttpGet("products/{productId}/movements")]
    public async Task<IActionResult> GetProductMovementHistory(
        Guid tenantId,
        Guid productId,
        [FromQuery] int limit = 50)
    {
        var result = await _stockService.GetProductMovementHistoryAsync(tenantId, productId, limit);
        return result.Success ? Ok(result) : NotFound(result);
    }

    #endregion

    #region Stock Operations

    /// <summary>
    /// Receive stock (goods receipt)
    /// </summary>
    [HttpPost("receive")]
    public async Task<IActionResult> ReceiveStock(Guid tenantId, [FromBody] ReceiveStockRequest request)
    {
        var result = await _stockService.ReceiveStockAsync(tenantId, request);
        return result.Success ? CreatedAtAction(nameof(GetStockMovements), new { tenantId }, result) : BadRequest(result);
    }

    /// <summary>
    /// Issue/pick stock (for sales orders)
    /// </summary>
    [HttpPost("issue")]
    public async Task<IActionResult> IssueStock(Guid tenantId, [FromBody] IssueStockRequest request)
    {
        var result = await _stockService.IssueStockAsync(tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Transfer stock between locations
    /// </summary>
    [HttpPost("transfer")]
    public async Task<IActionResult> TransferStock(Guid tenantId, [FromBody] TransferStockRequest request)
    {
        var result = await _stockService.TransferStockAsync(tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Adjust stock (inventory count adjustment)
    /// </summary>
    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock(Guid tenantId, [FromBody] AdjustStockRequest request)
    {
        var result = await _stockService.AdjustStockAsync(tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Reservations

    /// <summary>
    /// Reserve stock for an order
    /// </summary>
    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveStock(Guid tenantId, [FromBody] ReserveStockRequest request)
    {
        var result = await _stockService.ReserveStockAsync(tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Release reserved stock
    /// </summary>
    [HttpPost("release-reservation")]
    public async Task<IActionResult> ReleaseReservation(Guid tenantId, [FromBody] ReleaseReservationRequest request)
    {
        var result = await _stockService.ReleaseReservationAsync(tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
