using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Carriers.DTOs;
using SmartWMS.API.Modules.Carriers.Models;

namespace SmartWMS.API.Modules.Carriers.Services;

public class CarriersService : ICarriersService
{
    private readonly ApplicationDbContext _context;

    public CarriersService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Carriers

    public async Task<ApiResponse<PaginatedResult<CarrierListDto>>> GetCarriersAsync(
        Guid tenantId, CarrierFilters? filters, int page, int pageSize)
    {
        var query = _context.Carriers
            .Include(c => c.Services)
            .Where(c => c.TenantId == tenantId);

        if (filters != null)
        {
            if (filters.IntegrationType.HasValue)
                query = query.Where(c => c.IntegrationType == filters.IntegrationType.Value);

            if (filters.IsActive.HasValue)
                query = query.Where(c => c.IsActive == filters.IsActive.Value);

            if (!string.IsNullOrEmpty(filters.Search))
                query = query.Where(c => c.Code.Contains(filters.Search) ||
                                        c.Name.Contains(filters.Search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CarrierListDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                IntegrationType = c.IntegrationType,
                IsActive = c.IsActive,
                ServiceCount = c.Services.Count,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        var result = new PaginatedResult<CarrierListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<CarrierListDto>>.Ok(result);
    }

    public async Task<ApiResponse<List<CarrierListDto>>> GetActiveCarriersAsync(Guid tenantId)
    {
        var carriers = await _context.Carriers
            .Include(c => c.Services)
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CarrierListDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                IntegrationType = c.IntegrationType,
                IsActive = c.IsActive,
                ServiceCount = c.Services.Count(s => s.IsActive),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<CarrierListDto>>.Ok(carriers);
    }

    public async Task<ApiResponse<CarrierDto>> GetCarrierByIdAsync(Guid tenantId, Guid id)
    {
        var carrier = await _context.Carriers
            .Include(c => c.Services)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);

        if (carrier == null)
            return ApiResponse<CarrierDto>.Fail("Carrier not found");

        return ApiResponse<CarrierDto>.Ok(MapToDto(carrier));
    }

    public async Task<ApiResponse<CarrierDto>> GetCarrierByCodeAsync(Guid tenantId, string code)
    {
        var carrier = await _context.Carriers
            .Include(c => c.Services)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Code == code);

        if (carrier == null)
            return ApiResponse<CarrierDto>.Fail("Carrier not found");

        return ApiResponse<CarrierDto>.Ok(MapToDto(carrier));
    }

    public async Task<ApiResponse<CarrierDto>> CreateCarrierAsync(Guid tenantId, CreateCarrierRequest request)
    {
        // Check for duplicate code
        var exists = await _context.Carriers
            .AnyAsync(c => c.TenantId == tenantId && c.Code == request.Code);

        if (exists)
            return ApiResponse<CarrierDto>.Fail($"Carrier with code '{request.Code}' already exists");

        var carrier = new Carrier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            ContactName = request.ContactName,
            Phone = request.Phone,
            Email = request.Email,
            Website = request.Website,
            AccountNumber = request.AccountNumber,
            IntegrationType = request.IntegrationType,
            DefaultServiceCode = request.DefaultServiceCode,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Carriers.Add(carrier);
        await _context.SaveChangesAsync();

        return await GetCarrierByIdAsync(tenantId, carrier.Id);
    }

    public async Task<ApiResponse<CarrierDto>> UpdateCarrierAsync(
        Guid tenantId, Guid id, UpdateCarrierRequest request)
    {
        var carrier = await _context.Carriers
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);

        if (carrier == null)
            return ApiResponse<CarrierDto>.Fail("Carrier not found");

        carrier.Name = request.Name;
        carrier.Description = request.Description;
        carrier.ContactName = request.ContactName;
        carrier.Phone = request.Phone;
        carrier.Email = request.Email;
        carrier.Website = request.Website;
        carrier.AccountNumber = request.AccountNumber;
        carrier.IntegrationType = request.IntegrationType;
        carrier.IsActive = request.IsActive;
        carrier.DefaultServiceCode = request.DefaultServiceCode;
        carrier.Notes = request.Notes;
        carrier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetCarrierByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<bool>> DeleteCarrierAsync(Guid tenantId, Guid id)
    {
        var carrier = await _context.Carriers
            .Include(c => c.Services)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);

        if (carrier == null)
            return ApiResponse<bool>.Fail("Carrier not found");

        _context.Carriers.Remove(carrier);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Carrier deleted");
    }

    #endregion

    #region Carrier Services

    public async Task<ApiResponse<List<CarrierServiceDto>>> GetCarrierServicesAsync(Guid tenantId, Guid carrierId)
    {
        var services = await _context.CarrierServices
            .Where(s => s.TenantId == tenantId && s.CarrierId == carrierId)
            .OrderBy(s => s.Name)
            .Select(s => MapServiceToDto(s))
            .ToListAsync();

        return ApiResponse<List<CarrierServiceDto>>.Ok(services);
    }

