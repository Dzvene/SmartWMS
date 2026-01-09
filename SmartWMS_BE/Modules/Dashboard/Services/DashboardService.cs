using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Dashboard.DTOs;
using SmartWMS.API.Common.Enums;

namespace SmartWMS.API.Modules.Dashboard.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Overview

    public async Task<ApiResponse<DashboardOverviewDto>> GetOverviewAsync(
        Guid tenantId, DashboardQueryParams? query = null)
    {
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);

        // Orders
        var salesOrders = await _context.SalesOrders
            .Where(o => o.TenantId == tenantId)
            .ToListAsync();

        var purchaseOrders = await _context.PurchaseOrders
            .Where(o => o.TenantId == tenantId)
            .ToListAsync();

        var ordersOverview = new OrdersOverviewDto(
            TotalSalesOrders: salesOrders.Count,
            PendingSalesOrders: salesOrders.Count(o => o.Status == SalesOrderStatus.Pending ||
                                                       o.Status == SalesOrderStatus.Confirmed),
            TotalPurchaseOrders: purchaseOrders.Count,
            PendingPurchaseOrders: purchaseOrders.Count(o => o.Status == PurchaseOrderStatus.Draft ||
                                                            o.Status == PurchaseOrderStatus.Confirmed),
            OrdersToday: salesOrders.Count(o => o.CreatedAt >= today),
            OrdersThisWeek: salesOrders.Count(o => o.CreatedAt >= weekStart),
            TotalOrderValue: salesOrders.Sum(o => o.TotalQuantity) // Using quantity as placeholder
        );

        // Inventory
        var products = await _context.Products
            .Where(p => p.TenantId == tenantId)
            .ToListAsync();

        var stockLevels = await _context.StockLevels
            .Where(s => s.TenantId == tenantId)
            .ToListAsync();

        var locations = await _context.Locations
            .Where(l => l.TenantId == tenantId)
            .ToListAsync();

        var inventoryOverview = new InventoryOverviewDto(
            TotalProducts: products.Count,
            ActiveProducts: products.Count(p => p.IsActive),
            LowStockProducts: products.Count(p => stockLevels
                .Where(s => s.ProductId == p.Id).Sum(s => s.QuantityOnHand) <= p.ReorderPoint),
            OutOfStockProducts: products.Count(p => !stockLevels.Any(s => s.ProductId == p.Id && s.QuantityOnHand > 0)),
            TotalLocations: locations.Count,
            OccupiedLocations: stockLevels.Select(s => s.LocationId).Distinct().Count(),
            TotalStockValue: stockLevels.Sum(s => s.QuantityOnHand) // Simplified - no price data in product model
        );

        // Fulfillment
        var pickTasks = await _context.PickTasks
            .Where(t => t.TenantId == tenantId)
            .ToListAsync();

        var packTasks = await _context.PackingTasks
            .Where(t => t.TenantId == tenantId)
            .ToListAsync();

        var shipments = await _context.Shipments
            .Where(s => s.TenantId == tenantId)
            .ToListAsync();

        var completedPicks = pickTasks.Where(t => t.Status == PickTaskStatus.Complete).ToList();

        var fulfillmentOverview = new FulfillmentOverviewDto(
            PendingPickTasks: pickTasks.Count(t => t.Status == PickTaskStatus.Pending),
            InProgressPickTasks: pickTasks.Count(t => t.Status == PickTaskStatus.InProgress),
            CompletedToday: pickTasks.Count(t => t.Status == PickTaskStatus.Complete && t.CompletedAt >= today),
            PendingPackTasks: packTasks.Count(t => t.Status == Modules.Packing.Models.PackingTaskStatus.Pending),
            ShipmentsPending: shipments.Count(s => s.Status == ShipmentStatus.Created),
            ShipmentsToday: shipments.Count(s => s.ShippedAt >= today),
            PickAccuracyRate: completedPicks.Count > 0
                ? completedPicks.Average(t => t.QuantityPicked == t.QuantityRequired ? 100.0 : 0.0)
                : 100.0
        );

        // Warehouse
        var warehouses = await _context.Warehouses
            .Where(w => w.TenantId == tenantId)
            .ToListAsync();

        var zones = await _context.Zones
            .Where(z => z.TenantId == tenantId)
            .ToListAsync();

        var equipment = await _context.Equipment
            .Where(e => e.TenantId == tenantId)
            .ToListAsync();

        var warehouseOverview = new WarehouseOverviewDto(
            TotalWarehouses: warehouses.Count,
            TotalZones: zones.Count,
            TotalLocations: locations.Count,
            LocationUtilization: locations.Count > 0
                ? (double)stockLevels.Select(s => s.LocationId).Distinct().Count() / locations.Count * 100
                : 0,
            ActiveEquipment: equipment.Count(e => e.IsActive),
            MaintenanceNeeded: 0 // Equipment model doesn't have MaintenanceDueDate
        );

        var overview = new DashboardOverviewDto(
            Orders: ordersOverview,
            Inventory: inventoryOverview,
            Fulfillment: fulfillmentOverview,
            Warehouse: warehouseOverview
        );

        return ApiResponse<DashboardOverviewDto>.Ok(overview);
    }

    public async Task<ApiResponse<QuickStatsDto>> GetQuickStatsAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow.Date;

        var ordersToShip = await _context.SalesOrders
            .CountAsync(o => o.TenantId == tenantId &&
                            (o.Status == SalesOrderStatus.Confirmed || o.Status == SalesOrderStatus.Allocated));

        var products = await _context.Products
            .Where(p => p.TenantId == tenantId)
            .ToListAsync();

        var stockLevels = await _context.StockLevels
            .Where(s => s.TenantId == tenantId)
            .ToListAsync();

        var lowStockItems = products.Count(p => stockLevels
            .Where(s => s.ProductId == p.Id).Sum(s => s.QuantityOnHand) <= p.ReorderPoint);

        var pendingReceipts = await _context.GoodsReceipts
            .CountAsync(r => r.TenantId == tenantId &&
                            r.Status == Modules.Receiving.Models.GoodsReceiptStatus.InProgress);

        var openTasks = await _context.PickTasks
            .CountAsync(t => t.TenantId == tenantId &&
                            t.Status != PickTaskStatus.Complete &&
                            t.Status != PickTaskStatus.Cancelled);

        openTasks += await _context.PutawayTasks
            .CountAsync(t => t.TenantId == tenantId &&
                            t.Status != Modules.Putaway.Models.PutawayTaskStatus.Complete &&
                            t.Status != Modules.Putaway.Models.PutawayTaskStatus.Cancelled);

        var todayRevenue = await _context.SalesOrders
            .Where(o => o.TenantId == tenantId && o.CreatedAt >= today)
            .SumAsync(o => o.TotalQuantity);

        var stats = new QuickStatsDto(
            OrdersToShip: ordersToShip,
            LowStockItems: lowStockItems,
            PendingReceipts: pendingReceipts,
            OpenTasks: openTasks,
            ActiveUsers: 0,
            TodayRevenue: todayRevenue
        );

        return ApiResponse<QuickStatsDto>.Ok(stats);
    }

    #endregion

    #region KPIs

    public async Task<ApiResponse<KpiMetricsDto>> GetKpiMetricsAsync(
        Guid tenantId, DashboardQueryParams? query = null)
    {
        var period = query?.Period ?? "Day";
        var today = DateTime.UtcNow.Date;

        var metrics = new List<KpiMetricDto>();

        // Order Fulfillment Rate
        var completedOrders = await _context.SalesOrders
            .CountAsync(o => o.TenantId == tenantId && o.Status == SalesOrderStatus.Shipped);
        var totalOrders = await _context.SalesOrders
            .CountAsync(o => o.TenantId == tenantId);

        metrics.Add(new KpiMetricDto(
            Code: "order_fulfillment_rate",
            Name: "Order Fulfillment Rate",
            Category: "Orders",
            Value: totalOrders > 0 ? (decimal)completedOrders / totalOrders * 100 : 0,
            Target: 95,
            PreviousValue: null,
            Unit: "%",
            Trend: null,
            ChangePercent: null
        ));

        // Pick Accuracy
        var pickTasks = await _context.PickTasks
            .Where(t => t.TenantId == tenantId && t.Status == PickTaskStatus.Complete)
            .ToListAsync();

        var accuratePicks = pickTasks.Count(t => t.QuantityPicked == t.QuantityRequired);
        metrics.Add(new KpiMetricDto(
            Code: "pick_accuracy",
            Name: "Pick Accuracy",
            Category: "Fulfillment",
            Value: pickTasks.Count > 0 ? (decimal)accuratePicks / pickTasks.Count * 100 : 100,
            Target: 99.5m,
            PreviousValue: null,
            Unit: "%",
            Trend: null,
            ChangePercent: null
        ));

        // Inventory Turnover (simplified)
        var totalStock = await _context.StockLevels
            .Where(s => s.TenantId == tenantId)
            .SumAsync(s => s.QuantityOnHand);

        metrics.Add(new KpiMetricDto(
            Code: "inventory_turnover",
            Name: "Inventory Turnover",
            Category: "Inventory",
            Value: totalStock > 0 ? Math.Round(completedOrders / totalStock * 12, 2) : 0,
            Target: 12,
            PreviousValue: null,
            Unit: "x/year",
            Trend: null,
            ChangePercent: null
        ));

        // Location Utilization
        var totalLocations = await _context.Locations.CountAsync(l => l.TenantId == tenantId);
        var usedLocations = await _context.StockLevels
            .Where(s => s.TenantId == tenantId && s.QuantityOnHand > 0)
            .Select(s => s.LocationId)
            .Distinct()
            .CountAsync();

        metrics.Add(new KpiMetricDto(
            Code: "location_utilization",
            Name: "Location Utilization",
            Category: "Warehouse",
            Value: totalLocations > 0 ? (decimal)usedLocations / totalLocations * 100 : 0,
            Target: 80,
            PreviousValue: null,
            Unit: "%",
            Trend: null,
            ChangePercent: null
        ));

        var result = new KpiMetricsDto(
            Metrics: metrics,
            Period: today,
            PeriodType: period
        );

        return ApiResponse<KpiMetricsDto>.Ok(result);
    }

    public async Task<ApiResponse<KpiTrendDto>> GetKpiTrendAsync(
        Guid tenantId, string kpiCode, TrendQueryParams? query = null)
    {
        var days = query?.Days ?? 30;
        var dataPoints = new List<KpiDataPointDto>();
        decimal target = 95;
        string name = kpiCode.Replace("_", " ");

        for (int i = days - 1; i >= 0; i--)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            var nextDate = date.AddDays(1);
            decimal value = 0;

            switch (kpiCode)
            {
                case "order_fulfillment_rate":
                    target = 95;
                    name = "Order Fulfillment Rate";
                    var totalOrders = await _context.SalesOrders
                        .CountAsync(o => o.TenantId == tenantId && o.CreatedAt >= date && o.CreatedAt < nextDate);
                    var completedOrders = await _context.SalesOrders
                        .CountAsync(o => o.TenantId == tenantId && o.CreatedAt >= date && o.CreatedAt < nextDate &&
                                        o.Status == SalesOrderStatus.Shipped);
                    value = totalOrders > 0 ? (decimal)completedOrders / totalOrders * 100 : 100;
                    break;

                case "pick_accuracy":
                    target = 99.5m;
                    name = "Pick Accuracy";
                    var pickTasks = await _context.PickTasks
                        .Where(t => t.TenantId == tenantId && t.Status == PickTaskStatus.Complete &&
                                   t.CompletedAt >= date && t.CompletedAt < nextDate)
                        .ToListAsync();
                    var accuratePicks = pickTasks.Count(t => t.QuantityPicked == t.QuantityRequired);
                    value = pickTasks.Count > 0 ? (decimal)accuratePicks / pickTasks.Count * 100 : 100;
                    break;

                case "location_utilization":
                    target = 80;
                    name = "Location Utilization";
                    // Get snapshot for end of each day (simplified - using current as historical isn't tracked)
                    var totalLocations = await _context.Locations.CountAsync(l => l.TenantId == tenantId);
                    var usedLocations = await _context.StockLevels
                        .Where(s => s.TenantId == tenantId && s.QuantityOnHand > 0)
                        .Select(s => s.LocationId)
                        .Distinct()
                        .CountAsync();
                    value = totalLocations > 0 ? (decimal)usedLocations / totalLocations * 100 : 0;
                    break;

                case "on_time_delivery":
                    target = 98;
                    name = "On-Time Delivery";
                    var shipments = await _context.Shipments
                        .Where(s => s.TenantId == tenantId && s.ShippedAt >= date && s.ShippedAt < nextDate)
                        .ToListAsync();
                    // Count delivered shipments as on-time (simplified - no estimated date in model)
                    var onTime = shipments.Count(s => s.Status == ShipmentStatus.Delivered);
                    value = shipments.Count > 0 ? (decimal)onTime / shipments.Count * 100 : 100;
                    break;

                case "receiving_accuracy":
                    target = 99;
                    name = "Receiving Accuracy";
                    var receipts = await _context.GoodsReceipts
                        .Where(r => r.TenantId == tenantId && r.CreatedAt >= date && r.CreatedAt < nextDate)
                        .ToListAsync();
                    var accurateReceipts = receipts.Count(r => r.TotalQuantityExpected == r.TotalQuantityReceived);
                    value = receipts.Count > 0 ? (decimal)accurateReceipts / receipts.Count * 100 : 100;
                    break;

                default:
                    // Default to order count as a fallback
                    value = await _context.SalesOrders
                        .CountAsync(o => o.TenantId == tenantId && o.CreatedAt >= date && o.CreatedAt < nextDate);
                    break;
            }

            dataPoints.Add(new KpiDataPointDto(date, Math.Round(value, 2)));
        }

        var trend = new KpiTrendDto(
            Code: kpiCode,
            Name: name,
            DataPoints: dataPoints,
            Target: target
        );

        return ApiResponse<KpiTrendDto>.Ok(trend);
    }

    #endregion

    #region Trends

    public async Task<ApiResponse<OrdersTrendDto>> GetOrdersTrendAsync(Guid tenantId, TrendQueryParams? query = null)
    {
        var days = query?.Days ?? 30;
        var labels = new List<string>();
        var salesOrders = new List<decimal>();
        var purchaseOrders = new List<decimal>();
        var shipments = new List<decimal>();

        for (int i = days - 1; i >= 0; i--)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            var nextDate = date.AddDays(1);

            labels.Add(date.ToString("MMM dd"));

            salesOrders.Add(await _context.SalesOrders
                .CountAsync(o => o.TenantId == tenantId && o.CreatedAt >= date && o.CreatedAt < nextDate));

            purchaseOrders.Add(await _context.PurchaseOrders
                .CountAsync(o => o.TenantId == tenantId && o.CreatedAt >= date && o.CreatedAt < nextDate));

            shipments.Add(await _context.Shipments
                .CountAsync(s => s.TenantId == tenantId && s.ShippedAt >= date && s.ShippedAt < nextDate));
        }

        var trend = new OrdersTrendDto(
            Labels: labels,
            SalesOrders: salesOrders,
            PurchaseOrders: purchaseOrders,
            Shipments: shipments
        );

        return ApiResponse<OrdersTrendDto>.Ok(trend);
    }

    public async Task<ApiResponse<InventoryTrendDto>> GetInventoryTrendAsync(Guid tenantId, TrendQueryParams? query = null)
    {
        var days = query?.Days ?? 30;
        var labels = new List<string>();
        var stockLevelsData = new List<decimal>();
        var movements = new List<decimal>();
        var adjustments = new List<decimal>();

        // Get current stock once
        var currentStock = await _context.StockLevels
            .Where(s => s.TenantId == tenantId)
            .SumAsync(s => s.QuantityOnHand);

        for (int i = days - 1; i >= 0; i--)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            var nextDate = date.AddDays(1);

            labels.Add(date.ToString("MMM dd"));
            stockLevelsData.Add(currentStock);

            movements.Add(await _context.StockMovements
                .CountAsync(m => m.TenantId == tenantId && m.CreatedAt >= date && m.CreatedAt < nextDate));

            adjustments.Add(await _context.StockAdjustments
                .CountAsync(a => a.TenantId == tenantId && a.CreatedAt >= date && a.CreatedAt < nextDate));
        }

        var trend = new InventoryTrendDto(
            Labels: labels,
            StockLevels: stockLevelsData,
            Movements: movements,
            Adjustments: adjustments
        );

        return ApiResponse<InventoryTrendDto>.Ok(trend);
    }

    public async Task<ApiResponse<FulfillmentTrendDto>> GetFulfillmentTrendAsync(Guid tenantId, TrendQueryParams? query = null)
    {
        var days = query?.Days ?? 30;
        var labels = new List<string>();
        var pickTasks = new List<decimal>();
        var packTasks = new List<decimal>();
        var shipments = new List<decimal>();

        for (int i = days - 1; i >= 0; i--)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            var nextDate = date.AddDays(1);

            labels.Add(date.ToString("MMM dd"));

            pickTasks.Add(await _context.PickTasks
                .CountAsync(t => t.TenantId == tenantId &&
                                t.Status == PickTaskStatus.Complete &&
                                t.CompletedAt >= date && t.CompletedAt < nextDate));

            packTasks.Add(await _context.PackingTasks
                .CountAsync(t => t.TenantId == tenantId &&
                                t.Status == Modules.Packing.Models.PackingTaskStatus.Complete &&
                                t.CompletedAt >= date && t.CompletedAt < nextDate));

            shipments.Add(await _context.Shipments
                .CountAsync(s => s.TenantId == tenantId &&
                                s.ShippedAt >= date && s.ShippedAt < nextDate));
        }

        var trend = new FulfillmentTrendDto(
            Labels: labels,
            PickTasks: pickTasks,
            PackTasks: packTasks,
            Shipments: shipments
        );

        return ApiResponse<FulfillmentTrendDto>.Ok(trend);
    }

    #endregion

    #region Activity & Alerts

    public async Task<ApiResponse<ActivityFeedDto>> GetActivityFeedAsync(Guid tenantId, int limit = 20)
    {
        var activities = await _context.ActivityLogs
            .Where(a => a.TenantId == tenantId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .Select(a => new ActivityItemDto(
                a.Id,
                a.ActivityType,
                a.Description,
                a.Notes,
                a.RelatedEntityType,
                a.RelatedEntityId,
                a.UserName,
                a.CreatedAt,
                null,
                null
            ))
            .ToListAsync();

        var feed = new ActivityFeedDto(
            Items: activities,
            TotalCount: activities.Count,
            LastUpdated: activities.FirstOrDefault()?.CreatedAt
        );

        return ApiResponse<ActivityFeedDto>.Ok(feed);
    }

    public async Task<ApiResponse<DashboardAlertsDto>> GetAlertsAsync(Guid tenantId)
    {
        var critical = new List<AlertItemDto>();
        var warning = new List<AlertItemDto>();
        var info = new List<AlertItemDto>();

        // Check for out of stock products
        var outOfStock = await _context.Products
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .Where(p => !_context.StockLevels.Any(s => s.ProductId == p.Id && s.QuantityOnHand > 0))
            .CountAsync();

        if (outOfStock > 0)
        {
            critical.Add(new AlertItemDto(
                Type: "out_of_stock",
                Severity: "Critical",
                Title: $"{outOfStock} products out of stock",
                Description: "These products have no available inventory",
                ActionUrl: "/inventory/products?filter=out_of_stock",
                CreatedAt: DateTime.UtcNow
            ));
        }

        // Check for low stock
        var products = await _context.Products
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .ToListAsync();

        var stockLevels = await _context.StockLevels
            .Where(s => s.TenantId == tenantId)
            .ToListAsync();

        var lowStock = products.Count(p =>
        {
            var stock = stockLevels.Where(s => s.ProductId == p.Id).Sum(s => s.QuantityOnHand);
            return stock > 0 && stock <= p.ReorderPoint;
        });

        if (lowStock > 0)
        {
            warning.Add(new AlertItemDto(
                Type: "low_stock",
                Severity: "Warning",
                Title: $"{lowStock} products below reorder point",
                Description: "Consider reordering these items",
                ActionUrl: "/inventory/products?filter=low_stock",
                CreatedAt: DateTime.UtcNow
            ));
        }

        // Check for pending tasks
        var pendingPicks = await _context.PickTasks
            .CountAsync(t => t.TenantId == tenantId && t.Status == PickTaskStatus.Pending);

        if (pendingPicks > 10)
        {
            info.Add(new AlertItemDto(
                Type: "pending_picks",
                Severity: "Info",
                Title: $"{pendingPicks} pick tasks pending",
                Description: "Tasks waiting to be assigned",
                ActionUrl: "/fulfillment/pick-tasks",
                CreatedAt: DateTime.UtcNow
            ));
        }

        var alerts = new DashboardAlertsDto(
            Critical: critical,
            Warning: warning,
            Info: info,
            TotalCount: critical.Count + warning.Count + info.Count
        );

        return ApiResponse<DashboardAlertsDto>.Ok(alerts);
    }

    public async Task<ApiResponse<PendingTasksDto>> GetPendingTasksAsync(Guid tenantId, Guid? userId = null)
    {
        var pickTasksQuery = _context.PickTasks
            .Where(t => t.TenantId == tenantId && t.Status == PickTaskStatus.Pending);

        if (userId.HasValue)
            pickTasksQuery = pickTasksQuery.Where(t => t.AssignedToUserId == userId.Value);

        var pickTasks = await pickTasksQuery
            .Take(10)
            .Select(t => new TaskSummaryDto(
                t.Id,
                "Pick",
                t.Status.ToString(),
                t.Priority.ToString(),
                null,
                null,
                null
            ))
            .ToListAsync();

        var putawayTasksQuery = _context.PutawayTasks
            .Where(t => t.TenantId == tenantId &&
                       t.Status != Modules.Putaway.Models.PutawayTaskStatus.Complete &&
                       t.Status != Modules.Putaway.Models.PutawayTaskStatus.Cancelled);

        var putawayTasks = await putawayTasksQuery
            .Take(10)
            .Select(t => new TaskSummaryDto(
                t.Id,
                "Putaway",
                t.Status.ToString(),
                t.Priority.ToString(),
                null,
                null,
                null
            ))
            .ToListAsync();

        var packTasksQuery = _context.PackingTasks
            .Where(t => t.TenantId == tenantId &&
                       t.Status == Modules.Packing.Models.PackingTaskStatus.Pending);

        var packTasks = await packTasksQuery
            .Take(10)
            .Select(t => new TaskSummaryDto(
                t.Id,
                "Pack",
                t.Status.ToString(),
                t.Priority.ToString(),
                null,
                null,
                null
            ))
            .ToListAsync();

        var cycleCountTasksQuery = _context.CycleCountSessions
            .Where(s => s.TenantId == tenantId &&
                       s.Status == Modules.CycleCount.Models.CycleCountStatus.InProgress);

        var cycleCountTasks = await cycleCountTasksQuery
            .Take(10)
            .Select(s => new TaskSummaryDto(
                s.Id,
                "CycleCount",
                s.Status.ToString(),
                null,
                null,
                null,
                null
            ))
            .ToListAsync();

        var result = new PendingTasksDto(
            PickTasks: pickTasks,
            PutawayTasks: putawayTasks,
            PackTasks: packTasks,
            CycleCountTasks: cycleCountTasks,
            TotalPending: pickTasks.Count + putawayTasks.Count + packTasks.Count + cycleCountTasks.Count
        );

        return ApiResponse<PendingTasksDto>.Ok(result);
    }

    #endregion

    #region Warehouse Stats

    public async Task<ApiResponse<List<WarehouseStatsDto>>> GetWarehouseStatsAsync(Guid tenantId)
    {
        var warehouses = await _context.Warehouses
            .Where(w => w.TenantId == tenantId)
            .ToListAsync();

        var stats = new List<WarehouseStatsDto>();

        foreach (var warehouse in warehouses)
        {
            var locations = await _context.Locations
                .CountAsync(l => l.TenantId == tenantId && l.WarehouseId == warehouse.Id);

            var usedLocations = await _context.StockLevels
                .Where(s => s.TenantId == tenantId && s.QuantityOnHand > 0)
                .Join(_context.Locations.Where(l => l.WarehouseId == warehouse.Id),
                      s => s.LocationId,
                      l => l.Id,
                      (s, l) => s.LocationId)
                .Distinct()
                .CountAsync();

            var pendingTasks = await _context.PickTasks
                .CountAsync(t => t.TenantId == tenantId &&
                                t.Status == PickTaskStatus.Pending);

            stats.Add(new WarehouseStatsDto(
                WarehouseId: warehouse.Id,
                WarehouseName: warehouse.Name,
                TotalLocations: locations,
                UsedLocations: usedLocations,
                Utilization: locations > 0 ? (double)usedLocations / locations * 100 : 0,
                PendingTasks: pendingTasks,
                ActiveWorkers: 0
            ));
        }

        return ApiResponse<List<WarehouseStatsDto>>.Ok(stats);
    }

    public async Task<ApiResponse<WarehouseStatsDto>> GetWarehouseStatsByIdAsync(Guid tenantId, Guid warehouseId)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == warehouseId);

        if (warehouse == null)
            return ApiResponse<WarehouseStatsDto>.Fail("Warehouse not found");

        var locations = await _context.Locations
            .CountAsync(l => l.TenantId == tenantId && l.WarehouseId == warehouseId);

        var usedLocations = await _context.StockLevels
            .Where(s => s.TenantId == tenantId && s.QuantityOnHand > 0)
            .Join(_context.Locations.Where(l => l.WarehouseId == warehouseId),
                  s => s.LocationId,
                  l => l.Id,
                  (s, l) => s.LocationId)
            .Distinct()
            .CountAsync();

        var pendingTasks = await _context.PickTasks
            .CountAsync(t => t.TenantId == tenantId &&
                            t.Status == PickTaskStatus.Pending);

        var stats = new WarehouseStatsDto(
            WarehouseId: warehouse.Id,
            WarehouseName: warehouse.Name,
            TotalLocations: locations,
            UsedLocations: usedLocations,
            Utilization: locations > 0 ? (double)usedLocations / locations * 100 : 0,
            PendingTasks: pendingTasks,
            ActiveWorkers: 0
        );

        return ApiResponse<WarehouseStatsDto>.Ok(stats);
    }

    #endregion
}
