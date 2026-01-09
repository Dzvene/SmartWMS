using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Returns.DTOs;

namespace SmartWMS.API.Modules.Returns.Services;

public interface IReturnsService
{
    // Queries
    Task<ApiResponse<PaginatedResult<ReturnOrderListDto>>> GetReturnOrdersAsync(
        Guid tenantId, ReturnOrderFilters? filters, int page, int pageSize);
    Task<ApiResponse<ReturnOrderDto>> GetReturnOrderByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<ReturnOrderDto>> GetReturnOrderByNumberAsync(Guid tenantId, string returnNumber);
    Task<ApiResponse<List<ReturnOrderListDto>>> GetMyReturnsAsync(Guid tenantId, Guid userId);

    // Return Order CRUD
    Task<ApiResponse<ReturnOrderDto>> CreateReturnOrderAsync(Guid tenantId, CreateReturnOrderRequest request);
    Task<ApiResponse<ReturnOrderDto>> UpdateReturnOrderAsync(Guid tenantId, Guid id, UpdateReturnOrderRequest request);
    Task<ApiResponse<bool>> DeleteReturnOrderAsync(Guid tenantId, Guid id);

    // Return Order Lines
    Task<ApiResponse<ReturnOrderDto>> AddLineAsync(Guid tenantId, Guid returnOrderId, AddReturnLineRequest request);
    Task<ApiResponse<ReturnOrderDto>> RemoveLineAsync(Guid tenantId, Guid returnOrderId, Guid lineId);

    // Workflow
    Task<ApiResponse<ReturnOrderDto>> AssignReturnAsync(Guid tenantId, Guid id, AssignReturnRequest request);
    Task<ApiResponse<ReturnOrderDto>> MarkInTransitAsync(Guid tenantId, Guid id, SetReturnShippingRequest request);
    Task<ApiResponse<ReturnOrderDto>> StartReceivingAsync(Guid tenantId, Guid id);
    Task<ApiResponse<ReturnOrderDto>> ReceiveLineAsync(Guid tenantId, Guid returnOrderId, Guid lineId, ReceiveReturnLineRequest request);
    Task<ApiResponse<ReturnOrderDto>> ProcessLineAsync(Guid tenantId, Guid returnOrderId, Guid lineId, ProcessReturnLineRequest request);
    Task<ApiResponse<ReturnOrderDto>> CompleteReturnAsync(Guid tenantId, Guid id);
    Task<ApiResponse<ReturnOrderDto>> CancelReturnAsync(Guid tenantId, Guid id, string? reason);
}
