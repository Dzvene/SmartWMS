using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Sites.DTOs;

namespace SmartWMS.API.Modules.Sites.Services;

/// <summary>
/// Sites management service interface
/// </summary>
public interface ISitesService
{
    /// <summary>
    /// Get paginated list of sites for a tenant
    /// </summary>
    Task<ApiResponse<PaginatedResult<SiteDto>>> GetSitesAsync(
        Guid tenantId,
        int page = 1,
        int pageSize = 25,
        string? search = null,
        bool? isActive = null);

    /// <summary>
    /// Get site by ID
    /// </summary>
    Task<ApiResponse<SiteDto>> GetSiteByIdAsync(Guid tenantId, Guid siteId);

    /// <summary>
    /// Create a new site
    /// </summary>
    Task<ApiResponse<SiteDto>> CreateSiteAsync(Guid tenantId, CreateSiteRequest request);

    /// <summary>
    /// Update an existing site
    /// </summary>
    Task<ApiResponse<SiteDto>> UpdateSiteAsync(Guid tenantId, Guid siteId, UpdateSiteRequest request);

    /// <summary>
    /// Delete a site
    /// </summary>
    Task<ApiResponse> DeleteSiteAsync(Guid tenantId, Guid siteId);
}
