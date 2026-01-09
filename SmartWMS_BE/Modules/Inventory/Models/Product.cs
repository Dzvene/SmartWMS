using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Inventory.Models;

/// <summary>
/// Product - represents a SKU/item in the inventory.
/// (Matches SmartWMS_UI: inventory/product.types.ts - Product type)
/// </summary>
public class Product : TenantEntity
{
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Categorization
    public Guid? CategoryId { get; set; }

    // Identification
    public string? Barcode { get; set; }
    public string? AlternativeBarcodes { get; set; } // JSON array

    // Dimensions (in millimeters)
    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }
    public int? DepthMm { get; set; }

    // Weight (in kilograms)
    public decimal? GrossWeightKg { get; set; }
    public decimal? NetWeightKg { get; set; }

    // Unit of measure
    public required string UnitOfMeasure { get; set; }
    public int? UnitsPerCase { get; set; }
    public int? CasesPerPallet { get; set; }

    // Status
    public bool IsActive { get; set; } = true;

    // Tracking requirements
    public bool IsBatchTracked { get; set; }
    public bool IsSerialTracked { get; set; }
    public bool HasExpiryDate { get; set; }

    // Inventory levels for reordering
    public decimal? MinStockLevel { get; set; }
    public decimal? MaxStockLevel { get; set; }
    public decimal? ReorderPoint { get; set; }

    // Supplier
    public Guid? DefaultSupplierId { get; set; }

    // Media
    public string? ImageUrl { get; set; }

    // Navigation properties
    public virtual ProductCategory? Category { get; set; }
    public virtual Orders.Models.Supplier? DefaultSupplier { get; set; }
    public virtual ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
}
