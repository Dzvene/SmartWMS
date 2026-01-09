namespace SmartWMS.API.Modules.Inventory.DTOs;

/// <summary>
/// Product data transfer object
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Category
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryPath { get; set; }

    // Identification
    public string? Barcode { get; set; }
    public string? AlternativeBarcodes { get; set; }

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
    public bool IsActive { get; set; }

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
    public string? DefaultSupplierName { get; set; }

    // Media
    public string? ImageUrl { get; set; }

    // Calculated
    public int StockLevelCount { get; set; }
    public decimal? TotalOnHand { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request to create a new product
/// </summary>
public class CreateProductRequest
{
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Category
    public Guid? CategoryId { get; set; }

    // Identification
    public string? Barcode { get; set; }
    public string? AlternativeBarcodes { get; set; }

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
}

/// <summary>
/// Request to update a product
/// </summary>
public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }

    // Category
    public Guid? CategoryId { get; set; }

    // Identification
    public string? Barcode { get; set; }
    public string? AlternativeBarcodes { get; set; }

    // Dimensions (in millimeters)
    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }
    public int? DepthMm { get; set; }

    // Weight (in kilograms)
    public decimal? GrossWeightKg { get; set; }
    public decimal? NetWeightKg { get; set; }

    // Unit of measure
    public string? UnitOfMeasure { get; set; }
    public int? UnitsPerCase { get; set; }
    public int? CasesPerPallet { get; set; }

    // Status
    public bool? IsActive { get; set; }

    // Tracking requirements
    public bool? IsBatchTracked { get; set; }
    public bool? IsSerialTracked { get; set; }
    public bool? HasExpiryDate { get; set; }

    // Inventory levels for reordering
    public decimal? MinStockLevel { get; set; }
    public decimal? MaxStockLevel { get; set; }
    public decimal? ReorderPoint { get; set; }

    // Supplier
    public Guid? DefaultSupplierId { get; set; }

    // Media
    public string? ImageUrl { get; set; }
}