    public async Task<ApiResponse<CarrierServiceDto>> GetCarrierServiceByIdAsync(
        Guid tenantId, Guid carrierId, Guid serviceId)
    {
        var service = await _context.CarrierServices
            .FirstOrDefaultAsync(s => s.TenantId == tenantId &&
                                     s.CarrierId == carrierId &&
                                     s.Id == serviceId);

        if (service == null)
            return ApiResponse<CarrierServiceDto>.Fail("Carrier service not found");

        return ApiResponse<CarrierServiceDto>.Ok(MapServiceToDto(service));
    }

    public async Task<ApiResponse<CarrierServiceDto>> CreateCarrierServiceAsync(
        Guid tenantId, Guid carrierId, CreateCarrierServiceRequest request)
    {
        var carrierExists = await _context.Carriers
            .AnyAsync(c => c.TenantId == tenantId && c.Id == carrierId);

        if (!carrierExists)
            return ApiResponse<CarrierServiceDto>.Fail("Carrier not found");

        // Check for duplicate code within carrier
        var exists = await _context.CarrierServices
            .AnyAsync(s => s.TenantId == tenantId &&
                          s.CarrierId == carrierId &&
                          s.Code == request.Code);

        if (exists)
            return ApiResponse<CarrierServiceDto>.Fail($"Service with code '{request.Code}' already exists");

        var service = new CarrierService
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CarrierId = carrierId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            MinTransitDays = request.MinTransitDays,
            MaxTransitDays = request.MaxTransitDays,
            ServiceType = request.ServiceType,
            HasTracking = request.HasTracking,
            TrackingUrlTemplate = request.TrackingUrlTemplate,
            MaxWeightKg = request.MaxWeightKg,
            MaxLengthCm = request.MaxLengthCm,
            MaxWidthCm = request.MaxWidthCm,
            MaxHeightCm = request.MaxHeightCm,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.CarrierServices.Add(service);
        await _context.SaveChangesAsync();

        return await GetCarrierServiceByIdAsync(tenantId, carrierId, service.Id);
    }

    public async Task<ApiResponse<CarrierServiceDto>> UpdateCarrierServiceAsync(
        Guid tenantId, Guid carrierId, Guid serviceId, UpdateCarrierServiceRequest request)
    {
        var service = await _context.CarrierServices
            .FirstOrDefaultAsync(s => s.TenantId == tenantId &&
                                     s.CarrierId == carrierId &&
                                     s.Id == serviceId);

        if (service == null)
            return ApiResponse<CarrierServiceDto>.Fail("Carrier service not found");

        service.Name = request.Name;
        service.Description = request.Description;
        service.MinTransitDays = request.MinTransitDays;
        service.MaxTransitDays = request.MaxTransitDays;
        service.ServiceType = request.ServiceType;
        service.HasTracking = request.HasTracking;
        service.TrackingUrlTemplate = request.TrackingUrlTemplate;
        service.MaxWeightKg = request.MaxWeightKg;
        service.MaxLengthCm = request.MaxLengthCm;
        service.MaxWidthCm = request.MaxWidthCm;
        service.MaxHeightCm = request.MaxHeightCm;
        service.IsActive = request.IsActive;
        service.Notes = request.Notes;
        service.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetCarrierServiceByIdAsync(tenantId, carrierId, serviceId);
    }

    public async Task<ApiResponse<bool>> DeleteCarrierServiceAsync(Guid tenantId, Guid carrierId, Guid serviceId)
    {
        var service = await _context.CarrierServices
            .FirstOrDefaultAsync(s => s.TenantId == tenantId &&
                                     s.CarrierId == carrierId &&
                                     s.Id == serviceId);

        if (service == null)
            return ApiResponse<bool>.Fail("Carrier service not found");

        _context.CarrierServices.Remove(service);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Carrier service deleted");
    }

    #endregion

    #region Helpers

    private static CarrierDto MapToDto(Carrier carrier)
    {
        return new CarrierDto
        {
            Id = carrier.Id,
            Code = carrier.Code,
            Name = carrier.Name,
            Description = carrier.Description,
            ContactName = carrier.ContactName,
            Phone = carrier.Phone,
            Email = carrier.Email,
            Website = carrier.Website,
            AccountNumber = carrier.AccountNumber,
            IntegrationType = carrier.IntegrationType,
            IsActive = carrier.IsActive,
            DefaultServiceCode = carrier.DefaultServiceCode,
            Notes = carrier.Notes,
            Services = carrier.Services.Select(MapServiceToDto).ToList(),
            CreatedAt = carrier.CreatedAt,
            UpdatedAt = carrier.UpdatedAt
        };
    }

    private static CarrierServiceDto MapServiceToDto(CarrierService service)
    {
        return new CarrierServiceDto
        {
            Id = service.Id,
            CarrierId = service.CarrierId,
            Code = service.Code,
            Name = service.Name,
            Description = service.Description,
            MinTransitDays = service.MinTransitDays,
            MaxTransitDays = service.MaxTransitDays,
            ServiceType = service.ServiceType,
            HasTracking = service.HasTracking,
            TrackingUrlTemplate = service.TrackingUrlTemplate,
            MaxWeightKg = service.MaxWeightKg,
            MaxLengthCm = service.MaxLengthCm,
            MaxWidthCm = service.MaxWidthCm,
            MaxHeightCm = service.MaxHeightCm,
            IsActive = service.IsActive,
            Notes = service.Notes,
            CreatedAt = service.CreatedAt
        };
    }

    #endregion
}
