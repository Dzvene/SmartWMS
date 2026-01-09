using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Orders.DTOs;

namespace SmartWMS.API.Modules.Orders.Services;

public interface ISuppliersService
{
    Task<ApiResponse<PaginatedResult<SupplierDto>>> GetSuppliersAsync(
        Guid tenantId,
        SupplierFilters? filters = null,
        int page = 1,
        int pageSize = 25);

    Task<ApiResponse<SupplierDto>> GetSupplierByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<SupplierDto>> GetSupplierByCodeAsync(Guid tenantId, string code);
    Task<ApiResponse<SupplierDto>> CreateSupplierAsync(Guid tenantId, CreateSupplierRequest request);
    Task<ApiResponse<SupplierDto>> UpdateSupplierAsync(Guid tenantId, Guid id, UpdateSupplierRequest request);
    Task<ApiResponse> DeleteSupplierAsync(Guid tenantId, Guid id);
}
