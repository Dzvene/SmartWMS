using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Warehouse.DTOs;
using SmartWMS.API.Modules.Warehouse.Models;

namespace SmartWMS.API.Modules.Warehouse.Services;

/// <summary>
/// Zone management service implementation
/// </summary>
public class ZonesService : IZonesService
{
    private readonly ApplicationDbContext _context;

    public ZonesService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ApiResponse<PaginatedResult<ZoneDto>>> GetZonesAsync(
        Guid tenantId,
        Guid? warehouseId = null,
        int page = 1,
        int pageSize = 25,
        string? search = null,
        bool? isActive = null)
    {
        var query = _context.Zones
            .Include(z => z.Warehouse)
            .Where(z => z.TenantId == tenantId)
            .AsQueryable();

        // Filter by warehouse
        if (warehouseId.HasValue)
        {
            query = query.Where(z => z.WarehouseId == warehouseId.Value);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(z =>
                z.Name.ToLower().Contains(searchLower) ||
                z.Code.ToLower().Contains(searchLower) ||
                (z.Description != null && z.Description.ToLower().Contains(searchLower)));
        }

        // Filter by active status
        if (isActive.HasValue)
        {
            query = query.Where(z => z.IsActive == isActive.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination and get items
        var items = await query
            .OrderBy(z => z.Warehouse.Name)
            .ThenBy(z => z.PickSequence ?? int.MaxValue)
            .ThenBy(z => z.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(z => new ZoneDto
            {
                Id = z.Id,
                Code = z.Code,
                Name = z.Name,
                Description = z.Description,
                WarehouseId = z.WarehouseId,
                WarehouseName = z.Warehouse.Name,
                ZoneType = z.ZoneType,
                PickSequence = z.PickSequence,
                IsActive = z.IsActive,
                LocationCount = z.LocationCount,
                CreatedAt = z.CreatedAt,
                UpdatedAt = z.UpdatedAt
            })
            .ToListAsync();

        var result = new PaginatedResult<ZoneDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return ApiResponse<PaginatedResult<ZoneDto>>.Ok(result);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<ZoneDto>> GetZoneByIdAsync(Guid tenantId, Guid zoneId)
    {
        var zone = await _context.Zones
            .Include(z => z.Warehouse)
            .Where(z => z.TenantId == tenantId && z.Id == zoneId)
            .Select(z => new ZoneDto
            {
                Id = z.Id,
                Code = z.Code,
                Name = z.Name,
                Description = z.Description,
                WarehouseId = z.WarehouseId,
                WarehouseName = z.Warehouse.Name,
                ZoneType = z.ZoneType,
                PickSequence = z.PickSequence,
                IsActive = z.IsActive,
                LocationCount = z.LocationCount,
                CreatedAt = z.CreatedAt,
                UpdatedAt = z.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (zone == null)
        {
            return ApiResponse<ZoneDto>.Fail("Zone not found");
        }

        return ApiResponse<ZoneDto>.Ok(zone);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<ZoneDto>> CreateZoneAsync(Guid tenantId, CreateZoneRequest request)
    {
        // Validate warehouse exists and belongs to tenant
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == request.WarehouseId);

        if (warehouse == null)
        {
            return ApiResponse<ZoneDto>.Fail("Warehouse not found");
        }

        // Check if code already exists in this warehouse
        var codeExists = await _context.Zones
            .AnyAsync(z => z.TenantId == tenantId &&
                          z.WarehouseId == request.WarehouseId &&
                          z.Code == request.Code);

        if (codeExists)
        {
            return ApiResponse<ZoneDto>.Fail($"Zone with code '{request.Code}' already exists in this warehouse");
        }

        var zone = new Zone
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            WarehouseId = request.WarehouseId,
            ZoneType = request.ZoneType,
            PickSequence = request.PickSequence,
            IsActive = request.IsActive,
            LocationCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Zones.Add(zone);
        await _context.SaveChangesAsync();

        return await GetZoneByIdAsync(tenantId, zone.Id);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<ZoneDto>> UpdateZoneAsync(Guid tenantId, Guid zoneId, UpdateZoneRequest request)
    {
        var zone = await _context.Zones
            .FirstOrDefaultAsync(z => z.TenantId == tenantId && z.Id == zoneId);

        if (zone == null)
        {
            return ApiResponse<ZoneDto>.Fail("Zone not found");
        }

        // Update fields if provided
        if (request.Name != null) zone.Name = request.Name;
        if (request.Description != null) zone.Description = request.Description;
        if (request.ZoneType.HasValue) zone.ZoneType = request.ZoneType.Value;
        if (request.PickSequence.HasValue) zone.PickSequence = request.PickSequence.Value;
        if (request.IsActive.HasValue) zone.IsActive = request.IsActive.Value;

        zone.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetZoneByIdAsync(tenantId, zoneId);
    }

    /// <inheritdoc />
    public async Task<ApiResponse> DeleteZoneAsync(Guid tenantId, Guid zoneId)
    {
        var zone = await _context.Zones
            .Include(z => z.Locations)
            .FirstOrDefaultAsync(z => z.TenantId == tenantId && z.Id == zoneId);

        if (zone == null)
        {
            return ApiResponse.Fail("Zone not found");
        }

        // Cannot delete zone with locations
        if (zone.Locations.Any())
        {
            return ApiResponse.Fail($"Cannot delete zone with {zone.Locations.Count} location(s). Remove locations first.");
        }

        _context.Zones.Remove(zone);
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Zone deleted successfully");
    }
}
