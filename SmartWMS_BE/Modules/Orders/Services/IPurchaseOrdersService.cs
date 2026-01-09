using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Orders.DTOs;

namespace SmartWMS.API.Modules.Orders.Services;

public interface IPurchaseOrdersService
{
    // Orders
    Task<ApiResponse<PaginatedResult<PurchaseOrderDto>>> GetOrdersAsync(
        Guid tenantId,
        PurchaseOrderFilters? filters = null,
        int page = 1,
        int pageSize = 25);

    Task<ApiResponse<PurchaseOrderDto>> GetOrderByIdAsync(Guid tenantId, Guid id, bool includeLines = true);
    Task<ApiResponse<PurchaseOrderDto>> GetOrderByNumberAsync(Guid tenantId, string orderNumber, bool includeLines = true);
    Task<ApiResponse<PurchaseOrderDto>> CreateOrderAsync(Guid tenantId, CreatePurchaseOrderRequest request);
    Task<ApiResponse<PurchaseOrderDto>> UpdateOrderAsync(Guid tenantId, Guid id, UpdatePurchaseOrderRequest request);
    Task<ApiResponse<PurchaseOrderDto>> UpdateOrderStatusAsync(Guid tenantId, Guid id, UpdatePurchaseOrderStatusRequest request);
    Task<ApiResponse> DeleteOrderAsync(Guid tenantId, Guid id);

    // Order Lines
    Task<ApiResponse<PurchaseOrderLineDto>> AddLineAsync(Guid tenantId, Guid orderId, AddPurchaseOrderLineRequest request);
    Task<ApiResponse<PurchaseOrderLineDto>> UpdateLineAsync(Guid tenantId, Guid orderId, Guid lineId, UpdatePurchaseOrderLineRequest request);
    Task<ApiResponse> RemoveLineAsync(Guid tenantId, Guid orderId, Guid lineId);

    // Receiving
    Task<ApiResponse<PurchaseOrderLineDto>> ReceiveLineAsync(Guid tenantId, Guid orderId, Guid lineId, ReceivePurchaseOrderLineRequest request);
}
