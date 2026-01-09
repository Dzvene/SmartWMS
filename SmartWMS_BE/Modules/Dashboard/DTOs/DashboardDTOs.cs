namespace SmartWMS.API.Modules.Dashboard.DTOs;

#region Overview

public record DashboardOverviewDto(
    OrdersOverviewDto Orders,
    InventoryOverviewDto Inventory,
    FulfillmentOverviewDto Fulfillment,
    WarehouseOverviewDto Warehouse
);

public record OrdersOverviewDto(
    int TotalSalesOrders,
    int PendingSalesOrders,
    int TotalPurchaseOrders,
    int PendingPurchaseOrders,
    int OrdersToday,
    int OrdersThisWeek,
    decimal TotalOrderValue
);

public record InventoryOverviewDto(
    int TotalProducts,
    int ActiveProducts,
    int LowStockProducts,
    int OutOfStockProducts,
    int TotalLocations,
    int OccupiedLocations,
    decimal TotalStockValue
);

public record FulfillmentOverviewDto(
    int PendingPickTasks,
    int InProgressPickTasks,
    int CompletedToday,
    int PendingPackTasks,
    int ShipmentsPending,
    int ShipmentsToday,
    double PickAccuracyRate
);

public record WarehouseOverviewDto(
    int TotalWarehouses,
    int TotalZones,
    int TotalLocations,
    double LocationUtilization,
    int ActiveEquipment,
    int MaintenanceNeeded
);

#endregion

#region KPI Metrics

public record KpiMetricsDto(
    List<KpiMetricDto> Metrics,
    DateTime Period,
    string PeriodType // "Day", "Week", "Month", "Year"
);

public record KpiMetricDto(
    string Code,
    string Name,
    string Category,
    decimal Value,
    decimal? Target,
    decimal? PreviousValue,
    string Unit,
    string? Trend, // "Up", "Down", "Stable"
    double? ChangePercent
);

public record KpiTrendDto(
    string Code,
    string Name,
    List<KpiDataPointDto> DataPoints,
    decimal? Target
);

public record KpiDataPointDto(
    DateTime Date,
    decimal Value
);

#endregion

#region Activity Feed

public record ActivityFeedDto(
    List<ActivityItemDto> Items,
    int TotalCount,
    DateTime? LastUpdated
);

public record ActivityItemDto(
    Guid Id,
    string Type,
    string Title,
    string? Description,
    string? EntityType,
    Guid? EntityId,
    string? UserName,
    DateTime CreatedAt,
    string? Icon,
    string? Color
);

#endregion

#region Charts Data

public record ChartDataDto(
    string ChartType,
    string Title,
    List<string> Labels,
    List<ChartSeriesDto> Series
);

public record ChartSeriesDto(
    string Name,
    List<decimal> Data,
    string? Color
);

public record OrdersTrendDto(
    List<string> Labels,
    List<decimal> SalesOrders,
    List<decimal> PurchaseOrders,
    List<decimal> Shipments
);

public record InventoryTrendDto(
    List<string> Labels,
    List<decimal> StockLevels,
    List<decimal> Movements,
    List<decimal> Adjustments
);

public record FulfillmentTrendDto(
    List<string> Labels,
    List<decimal> PickTasks,
    List<decimal> PackTasks,
    List<decimal> Shipments
);

#endregion

#region Alerts & Tasks

public record DashboardAlertsDto(
    List<AlertItemDto> Critical,
    List<AlertItemDto> Warning,
    List<AlertItemDto> Info,
    int TotalCount
);

public record AlertItemDto(
    string Type,
    string Severity, // "Critical", "Warning", "Info"
    string Title,
    string? Description,
    string? ActionUrl,
    DateTime CreatedAt
);

public record PendingTasksDto(
    List<TaskSummaryDto> PickTasks,
    List<TaskSummaryDto> PutawayTasks,
    List<TaskSummaryDto> PackTasks,
    List<TaskSummaryDto> CycleCountTasks,
    int TotalPending
);

public record TaskSummaryDto(
    Guid Id,
    string Type,
    string Status,
    string? Priority,
    string? AssignedTo,
    DateTime? DueDate,
    string? Location
);

#endregion

#region Quick Stats

public record QuickStatsDto(
    int OrdersToShip,
    int LowStockItems,
    int PendingReceipts,
    int OpenTasks,
    int ActiveUsers,
    decimal TodayRevenue
);

public record WarehouseStatsDto(
    Guid WarehouseId,
    string WarehouseName,
    int TotalLocations,
    int UsedLocations,
    double Utilization,
    int PendingTasks,
    int ActiveWorkers
);

#endregion

#region Query Parameters

public record DashboardQueryParams(
    Guid? WarehouseId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string Period = "Day" // "Day", "Week", "Month", "Year"
);

public record TrendQueryParams(
    int Days = 30,
    string Granularity = "Day" // "Hour", "Day", "Week", "Month"
);

#endregion
