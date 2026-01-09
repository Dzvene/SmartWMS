namespace SmartWMS.API.Modules.Inventory.DTOs;

/// <summary>
/// Product category data transfer object
/// </summary>
public class ProductCategoryDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Hierarchy
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public int Level { get; set; }
    public string? Path { get; set; }

    public bool IsActive { get; set; }

    // Product Defaults
    public string? DefaultUnitOfMeasure { get; set; }
    public string? DefaultStorageZoneType { get; set; }

    // Tracking Requirements
    public bool RequiresBatchTracking { get; set; }
    public bool RequiresSerialTracking { get; set; }
    public bool RequiresExpiryDate { get; set; }

    // Handling & Storage
    public string? HandlingInstructions { get; set; }
    public decimal? MinTemperature { get; set; }
    public decimal? MaxTemperature { get; set; }
    public bool IsHazardous { get; set; }
    public bool IsFragile { get; set; }

    // Counts
    public int ProductCount { get; set; }
    public int ChildCategoryCount { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Product category tree node for hierarchical display
/// </summary>
public class ProductCategoryTreeDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public int Level { get; set; }
    public int ProductCount { get; set; }
    public bool IsActive { get; set; }

    // Include requirements for UI to show indicators
    public bool RequiresBatchTracking { get; set; }
    public bool RequiresSerialTracking { get; set; }
    public bool RequiresExpiryDate { get; set; }

    public List<ProductCategoryTreeDto> Children { get; set; } = new();
}

/// <summary>
/// Request to create a new product category
/// </summary>
public class CreateProductCategoryRequest
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; } = true;

    // Product Defaults
    public string? DefaultUnitOfMeasure { get; set; }
    public string? DefaultStorageZoneType { get; set; }

    // Tracking Requirements
    public bool RequiresBatchTracking { get; set; }
    public bool RequiresSerialTracking { get; set; }
    public bool RequiresExpiryDate { get; set; }

    // Handling & Storage
    public string? HandlingInstructions { get; set; }
    public decimal? MinTemperature { get; set; }
    public decimal? MaxTemperature { get; set; }
    public bool IsHazardous { get; set; }
    public bool IsFragile { get; set; }
}

/// <summary>
/// Request to update a product category
/// </summary>
public class UpdateProductCategoryRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool? IsActive { get; set; }

    // Product Defaults
    public string? DefaultUnitOfMeasure { get; set; }
    public string? DefaultStorageZoneType { get; set; }

    // Tracking Requirements
    public bool? RequiresBatchTracking { get; set; }
    public bool? RequiresSerialTracking { get; set; }
    public bool? RequiresExpiryDate { get; set; }

    // Handling & Storage
    public string? HandlingInstructions { get; set; }
    public decimal? MinTemperature { get; set; }
    public decimal? MaxTemperature { get; set; }
    public bool? IsHazardous { get; set; }
    public bool? IsFragile { get; set; }
}
