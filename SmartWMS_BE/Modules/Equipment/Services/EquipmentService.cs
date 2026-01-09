using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Equipment.DTOs;
using SmartWMS.API.Modules.Equipment.Models;

namespace SmartWMS.API.Modules.Equipment.Services;

public class EquipmentService : IEquipmentService
{
    private readonly ApplicationDbContext _context;

    public EquipmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PaginatedResult<EquipmentDto>>> GetEquipmentAsync(
        Guid tenantId,
        EquipmentFilters? filters = null,
        int page = 1,
        int pageSize = 25)
    {
        var query = _context.Equipment
            .Include(e => e.Warehouse)
            .Include(e => e.Zone)
            .Where(e => e.TenantId == tenantId)
            .AsQueryable();

        // Apply filters
        if (filters != null)
        {
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var search = filters.Search.ToLower();
                query = query.Where(e =>
                    e.Code.ToLower().Contains(search) ||
                    e.Name.ToLower().Contains(search) ||
                    (e.SerialNumber != null && e.SerialNumber.ToLower().Contains(search)));
            }

            if (filters.Type.HasValue)
                query = query.Where(e => e.Type == filters.Type.Value);

            if (filters.Status.HasValue)
                query = query.Where(e => e.Status == filters.Status.Value);

            if (filters.WarehouseId.HasValue)
                query = query.Where(e => e.WarehouseId == filters.WarehouseId.Value);

            if (filters.ZoneId.HasValue)
                query = query.Where(e => e.ZoneId == filters.ZoneId.Value);

            if (filters.AssignedToUserId.HasValue)
                query = query.Where(e => e.AssignedToUserId == filters.AssignedToUserId.Value);

            if (filters.IsAssigned.HasValue)
                query = filters.IsAssigned.Value
                    ? query.Where(e => e.AssignedToUserId != null)
                    : query.Where(e => e.AssignedToUserId == null);

            if (filters.IsActive.HasValue)
                query = query.Where(e => e.IsActive == filters.IsActive.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(e => e.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => MapToDto(e))
            .ToListAsync();

        var result = new PaginatedResult<EquipmentDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<EquipmentDto>>.Ok(result);
    }

    public async Task<ApiResponse<EquipmentDto>> GetEquipmentByIdAsync(Guid tenantId, Guid id)
    {
        var equipment = await _context.Equipment
            .Include(e => e.Warehouse)
            .Include(e => e.Zone)
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id);

        if (equipment == null)
            return ApiResponse<EquipmentDto>.Fail("Equipment not found");

        return ApiResponse<EquipmentDto>.Ok(MapToDto(equipment));
    }

    public async Task<ApiResponse<EquipmentDto>> GetEquipmentByCodeAsync(Guid tenantId, string code)
    {
        var equipment = await _context.Equipment
            .Include(e => e.Warehouse)
            .Include(e => e.Zone)
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Code == code);

        if (equipment == null)
            return ApiResponse<EquipmentDto>.Fail("Equipment not found");

