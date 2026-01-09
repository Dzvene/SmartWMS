using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Fulfillment.DTOs;

namespace SmartWMS.API.Modules.Fulfillment.Services;

public interface IPickTasksService
{
    Task<ApiResponse<PaginatedResult<PickTaskDto>>> GetTasksAsync(
        Guid tenantId,
        PickTaskFilters? filters = null,
        int page = 1,
        int pageSize = 25);

    Task<ApiResponse<PickTaskDto>> GetTaskByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<PickTaskDto>> GetTaskByNumberAsync(Guid tenantId, string taskNumber);
    Task<ApiResponse<PickTaskDto>> CreateTaskAsync(Guid tenantId, CreatePickTaskRequest request);
    Task<ApiResponse> DeleteTaskAsync(Guid tenantId, Guid id);

    // Task workflow
    Task<ApiResponse<PickTaskDto>> AssignTaskAsync(Guid tenantId, Guid id, AssignPickTaskRequest request);
    Task<ApiResponse<PickTaskDto>> StartTaskAsync(Guid tenantId, Guid id, StartPickTaskRequest? request = null);
    Task<ApiResponse<PickTaskDto>> ConfirmPickAsync(Guid tenantId, Guid id, ConfirmPickRequest request);
    Task<ApiResponse<PickTaskDto>> ShortPickAsync(Guid tenantId, Guid id, ShortPickRequest request);
    Task<ApiResponse<PickTaskDto>> CancelTaskAsync(Guid tenantId, Guid id, string? reason = null);

    // Bulk operations
    Task<ApiResponse<IEnumerable<PickTaskDto>>> GetMyTasksAsync(Guid tenantId, Guid userId);
    Task<ApiResponse<PickTaskDto>> GetNextTaskAsync(Guid tenantId, Guid userId, Guid? warehouseId = null);
}
