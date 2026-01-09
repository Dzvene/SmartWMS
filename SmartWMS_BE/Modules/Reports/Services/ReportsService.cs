using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Receiving.Models;
using SmartWMS.API.Modules.Reports.DTOs;

namespace SmartWMS.API.Modules.Reports.Services;

public class ReportsService : IReportsService
{
    private readonly ApplicationDbContext _context;

    public ReportsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<InventorySummaryReport>> GetInventorySummaryAsync(
        Guid tenantId, Guid? warehouseId = null)
    {
        var report = new InventorySummaryReport
        {
            GeneratedAt = DateTime.UtcNow,
            WarehouseId = warehouseId
        };

        // Get warehouse name if specified
        if (warehouseId.HasValue)
        {
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == warehouseId);
            report.WarehouseName = warehouse?.Name;
        }

        // Product metrics
        var productsQuery = _context.Products
            .Where(p => p.TenantId == tenantId && p.IsActive);

        report.TotalProducts = await productsQuery.CountAsync();

        // Stock levels query
        var stockQuery = _context.StockLevels
            .Include(s => s.Location)
                .ThenInclude(l => l.Zone)
            .Where(s => s.TenantId == tenantId);

        if (warehouseId.HasValue)
        {
            stockQuery = stockQuery.Where(s => s.Location.Zone!.WarehouseId == warehouseId);
        }

        var stockLevels = await stockQuery.ToListAsync();

