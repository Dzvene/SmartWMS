using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Reports.DTOs;
using SmartWMS.API.Modules.Reports.Services;

namespace SmartWMS.API.Modules.Reports.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;

    public ReportsController(IReportsService reportsService)
    {
        _reportsService = reportsService;
    }

    /// <summary>
    /// Get inventory summary report
    /// </summary>
    [HttpGet("inventory-summary")]
    public async Task<IActionResult> GetInventorySummary(
        Guid tenantId,
        [FromQuery] Guid? warehouseId = null)
    {
        var result = await _reportsService.GetInventorySummaryAsync(tenantId, warehouseId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get stock movement report
    /// </summary>
    [HttpGet("stock-movements")]
    public async Task<IActionResult> GetStockMovementReport(
        Guid tenantId,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] Guid? warehouseId = null)
    {
        var filter = new ReportDateRangeFilter
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            WarehouseId = warehouseId
        };

        var result = await _reportsService.GetStockMovementReportAsync(tenantId, filter);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get order fulfillment report
    /// </summary>
    [HttpGet("order-fulfillment")]
    public async Task<IActionResult> GetOrderFulfillmentReport(
        Guid tenantId,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var filter = new ReportDateRangeFilter
        {
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        var result = await _reportsService.GetOrderFulfillmentReportAsync(tenantId, filter);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get receiving report
    /// </summary>
    [HttpGet("receiving")]
    public async Task<IActionResult> GetReceivingReport(
        Guid tenantId,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var filter = new ReportDateRangeFilter
        {
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        var result = await _reportsService.GetReceivingReportAsync(tenantId, filter);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get warehouse utilization report
    /// </summary>
    [HttpGet("warehouse-utilization/{warehouseId}")]
    public async Task<IActionResult> GetWarehouseUtilization(Guid tenantId, Guid warehouseId)
    {
        var result = await _reportsService.GetWarehouseUtilizationAsync(tenantId, warehouseId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
