using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.CycleCount.DTOs;

namespace SmartWMS.API.Modules.CycleCount.Services;

public interface ICycleCountService
{
    // Queries
    Task<ApiResponse<PaginatedResult<CycleCountSessionListDto>>> GetCycleCountsAsync(
        Guid tenantId, CycleCountFilters? filters, int page, int pageSize);
    Task<ApiResponse<CycleCountSessionDto>> GetCycleCountByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<List<CycleCountSessionListDto>>> GetMyCycleCountsAsync(Guid tenantId, Guid userId);

    // CRUD
    Task<ApiResponse<CycleCountSessionDto>> CreateCycleCountAsync(Guid tenantId, CreateCycleCountRequest request);
    Task<ApiResponse<CycleCountSessionDto>> UpdateCycleCountAsync(Guid tenantId, Guid id, UpdateCycleCountRequest request);
    Task<ApiResponse<bool>> DeleteCycleCountAsync(Guid tenantId, Guid id);

    // Workflow
    Task<ApiResponse<CycleCountSessionDto>> AssignCycleCountAsync(Guid tenantId, Guid id, AssignCycleCountRequest request);
    Task<ApiResponse<CycleCountSessionDto>> ScheduleCycleCountAsync(Guid tenantId, Guid id, DateTime scheduledDate);
    Task<ApiResponse<CycleCountSessionDto>> StartCycleCountAsync(Guid tenantId, Guid id);
    Task<ApiResponse<CycleCountSessionDto>> RecordCountAsync(Guid tenantId, Guid sessionId, Guid itemId, RecordCountRequest request);
    Task<ApiResponse<CycleCountSessionDto>> RequestRecountAsync(Guid tenantId, Guid sessionId, Guid itemId, string? reason);
    Task<ApiResponse<CycleCountSessionDto>> ApproveVarianceAsync(Guid tenantId, Guid sessionId, Guid itemId, ApproveVarianceRequest request);
    Task<ApiResponse<CycleCountSessionDto>> AdjustStockAsync(Guid tenantId, Guid sessionId, Guid itemId, AdjustStockRequest request);
    Task<ApiResponse<CycleCountSessionDto>> CompleteCycleCountAsync(Guid tenantId, Guid id);
    Task<ApiResponse<CycleCountSessionDto>> CancelCycleCountAsync(Guid tenantId, Guid id, string? reason);
}
