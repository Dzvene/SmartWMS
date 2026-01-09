using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Putaway.DTOs;

namespace SmartWMS.API.Modules.Putaway.Services;

public interface IPutawayService
{
    // Queries
    Task<ApiResponse<PaginatedResult<PutawayTaskDto>>> GetTasksAsync(
        Guid tenantId,
        PutawayTaskFilters? filters = null,
        int page = 1,
        int pageSize = 25);

    Task<ApiResponse<PutawayTaskDto>> GetTaskByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<IEnumerable<PutawayTaskDto>>> GetMyTasksAsync(Guid tenantId, Guid userId);
    Task<ApiResponse<PutawayTaskDto>> GetNextTaskAsync(Guid tenantId, Guid userId);

    // Task creation
    Task<ApiResponse<IEnumerable<PutawayTaskDto>>> CreateFromGoodsReceiptAsync(
        Guid tenantId, CreatePutawayFromReceiptRequest request);
    Task<ApiResponse<PutawayTaskDto>> CreateTaskAsync(Guid tenantId, CreatePutawayTaskRequest request);

    // Task workflow
    Task<ApiResponse<PutawayTaskDto>> AssignTaskAsync(Guid tenantId, Guid taskId, AssignPutawayTaskRequest request);
    Task<ApiResponse<PutawayTaskDto>> StartTaskAsync(Guid tenantId, Guid taskId);
    Task<ApiResponse<PutawayTaskDto>> CompleteTaskAsync(Guid tenantId, Guid taskId, CompletePutawayTaskRequest request);
    Task<ApiResponse<PutawayTaskDto>> CancelTaskAsync(Guid tenantId, Guid taskId, string? reason = null);

    // Location suggestion
    Task<ApiResponse<IEnumerable<LocationSuggestionDto>>> SuggestLocationsAsync(
        Guid tenantId, SuggestLocationRequest request);
}
