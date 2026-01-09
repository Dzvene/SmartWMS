using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Adjustments.Models;

/// <summary>
/// Stock Adjustment - document for inventory corrections
/// </summary>
public class StockAdjustment : TenantEntity
{
    public required string AdjustmentNumber { get; set; }

    // Location context
    public Guid WarehouseId { get; set; }

    // Status
    public AdjustmentStatus Status { get; set; } = AdjustmentStatus.Draft;
    public AdjustmentType AdjustmentType { get; set; } = AdjustmentType.Correction;

    // Reason
    public Guid? ReasonCodeId { get; set; }
    public string? ReasonNotes { get; set; }

    // Reference to source document (if applicable)
    public string? SourceDocumentType { get; set; } // CycleCount, Return, etc.
    public Guid? SourceDocumentId { get; set; }
    public string? SourceDocumentNumber { get; set; }

    // User tracking
    public Guid CreatedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? PostedByUserId { get; set; }
    public DateTime? PostedAt { get; set; }

    // Summary
    public string? Notes { get; set; }
    public int TotalLines { get; set; }
    public decimal TotalQuantityChange { get; set; }
    public decimal? TotalValueChange { get; set; }

    // Navigation
    public virtual Warehouse.Models.Warehouse Warehouse { get; set; } = null!;
    public virtual Configuration.Models.ReasonCode? ReasonCode { get; set; }
    public virtual List<StockAdjustmentLine> Lines { get; set; } = new();
}

/// <summary>
/// Stock Adjustment Line - individual item adjustment
/// </summary>
public class StockAdjustmentLine : TenantEntity
{
    public Guid AdjustmentId { get; set; }
    public int LineNumber { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public required string Sku { get; set; }

    // Location
    public Guid LocationId { get; set; }

    // Batch/Serial (if applicable)
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }

    // Quantities
    public decimal QuantityBefore { get; set; }
    public decimal QuantityAdjustment { get; set; } // Can be positive or negative
    public decimal QuantityAfter => QuantityBefore + QuantityAdjustment;

    // Value (optional cost tracking)
    public decimal? UnitCost { get; set; }
    public decimal? ValueChange => UnitCost.HasValue ? UnitCost.Value * QuantityAdjustment : null;

    // Line-level reason (can override header)
    public Guid? ReasonCodeId { get; set; }
    public string? ReasonNotes { get; set; }

    // Processing
    public bool IsProcessed { get; set; } = false;
    public DateTime? ProcessedAt { get; set; }

    // Navigation
    public virtual StockAdjustment Adjustment { get; set; } = null!;
    public virtual Inventory.Models.Product Product { get; set; } = null!;
    public virtual Warehouse.Models.Location Location { get; set; } = null!;
    public virtual Configuration.Models.ReasonCode? ReasonCode { get; set; }
}

#region Enums

public enum AdjustmentStatus
{
    Draft,
    PendingApproval,
    Approved,
    Posted,
    Cancelled
}

public enum AdjustmentType
{
    Correction,      // General correction
    CycleCount,      // From cycle count variance
    Damage,          // Damaged goods write-off
    Scrap,           // Scrap/dispose
    Found,           // Found inventory (positive)
    Lost,            // Lost/missing inventory (negative)
    Expiry,          // Expired goods
    QualityHold,     // Quality issue
    Revaluation,     // Cost revaluation
    Opening,         // Opening balance
    Other
}

#endregion
