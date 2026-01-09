using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Warehouse.DTOs;
using SmartWMS.API.Modules.Warehouse.Models;

namespace SmartWMS.API.Modules.Warehouse.Services;

/// <summary>
/// Warehouse management service implementation
/// </summary>
public class WarehousesService : IWarehousesService
{
    private readonly ApplicationDbContext _context;

    public WarehousesService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ApiResponse<PaginatedResult<WarehouseDto>>> GetWarehousesAsync(
        Guid tenantId,
        Guid? siteId = null,
        int page = 1,
        int pageSize = 25,
        string? search = null,
        bool? isActive = null)
    {
        var query = _context.Warehouses
            .Where(w => w.TenantId == tenantId)
            .AsQueryable();

        // Filter by site
        if (siteId.HasValue)
        {
            query = query.Where(w => w.SiteId == siteId.Value);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(w =>
                w.Name.ToLower().Contains(searchLower) ||
                w.Code.ToLower().Contains(searchLower) ||
                (w.City != null && w.City.ToLower().Contains(searchLower)));
        }

        // Filter by active status
        if (isActive.HasValue)
        {
            query = query.Where(w => w.IsActive == isActive.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination and get items
        var items = await query
            .OrderBy(w => w.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new WarehouseDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name,
                Description = w.Description,
                SiteId = w.SiteId,
                AddressLine1 = w.AddressLine1,
                AddressLine2 = w.AddressLine2,
                City = w.City,
                Region = w.Region,
                PostalCode = w.PostalCode,
                CountryCode = w.CountryCode,
                Timezone = w.Timezone,
                IsPrimary = w.IsPrimary,
                IsActive = w.IsActive,
                ZoneCount = w.Zones.Count,
                LocationCount = w.Locations.Count,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            })
            .ToListAsync();

        var result = new PaginatedResult<WarehouseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return ApiResponse<PaginatedResult<WarehouseDto>>.Ok(result);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<List<WarehouseOptionDto>>> GetWarehouseOptionsAsync(Guid tenantId, Guid? siteId = null)
    {
        var query = _context.Warehouses
            .Where(w => w.TenantId == tenantId && w.IsActive)
            .AsQueryable();

        if (siteId.HasValue)
        {
            query = query.Where(w => w.SiteId == siteId.Value);
        }

        var options = await query
            .OrderBy(w => w.Name)
            .Select(w => new WarehouseOptionDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name
            })
            .ToListAsync();

        return ApiResponse<List<WarehouseOptionDto>>.Ok(options);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(Guid tenantId, Guid warehouseId)
    {
        var warehouse = await _context.Warehouses
            .Where(w => w.TenantId == tenantId && w.Id == warehouseId)
            .Select(w => new WarehouseDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name,
                Description = w.Description,
                SiteId = w.SiteId,
                AddressLine1 = w.AddressLine1,
                AddressLine2 = w.AddressLine2,
                City = w.City,
                Region = w.Region,
                PostalCode = w.PostalCode,
                CountryCode = w.CountryCode,
                Timezone = w.Timezone,
                IsPrimary = w.IsPrimary,
                IsActive = w.IsActive,
                ZoneCount = w.Zones.Count,
                LocationCount = w.Locations.Count,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (warehouse == null)
        {
            return ApiResponse<WarehouseDto>.Fail("Warehouse not found");
        }

        return ApiResponse<WarehouseDto>.Ok(warehouse);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(Guid tenantId, CreateWarehouseRequest request)
    {
        // Validate site exists and belongs to tenant
        var site = await _context.Sites
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == request.SiteId);

        if (site == null)
        {
            return ApiResponse<WarehouseDto>.Fail("Site not found");
        }

        // Check if code already exists
        var codeExists = await _context.Warehouses
            .AnyAsync(w => w.TenantId == tenantId && w.Code == request.Code);

        if (codeExists)
        {
            return ApiResponse<WarehouseDto>.Fail($"Warehouse with code '{request.Code}' already exists");
        }

        // If this is set as primary, unset other primary warehouses in this site
        if (request.IsPrimary)
        {
            await UnsetPrimaryWarehousesAsync(tenantId, request.SiteId);
        }

        var warehouse = new Models.Warehouse
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            SiteId = request.SiteId,
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

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();

        return await GetWarehouseByIdAsync(tenantId, warehouse.Id);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<WarehouseDto>> UpdateWarehouseAsync(Guid tenantId, Guid warehouseId, UpdateWarehouseRequest request)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == warehouseId);

        if (warehouse == null)
        {
            return ApiResponse<WarehouseDto>.Fail("Warehouse not found");
        }

        // If this is being set as primary, unset other primary warehouses in this site
        if (request.IsPrimary == true && !warehouse.IsPrimary)
        {
            await UnsetPrimaryWarehousesAsync(tenantId, warehouse.SiteId);
        }

        // Update fields if provided
        if (request.Name != null) warehouse.Name = request.Name;
        if (request.Description != null) warehouse.Description = request.Description;
        if (request.AddressLine1 != null) warehouse.AddressLine1 = request.AddressLine1;
        if (request.AddressLine2 != null) warehouse.AddressLine2 = request.AddressLine2;
        if (request.City != null) warehouse.City = request.City;
        if (request.Region != null) warehouse.Region = request.Region;
        if (request.PostalCode != null) warehouse.PostalCode = request.PostalCode;
        if (request.CountryCode != null) warehouse.CountryCode = request.CountryCode;
        if (request.Timezone != null) warehouse.Timezone = request.Timezone;
        if (request.IsPrimary.HasValue) warehouse.IsPrimary = request.IsPrimary.Value;
        if (request.IsActive.HasValue) warehouse.IsActive = request.IsActive.Value;

        warehouse.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetWarehouseByIdAsync(tenantId, warehouseId);
    }

    /// <inheritdoc />
    public async Task<ApiResponse> DeleteWarehouseAsync(Guid tenantId, Guid warehouseId)
    {
        var warehouse = await _context.Warehouses
            .Include(w => w.Zones)
            .Include(w => w.Locations)
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == warehouseId);

        if (warehouse == null)
        {
            return ApiResponse.Fail("Warehouse not found");
        }

        // Cannot delete primary warehouse
        if (warehouse.IsPrimary)
        {
            return ApiResponse.Fail("Cannot delete primary warehouse. Set another warehouse as primary first.");
        }

        // Cannot delete warehouse with zones
        if (warehouse.Zones.Any())
        {
            return ApiResponse.Fail($"Cannot delete warehouse with {warehouse.Zones.Count} zone(s). Remove zones first.");
        }

        // Cannot delete warehouse with locations
        if (warehouse.Locations.Any())
        {
            return ApiResponse.Fail($"Cannot delete warehouse with {warehouse.Locations.Count} location(s). Remove locations first.");
        }

        _context.Warehouses.Remove(warehouse);
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Warehouse deleted successfully");
    }

    /// <summary>
    /// Unset all primary warehouses in a site
    /// </summary>
    private async Task UnsetPrimaryWarehousesAsync(Guid tenantId, Guid siteId)
    {
        var primaryWarehouses = await _context.Warehouses
            .Where(w => w.TenantId == tenantId && w.SiteId == siteId && w.IsPrimary)
            .ToListAsync();

        foreach (var warehouse in primaryWarehouses)
        {
            warehouse.IsPrimary = false;
            warehouse.UpdatedAt = DateTime.UtcNow;
        }
    }
}