        // Aggregate stock metrics
        var productStockGroups = stockLevels
            .GroupBy(s => s.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalOnHand = g.Sum(x => x.QuantityOnHand),
                TotalReserved = g.Sum(x => x.QuantityReserved)
            })
            .ToList();

        report.ProductsWithStock = productStockGroups.Count(g => g.TotalOnHand > 0);
        report.ProductsOutOfStock = report.TotalProducts - report.ProductsWithStock;

        report.TotalQuantityOnHand = productStockGroups.Sum(g => g.TotalOnHand);
        report.TotalQuantityReserved = productStockGroups.Sum(g => g.TotalReserved);
        report.TotalQuantityAvailable = report.TotalQuantityOnHand - report.TotalQuantityReserved;

        // Low stock products
        var productsWithMinLevel = await _context.Products
            .Where(p => p.TenantId == tenantId && p.IsActive && p.MinStockLevel.HasValue)
            .Select(p => new { p.Id, p.Sku, p.Name, p.MinStockLevel })
            .ToListAsync();

        var lowStockCount = 0;
        var lowStockProducts = new List<ProductQuantityItem>();

        foreach (var product in productsWithMinLevel)
        {
            var stockOnHand = productStockGroups
                .FirstOrDefault(g => g.ProductId == product.Id)?.TotalOnHand ?? 0;

            if (stockOnHand < product.MinStockLevel)
            {
                lowStockCount++;
                lowStockProducts.Add(new ProductQuantityItem
                {
                    ProductId = product.Id,
                    Sku = product.Sku,
                    ProductName = product.Name,
                    QuantityOnHand = stockOnHand,
                    MinStockLevel = product.MinStockLevel
                });
            }
        }

        report.ProductsLowStock = lowStockCount;
        report.LowStockProducts = lowStockProducts.OrderBy(p => p.QuantityOnHand).Take(10).ToList();

        // Location metrics
        var locationsQuery = _context.Locations
            .Include(l => l.Zone)
            .Where(l => l.TenantId == tenantId && l.IsActive);

        if (warehouseId.HasValue)
        {
            locationsQuery = locationsQuery.Where(l => l.Zone!.WarehouseId == warehouseId);
        }

        var locations = await locationsQuery.ToListAsync();
        report.TotalLocations = locations.Count;

        var occupiedLocationIds = stockLevels
            .Where(s => s.QuantityOnHand > 0)
            .Select(s => s.LocationId)
            .Distinct()
            .ToHashSet();

        report.OccupiedLocations = locations.Count(l => occupiedLocationIds.Contains(l.Id));
        report.EmptyLocations = report.TotalLocations - report.OccupiedLocations;
        report.LocationUtilizationPercent = report.TotalLocations > 0
            ? Math.Round((decimal)report.OccupiedLocations / report.TotalLocations * 100, 2)
            : 0;

        // Top products by quantity
        var topProducts = await _context.Products
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .Select(p => new { p.Id, p.Sku, p.Name })
            .ToListAsync();

        report.TopProductsByQuantity = productStockGroups
            .OrderByDescending(g => g.TotalOnHand)
            .Take(10)
            .Select(g =>
            {
                var product = topProducts.FirstOrDefault(p => p.Id == g.ProductId);
                return new ProductQuantityItem
                {
                    ProductId = g.ProductId,
                    Sku = product?.Sku,
                    ProductName = product?.Name,
                    QuantityOnHand = g.TotalOnHand
                };
            })
            .ToList();

        // Expiring stock (within 30 days)
        var expiryThreshold = DateTime.UtcNow.AddDays(30);
        report.ExpiringStock = stockLevels
            .Where(s => s.ExpiryDate.HasValue && s.ExpiryDate <= expiryThreshold && s.QuantityOnHand > 0)
            .OrderBy(s => s.ExpiryDate)
            .Take(10)
            .Select(s =>
            {
                var product = topProducts.FirstOrDefault(p => p.Id == s.ProductId);
                return new ExpiringStockItem
                {
                    ProductId = s.ProductId,
                    Sku = product?.Sku,
                    ProductName = product?.Name,
                    BatchNumber = s.BatchNumber,
                    ExpiryDate = s.ExpiryDate,
                    Quantity = s.QuantityOnHand,
                    DaysUntilExpiry = s.ExpiryDate.HasValue
                        ? (int)(s.ExpiryDate.Value - DateTime.UtcNow).TotalDays
                        : 0
                };
            })
            .ToList();

        return ApiResponse<InventorySummaryReport>.Ok(report);
    }

    public async Task<ApiResponse<StockMovementReport>> GetStockMovementReportAsync(
        Guid tenantId, ReportDateRangeFilter filter)
    {
        var dateFrom = filter.DateFrom ?? DateTime.UtcNow.AddDays(-30);
        var dateTo = filter.DateTo ?? DateTime.UtcNow;

        var report = new StockMovementReport
        {
            GeneratedAt = DateTime.UtcNow,
            DateFrom = dateFrom,
            DateTo = dateTo,
            WarehouseId = filter.WarehouseId
        };

        // Get warehouse name if specified
        if (filter.WarehouseId.HasValue)
        {
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == filter.WarehouseId);
            report.WarehouseName = warehouse?.Name;
        }

        // Get movements in date range
        var movementsQuery = _context.StockMovements
            .Include(m => m.FromLocation)
                .ThenInclude(l => l!.Zone)
            .Include(m => m.ToLocation)
                .ThenInclude(l => l!.Zone)
            .Where(m => m.TenantId == tenantId &&
                        m.MovementDate >= dateFrom &&
                        m.MovementDate <= dateTo);

        if (filter.WarehouseId.HasValue)
        {
            movementsQuery = movementsQuery.Where(m =>
                (m.FromLocation != null && m.FromLocation.Zone!.WarehouseId == filter.WarehouseId) ||
                (m.ToLocation != null && m.ToLocation.Zone!.WarehouseId == filter.WarehouseId));
        }

        var movements = await movementsQuery.ToListAsync();

        // Summary by movement type
        report.MovementsByType = movements
            .GroupBy(m => m.MovementType)
            .Select(g => new MovementTypeSummary
            {
                MovementType = g.Key.ToString(),
                Count = g.Count(),
                TotalQuantity = g.Sum(m => m.Quantity)
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        // Daily breakdown
        report.DailyMovements = movements
            .GroupBy(m => m.MovementDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyMovementSummary
            {
                Date = g.Key,
                ReceiptCount = g.Count(m => m.MovementType == MovementType.Receipt),
                IssueCount = g.Count(m => m.MovementType == MovementType.Issue),
                TransferCount = g.Count(m => m.MovementType == MovementType.Transfer),
                AdjustmentCount = g.Count(m => m.MovementType == MovementType.Adjustment),
                TotalQuantityMoved = g.Sum(m => m.Quantity)
            })
            .ToList();

        // Top moved products
        var products = await _context.Products
            .Where(p => p.TenantId == tenantId)
            .Select(p => new { p.Id, p.Sku, p.Name })
            .ToListAsync();

        report.TopMovedProducts = movements
            .GroupBy(m => m.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                MovementCount = g.Count(),
                TotalQuantity = g.Sum(m => m.Quantity)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(10)
            .Select(x =>
            {
                var product = products.FirstOrDefault(p => p.Id == x.ProductId);
                return new ProductMovementItem
                {
                    ProductId = x.ProductId,
                    Sku = product?.Sku,
                    ProductName = product?.Name,
                    MovementCount = x.MovementCount,
                    TotalQuantityMoved = x.TotalQuantity
                };
            })
            .ToList();

        return ApiResponse<StockMovementReport>.Ok(report);
    }

    public async Task<ApiResponse<OrderFulfillmentReport>> GetOrderFulfillmentReportAsync(
        Guid tenantId, ReportDateRangeFilter filter)
    {
        var dateFrom = filter.DateFrom ?? DateTime.UtcNow.AddDays(-30);
        var dateTo = filter.DateTo ?? DateTime.UtcNow;

        var report = new OrderFulfillmentReport
        {
            GeneratedAt = DateTime.UtcNow,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        // Sales orders metrics
        var salesOrders = await _context.SalesOrders
            .Where(o => o.TenantId == tenantId &&
                        o.CreatedAt >= dateFrom &&
                        o.CreatedAt <= dateTo)
            .ToListAsync();

        report.TotalSalesOrders = salesOrders.Count;
        report.OrdersDelivered = salesOrders.Count(o => o.Status == SalesOrderStatus.Delivered);
        report.OrdersCancelled = salesOrders.Count(o => o.Status == SalesOrderStatus.Cancelled);
        report.OrdersPending = salesOrders.Count(o =>
            o.Status == SalesOrderStatus.Draft ||
            o.Status == SalesOrderStatus.Pending ||
            o.Status == SalesOrderStatus.Confirmed);
        report.OrdersInProgress = salesOrders.Count(o =>
            o.Status == SalesOrderStatus.Allocated ||
            o.Status == SalesOrderStatus.PartiallyPicked ||
            o.Status == SalesOrderStatus.Picked ||
            o.Status == SalesOrderStatus.Packed ||
            o.Status == SalesOrderStatus.Shipped);

        report.FulfillmentRatePercent = report.TotalSalesOrders > 0
            ? Math.Round((decimal)report.OrdersDelivered / report.TotalSalesOrders * 100, 2)
            : 0;

        // Pick tasks metrics
        var pickTasks = await _context.PickTasks
            .Where(t => t.TenantId == tenantId &&
                        t.CreatedAt >= dateFrom &&
                        t.CreatedAt <= dateTo)
            .ToListAsync();

        report.TotalPickTasks = pickTasks.Count;
        report.PickTasksCompleted = pickTasks.Count(t =>
            t.Status == PickTaskStatus.Complete ||
            t.Status == PickTaskStatus.ShortPicked);
        report.PickTasksPending = pickTasks.Count(t =>
            t.Status == PickTaskStatus.Pending ||
            t.Status == PickTaskStatus.Assigned ||
            t.Status == PickTaskStatus.InProgress);

        report.PickCompletionRatePercent = report.TotalPickTasks > 0
            ? Math.Round((decimal)report.PickTasksCompleted / report.TotalPickTasks * 100, 2)
            : 0;

        // Shipment metrics
        var shipments = await _context.Shipments
            .Where(s => s.TenantId == tenantId &&
                        s.CreatedAt >= dateFrom &&
                        s.CreatedAt <= dateTo)
            .ToListAsync();

        report.TotalShipments = shipments.Count;
        report.ShipmentsDelivered = shipments.Count(s => s.Status == ShipmentStatus.Delivered);
        report.ShipmentsInTransit = shipments.Count(s => s.Status == ShipmentStatus.InTransit);

        // Daily breakdown
        report.DailyOrders = salesOrders
            .GroupBy(o => o.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyOrderSummary
            {
                Date = g.Key,
                OrdersCreated = g.Count(),
                OrdersShipped = g.Count(o => o.Status >= SalesOrderStatus.Shipped),
                OrdersDelivered = g.Count(o => o.Status == SalesOrderStatus.Delivered),
                LinesProcessed = 0 // Would need to join with lines
            })
            .ToList();

        return ApiResponse<OrderFulfillmentReport>.Ok(report);
    }

    public async Task<ApiResponse<ReceivingReport>> GetReceivingReportAsync(
        Guid tenantId, ReportDateRangeFilter filter)
    {
        var dateFrom = filter.DateFrom ?? DateTime.UtcNow.AddDays(-30);
        var dateTo = filter.DateTo ?? DateTime.UtcNow;

        var report = new ReceivingReport
        {
            GeneratedAt = DateTime.UtcNow,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        // Purchase orders metrics
        var purchaseOrders = await _context.PurchaseOrders
            .Where(po => po.TenantId == tenantId &&
                         po.CreatedAt >= dateFrom &&
                         po.CreatedAt <= dateTo)
            .ToListAsync();

        report.TotalPurchaseOrders = purchaseOrders.Count;
        report.POsReceived = purchaseOrders.Count(po => po.Status == PurchaseOrderStatus.Received);
        report.POsPartiallyReceived = purchaseOrders.Count(po => po.Status == PurchaseOrderStatus.PartiallyReceived);
        report.POsPending = purchaseOrders.Count(po =>
            po.Status == PurchaseOrderStatus.Draft ||
            po.Status == PurchaseOrderStatus.Pending ||
            po.Status == PurchaseOrderStatus.Confirmed);

        // Goods receipts metrics
        var goodsReceipts = await _context.GoodsReceipts
            .Include(gr => gr.Lines)
            .Where(gr => gr.TenantId == tenantId &&
                         gr.CreatedAt >= dateFrom &&
                         gr.CreatedAt <= dateTo)
            .ToListAsync();

        report.TotalGoodsReceipts = goodsReceipts.Count;
        report.ReceiptsCompleted = goodsReceipts.Count(gr =>
            gr.Status == GoodsReceiptStatus.Complete ||
            gr.Status == GoodsReceiptStatus.PartiallyComplete);
        report.ReceiptsInProgress = goodsReceipts.Count(gr =>
            gr.Status == GoodsReceiptStatus.Draft ||
            gr.Status == GoodsReceiptStatus.InProgress);

        // Calculate quantities from receipt lines
        var allLines = goodsReceipts.SelectMany(gr => gr.Lines).ToList();
        report.TotalQuantityReceived = allLines.Sum(l => l.QuantityReceived);

        report.QuantityGood = allLines
            .Where(l => l.QualityStatus == "Good")
            .Sum(l => l.QuantityReceived);
        report.QuantityDamaged = allLines
            .Where(l => l.QualityStatus == "Damaged")
            .Sum(l => l.QuantityReceived);
        report.QuantityQuarantine = allLines
            .Where(l => l.QualityStatus == "Quarantine")
            .Sum(l => l.QuantityReceived);

        report.QualityPassRatePercent = report.TotalQuantityReceived > 0
            ? Math.Round(report.QuantityGood / report.TotalQuantityReceived * 100, 2)
            : 0;

        // Daily breakdown
        report.DailyReceiving = goodsReceipts
            .GroupBy(gr => gr.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyReceivingSummary
            {
                Date = g.Key,
                ReceiptsCreated = g.Count(),
                ReceiptsCompleted = g.Count(gr =>
                    gr.Status == GoodsReceiptStatus.Complete ||
                    gr.Status == GoodsReceiptStatus.PartiallyComplete),
                QuantityReceived = g.SelectMany(gr => gr.Lines).Sum(l => l.QuantityReceived)
            })
            .ToList();

        return ApiResponse<ReceivingReport>.Ok(report);
    }

    public async Task<ApiResponse<WarehouseUtilizationReport>> GetWarehouseUtilizationAsync(
        Guid tenantId, Guid warehouseId)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == warehouseId);

        if (warehouse == null)
            return ApiResponse<WarehouseUtilizationReport>.Fail("Warehouse not found");

        var report = new WarehouseUtilizationReport
        {
            GeneratedAt = DateTime.UtcNow,
            WarehouseId = warehouseId,
            WarehouseName = warehouse.Name
        };

        // Get zones with locations
        var zones = await _context.Zones
            .Include(z => z.Locations)
            .Where(z => z.TenantId == tenantId && z.WarehouseId == warehouseId && z.IsActive)
            .ToListAsync();

        // Get stock levels for this warehouse
        var stockLevels = await _context.StockLevels
            .Include(s => s.Location)
                .ThenInclude(l => l.Zone)
            .Where(s => s.TenantId == tenantId &&
                        s.Location.Zone!.WarehouseId == warehouseId &&
                        s.QuantityOnHand > 0)
            .ToListAsync();

        var occupiedLocationIds = stockLevels
            .Select(s => s.LocationId)
            .Distinct()
            .ToHashSet();

        // Zone utilizations
        report.ZoneUtilizations = zones.Select(z =>
        {
            var activeLocations = z.Locations.Where(l => l.IsActive).ToList();
            var occupied = activeLocations.Count(l => occupiedLocationIds.Contains(l.Id));

            return new ZoneUtilization
            {
                ZoneId = z.Id,
                ZoneName = z.Name,
                ZoneType = z.ZoneType.ToString(),
                TotalLocations = activeLocations.Count,
                OccupiedLocations = occupied,
                UtilizationPercent = activeLocations.Count > 0
                    ? Math.Round((decimal)occupied / activeLocations.Count * 100, 2)
                    : 0
            };
        }).ToList();

        // Overall metrics
        report.TotalLocations = report.ZoneUtilizations.Sum(z => z.TotalLocations);
        report.OccupiedLocations = report.ZoneUtilizations.Sum(z => z.OccupiedLocations);
        report.EmptyLocations = report.TotalLocations - report.OccupiedLocations;
        report.OverallUtilizationPercent = report.TotalLocations > 0
            ? Math.Round((decimal)report.OccupiedLocations / report.TotalLocations * 100, 2)
            : 0;

        // Estimated capacity (simplified - could be enhanced with actual capacity data)
        report.EstimatedCapacityUsedPercent = report.OverallUtilizationPercent;

        return ApiResponse<WarehouseUtilizationReport>.Ok(report);
    }
}
