using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Carriers.DTOs;

namespace SmartWMS.API.Modules.Carriers.Services;

public interface ICarriersService
{
    // Carriers
    Task<ApiResponse<PaginatedResult<CarrierListDto>>> GetCarriersAsync(
        Guid tenantId, CarrierFilters? filters, int page, int pageSize);
    Task<ApiResponse<List<CarrierListDto>>> GetActiveCarriersAsync(Guid tenantId);
    Task<ApiResponse<CarrierDto>> GetCarrierByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<CarrierDto>> GetCarrierByCodeAsync(Guid tenantId, string code);
    Task<ApiResponse<CarrierDto>> CreateCarrierAsync(Guid tenantId, CreateCarrierRequest request);
    Task<ApiResponse<CarrierDto>> UpdateCarrierAsync(Guid tenantId, Guid id, UpdateCarrierRequest request);
    Task<ApiResponse<bool>> DeleteCarrierAsync(Guid tenantId, Guid id);

    // Carrier Services
    Task<ApiResponse<List<CarrierServiceDto>>> GetCarrierServicesAsync(Guid tenantId, Guid carrierId);
    Task<ApiResponse<CarrierServiceDto>> GetCarrierServiceByIdAsync(Guid tenantId, Guid carrierId, Guid serviceId);
    Task<ApiResponse<CarrierServiceDto>> CreateCarrierServiceAsync(
        Guid tenantId, Guid carrierId, CreateCarrierServiceRequest request);
    Task<ApiResponse<CarrierServiceDto>> UpdateCarrierServiceAsync(
        Guid tenantId, Guid carrierId, Guid serviceId, UpdateCarrierServiceRequest request);
    Task<ApiResponse<bool>> DeleteCarrierServiceAsync(Guid tenantId, Guid carrierId, Guid serviceId);
}
