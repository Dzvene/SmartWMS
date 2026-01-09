using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.OperationHub.DTOs;
using SmartWMS.API.Modules.OperationHub.Models;

namespace SmartWMS.API.Modules.OperationHub.Services;

public interface IOperationHubService
{
    #region Session Management

    Task<ApiResponse<OperatorSessionDto>> StartSessionAsync(Guid tenantId, Guid userId, StartSessionRequest request);
    Task<ApiResponse<OperatorSessionDto>> GetCurrentSessionAsync(Guid tenantId, Guid userId);
    Task<ApiResponse<OperatorSessionDto>> UpdateSessionStatusAsync(Guid tenantId, Guid sessionId, UpdateSessionStatusRequest request);
    Task<ApiResponse<OperatorSessionDto>> EndSessionAsync(Guid tenantId, Guid sessionId);
    Task<ApiResponse<PaginatedResult<OperatorSessionDto>>> GetActiveSessionsAsync(Guid tenantId, Guid? warehouseId = null);

    #endregion

    #region Unified Task Queue

    Task<ApiResponse<PaginatedResult<UnifiedTaskDto>>> GetTaskQueueAsync(Guid tenantId, TaskQueueQueryParams query);
    Task<ApiResponse<UnifiedTaskDto>> GetTaskByIdAsync(Guid tenantId, string taskType, Guid taskId);
    Task<ApiResponse<UnifiedTaskDto>> GetNextTaskAsync(Guid tenantId, Guid userId, Guid warehouseId, string? preferredTaskType = null);
    Task<ApiResponse<UnifiedTaskDto>> AssignTaskAsync(Guid tenantId, AssignTaskRequest request);
    Task<ApiResponse<UnifiedTaskDto>> StartTaskAsync(Guid tenantId, Guid userId, StartTaskRequest request);
    Task<ApiResponse<UnifiedTaskDto>> CompleteTaskAsync(Guid tenantId, Guid userId, CompleteTaskRequest request);
    Task<ApiResponse<UnifiedTaskDto>> PauseTaskAsync(Guid tenantId, Guid userId, PauseTaskRequest request);
    Task<ApiResponse<UnifiedTaskDto>> ResumeTaskAsync(Guid tenantId, Guid userId, StartTaskRequest request);

    #endregion

    #region Barcode Scanning

    Task<ApiResponse<ScanResponse>> ProcessScanAsync(Guid tenantId, Guid userId, ScanRequest request);
    Task<ApiResponse<ScanResponse>> ValidateBarcodeAsync(Guid tenantId, string barcode, ScanContext context);
    Task<ApiResponse<PaginatedResult<ScanLogDto>>> GetScanLogsAsync(Guid tenantId, ScanLogQueryParams query);

    #endregion

    #region Operator Productivity

    Task<ApiResponse<OperatorStatsDto>> GetOperatorStatsAsync(Guid tenantId, Guid userId);
    Task<ApiResponse<OperatorProductivityDto>> GetOperatorProductivityAsync(Guid tenantId, Guid userId, DateTime date);
    Task<ApiResponse<List<OperatorProductivityDto>>> GetProductivityHistoryAsync(Guid tenantId, ProductivityQueryParams query);
    Task<ApiResponse<ProductivitySummaryDto>> GetProductivitySummaryAsync(Guid tenantId, ProductivityQueryParams query);
    Task<ApiResponse<WarehouseOperatorsOverviewDto>> GetWarehouseOperatorsOverviewAsync(Guid tenantId, Guid warehouseId);

    #endregion

    #region Task Action Logs

    Task<ApiResponse<PaginatedResult<TaskActionLogDto>>> GetTaskActionLogsAsync(Guid tenantId, TaskActionLogQueryParams query);
    Task<ApiResponse<List<TaskActionLogDto>>> GetTaskHistoryAsync(Guid tenantId, string taskType, Guid taskId);

    #endregion
}
