using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Fulfillment.DTOs;

namespace SmartWMS.API.Modules.Fulfillment.Services;

public interface IShipmentsService
{
    Task<ApiResponse<PaginatedResult<ShipmentDto>>> GetShipmentsAsync(
        Guid tenantId,
        ShipmentFilters? filters = null,
        int page = 1,
        int pageSize = 25);

    Task<ApiResponse<ShipmentDto>> GetShipmentByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<ShipmentDto>> GetShipmentByNumberAsync(Guid tenantId, string shipmentNumber);
    Task<ApiResponse<ShipmentDto>> CreateShipmentAsync(Guid tenantId, CreateShipmentRequest request);
    Task<ApiResponse<ShipmentDto>> UpdateShipmentAsync(Guid tenantId, Guid id, UpdateShipmentRequest request);
    Task<ApiResponse> DeleteShipmentAsync(Guid tenantId, Guid id);

    // Shipment workflow
    Task<ApiResponse<ShipmentDto>> AddTrackingAsync(Guid tenantId, Guid id, AddTrackingRequest request);
    Task<ApiResponse<ShipmentDto>> UpdateLabelAsync(Guid tenantId, Guid id, UpdateLabelRequest request);
    Task<ApiResponse<ShipmentDto>> MarkAsPackedAsync(Guid tenantId, Guid id);
    Task<ApiResponse<ShipmentDto>> ShipAsync(Guid tenantId, Guid id, ShipShipmentRequest? request = null);
    Task<ApiResponse<ShipmentDto>> MarkAsDeliveredAsync(Guid tenantId, Guid id);
    Task<ApiResponse<ShipmentDto>> CancelShipmentAsync(Guid tenantId, Guid id, string? reason = null);
}
