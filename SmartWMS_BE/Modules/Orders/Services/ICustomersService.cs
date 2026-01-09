using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Orders.DTOs;

namespace SmartWMS.API.Modules.Orders.Services;

public interface ICustomersService
{
    Task<ApiResponse<PaginatedResult<CustomerDto>>> GetCustomersAsync(
        Guid tenantId,
        CustomerFilters? filters = null,
        int page = 1,
        int pageSize = 25);

    Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<CustomerDto>> GetCustomerByCodeAsync(Guid tenantId, string code);
    Task<ApiResponse<CustomerDto>> CreateCustomerAsync(Guid tenantId, CreateCustomerRequest request);
    Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(Guid tenantId, Guid id, UpdateCustomerRequest request);
    Task<ApiResponse> DeleteCustomerAsync(Guid tenantId, Guid id);
}
