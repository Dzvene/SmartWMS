using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Dashboard.DTOs;

namespace SmartWMS.API.Modules.Dashboard.Services;

public interface IDashboardService
{
    // Overview
    Task<ApiResponse<DashboardOverviewDto>> GetOverviewAsync(Guid tenantId, DashboardQueryParams? query = null);
    Task<ApiResponse<QuickStatsDto>> GetQuickStatsAsync(Guid tenantId);

    // KPIs
    Task<ApiResponse<KpiMetricsDto>> GetKpiMetricsAsync(Guid tenantId, DashboardQueryParams? query = null);
    Task<ApiResponse<KpiTrendDto>> GetKpiTrendAsync(Guid tenantId, string kpiCode, TrendQueryParams? query = null);

    // Trends
    Task<ApiResponse<OrdersTrendDto>> GetOrdersTrendAsync(Guid tenantId, TrendQueryParams? query = null);
    Task<ApiResponse<InventoryTrendDto>> GetInventoryTrendAsync(Guid tenantId, TrendQueryParams? query = null);
    Task<ApiResponse<FulfillmentTrendDto>> GetFulfillmentTrendAsync(Guid tenantId, TrendQueryParams? query = null);

    // Activity & Alerts
    Task<ApiResponse<ActivityFeedDto>> GetActivityFeedAsync(Guid tenantId, int limit = 20);
    Task<ApiResponse<DashboardAlertsDto>> GetAlertsAsync(Guid tenantId);
    Task<ApiResponse<PendingTasksDto>> GetPendingTasksAsync(Guid tenantId, Guid? userId = null);

    // Warehouse Stats
    Task<ApiResponse<List<WarehouseStatsDto>>> GetWarehouseStatsAsync(Guid tenantId);
    Task<ApiResponse<WarehouseStatsDto>> GetWarehouseStatsByIdAsync(Guid tenantId, Guid warehouseId);
}
