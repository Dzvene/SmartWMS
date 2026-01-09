using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Warehouse.DTOs;

namespace SmartWMS.API.Modules.Warehouse.Services;

/// <summary>
/// Zone management service interface
/// </summary>
public interface IZonesService
{
    /// <summary>
    /// Get paginated list of zones
    /// </summary>
    Task<ApiResponse<PaginatedResult<ZoneDto>>> GetZonesAsync(
        Guid tenantId,
        Guid? warehouseId = null,
        int page = 1,
        int pageSize = 25,
        string? search = null,
        bool? isActive = null);

    /// <summary>
    /// Get zone by ID
    /// </summary>
    Task<ApiResponse<ZoneDto>> GetZoneByIdAsync(Guid tenantId, Guid zoneId);

    /// <summary>
    /// Create new zone
    /// </summary>
    Task<ApiResponse<ZoneDto>> CreateZoneAsync(Guid tenantId, CreateZoneRequest request);

    /// <summary>
    /// Update zone
    /// </summary>
    Task<ApiResponse<ZoneDto>> UpdateZoneAsync(Guid tenantId, Guid zoneId, UpdateZoneRequest request);

    /// <summary>
    /// Delete zone
    /// </summary>
    Task<ApiResponse> DeleteZoneAsync(Guid tenantId, Guid zoneId);
}
