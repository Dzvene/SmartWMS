using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Fulfillment.Models;
using SmartWMS.API.Modules.Orders.Models;

namespace SmartWMS.API.Modules.Packing.Models;

/// <summary>
/// Packing task - packs picked items for shipment
/// </summary>
public class PackingTask : TenantEntity
{
    public required string TaskNumber { get; set; }

    // Source order
    public Guid SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }

    // Related pick tasks (all picks should be complete before packing)
    public Guid? FulfillmentBatchId { get; set; }
    public FulfillmentBatch? FulfillmentBatch { get; set; }

    // Packing station
    public Guid? PackingStationId { get; set; }
    public PackingStation? PackingStation { get; set; }

    // Assignment
    public Guid? AssignedToUserId { get; set; }
    public DateTime? AssignedAt { get; set; }

    // Status
    public PackingTaskStatus Status { get; set; } = PackingTaskStatus.Pending;

    // Quantities
    public int TotalItems { get; set; }
    public int PackedItems { get; set; }

    // Packaging used
    public int BoxCount { get; set; }
    public decimal TotalWeightKg { get; set; }

    // Timing
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int Priority { get; set; } = 5;
    public string? Notes { get; set; }

    // Packages created
    public List<Package> Packages { get; set; } = new();
}

/// <summary>
/// Package/box in a packing task
/// </summary>
public class Package : TenantEntity
{
    public Guid PackingTaskId { get; set; }
    public PackingTask? PackingTask { get; set; }

    public required string PackageNumber { get; set; }
    public int SequenceNumber { get; set; }

    // Dimensions (mm)
    public int? LengthMm { get; set; }
    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }

    // Weight
    public decimal WeightKg { get; set; }

    // Packaging type
    public string? PackagingType { get; set; } // Box, Envelope, Pallet, etc.

    // Tracking
    public string? TrackingNumber { get; set; }
    public string? LabelUrl { get; set; }

    // Contents
    public List<PackageItem> Items { get; set; } = new();
}

/// <summary>
/// Item in a package
/// </summary>
public class PackageItem : TenantEntity
{
    public Guid PackageId { get; set; }
    public Package? Package { get; set; }

    public Guid ProductId { get; set; }
    public Inventory.Models.Product? Product { get; set; }
    public required string Sku { get; set; }

    public decimal Quantity { get; set; }

    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
}

/// <summary>
/// Packing station in warehouse
/// </summary>
public class PackingStation : TenantEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }

    public Guid WarehouseId { get; set; }
    public Warehouse.Models.Warehouse? Warehouse { get; set; }

    public bool IsActive { get; set; } = true;

    // Capabilities
    public bool CanPrintLabels { get; set; } = true;
    public bool HasScale { get; set; } = true;
    public bool HasDimensioner { get; set; } = false;

    public string? Notes { get; set; }
}

/// <summary>
/// Status of a packing task
/// </summary>
public enum PackingTaskStatus
{
    Pending,
    Assigned,
    InProgress,
    Complete,
    Cancelled
}
