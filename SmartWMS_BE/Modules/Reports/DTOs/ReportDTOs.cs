namespace SmartWMS.API.Modules.Reports.DTOs;

/// <summary>
/// Inventory summary report data
/// </summary>
public class InventorySummaryReport
{
    public DateTime GeneratedAt { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }

    // Overall metrics
    public int TotalProducts { get; set; }
    public int ProductsWithStock { get; set; }
    public int ProductsOutOfStock { get; set; }
    public int ProductsLowStock { get; set; }

    public decimal TotalQuantityOnHand { get; set; }
    public decimal TotalQuantityReserved { get; set; }
    public decimal TotalQuantityAvailable { get; set; }

    // Location metrics
    public int TotalLocations { get; set; }
    public int OccupiedLocations { get; set; }
    public int EmptyLocations { get; set; }
    public decimal LocationUtilizationPercent { get; set; }

    // Top items
    public List<ProductQuantityItem> TopProductsByQuantity { get; set; } = new();
    public List<ProductQuantityItem> LowStockProducts { get; set; } = new();
    public List<ExpiringStockItem> ExpiringStock { get; set; } = new();
}

/// <summary>
/// Stock movement report data
/// </summary>
public class StockMovementReport
{
    public DateTime GeneratedAt { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }

    // Summary by movement type
    public List<MovementTypeSummary> MovementsByType { get; set; } = new();

    // Daily breakdown
    public List<DailyMovementSummary> DailyMovements { get; set; } = new();

    // Top movers
    public List<ProductMovementItem> TopMovedProducts { get; set; } = new();
}

/// <summary>
/// Order fulfillment report data
/// </summary>
public class OrderFulfillmentReport
{
    public DateTime GeneratedAt { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }

    // Sales orders metrics
    public int TotalSalesOrders { get; set; }
    public int OrdersDelivered { get; set; }
    public int OrdersInProgress { get; set; }
    public int OrdersPending { get; set; }
    public int OrdersCancelled { get; set; }
    public decimal FulfillmentRatePercent { get; set; }

    // Picking metrics
    public int TotalPickTasks { get; set; }
    public int PickTasksCompleted { get; set; }
    public int PickTasksPending { get; set; }
    public decimal PickCompletionRatePercent { get; set; }

    // Shipment metrics
    public int TotalShipments { get; set; }
    public int ShipmentsDelivered { get; set; }
    public int ShipmentsInTransit { get; set; }

    // Daily breakdown
    public List<DailyOrderSummary> DailyOrders { get; set; } = new();
}

/// <summary>
/// Receiving report data
/// </summary>
public class ReceivingReport
{
    public DateTime GeneratedAt { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }

    // Purchase orders metrics
    public int TotalPurchaseOrders { get; set; }
    public int POsReceived { get; set; }
    public int POsPartiallyReceived { get; set; }
    public int POsPending { get; set; }

    // Goods receipts metrics
    public int TotalGoodsReceipts { get; set; }
    public int ReceiptsCompleted { get; set; }
    public int ReceiptsInProgress { get; set; }
    public decimal TotalQuantityReceived { get; set; }

    // Quality metrics
    public decimal QuantityGood { get; set; }
    public decimal QuantityDamaged { get; set; }
    public decimal QuantityQuarantine { get; set; }
    public decimal QualityPassRatePercent { get; set; }

    // Daily breakdown
    public List<DailyReceivingSummary> DailyReceiving { get; set; } = new();
}

/// <summary>
/// Warehouse utilization report
/// </summary>
public class WarehouseUtilizationReport
{
    public DateTime GeneratedAt { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }

    // Zone breakdown
    public List<ZoneUtilization> ZoneUtilizations { get; set; } = new();

    // Overall metrics
    public int TotalLocations { get; set; }
    public int OccupiedLocations { get; set; }
    public int EmptyLocations { get; set; }
    public decimal OverallUtilizationPercent { get; set; }

    // Capacity
    public decimal EstimatedCapacityUsedPercent { get; set; }
}

#region Helper DTOs

public class ProductQuantityItem
{
    public Guid ProductId { get; set; }
    public string? Sku { get; set; }
    public string? ProductName { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal? MinStockLevel { get; set; }
}

public class ExpiringStockItem
{
    public Guid ProductId { get; set; }
    public string? Sku { get; set; }
    public string? ProductName { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public int DaysUntilExpiry { get; set; }
}

public class MovementTypeSummary
{
    public string MovementType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalQuantity { get; set; }
}

public class DailyMovementSummary
{
    public DateTime Date { get; set; }
    public int ReceiptCount { get; set; }
    public int IssueCount { get; set; }
    public int TransferCount { get; set; }
    public int AdjustmentCount { get; set; }
    public decimal TotalQuantityMoved { get; set; }
}

public class ProductMovementItem
{
    public Guid ProductId { get; set; }
    public string? Sku { get; set; }
    public string? ProductName { get; set; }
    public int MovementCount { get; set; }
    public decimal TotalQuantityMoved { get; set; }
}

public class DailyOrderSummary
{
    public DateTime Date { get; set; }
    public int OrdersCreated { get; set; }
    public int OrdersShipped { get; set; }
    public int OrdersDelivered { get; set; }
    public int LinesProcessed { get; set; }
}

public class DailyReceivingSummary
{
    public DateTime Date { get; set; }
    public int ReceiptsCreated { get; set; }
    public int ReceiptsCompleted { get; set; }
    public decimal QuantityReceived { get; set; }
}

public class ZoneUtilization
{
    public Guid ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public string? ZoneType { get; set; }
    public int TotalLocations { get; set; }
    public int OccupiedLocations { get; set; }
    public decimal UtilizationPercent { get; set; }
}

#endregion

#region Filter DTOs

public class ReportDateRangeFilter
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public Guid? WarehouseId { get; set; }
}

#endregion
