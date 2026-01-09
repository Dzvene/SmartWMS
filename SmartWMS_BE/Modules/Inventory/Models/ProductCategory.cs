using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Inventory.Models;

/// <summary>
/// ProductCategory - hierarchical categorization of products.
/// Categories can define default values and requirements for products.
/// </summary>
public class ProductCategory : TenantEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Hierarchy support
    public Guid? ParentCategoryId { get; set; }
    public int Level { get; set; }
    public string? Path { get; set; } // Materialized path for efficient queries

    public bool IsActive { get; set; } = true;

    // ==========================================================================
    // Product Defaults - applied when creating products in this category
    // ==========================================================================

    /// <summary>Default unit of measure for products in this category</summary>
    public string? DefaultUnitOfMeasure { get; set; }

    /// <summary>Default storage zone type (e.g., "AMBIENT", "COLD", "FROZEN")</summary>
    public string? DefaultStorageZoneType { get; set; }

    // ==========================================================================
    // Tracking Requirements - enforced for all products in this category
    // ==========================================================================

    /// <summary>Products must have batch/lot tracking enabled</summary>
    public bool RequiresBatchTracking { get; set; }

    /// <summary>Products must have serial number tracking enabled</summary>
    public bool RequiresSerialTracking { get; set; }

    /// <summary>Products must have expiry date tracking enabled</summary>
    public bool RequiresExpiryDate { get; set; }

    // ==========================================================================
    // Handling & Storage
    // ==========================================================================

    /// <summary>Special handling instructions for products in this category</summary>
    public string? HandlingInstructions { get; set; }

    /// <summary>Minimum temperature for storage (Celsius)</summary>
    public decimal? MinTemperature { get; set; }

    /// <summary>Maximum temperature for storage (Celsius)</summary>
    public decimal? MaxTemperature { get; set; }

    /// <summary>Is hazardous material (requires special handling)</summary>
    public bool IsHazardous { get; set; }

    /// <summary>Is fragile (requires careful handling)</summary>
    public bool IsFragile { get; set; }

    // Navigation properties
    public virtual ProductCategory? ParentCategory { get; set; }
    public virtual ICollection<ProductCategory> ChildCategories { get; set; } = new List<ProductCategory>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
