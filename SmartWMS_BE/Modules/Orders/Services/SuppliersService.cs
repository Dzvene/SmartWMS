using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Orders.DTOs;
using SmartWMS.API.Modules.Orders.Models;

namespace SmartWMS.API.Modules.Orders.Services;

public class SuppliersService : ISuppliersService
{
    private readonly ApplicationDbContext _context;

    public SuppliersService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PaginatedResult<SupplierDto>>> GetSuppliersAsync(
        Guid tenantId,
        SupplierFilters? filters = null,
        int page = 1,
        int pageSize = 25)
    {
        var query = _context.Suppliers
            .Where(s => s.TenantId == tenantId)
            .AsQueryable();

        // Apply filters
        if (filters != null)
        {
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var search = filters.Search.ToLower();
                query = query.Where(s =>
                    s.Code.ToLower().Contains(search) ||
                    s.Name.ToLower().Contains(search) ||
                    (s.Email != null && s.Email.ToLower().Contains(search)) ||
                    (s.ContactName != null && s.ContactName.ToLower().Contains(search)));
            }

            if (filters.IsActive.HasValue)
                query = query.Where(s => s.IsActive == filters.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filters.CountryCode))
                query = query.Where(s => s.CountryCode == filters.CountryCode);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(s => s.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => MapToDto(s))
            .ToListAsync();

        var result = new PaginatedResult<SupplierDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<SupplierDto>>.Ok(result);
    }

    public async Task<ApiResponse<SupplierDto>> GetSupplierByIdAsync(Guid tenantId, Guid id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.PurchaseOrders)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (supplier == null)
            return ApiResponse<SupplierDto>.Fail("Supplier not found");

        return ApiResponse<SupplierDto>.Ok(MapToDto(supplier));
    }

    public async Task<ApiResponse<SupplierDto>> GetSupplierByCodeAsync(Guid tenantId, string code)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.PurchaseOrders)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Code == code);

        if (supplier == null)
            return ApiResponse<SupplierDto>.Fail("Supplier not found");

        return ApiResponse<SupplierDto>.Ok(MapToDto(supplier));
    }

    public async Task<ApiResponse<SupplierDto>> CreateSupplierAsync(Guid tenantId, CreateSupplierRequest request)
    {
        // Check for duplicate code
        var existingCode = await _context.Suppliers
            .AnyAsync(s => s.TenantId == tenantId && s.Code == request.Code);

        if (existingCode)
            return ApiResponse<SupplierDto>.Fail($"Supplier with code '{request.Code}' already exists");

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code,
            Name = request.Name,
            ContactName = request.ContactName,
            Email = request.Email,
            Phone = request.Phone,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            Region = request.Region,
            PostalCode = request.PostalCode,
            CountryCode = request.CountryCode,
            TaxId = request.TaxId,
            PaymentTerms = request.PaymentTerms,
            LeadTimeDays = request.LeadTimeDays,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return ApiResponse<SupplierDto>.Ok(MapToDto(supplier), "Supplier created successfully");
    }

    public async Task<ApiResponse<SupplierDto>> UpdateSupplierAsync(Guid tenantId, Guid id, UpdateSupplierRequest request)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (supplier == null)
            return ApiResponse<SupplierDto>.Fail("Supplier not found");

        // Update fields
        if (request.Name != null) supplier.Name = request.Name;
        if (request.ContactName != null) supplier.ContactName = request.ContactName;
        if (request.Email != null) supplier.Email = request.Email;
        if (request.Phone != null) supplier.Phone = request.Phone;
        if (request.AddressLine1 != null) supplier.AddressLine1 = request.AddressLine1;
        if (request.AddressLine2 != null) supplier.AddressLine2 = request.AddressLine2;
        if (request.City != null) supplier.City = request.City;
        if (request.Region != null) supplier.Region = request.Region;
        if (request.PostalCode != null) supplier.PostalCode = request.PostalCode;
        if (request.CountryCode != null) supplier.CountryCode = request.CountryCode;
        if (request.TaxId != null) supplier.TaxId = request.TaxId;
        if (request.PaymentTerms != null) supplier.PaymentTerms = request.PaymentTerms;
        if (request.LeadTimeDays.HasValue) supplier.LeadTimeDays = request.LeadTimeDays;
        if (request.IsActive.HasValue) supplier.IsActive = request.IsActive.Value;

        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<SupplierDto>.Ok(MapToDto(supplier), "Supplier updated successfully");
    }

    public async Task<ApiResponse> DeleteSupplierAsync(Guid tenantId, Guid id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.PurchaseOrders)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (supplier == null)
            return ApiResponse.Fail("Supplier not found");

        // Check for existing orders
        if (supplier.PurchaseOrders.Any())
            return ApiResponse.Fail("Cannot delete supplier with existing orders. Deactivate instead.");

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Supplier deleted successfully");
    }

    private static SupplierDto MapToDto(Supplier supplier)
    {
        return new SupplierDto
        {
            Id = supplier.Id,
            Code = supplier.Code,
            Name = supplier.Name,
            ContactName = supplier.ContactName,
            Email = supplier.Email,
            Phone = supplier.Phone,
            AddressLine1 = supplier.AddressLine1,
            AddressLine2 = supplier.AddressLine2,
            City = supplier.City,
            Region = supplier.Region,
            PostalCode = supplier.PostalCode,
            CountryCode = supplier.CountryCode,
            TaxId = supplier.TaxId,
            PaymentTerms = supplier.PaymentTerms,
            LeadTimeDays = supplier.LeadTimeDays,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt,
            UpdatedAt = supplier.UpdatedAt,
            OrderCount = supplier.PurchaseOrders?.Count ?? 0
        };
    }
}
