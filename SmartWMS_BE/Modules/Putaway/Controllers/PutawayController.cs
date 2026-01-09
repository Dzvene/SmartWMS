using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Putaway.DTOs;
using SmartWMS.API.Modules.Putaway.Models;
using SmartWMS.API.Modules.Putaway.Services;

namespace SmartWMS.API.Modules.Putaway.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/putaway")]
[Authorize]
public class PutawayController : ControllerBase
{
    private readonly IPutawayService _putawayService;

    public PutawayController(IPutawayService putawayService)
    {
        _putawayService = putawayService;
    }

    #region Queries

    /// <summary>
    /// Get paginated putaway tasks with optional filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTasks(
        Guid tenantId,
        [FromQuery] PutawayTaskStatus? status = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? fromLocationId = null,
        [FromQuery] Guid? goodsReceiptId = null,
        [FromQuery] int? priority = null,
        [FromQuery] bool? unassigned = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filters = new PutawayTaskFilters
        {
            Status = status,
            AssignedToUserId = assignedToUserId,
            ProductId = productId,
            FromLocationId = fromLocationId,
            GoodsReceiptId = goodsReceiptId,
            Priority = priority,
            Unassigned = unassigned
        };

        var result = await _putawayService.GetTasksAsync(tenantId, filters, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get putaway task by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(Guid tenantId, Guid id)
    {
        var result = await _putawayService.GetTaskByIdAsync(tenantId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get my assigned putaway tasks
    /// </summary>
    [HttpGet("my-tasks/{userId}")]
    public async Task<IActionResult> GetMyTasks(Guid tenantId, Guid userId)
    {
        var result = await _putawayService.GetMyTasksAsync(tenantId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get next task for user (for mobile)
    /// </summary>
    [HttpGet("next/{userId}")]
    public async Task<IActionResult> GetNextTask(Guid tenantId, Guid userId)
    {
        var result = await _putawayService.GetNextTaskAsync(tenantId, userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    #endregion

    #region Task Creation

    /// <summary>
    /// Create putaway tasks from goods receipt
    /// </summary>
    [HttpPost("from-goods-receipt")]
    public async Task<IActionResult> CreateFromGoodsReceipt(
        Guid tenantId,
        [FromBody] CreatePutawayFromReceiptRequest request)
    {
        var result = await _putawayService.CreateFromGoodsReceiptAsync(tenantId, request);
        return result.Success ? CreatedAtAction(nameof(GetTasks), new { tenantId }, result) : BadRequest(result);
    }

    /// <summary>
    /// Create manual putaway task
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTask(Guid tenantId, [FromBody] CreatePutawayTaskRequest request)
    {
        var result = await _putawayService.CreateTaskAsync(tenantId, request);
        return result.Success ? CreatedAtAction(nameof(GetTaskById), new { tenantId, id = result.Data!.Id }, result) : BadRequest(result);
    }

    #endregion

    #region Task Workflow

    /// <summary>
    /// Assign task to user
    /// </summary>
    [HttpPost("{id}/assign")]
    public async Task<IActionResult> AssignTask(Guid tenantId, Guid id, [FromBody] AssignPutawayTaskRequest request)
    {
        var result = await _putawayService.AssignTaskAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Start working on task
    /// </summary>
    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartTask(Guid tenantId, Guid id)
    {
        var result = await _putawayService.StartTaskAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Complete putaway task
    /// </summary>
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteTask(Guid tenantId, Guid id, [FromBody] CompletePutawayTaskRequest request)
    {
        var result = await _putawayService.CompleteTaskAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Cancel putaway task
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelTask(Guid tenantId, Guid id, [FromBody] string? reason = null)
    {
        var result = await _putawayService.CancelTaskAsync(tenantId, id, reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Location Suggestion

    /// <summary>
    /// Get suggested locations for putaway
    /// </summary>
    [HttpPost("suggest-location")]
    public async Task<IActionResult> SuggestLocation(Guid tenantId, [FromBody] SuggestLocationRequest request)
    {
        var result = await _putawayService.SuggestLocationsAsync(tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
