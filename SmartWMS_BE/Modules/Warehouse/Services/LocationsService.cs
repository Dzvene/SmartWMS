using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Warehouse.DTOs;
using SmartWMS.API.Modules.Warehouse.Models;

namespace SmartWMS.API.Modules.Warehouse.Services;

/// <summary>
/// Location management service implementation
/// </summary>
public class LocationsService : ILocationsService
{
    private readonly ApplicationDbContext _context;

    public LocationsService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ApiResponse<PaginatedResult<LocationDto>>> GetLocationsAsync(
        Guid tenantId,
        Guid? warehouseId = null,
        Guid? zoneId = null,
        int page = 1,
        int pageSize = 25,
        string? search = null,
        bool? isActive = null)
    {
        var query = _context.Locations
            .Include(l => l.Warehouse)
            .Include(l => l.Zone)
            .Where(l => l.TenantId == tenantId)
            .AsQueryable();

        // Filter by warehouse
        if (warehouseId.HasValue)
        {
            query = query.Where(l => l.WarehouseId == warehouseId.Value);
        }

        // Filter by zone
        if (zoneId.HasValue)
        {
            query = query.Where(l => l.ZoneId == zoneId.Value);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(l =>
                l.Code.ToLower().Contains(searchLower) ||
                (l.Name != null && l.Name.ToLower().Contains(searchLower)) ||
                (l.Aisle != null && l.Aisle.ToLower().Contains(searchLower)));
        }

        // Filter by active status
        if (isActive.HasValue)
        {
            query = query.Where(l => l.IsActive == isActive.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination and get items
        var items = await query
            .OrderBy(l => l.Warehouse.Name)
            .ThenBy(l => l.Zone != null ? l.Zone.Name : "")
            .ThenBy(l => l.Aisle)
            .ThenBy(l => l.Rack)
            .ThenBy(l => l.Level)
            .ThenBy(l => l.Position)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LocationDto
            {
                Id = l.Id,
                Code = l.Code,
                Name = l.Name,
                WarehouseId = l.WarehouseId,
                WarehouseName = l.Warehouse.Name,
                ZoneId = l.ZoneId,
                ZoneName = l.Zone != null ? l.Zone.Name : null,
                Aisle = l.Aisle,
                Rack = l.Rack,
                Level = l.Level,
                Position = l.Position,
                LocationType = l.LocationType,
                WidthMm = l.WidthMm,
                HeightMm = l.HeightMm,
                DepthMm = l.DepthMm,
                MaxWeight = l.MaxWeight,
                MaxVolume = l.MaxVolume,
                IsActive = l.IsActive,
                IsPickLocation = l.IsPickLocation,
                IsPutawayLocation = l.IsPutawayLocation,
                IsReceivingDock = l.IsReceivingDock,
                IsShippingDock = l.IsShippingDock,
                PickSequence = l.PickSequence,
                PutawaySequence = l.PutawaySequence,
                StockLevelCount = l.StockLevels.Count,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .ToListAsync();

        var result = new PaginatedResult<LocationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return ApiResponse<PaginatedResult<LocationDto>>.Ok(result);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<LocationDto>> GetLocationByIdAsync(Guid tenantId, Guid locationId)
    {
        var location = await _context.Locations
            .Include(l => l.Warehouse)
            .Include(l => l.Zone)
            .Where(l => l.TenantId == tenantId && l.Id == locationId)
            .Select(l => new LocationDto
            {
                Id = l.Id,
                Code = l.Code,
                Name = l.Name,
                WarehouseId = l.WarehouseId,
                WarehouseName = l.Warehouse.Name,
                ZoneId = l.ZoneId,
                ZoneName = l.Zone != null ? l.Zone.Name : null,
                Aisle = l.Aisle,
                Rack = l.Rack,
                Level = l.Level,
                Position = l.Position,
                LocationType = l.LocationType,
                WidthMm = l.WidthMm,
                HeightMm = l.HeightMm,
                DepthMm = l.DepthMm,
                MaxWeight = l.MaxWeight,
                MaxVolume = l.MaxVolume,
                IsActive = l.IsActive,
                IsPickLocation = l.IsPickLocation,
                IsPutawayLocation = l.IsPutawayLocation,
                IsReceivingDock = l.IsReceivingDock,
                IsShippingDock = l.IsShippingDock,
                PickSequence = l.PickSequence,
                PutawaySequence = l.PutawaySequence,
                StockLevelCount = l.StockLevels.Count,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (location == null)
        {
            return ApiResponse<LocationDto>.Fail("Location not found");
        }

        return ApiResponse<LocationDto>.Ok(location);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<LocationDto>> CreateLocationAsync(Guid tenantId, CreateLocationRequest request)
    {
        // Validate warehouse exists and belongs to tenant
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == request.WarehouseId);

        if (warehouse == null)
        {
            return ApiResponse<LocationDto>.Fail("Warehouse not found");
        }

        // Validate zone if provided
        if (request.ZoneId.HasValue)
        {
            var zone = await _context.Zones
                .FirstOrDefaultAsync(z => z.TenantId == tenantId && z.Id == request.ZoneId.Value);

            if (zone == null)
            {
                return ApiResponse<LocationDto>.Fail("Zone not found");
            }

            if (zone.WarehouseId != request.WarehouseId)
            {
                return ApiResponse<LocationDto>.Fail("Zone does not belong to the selected warehouse");
            }
        }

        // Check if code already exists in this warehouse
        var codeExists = await _context.Locations
            .AnyAsync(l => l.TenantId == tenantId &&
                          l.WarehouseId == request.WarehouseId &&
                          l.Code == request.Code);

        if (codeExists)
        {
            return ApiResponse<LocationDto>.Fail($"Location with code '{request.Code}' already exists in this warehouse");
        }

        var location = new Location
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code,
            Name = request.Name,
            WarehouseId = request.WarehouseId,
            ZoneId = request.ZoneId,
            Aisle = request.Aisle,
            Rack = request.Rack,
            Level = request.Level,
            Position = request.Position,
            LocationType = request.LocationType,
            WidthMm = request.WidthMm,
            HeightMm = request.HeightMm,
            DepthMm = request.DepthMm,
            MaxWeight = request.MaxWeight,
            MaxVolume = request.MaxVolume,
            IsActive = request.IsActive,
            IsPickLocation = request.IsPickLocation,
            IsPutawayLocation = request.IsPutawayLocation,
            IsReceivingDock = request.IsReceivingDock,
            IsShippingDock = request.IsShippingDock,
            PickSequence = request.PickSequence,
            PutawaySequence = request.PutawaySequence,
            CreatedAt = DateTime.UtcNow
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        // Update zone location count
        if (request.ZoneId.HasValue)
        {
            await UpdateZoneLocationCountAsync(request.ZoneId.Value);
        }

        return await GetLocationByIdAsync(tenantId, location.Id);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<LocationDto>> UpdateLocationAsync(Guid tenantId, Guid locationId, UpdateLocationRequest request)
    {
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == locationId);

        if (location == null)
        {
            return ApiResponse<LocationDto>.Fail("Location not found");
        }

        var oldZoneId = location.ZoneId;

        // Validate zone if provided
        if (request.ZoneId.HasValue)
        {
            var zone = await _context.Zones
                .FirstOrDefaultAsync(z => z.TenantId == tenantId && z.Id == request.ZoneId.Value);

            if (zone == null)
            {
                return ApiResponse<LocationDto>.Fail("Zone not found");
            }

            if (zone.WarehouseId != location.WarehouseId)
            {
                return ApiResponse<LocationDto>.Fail("Zone does not belong to the location's warehouse");
            }
        }

        // Update fields if provided
        if (request.Name != null) location.Name = request.Name;
        if (request.ZoneId.HasValue) location.ZoneId = request.ZoneId;
        if (request.Aisle != null) location.Aisle = request.Aisle;
        if (request.Rack != null) location.Rack = request.Rack;
        if (request.Level != null) location.Level = request.Level;
        if (request.Position != null) location.Position = request.Position;
        if (request.LocationType.HasValue) location.LocationType = request.LocationType.Value;
        if (request.WidthMm.HasValue) location.WidthMm = request.WidthMm.Value;
        if (request.HeightMm.HasValue) location.HeightMm = request.HeightMm.Value;
        if (request.DepthMm.HasValue) location.DepthMm = request.DepthMm.Value;
        if (request.MaxWeight.HasValue) location.MaxWeight = request.MaxWeight.Value;
        if (request.MaxVolume.HasValue) location.MaxVolume = request.MaxVolume.Value;
        if (request.IsActive.HasValue) location.IsActive = request.IsActive.Value;
        if (request.IsPickLocation.HasValue) location.IsPickLocation = request.IsPickLocation.Value;
        if (request.IsPutawayLocation.HasValue) location.IsPutawayLocation = request.IsPutawayLocation.Value;
        if (request.IsReceivingDock.HasValue) location.IsReceivingDock = request.IsReceivingDock.Value;
        if (request.IsShippingDock.HasValue) location.IsShippingDock = request.IsShippingDock.Value;
        if (request.PickSequence.HasValue) location.PickSequence = request.PickSequence.Value;
        if (request.PutawaySequence.HasValue) location.PutawaySequence = request.PutawaySequence.Value;

        location.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Update zone location counts if zone changed
        if (oldZoneId != location.ZoneId)
        {
            if (oldZoneId.HasValue)
            {
                await UpdateZoneLocationCountAsync(oldZoneId.Value);
            }
            if (location.ZoneId.HasValue)
            {
                await UpdateZoneLocationCountAsync(location.ZoneId.Value);
            }
        }

        return await GetLocationByIdAsync(tenantId, locationId);
    }

    /// <inheritdoc />
    public async Task<ApiResponse> DeleteLocationAsync(Guid tenantId, Guid locationId)
    {
        var location = await _context.Locations
            .Include(l => l.StockLevels)
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == locationId);

        if (location == null)
        {
            return ApiResponse.Fail("Location not found");
        }

        // Cannot delete location with stock
        if (location.StockLevels.Any())
        {
            return ApiResponse.Fail($"Cannot delete location with {location.StockLevels.Count} stock level(s). Remove stock first.");
        }

        var zoneId = location.ZoneId;

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();

        // Update zone location count
        if (zoneId.HasValue)
        {
            await UpdateZoneLocationCountAsync(zoneId.Value);
        }

        return ApiResponse.Ok("Location deleted successfully");
    }

    /// <summary>
    /// Update zone's location count
    /// </summary>
    private async Task UpdateZoneLocationCountAsync(Guid zoneId)
    {
        var zone = await _context.Zones.FindAsync(zoneId);
        if (zone != null)
        {
            zone.LocationCount = await _context.Locations
                .CountAsync(l => l.ZoneId == zoneId);
            await _context.SaveChangesAsync();
        }
    }
}
