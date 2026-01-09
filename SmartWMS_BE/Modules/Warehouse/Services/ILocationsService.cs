using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Warehouse.DTOs;

namespace SmartWMS.API.Modules.Warehouse.Services;

/// <summary>
/// Location management service interface
/// </summary>
public interface ILocationsService
{
    /// <summary>
    /// Get paginated list of locations
    /// </summary>
    Task<ApiResponse<PaginatedResult<LocationDto>>> GetLocationsAsync(
        Guid tenantId,
        Guid? warehouseId = null,
        Guid? zoneId = null,
        int page = 1,
        int pageSize = 25,
        string? search = null,
        bool? isActive = null);

    /// <summary>
    /// Get location by ID
    /// </summary>
    Task<ApiResponse<LocationDto>> GetLocationByIdAsync(Guid tenantId, Guid locationId);

    /// <summary>
    /// Create new location
    /// </summary>
    Task<ApiResponse<LocationDto>> CreateLocationAsync(Guid tenantId, CreateLocationRequest request);

    /// <summary>
    /// Update location
    /// </summary>
    Task<ApiResponse<LocationDto>> UpdateLocationAsync(Guid tenantId, Guid locationId, UpdateLocationRequest request);

    /// <summary>
    /// Delete location
    /// </summary>
    Task<ApiResponse> DeleteLocationAsync(Guid tenantId, Guid locationId);
}
