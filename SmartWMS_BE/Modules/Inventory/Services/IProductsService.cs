using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Inventory.DTOs;

namespace SmartWMS.API.Modules.Inventory.Services;

/// <summary>
/// Product management service interface
/// </summary>
public interface IProductsService
{
    /// <summary>
    /// Get paginated list of products
    /// </summary>
    Task<ApiResponse<PaginatedResult<ProductDto>>> GetProductsAsync(
        Guid tenantId,
        Guid? categoryId = null,
        int page = 1,
        int pageSize = 25,
        string? search = null,
        bool? isActive = null);

    /// <summary>
    /// Get product by ID
    /// </summary>
    Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid tenantId, Guid productId);

    /// <summary>
    /// Get product by SKU
    /// </summary>
    Task<ApiResponse<ProductDto>> GetProductBySkuAsync(Guid tenantId, string sku);

    /// <summary>
    /// Get product by barcode
    /// </summary>
    Task<ApiResponse<ProductDto>> GetProductByBarcodeAsync(Guid tenantId, string barcode);

    /// <summary>
    /// Create new product
    /// </summary>
    Task<ApiResponse<ProductDto>> CreateProductAsync(Guid tenantId, CreateProductRequest request);

    /// <summary>
    /// Update product
    /// </summary>
    Task<ApiResponse<ProductDto>> UpdateProductAsync(Guid tenantId, Guid productId, UpdateProductRequest request);

    /// <summary>
    /// Delete product
    /// </summary>
    Task<ApiResponse> DeleteProductAsync(Guid tenantId, Guid productId);
}
