using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.OperationHub.DTOs;
using SmartWMS.API.Modules.OperationHub.Models;
using SmartWMS.API.Modules.OperationHub.Services;

namespace SmartWMS.API.Modules.OperationHub.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/operation-hub")]
[Authorize]
public class OperationHubController : ControllerBase
{
    private readonly IOperationHubService _service;

    public OperationHubController(IOperationHubService service)
    {
        _service = service;
    }

    #region Session Management

    /// <summary>
    /// Start a new operator session
    /// </summary>
    [HttpPost("sessions/start")]
    public async Task<IActionResult> StartSession(Guid tenantId, [FromBody] StartSessionRequest request)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _service.StartSessionAsync(tenantId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get current active session for the logged-in user
    /// </summary>
    [HttpGet("sessions/current")]
    public async Task<IActionResult> GetCurrentSession(Guid tenantId)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _service.GetCurrentSessionAsync(tenantId, userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Update session status (Active, OnBreak, Idle)
    /// </summary>
    [HttpPut("sessions/{sessionId}/status")]
    public async Task<IActionResult> UpdateSessionStatus(Guid tenantId, Guid sessionId, [FromBody] UpdateSessionStatusRequest request)
    {
        var result = await _service.UpdateSessionStatusAsync(tenantId, sessionId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// End operator session
    /// </summary>
    [HttpPost("sessions/{sessionId}/end")]
    public async Task<IActionResult> EndSession(Guid tenantId, Guid sessionId)
    {
        var result = await _service.EndSessionAsync(tenantId, sessionId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get all active sessions (optionally filtered by warehouse)
    /// </summary>
    [HttpGet("sessions/active")]
    public async Task<IActionResult> GetActiveSessions(Guid tenantId, [FromQuery] Guid? warehouseId = null)
    {
        var result = await _service.GetActiveSessionsAsync(tenantId, warehouseId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Unified Task Queue

    /// <summary>
    /// Get unified task queue (all task types: Pick, Pack, Putaway, CycleCount)
    /// </summary>
    [HttpGet("tasks")]
    public async Task<IActionResult> GetTaskQueue(Guid tenantId, [FromQuery] TaskQueueQueryParams query)
    {
        var result = await _service.GetTaskQueueAsync(tenantId, query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get specific task by type and ID
    /// </summary>
    [HttpGet("tasks/{taskType}/{taskId}")]
    public async Task<IActionResult> GetTaskById(Guid tenantId, string taskType, Guid taskId)
    {
        var result = await _service.GetTaskByIdAsync(tenantId, taskType, taskId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get next available task for the operator
    /// </summary>
    [HttpGet("tasks/next")]
    public async Task<IActionResult> GetNextTask(Guid tenantId, [FromQuery] Guid warehouseId, [FromQuery] string? preferredTaskType = null)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _service.GetNextTaskAsync(tenantId, userId, warehouseId, preferredTaskType);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Assign task to an operator
    /// </summary>
    [HttpPost("tasks/assign")]
    public async Task<IActionResult> AssignTask(Guid tenantId, [FromBody] AssignTaskRequest request)
    {
        var result = await _service.AssignTaskAsync(tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Start working on a task
    /// </summary>
    [HttpPost("tasks/{taskType}/{taskId}/start")]
    public async Task<IActionResult> StartTask(Guid tenantId, string taskType, Guid taskId)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var request = new StartTaskRequest { TaskId = taskId, TaskType = taskType };
        var result = await _service.StartTaskAsync(tenantId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Complete a task
    /// </summary>
    [HttpPost("tasks/{taskType}/{taskId}/complete")]
    public async Task<IActionResult> CompleteTask(Guid tenantId, string taskType, Guid taskId, [FromBody] CompleteTaskRequest request)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        request.TaskId = taskId;
        request.TaskType = taskType;
        var result = await _service.CompleteTaskAsync(tenantId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Pause a task
    /// </summary>
    [HttpPost("tasks/{taskType}/{taskId}/pause")]
    public async Task<IActionResult> PauseTask(Guid tenantId, string taskType, Guid taskId, [FromBody] PauseTaskRequest? request = null)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        request ??= new PauseTaskRequest();
        request.TaskId = taskId;
        request.TaskType = taskType;
        var result = await _service.PauseTaskAsync(tenantId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Resume a paused task
    /// </summary>
    [HttpPost("tasks/{taskType}/{taskId}/resume")]
    public async Task<IActionResult> ResumeTask(Guid tenantId, string taskType, Guid taskId)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var request = new StartTaskRequest { TaskId = taskId, TaskType = taskType };
        var result = await _service.ResumeTaskAsync(tenantId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Barcode Scanning

    /// <summary>
    /// Process a barcode scan
    /// </summary>
    [HttpPost("scan")]
    public async Task<IActionResult> ProcessScan(Guid tenantId, [FromBody] ScanRequest request)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _service.ProcessScanAsync(tenantId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Validate a barcode without logging (for quick lookups)
    /// </summary>
    [HttpGet("scan/validate")]
    public async Task<IActionResult> ValidateBarcode(Guid tenantId, [FromQuery] string barcode, [FromQuery] ScanContext context = ScanContext.Other)
    {
        var result = await _service.ValidateBarcodeAsync(tenantId, barcode, context);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get scan logs
    /// </summary>
    [HttpGet("scan/logs")]
    public async Task<IActionResult> GetScanLogs(Guid tenantId, [FromQuery] ScanLogQueryParams query)
    {
        var result = await _service.GetScanLogsAsync(tenantId, query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Operator Productivity

    /// <summary>
    /// Get operator stats (current session + today's metrics)
    /// </summary>
    [HttpGet("operator/{userId}/stats")]
    public async Task<IActionResult> GetOperatorStats(Guid tenantId, Guid userId)
    {
        var result = await _service.GetOperatorStatsAsync(tenantId, userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get current user's stats
    /// </summary>
    [HttpGet("operator/me/stats")]
    public async Task<IActionResult> GetMyStats(Guid tenantId)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _service.GetOperatorStatsAsync(tenantId, userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get operator productivity for a specific date
    /// </summary>
    [HttpGet("operator/{userId}/productivity")]
    public async Task<IActionResult> GetOperatorProductivity(Guid tenantId, Guid userId, [FromQuery] DateTime? date = null)
    {
        var result = await _service.GetOperatorProductivityAsync(tenantId, userId, date ?? DateTime.UtcNow);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get productivity history
    /// </summary>
    [HttpGet("productivity/history")]
    public async Task<IActionResult> GetProductivityHistory(Guid tenantId, [FromQuery] ProductivityQueryParams query)
    {
        var result = await _service.GetProductivityHistoryAsync(tenantId, query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get productivity summary
    /// </summary>
    [HttpGet("productivity/summary")]
    public async Task<IActionResult> GetProductivitySummary(Guid tenantId, [FromQuery] ProductivityQueryParams query)
    {
        var result = await _service.GetProductivitySummaryAsync(tenantId, query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get warehouse operators overview
    /// </summary>
    [HttpGet("warehouse/{warehouseId}/operators")]
    public async Task<IActionResult> GetWarehouseOperatorsOverview(Guid tenantId, Guid warehouseId)
    {
        var result = await _service.GetWarehouseOperatorsOverviewAsync(tenantId, warehouseId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    #endregion

    #region Task Action Logs

    /// <summary>
    /// Get task action logs
    /// </summary>
    [HttpGet("logs/actions")]
    public async Task<IActionResult> GetTaskActionLogs(Guid tenantId, [FromQuery] TaskActionLogQueryParams query)
    {
        var result = await _service.GetTaskActionLogsAsync(tenantId, query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get task history (all actions for a specific task)
    /// </summary>
    [HttpGet("tasks/{taskType}/{taskId}/history")]
    public async Task<IActionResult> GetTaskHistory(Guid tenantId, string taskType, Guid taskId)
    {
        var result = await _service.GetTaskHistoryAsync(tenantId, taskType, taskId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    #endregion
}
