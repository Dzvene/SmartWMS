using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Dashboard.DTOs;
using SmartWMS.API.Modules.Dashboard.Services;

namespace SmartWMS.API.Modules.Dashboard.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    #region Overview

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(Guid tenantId, [FromQuery] DashboardQueryParams? query = null)
    {
        var result = await _dashboardService.GetOverviewAsync(tenantId, query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("quick-stats")]
    public async Task<IActionResult> GetQuickStats(Guid tenantId)
    {
        var result = await _dashboardService.GetQuickStatsAsync(tenantId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region KPIs

    [HttpGet("kpi")]
    public async Task<IActionResult> GetKpiMetrics(Guid tenantId, [FromQuery] DashboardQueryParams? query = null)
    {
        var result = await _dashboardService.GetKpiMetricsAsync(tenantId, query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("kpi/{kpiCode}/trend")]
    public async Task<IActionResult> GetKpiTrend(
        Guid tenantId, string kpiCode, [FromQuery] TrendQueryParams? query = null)
    {
        var result = await _dashboardService.GetKpiTrendAsync(tenantId, kpiCode, query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Trends

    [HttpGet("trends/orders")]
    public async Task<IActionResult> GetOrdersTrend(Guid tenantId, [FromQuery] TrendQueryParams? query = null)
    {
        var result = await _dashboardService.GetOrdersTrendAsync(tenantId, query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("trends/inventory")]
    public async Task<IActionResult> GetInventoryTrend(Guid tenantId, [FromQuery] TrendQueryParams? query = null)
    {
        var result = await _dashboardService.GetInventoryTrendAsync(tenantId, query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("trends/fulfillment")]
    public async Task<IActionResult> GetFulfillmentTrend(Guid tenantId, [FromQuery] TrendQueryParams? query = null)
    {
        var result = await _dashboardService.GetFulfillmentTrendAsync(tenantId, query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Activity & Alerts

    [HttpGet("activity")]
    public async Task<IActionResult> GetActivityFeed(Guid tenantId, [FromQuery] int limit = 20)
    {
        var result = await _dashboardService.GetActivityFeedAsync(tenantId, limit);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts(Guid tenantId)
    {
        var result = await _dashboardService.GetAlertsAsync(tenantId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("tasks/pending")]
    public async Task<IActionResult> GetPendingTasks(Guid tenantId, [FromQuery] Guid? userId = null)
    {
        var result = await _dashboardService.GetPendingTasksAsync(tenantId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Warehouse Stats

    [HttpGet("warehouses/stats")]
    public async Task<IActionResult> GetWarehouseStats(Guid tenantId)
    {
        var result = await _dashboardService.GetWarehouseStatsAsync(tenantId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("warehouses/{warehouseId}/stats")]
    public async Task<IActionResult> GetWarehouseStatsById(Guid tenantId, Guid warehouseId)
    {
        var result = await _dashboardService.GetWarehouseStatsByIdAsync(tenantId, warehouseId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    #endregion
}
