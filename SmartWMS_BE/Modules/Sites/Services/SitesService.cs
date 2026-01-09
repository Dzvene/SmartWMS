using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Sites.DTOs;
using SmartWMS.API.Modules.Sites.Models;

namespace SmartWMS.API.Modules.Sites.Services;

/// <summary>
/// Sites management service implementation
/// </summary>
public class SitesService : ISitesService
{
    private readonly ApplicationDbContext _context;

    public SitesService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ApiResponse<PaginatedResult<SiteDto>>> GetSitesAsync(
        Guid tenantId,
        int page = 1,
        int pageSize = 25,
        string? search = null,
        bool? isActive = null)
    {
        var query = _context.Sites
            .Where(s => s.TenantId == tenantId)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(searchLower) ||
                s.Code.ToLower().Contains(searchLower) ||
                (s.City != null && s.City.ToLower().Contains(searchLower)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination and get items
        var items = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SiteDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                AddressLine1 = s.AddressLine1,
                AddressLine2 = s.AddressLine2,
                City = s.City,
                Region = s.Region,
                PostalCode = s.PostalCode,
                CountryCode = s.CountryCode,
                Timezone = s.Timezone,
                IsActive = s.IsActive,
                IsPrimary = s.IsPrimary,
                WarehousesCount = s.Warehouses.Count,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();

        var result = new PaginatedResult<SiteDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return ApiResponse<PaginatedResult<SiteDto>>.Ok(result);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<SiteDto>> GetSiteByIdAsync(Guid tenantId, Guid siteId)
    {
        var site = await _context.Sites
            .Where(s => s.TenantId == tenantId && s.Id == siteId)
            .Select(s => new SiteDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                AddressLine1 = s.AddressLine1,
                AddressLine2 = s.AddressLine2,
                City = s.City,
                Region = s.Region,
                PostalCode = s.PostalCode,
                CountryCode = s.CountryCode,
                Timezone = s.Timezone,
                IsActive = s.IsActive,
                IsPrimary = s.IsPrimary,
                WarehousesCount = s.Warehouses.Count,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (site == null)
        {
            return ApiResponse<SiteDto>.Fail("Site not found");
        }

        return ApiResponse<SiteDto>.Ok(site);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<SiteDto>> CreateSiteAsync(Guid tenantId, CreateSiteRequest request)
    {
        // Check if code already exists
        var codeExists = await _context.Sites
            .AnyAsync(s => s.TenantId == tenantId && s.Code == request.Code);

        if (codeExists)
        {
            return ApiResponse<SiteDto>.Fail($"Site with code '{request.Code}' already exists");
        }

        // If this is set as primary, unset other primary sites
        if (request.IsPrimary)
        {
            await UnsetPrimarySitesAsync(tenantId);
        }

        var site = new Site
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = tenantId, // CompanyId is same as TenantId
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            Region = request.Region,
            PostalCode = request.PostalCode,
            CountryCode = request.CountryCode,
            Timezone = request.Timezone,
            IsPrimary = request.IsPrimary,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Sites.Add(site);
        await _context.SaveChangesAsync();

        return await GetSiteByIdAsync(tenantId, site.Id);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<SiteDto>> UpdateSiteAsync(Guid tenantId, Guid siteId, UpdateSiteRequest request)
    {
        var site = await _context.Sites
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == siteId);

        if (site == null)
        {
            return ApiResponse<SiteDto>.Fail("Site not found");
        }

        // If this is being set as primary, unset other primary sites
        if (request.IsPrimary == true && !site.IsPrimary)
        {
            await UnsetPrimarySitesAsync(tenantId);
        }

        // Update fields if provided
        if (request.Name != null) site.Name = request.Name;
        if (request.Description != null) site.Description = request.Description;
        if (request.AddressLine1 != null) site.AddressLine1 = request.AddressLine1;
        if (request.AddressLine2 != null) site.AddressLine2 = request.AddressLine2;
        if (request.City != null) site.City = request.City;
        if (request.Region != null) site.Region = request.Region;
        if (request.PostalCode != null) site.PostalCode = request.PostalCode;
        if (request.CountryCode != null) site.CountryCode = request.CountryCode;
        if (request.Timezone != null) site.Timezone = request.Timezone;
        if (request.IsPrimary.HasValue) site.IsPrimary = request.IsPrimary.Value;
        if (request.IsActive.HasValue) site.IsActive = request.IsActive.Value;

        site.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetSiteByIdAsync(tenantId, siteId);
    }

    /// <inheritdoc />
    public async Task<ApiResponse> DeleteSiteAsync(Guid tenantId, Guid siteId)
    {
        var site = await _context.Sites
            .Include(s => s.Warehouses)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == siteId);

        if (site == null)
        {
            return ApiResponse.Fail("Site not found");
        }

        // Cannot delete primary site
        if (site.IsPrimary)
        {
            return ApiResponse.Fail("Cannot delete primary site. Set another site as primary first.");
        }

        // Cannot delete site with warehouses
        if (site.Warehouses.Any())
        {
            return ApiResponse.Fail($"Cannot delete site with {site.Warehouses.Count} warehouse(s). Remove warehouses first.");
        }

        _context.Sites.Remove(site);
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Site deleted successfully");
    }

    /// <summary>
    /// Unset all primary sites for a tenant
    /// </summary>
    private async Task UnsetPrimarySitesAsync(Guid tenantId)
    {
        var primarySites = await _context.Sites
            .Where(s => s.TenantId == tenantId && s.IsPrimary)
            .ToListAsync();

        foreach (var site in primarySites)
        {
            site.IsPrimary = false;
            site.UpdatedAt = DateTime.UtcNow;
        }
    }
}
