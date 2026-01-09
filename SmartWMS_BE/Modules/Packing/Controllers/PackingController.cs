using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Packing.DTOs;
using SmartWMS.API.Modules.Packing.Models;
using SmartWMS.API.Modules.Packing.Services;

namespace SmartWMS.API.Modules.Packing.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/packing")]
[Authorize]
public class PackingController : ControllerBase
{
    private readonly IPackingService _packingService;

    public PackingController(IPackingService packingService)
    {
        _packingService = packingService;
    }

    #region Packing Tasks - Queries

    /// <summary>
    /// Get paginated packing tasks with optional filters
    /// </summary>
    [HttpGet("tasks")]
    public async Task<IActionResult> GetTasks(
        Guid tenantId,
        [FromQuery] PackingTaskStatus? status = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] Guid? salesOrderId = null,
        [FromQuery] Guid? fulfillmentBatchId = null,
        [FromQuery] Guid? packingStationId = null,
        [FromQuery] int? priority = null,
        [FromQuery] bool? unassigned = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filters = new PackingTaskFilters
        {
            Status = status,
            AssignedToUserId = assignedToUserId,
            SalesOrderId = salesOrderId,
            FulfillmentBatchId = fulfillmentBatchId,
            PackingStationId = packingStationId,
            Priority = priority,
            Unassigned = unassigned
        };

        var result = await _packingService.GetTasksAsync(tenantId, filters, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get packing task by ID
    /// </summary>
    [HttpGet("tasks/{id}")]
    public async Task<IActionResult> GetTaskById(Guid tenantId, Guid id)
    {
        var result = await _packingService.GetTaskByIdAsync(tenantId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get my assigned packing tasks
    /// </summary>
    [HttpGet("tasks/my-tasks/{userId}")]
    public async Task<IActionResult> GetMyTasks(Guid tenantId, Guid userId)
    {
        var result = await _packingService.GetMyTasksAsync(tenantId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get next task for user (for mobile/scanner)
    /// </summary>
    [HttpGet("tasks/next/{userId}")]
    public async Task<IActionResult> GetNextTask(Guid tenantId, Guid userId)
    {
        var result = await _packingService.GetNextTaskAsync(tenantId, userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    #endregion

    #region Packing Tasks - Creation

    /// <summary>
    /// Create packing task manually
    /// </summary>
    [HttpPost("tasks")]
    public async Task<IActionResult> CreateTask(Guid tenantId, [FromBody] CreatePackingTaskRequest request)
    {
        var result = await _packingService.CreateTaskAsync(tenantId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetTaskById), new { tenantId, id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Create packing tasks from fulfillment batch
    /// </summary>
    [HttpPost("tasks/from-fulfillment")]
    public async Task<IActionResult> CreateFromFulfillment(
        Guid tenantId,
        [FromBody] CreatePackingFromFulfillmentRequest request)
    {
        var result = await _packingService.CreateFromFulfillmentAsync(tenantId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetTasks), new { tenantId }, result)
            : BadRequest(result);
    }

    #endregion

    #region Packing Tasks - Workflow

    /// <summary>
    /// Assign task to user
    /// </summary>
    [HttpPost("tasks/{id}/assign")]
    public async Task<IActionResult> AssignTask(
        Guid tenantId, Guid id, [FromBody] AssignPackingTaskRequest request)
    {
        var result = await _packingService.AssignTaskAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Start working on task
    /// </summary>
    [HttpPost("tasks/{id}/start")]
    public async Task<IActionResult> StartTask(Guid tenantId, Guid id)
    {
        var result = await _packingService.StartTaskAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Complete packing task
    /// </summary>
    [HttpPost("tasks/{id}/complete")]
    public async Task<IActionResult> CompleteTask(
        Guid tenantId, Guid id, [FromBody] CompletePackingTaskRequest request)
    {
        var result = await _packingService.CompleteTaskAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Cancel packing task
    /// </summary>
    [HttpPost("tasks/{id}/cancel")]
    public async Task<IActionResult> CancelTask(
        Guid tenantId, Guid id, [FromBody] string? reason = null)
    {
        var result = await _packingService.CancelTaskAsync(tenantId, id, reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Packages

    /// <summary>
    /// Create new package in task
    /// </summary>
    [HttpPost("tasks/{taskId}/packages")]
    public async Task<IActionResult> CreatePackage(
        Guid tenantId, Guid taskId, [FromBody] CreatePackageRequest request)
    {
        var result = await _packingService.CreatePackageAsync(tenantId, taskId, request);
        return result.Success ? CreatedAtAction(nameof(GetTaskById), new { tenantId, id = taskId }, result) : BadRequest(result);
    }

    /// <summary>
    /// Add item to package
    /// </summary>
    [HttpPost("tasks/{taskId}/packages/{packageId}/items")]
    public async Task<IActionResult> AddItemToPackage(
        Guid tenantId, Guid taskId, Guid packageId, [FromBody] AddItemToPackageRequest request)
    {
        var result = await _packingService.AddItemToPackageAsync(tenantId, taskId, packageId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Remove item from package
    /// </summary>
    [HttpDelete("tasks/{taskId}/packages/{packageId}/items/{itemId}")]
    public async Task<IActionResult> RemoveItemFromPackage(
        Guid tenantId, Guid taskId, Guid packageId, Guid itemId)
    {
        var result = await _packingService.RemoveItemFromPackageAsync(tenantId, taskId, packageId, itemId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Set tracking number for package
    /// </summary>
    [HttpPost("tasks/{taskId}/packages/{packageId}/tracking")]
    public async Task<IActionResult> SetTracking(
        Guid tenantId, Guid taskId, Guid packageId, [FromBody] SetTrackingRequest request)
    {
        var result = await _packingService.SetTrackingAsync(tenantId, taskId, packageId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete package
    /// </summary>
    [HttpDelete("tasks/{taskId}/packages/{packageId}")]
    public async Task<IActionResult> DeletePackage(Guid tenantId, Guid taskId, Guid packageId)
    {
        var result = await _packingService.DeletePackageAsync(tenantId, taskId, packageId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Packing Stations

    /// <summary>
    /// Get all packing stations
    /// </summary>
    [HttpGet("stations")]
    public async Task<IActionResult> GetPackingStations(Guid tenantId, [FromQuery] Guid? warehouseId = null)
    {
        var result = await _packingService.GetPackingStationsAsync(tenantId, warehouseId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get packing station by ID
    /// </summary>
    [HttpGet("stations/{id}")]
    public async Task<IActionResult> GetPackingStationById(Guid tenantId, Guid id)
    {
        var result = await _packingService.GetPackingStationByIdAsync(tenantId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create packing station
    /// </summary>
    [HttpPost("stations")]
    public async Task<IActionResult> CreatePackingStation(
        Guid tenantId, [FromBody] CreatePackingStationRequest request)
    {
        var result = await _packingService.CreatePackingStationAsync(tenantId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetPackingStationById), new { tenantId, id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update packing station
    /// </summary>
    [HttpPut("stations/{id}")]
    public async Task<IActionResult> UpdatePackingStation(
        Guid tenantId, Guid id, [FromBody] UpdatePackingStationRequest request)
    {
        var result = await _packingService.UpdatePackingStationAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete packing station
    /// </summary>
    [HttpDelete("stations/{id}")]
    public async Task<IActionResult> DeletePackingStation(Guid tenantId, Guid id)
    {
        var result = await _packingService.DeletePackingStationAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
