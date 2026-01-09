using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Fulfillment.DTOs;

namespace SmartWMS.API.Modules.Fulfillment.Services;

public interface IFulfillmentBatchesService
{
    Task<ApiResponse<PaginatedResult<FulfillmentBatchDto>>> GetBatchesAsync(
        Guid tenantId,
        FulfillmentBatchFilters? filters = null,
        int page = 1,
        int pageSize = 25);

    Task<ApiResponse<FulfillmentBatchDto>> GetBatchByIdAsync(Guid tenantId, Guid id, bool includeDetails = true);
    Task<ApiResponse<FulfillmentBatchDto>> GetBatchByNumberAsync(Guid tenantId, string batchNumber, bool includeDetails = true);
    Task<ApiResponse<FulfillmentBatchDto>> CreateBatchAsync(Guid tenantId, CreateFulfillmentBatchRequest request);
    Task<ApiResponse<FulfillmentBatchDto>> UpdateBatchAsync(Guid tenantId, Guid id, UpdateFulfillmentBatchRequest request);
    Task<ApiResponse> DeleteBatchAsync(Guid tenantId, Guid id);

    // Batch operations
    Task<ApiResponse<FulfillmentBatchDto>> AddOrdersToBatchAsync(Guid tenantId, Guid id, AddOrdersToBatchRequest request);
    Task<ApiResponse> RemoveOrderFromBatchAsync(Guid tenantId, Guid batchId, Guid orderId);
    Task<ApiResponse<FulfillmentBatchDto>> ReleaseBatchAsync(Guid tenantId, Guid id, ReleaseBatchRequest request);
    Task<ApiResponse<FulfillmentBatchDto>> CompleteBatchAsync(Guid tenantId, Guid id);
    Task<ApiResponse<FulfillmentBatchDto>> CancelBatchAsync(Guid tenantId, Guid id, string? reason = null);
}
