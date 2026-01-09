using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Packing.DTOs;

namespace SmartWMS.API.Modules.Packing.Services;

public interface IPackingService
{
    #region Packing Tasks

    // Queries
    Task<ApiResponse<PaginatedResult<PackingTaskListDto>>> GetTasksAsync(
        Guid tenantId, PackingTaskFilters filters, int page, int pageSize);
    Task<ApiResponse<PackingTaskDto>> GetTaskByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<List<PackingTaskListDto>>> GetMyTasksAsync(Guid tenantId, Guid userId);
    Task<ApiResponse<PackingTaskDto?>> GetNextTaskAsync(Guid tenantId, Guid userId);

    // Creation
    Task<ApiResponse<PackingTaskDto>> CreateTaskAsync(Guid tenantId, CreatePackingTaskRequest request);
    Task<ApiResponse<List<PackingTaskDto>>> CreateFromFulfillmentAsync(
        Guid tenantId, CreatePackingFromFulfillmentRequest request);

    // Workflow
    Task<ApiResponse<PackingTaskDto>> AssignTaskAsync(Guid tenantId, Guid id, AssignPackingTaskRequest request);
    Task<ApiResponse<PackingTaskDto>> StartTaskAsync(Guid tenantId, Guid id);
    Task<ApiResponse<PackingTaskDto>> CompleteTaskAsync(Guid tenantId, Guid id, CompletePackingTaskRequest request);
    Task<ApiResponse<PackingTaskDto>> CancelTaskAsync(Guid tenantId, Guid id, string? reason);

    #endregion

    #region Packages

    Task<ApiResponse<PackageDto>> CreatePackageAsync(Guid tenantId, Guid taskId, CreatePackageRequest request);
    Task<ApiResponse<PackageDto>> AddItemToPackageAsync(
        Guid tenantId, Guid taskId, Guid packageId, AddItemToPackageRequest request);
    Task<ApiResponse<PackageDto>> RemoveItemFromPackageAsync(
        Guid tenantId, Guid taskId, Guid packageId, Guid itemId);
    Task<ApiResponse<PackageDto>> SetTrackingAsync(
        Guid tenantId, Guid taskId, Guid packageId, SetTrackingRequest request);
    Task<ApiResponse<bool>> DeletePackageAsync(Guid tenantId, Guid taskId, Guid packageId);

    #endregion

    #region Packing Stations

    Task<ApiResponse<List<PackingStationDto>>> GetPackingStationsAsync(Guid tenantId, Guid? warehouseId = null);
    Task<ApiResponse<PackingStationDto>> GetPackingStationByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<PackingStationDto>> CreatePackingStationAsync(Guid tenantId, CreatePackingStationRequest request);
    Task<ApiResponse<PackingStationDto>> UpdatePackingStationAsync(
        Guid tenantId, Guid id, UpdatePackingStationRequest request);
    Task<ApiResponse<bool>> DeletePackingStationAsync(Guid tenantId, Guid id);

    #endregion
}
