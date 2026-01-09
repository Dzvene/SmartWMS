using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Adjustments.DTOs;
using SmartWMS.API.Modules.Adjustments.Models;

namespace SmartWMS.API.Modules.Adjustments.Services;

public interface IAdjustmentsService
{
    // Stock Adjustments CRUD
    Task<ApiResponse<PaginatedResult<StockAdjustmentSummaryDto>>> GetAdjustmentsAsync(
        Guid tenantId, AdjustmentFilterRequest filter);
    Task<ApiResponse<StockAdjustmentDto>> GetAdjustmentByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<StockAdjustmentDto>> GetAdjustmentByNumberAsync(Guid tenantId, string adjustmentNumber);
    Task<ApiResponse<StockAdjustmentDto>> CreateAdjustmentAsync(
        Guid tenantId, Guid userId, CreateStockAdjustmentRequest request);
    Task<ApiResponse<StockAdjustmentDto>> UpdateAdjustmentAsync(
        Guid tenantId, Guid id, UpdateStockAdjustmentRequest request);
    Task<ApiResponse<bool>> DeleteAdjustmentAsync(Guid tenantId, Guid id);

    // Adjustment Lines
    Task<ApiResponse<StockAdjustmentLineDto>> AddLineAsync(
        Guid tenantId, Guid adjustmentId, AddAdjustmentLineRequest request);
    Task<ApiResponse<StockAdjustmentLineDto>> UpdateLineAsync(
        Guid tenantId, Guid adjustmentId, Guid lineId, UpdateAdjustmentLineRequest request);
    Task<ApiResponse<bool>> RemoveLineAsync(Guid tenantId, Guid adjustmentId, Guid lineId);

    // Workflow Actions
    Task<ApiResponse<StockAdjustmentDto>> SubmitForApprovalAsync(
        Guid tenantId, Guid id, Guid userId, SubmitForApprovalRequest request);
    Task<ApiResponse<StockAdjustmentDto>> ApproveAsync(
        Guid tenantId, Guid id, Guid userId, ApproveAdjustmentRequest request);
    Task<ApiResponse<StockAdjustmentDto>> RejectAsync(
        Guid tenantId, Guid id, Guid userId, RejectAdjustmentRequest request);
    Task<ApiResponse<StockAdjustmentDto>> PostAsync(
        Guid tenantId, Guid id, Guid userId, PostAdjustmentRequest request);
    Task<ApiResponse<StockAdjustmentDto>> CancelAsync(Guid tenantId, Guid id, Guid userId);

    // Quick adjustment (create and post in one step)
    Task<ApiResponse<StockAdjustmentDto>> QuickAdjustAsync(
        Guid tenantId, Guid userId, CreateStockAdjustmentRequest request);

    // From Cycle Count
    Task<ApiResponse<StockAdjustmentDto>> CreateFromCycleCountAsync(
        Guid tenantId, Guid userId, Guid cycleCountSessionId);
}
