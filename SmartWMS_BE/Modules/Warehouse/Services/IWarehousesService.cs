using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Warehouse.DTOs;

namespace SmartWMS.API.Modules.Warehouse.Services;

/// <summary>
/// Warehouse management service interface
/// </summary>
public interface IWarehousesService
{
    /// <summary>
    /// Get paginated list of warehouses
    /// </summary>
    Task<ApiResponse<PaginatedResult<WarehouseDto>>> GetWarehousesAsync(
        Guid tenantId,
        Guid? siteId = null,
        int page = 1,
        int pageSize = 25,
        string? search = null,
        bool? isActive = null);

    /// <summary>
    /// Get simple warehouse options for dropdowns
    /// </summary>
    Task<ApiResponse<List<WarehouseOptionDto>>> GetWarehouseOptionsAsync(Guid tenantId, Guid? siteId = null);

    /// <summary>
    /// Get warehouse by ID
    /// </summary>
    Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(Guid tenantId, Guid warehouseId);

    /// <summary>
    /// Create new warehouse
    /// </summary>
    Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(Guid tenantId, CreateWarehouseRequest request);

    /// <summary>
    /// Update warehouse
    /// </summary>
    Task<ApiResponse<WarehouseDto>> UpdateWarehouseAsync(Guid tenantId, Guid warehouseId, UpdateWarehouseRequest request);

    /// <summary>
    /// Delete warehouse
    /// </summary>
    Task<ApiResponse> DeleteWarehouseAsync(Guid tenantId, Guid warehouseId);
}
