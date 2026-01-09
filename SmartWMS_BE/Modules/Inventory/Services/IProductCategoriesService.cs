using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Inventory.DTOs;

namespace SmartWMS.API.Modules.Inventory.Services;

/// <summary>
/// Product category management service interface
/// </summary>
public interface IProductCategoriesService
{
    /// <summary>
    /// Get paginated list of categories
    /// </summary>
    Task<ApiResponse<PaginatedResult<ProductCategoryDto>>> GetCategoriesAsync(
        Guid tenantId,
        Guid? parentCategoryId = null,
        int page = 1,
        int pageSize = 25,
        string? search = null,
        bool? isActive = null);

    /// <summary>
    /// Get category tree (hierarchical structure)
    /// </summary>
    Task<ApiResponse<List<ProductCategoryTreeDto>>> GetCategoryTreeAsync(Guid tenantId);

    /// <summary>
    /// Get category by ID
    /// </summary>
    Task<ApiResponse<ProductCategoryDto>> GetCategoryByIdAsync(Guid tenantId, Guid categoryId);

    /// <summary>
    /// Create new category
    /// </summary>
    Task<ApiResponse<ProductCategoryDto>> CreateCategoryAsync(Guid tenantId, CreateProductCategoryRequest request);

    /// <summary>
    /// Update category
    /// </summary>
    Task<ApiResponse<ProductCategoryDto>> UpdateCategoryAsync(Guid tenantId, Guid categoryId, UpdateProductCategoryRequest request);

    /// <summary>
    /// Delete category
    /// </summary>
    Task<ApiResponse> DeleteCategoryAsync(Guid tenantId, Guid categoryId);
}
