using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Orders.DTOs;
using SmartWMS.API.Modules.Orders.Models;

namespace SmartWMS.API.Modules.Orders.Services;

public class CustomersService : ICustomersService
{
    private readonly ApplicationDbContext _context;

    public CustomersService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PaginatedResult<CustomerDto>>> GetCustomersAsync(
        Guid tenantId,
        CustomerFilters? filters = null,
        int page = 1,
        int pageSize = 25)
    {
        var query = _context.Customers
            .Where(c => c.TenantId == tenantId)
            .AsQueryable();

        // Apply filters
        if (filters != null)
        {
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var search = filters.Search.ToLower();
                query = query.Where(c =>
                    c.Code.ToLower().Contains(search) ||
                    c.Name.ToLower().Contains(search) ||
                    (c.Email != null && c.Email.ToLower().Contains(search)) ||
                    (c.ContactName != null && c.ContactName.ToLower().Contains(search)));
            }

            if (filters.IsActive.HasValue)
                query = query.Where(c => c.IsActive == filters.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filters.CountryCode))
                query = query.Where(c => c.CountryCode == filters.CountryCode);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => MapToDto(c))
            .ToListAsync();

        var result = new PaginatedResult<CustomerDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<CustomerDto>>.Ok(result);
    }

    public async Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(Guid tenantId, Guid id)
    {
        var customer = await _context.Customers
            .Include(c => c.SalesOrders)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);

        if (customer == null)
            return ApiResponse<CustomerDto>.Fail("Customer not found");

        return ApiResponse<CustomerDto>.Ok(MapToDto(customer));
    }

    public async Task<ApiResponse<CustomerDto>> GetCustomerByCodeAsync(Guid tenantId, string code)
    {
        var customer = await _context.Customers
            .Include(c => c.SalesOrders)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Code == code);

        if (customer == null)
            return ApiResponse<CustomerDto>.Fail("Customer not found");

        return ApiResponse<CustomerDto>.Ok(MapToDto(customer));
    }

    public async Task<ApiResponse<CustomerDto>> CreateCustomerAsync(Guid tenantId, CreateCustomerRequest request)
    {
        // Check for duplicate code
        var existingCode = await _context.Customers
            .AnyAsync(c => c.TenantId == tenantId && c.Code == request.Code);

        if (existingCode)
            return ApiResponse<CustomerDto>.Fail($"Customer with code '{request.Code}' already exists");

        var customer = new Customer
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
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return ApiResponse<CustomerDto>.Ok(MapToDto(customer), "Customer created successfully");
    }

    public async Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(Guid tenantId, Guid id, UpdateCustomerRequest request)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);

        if (customer == null)
            return ApiResponse<CustomerDto>.Fail("Customer not found");

        // Update fields
        if (request.Name != null) customer.Name = request.Name;
        if (request.ContactName != null) customer.ContactName = request.ContactName;
        if (request.Email != null) customer.Email = request.Email;
        if (request.Phone != null) customer.Phone = request.Phone;
        if (request.AddressLine1 != null) customer.AddressLine1 = request.AddressLine1;
        if (request.AddressLine2 != null) customer.AddressLine2 = request.AddressLine2;
        if (request.City != null) customer.City = request.City;
        if (request.Region != null) customer.Region = request.Region;
        if (request.PostalCode != null) customer.PostalCode = request.PostalCode;
        if (request.CountryCode != null) customer.CountryCode = request.CountryCode;
        if (request.TaxId != null) customer.TaxId = request.TaxId;
        if (request.PaymentTerms != null) customer.PaymentTerms = request.PaymentTerms;
        if (request.IsActive.HasValue) customer.IsActive = request.IsActive.Value;

        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<CustomerDto>.Ok(MapToDto(customer), "Customer updated successfully");
    }

    public async Task<ApiResponse> DeleteCustomerAsync(Guid tenantId, Guid id)
    {
        var customer = await _context.Customers
            .Include(c => c.SalesOrders)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);

        if (customer == null)
            return ApiResponse.Fail("Customer not found");

        // Check for existing orders
        if (customer.SalesOrders.Any())
            return ApiResponse.Fail("Cannot delete customer with existing orders. Deactivate instead.");

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Customer deleted successfully");
    }

    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Code = customer.Code,
            Name = customer.Name,
            ContactName = customer.ContactName,
            Email = customer.Email,
            Phone = customer.Phone,
            AddressLine1 = customer.AddressLine1,
            AddressLine2 = customer.AddressLine2,
            City = customer.City,
            Region = customer.Region,
            PostalCode = customer.PostalCode,
            CountryCode = customer.CountryCode,
            TaxId = customer.TaxId,
            PaymentTerms = customer.PaymentTerms,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            OrderCount = customer.SalesOrders?.Count ?? 0
        };
    }
}
