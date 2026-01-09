using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Inventory.Models;

namespace SmartWMS.API.Modules.CycleCount.Models;

/// <summary>
/// Cycle count - inventory counting session
/// </summary>
public class CycleCountSession : TenantEntity
{
    public required string CountNumber { get; set; }
    public string? Description { get; set; }

    // Warehouse/Zone scope
    public Guid WarehouseId { get; set; }
    public Warehouse.Models.Warehouse? Warehouse { get; set; }
    public Guid? ZoneId { get; set; }
    public Warehouse.Models.Zone? Zone { get; set; }

    // Count type
    public CountType CountType { get; set; } = CountType.Scheduled;
    public CountScope CountScope { get; set; } = CountScope.Location;

    // Status
    public CycleCountStatus Status { get; set; } = CycleCountStatus.Draft;

    // Schedule
    public DateTime? ScheduledDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Assignment
    public Guid? AssignedToUserId { get; set; }

    // Counts
    public int TotalLocations { get; set; }
    public int CountedLocations { get; set; }
    public int VarianceCount { get; set; }

    // Settings
    public bool RequireBlindCount { get; set; } = false; // Counter doesn't see expected qty
    public bool AllowRecounts { get; set; } = true;
    public int MaxRecounts { get; set; } = 3;

    // Notes
    public string? Notes { get; set; }

    // Count items
    public List<CycleCountItem> Items { get; set; } = new();
}

/// <summary>
/// Individual count item (location/product combination)
/// </summary>
public class CycleCountItem : TenantEntity
{
    public Guid CycleCountSessionId { get; set; }
    public CycleCountSession? CycleCountSession { get; set; }

    // Location
    public Guid LocationId { get; set; }
    public Warehouse.Models.Location? Location { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public required string Sku { get; set; }

    // Expected (from system)
    public decimal ExpectedQuantity { get; set; }
    public string? ExpectedBatchNumber { get; set; }

    // Counted
    public decimal? CountedQuantity { get; set; }
    public string? CountedBatchNumber { get; set; }
    public DateTime? CountedAt { get; set; }
    public Guid? CountedByUserId { get; set; }

    // Variance
    public decimal Variance => (CountedQuantity ?? 0) - ExpectedQuantity;
    public decimal VariancePercent => ExpectedQuantity != 0
        ? ((CountedQuantity ?? 0) - ExpectedQuantity) / ExpectedQuantity * 100
        : 0;

    // Status
    public CountItemStatus Status { get; set; } = CountItemStatus.Pending;

    // Recount tracking
    public int RecountNumber { get; set; } = 0;

    // Approval for variances
    public bool RequiresApproval { get; set; } = false;
    public bool IsApproved { get; set; } = false;
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Notes
    public string? Notes { get; set; }
}

/// <summary>
/// Type of cycle count
/// </summary>
public enum CountType
{
    Scheduled,      // Regular scheduled count
    Random,         // Random location selection
    ABC,            // ABC classification based
    Perpetual,      // Continuous counting
    Annual,         // Annual/Full inventory
    Triggered       // Triggered by event (e.g., negative stock)
}

/// <summary>
/// Scope of the count
/// </summary>
public enum CountScope
{
    Location,       // Count all products in selected locations
    Product,        // Count selected products across all locations
    Zone,           // Count entire zone
    Warehouse       // Count entire warehouse
}

/// <summary>
/// Status of a cycle count session
/// </summary>
public enum CycleCountStatus
{
    Draft,          // Being prepared
    Scheduled,      // Scheduled for future
    InProgress,     // Currently being counted
    Review,         // Variances being reviewed
    Complete,       // All counted and approved
    Cancelled       // Cancelled
}

/// <summary>
/// Status of individual count item
/// </summary>
public enum CountItemStatus
{
    Pending,        // Not yet counted
    Counted,        // Counted, may have variance
    Recounting,     // Scheduled for recount
    Approved,       // Variance approved
    Adjusted        // Stock adjusted
}