        return ApiResponse<EquipmentDto>.Ok(MapToDto(equipment));
    }

    public async Task<ApiResponse<EquipmentDto>> CreateEquipmentAsync(Guid tenantId, CreateEquipmentRequest request)
    {
        // Check for duplicate code
        var existingCode = await _context.Equipment
            .AnyAsync(e => e.TenantId == tenantId && e.Code == request.Code);

        if (existingCode)
            return ApiResponse<EquipmentDto>.Fail($"Equipment with code '{request.Code}' already exists");

        // Validate warehouse if provided
        if (request.WarehouseId.HasValue)
        {
            var warehouseExists = await _context.Warehouses
                .AnyAsync(w => w.TenantId == tenantId && w.Id == request.WarehouseId.Value);

            if (!warehouseExists)
                return ApiResponse<EquipmentDto>.Fail("Warehouse not found");
        }

        // Validate zone if provided
        if (request.ZoneId.HasValue)
        {
            var zoneExists = await _context.Zones
                .AnyAsync(z => z.TenantId == tenantId && z.Id == request.ZoneId.Value);

            if (!zoneExists)
                return ApiResponse<EquipmentDto>.Fail("Zone not found");
        }

        // Validate JSON specifications
        if (!string.IsNullOrWhiteSpace(request.Specifications))
        {
            try
            {
                JsonDocument.Parse(request.Specifications);
            }
            catch (JsonException)
            {
                return ApiResponse<EquipmentDto>.Fail("Invalid JSON format for specifications");
            }
        }

        // Get assigned user name if userId provided
        string? assignedUserName = null;
        if (request.AssignedToUserId.HasValue)
        {
            var user = await _context.Users.FindAsync(request.AssignedToUserId.Value);
            assignedUserName = user?.FullName;
        }

        var equipment = new Models.Equipment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Status = request.Status,
            WarehouseId = request.WarehouseId,
            ZoneId = request.ZoneId,
            AssignedToUserId = request.AssignedToUserId,
            AssignedToUserName = assignedUserName,
            LastMaintenanceDate = request.LastMaintenanceDate,
            NextMaintenanceDate = request.NextMaintenanceDate,
            MaintenanceNotes = request.MaintenanceNotes,
            Specifications = request.Specifications,
            SerialNumber = request.SerialNumber,
            Manufacturer = request.Manufacturer,
            Model = request.Model,
            PurchaseDate = request.PurchaseDate,
            WarrantyExpiryDate = request.WarrantyExpiryDate,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Equipment.Add(equipment);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(equipment).Reference(e => e.Warehouse).LoadAsync();
        await _context.Entry(equipment).Reference(e => e.Zone).LoadAsync();

        return ApiResponse<EquipmentDto>.Ok(MapToDto(equipment), "Equipment created successfully");
    }

    public async Task<ApiResponse<EquipmentDto>> UpdateEquipmentAsync(Guid tenantId, Guid id, UpdateEquipmentRequest request)
    {
        var equipment = await _context.Equipment
            .Include(e => e.Warehouse)
            .Include(e => e.Zone)
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id);

        if (equipment == null)
            return ApiResponse<EquipmentDto>.Fail("Equipment not found");

        // Validate warehouse if provided
        if (request.WarehouseId.HasValue)
        {
            var warehouseExists = await _context.Warehouses
                .AnyAsync(w => w.TenantId == tenantId && w.Id == request.WarehouseId.Value);

            if (!warehouseExists)
                return ApiResponse<EquipmentDto>.Fail("Warehouse not found");
        }

        // Validate zone if provided
        if (request.ZoneId.HasValue)
        {
            var zoneExists = await _context.Zones
                .AnyAsync(z => z.TenantId == tenantId && z.Id == request.ZoneId.Value);

            if (!zoneExists)
                return ApiResponse<EquipmentDto>.Fail("Zone not found");
        }

        // Validate JSON specifications
        if (!string.IsNullOrWhiteSpace(request.Specifications))
        {
            try
            {
                JsonDocument.Parse(request.Specifications);
            }
            catch (JsonException)
            {
                return ApiResponse<EquipmentDto>.Fail("Invalid JSON format for specifications");
            }
        }

        // Update fields
        if (request.Name != null) equipment.Name = request.Name;
        if (request.Description != null) equipment.Description = request.Description;
        if (request.Type.HasValue) equipment.Type = request.Type.Value;
        if (request.Status.HasValue) equipment.Status = request.Status.Value;
        if (request.WarehouseId.HasValue) equipment.WarehouseId = request.WarehouseId;
        if (request.ZoneId.HasValue) equipment.ZoneId = request.ZoneId;

        if (request.AssignedToUserId.HasValue)
        {
            equipment.AssignedToUserId = request.AssignedToUserId;
            var user = await _context.Users.FindAsync(request.AssignedToUserId.Value);
            equipment.AssignedToUserName = user?.FullName;
        }

        if (request.LastMaintenanceDate.HasValue) equipment.LastMaintenanceDate = request.LastMaintenanceDate;
        if (request.NextMaintenanceDate.HasValue) equipment.NextMaintenanceDate = request.NextMaintenanceDate;
        if (request.MaintenanceNotes != null) equipment.MaintenanceNotes = request.MaintenanceNotes;
        if (request.Specifications != null) equipment.Specifications = request.Specifications;
        if (request.SerialNumber != null) equipment.SerialNumber = request.SerialNumber;
        if (request.Manufacturer != null) equipment.Manufacturer = request.Manufacturer;
        if (request.Model != null) equipment.Model = request.Model;
        if (request.PurchaseDate.HasValue) equipment.PurchaseDate = request.PurchaseDate;
        if (request.WarrantyExpiryDate.HasValue) equipment.WarrantyExpiryDate = request.WarrantyExpiryDate;
        if (request.IsActive.HasValue) equipment.IsActive = request.IsActive.Value;

        equipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<EquipmentDto>.Ok(MapToDto(equipment), "Equipment updated successfully");
    }

    public async Task<ApiResponse> DeleteEquipmentAsync(Guid tenantId, Guid id)
    {
        var equipment = await _context.Equipment
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id);

        if (equipment == null)
            return ApiResponse.Fail("Equipment not found");

        _context.Equipment.Remove(equipment);
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Equipment deleted successfully");
    }

    public async Task<ApiResponse<EquipmentDto>> AssignEquipmentAsync(Guid tenantId, Guid id, AssignEquipmentRequest request)
    {
        var equipment = await _context.Equipment
            .Include(e => e.Warehouse)
            .Include(e => e.Zone)
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id);

        if (equipment == null)
            return ApiResponse<EquipmentDto>.Fail("Equipment not found");

        if (request.UserId.HasValue)
        {
            var user = await _context.Users.FindAsync(request.UserId.Value);
            if (user == null)
                return ApiResponse<EquipmentDto>.Fail("User not found");

            equipment.AssignedToUserId = request.UserId;
            equipment.AssignedToUserName = user.FullName;
            equipment.Status = EquipmentStatus.InUse;
        }
        else
        {
            // Unassign
            equipment.AssignedToUserId = null;
            equipment.AssignedToUserName = null;
            equipment.Status = EquipmentStatus.Available;
        }

        equipment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<EquipmentDto>.Ok(MapToDto(equipment),
            request.UserId.HasValue ? "Equipment assigned successfully" : "Equipment unassigned successfully");
    }

    public async Task<ApiResponse<EquipmentDto>> UpdateEquipmentStatusAsync(Guid tenantId, Guid id, UpdateEquipmentStatusRequest request)
    {
        var equipment = await _context.Equipment
            .Include(e => e.Warehouse)
            .Include(e => e.Zone)
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id);

        if (equipment == null)
            return ApiResponse<EquipmentDto>.Fail("Equipment not found");

        equipment.Status = request.Status;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            equipment.MaintenanceNotes = request.Notes;
        }

        // If set to maintenance, record the date
        if (request.Status == EquipmentStatus.Maintenance)
        {
            equipment.LastMaintenanceDate = DateTime.UtcNow;
        }

        equipment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<EquipmentDto>.Ok(MapToDto(equipment), "Equipment status updated successfully");
    }

    public async Task<ApiResponse<IEnumerable<EquipmentDto>>> GetAvailableEquipmentAsync(
        Guid tenantId,
        EquipmentType? type = null,
        Guid? warehouseId = null)
    {
        var query = _context.Equipment
            .Include(e => e.Warehouse)
            .Include(e => e.Zone)
            .Where(e => e.TenantId == tenantId)
            .Where(e => e.IsActive)
            .Where(e => e.Status == EquipmentStatus.Available)
            .Where(e => e.AssignedToUserId == null);

        if (type.HasValue)
            query = query.Where(e => e.Type == type.Value);

        if (warehouseId.HasValue)
            query = query.Where(e => e.WarehouseId == warehouseId.Value);

        var items = await query
            .OrderBy(e => e.Code)
            .Select(e => MapToDto(e))
            .ToListAsync();

        return ApiResponse<IEnumerable<EquipmentDto>>.Ok(items);
    }

    private static EquipmentDto MapToDto(Models.Equipment equipment)
    {
        JsonDocument? specifications = null;
        if (!string.IsNullOrWhiteSpace(equipment.Specifications))
        {
            try
            {
                specifications = JsonDocument.Parse(equipment.Specifications);
            }
            catch
            {
                // Invalid JSON, ignore
            }
        }

        return new EquipmentDto
        {
            Id = equipment.Id,
            Code = equipment.Code,
            Name = equipment.Name,
            Description = equipment.Description,
            Type = equipment.Type,
            Status = equipment.Status,
            WarehouseId = equipment.WarehouseId,
            WarehouseName = equipment.Warehouse?.Name,
            ZoneId = equipment.ZoneId,
            ZoneName = equipment.Zone?.Name,
            AssignedToUserId = equipment.AssignedToUserId,
            AssignedToUserName = equipment.AssignedToUserName,
            LastMaintenanceDate = equipment.LastMaintenanceDate,
            NextMaintenanceDate = equipment.NextMaintenanceDate,
            MaintenanceNotes = equipment.MaintenanceNotes,
            Specifications = specifications,
            SerialNumber = equipment.SerialNumber,
            Manufacturer = equipment.Manufacturer,
            Model = equipment.Model,
            PurchaseDate = equipment.PurchaseDate,
            WarrantyExpiryDate = equipment.WarrantyExpiryDate,
            IsActive = equipment.IsActive,
            CreatedAt = equipment.CreatedAt,
            UpdatedAt = equipment.UpdatedAt
        };
    }
}
