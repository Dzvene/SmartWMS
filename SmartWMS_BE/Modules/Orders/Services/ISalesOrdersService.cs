using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Orders.DTOs;

namespace SmartWMS.API.Modules.Orders.Services;

public interface ISalesOrdersService
{
    // Orders
    Task<ApiResponse<PaginatedResult<SalesOrderDto>>> GetOrdersAsync(
        Guid tenantId,
        SalesOrderFilters? filters = null,
        int page = 1,
        int pageSize = 25);

    Task<ApiResponse<SalesOrderDto>> GetOrderByIdAsync(Guid tenantId, Guid id, bool includeLines = true);
    Task<ApiResponse<SalesOrderDto>> GetOrderByNumberAsync(Guid tenantId, string orderNumber, bool includeLines = true);
    Task<ApiResponse<SalesOrderDto>> CreateOrderAsync(Guid tenantId, CreateSalesOrderRequest request);
    Task<ApiResponse<SalesOrderDto>> UpdateOrderAsync(Guid tenantId, Guid id, UpdateSalesOrderRequest request);
    Task<ApiResponse<SalesOrderDto>> UpdateOrderStatusAsync(Guid tenantId, Guid id, UpdateSalesOrderStatusRequest request);
    Task<ApiResponse> DeleteOrderAsync(Guid tenantId, Guid id);

    // Order Lines
    Task<ApiResponse<SalesOrderLineDto>> AddLineAsync(Guid tenantId, Guid orderId, AddSalesOrderLineRequest request);
    Task<ApiResponse<SalesOrderLineDto>> UpdateLineAsync(Guid tenantId, Guid orderId, Guid lineId, UpdateSalesOrderLineRequest request);
    Task<ApiResponse> RemoveLineAsync(Guid tenantId, Guid orderId, Guid lineId);
}
