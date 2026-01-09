using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Reports.DTOs;

namespace SmartWMS.API.Modules.Reports.Services;

public interface IReportsService
{
    /// <summary>
    /// Get inventory summary report
    /// </summary>
    Task<ApiResponse<InventorySummaryReport>> GetInventorySummaryAsync(
        Guid tenantId, Guid? warehouseId = null);

    /// <summary>
    /// Get stock movement report for a date range
    /// </summary>
    Task<ApiResponse<StockMovementReport>> GetStockMovementReportAsync(
        Guid tenantId, ReportDateRangeFilter filter);

    /// <summary>
    /// Get order fulfillment report for a date range
    /// </summary>
    Task<ApiResponse<OrderFulfillmentReport>> GetOrderFulfillmentReportAsync(
        Guid tenantId, ReportDateRangeFilter filter);

    /// <summary>
    /// Get receiving report for a date range
    /// </summary>
    Task<ApiResponse<ReceivingReport>> GetReceivingReportAsync(
        Guid tenantId, ReportDateRangeFilter filter);

    /// <summary>
    /// Get warehouse utilization report
    /// </summary>
    Task<ApiResponse<WarehouseUtilizationReport>> GetWarehouseUtilizationAsync(
        Guid tenantId, Guid warehouseId);
}
