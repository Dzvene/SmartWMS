using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Fulfillment.DTOs;
using SmartWMS.API.Modules.Fulfillment.Services;

namespace SmartWMS.API.Modules.Fulfillment.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/pick-tasks")]
[Authorize]
public class PickTasksController : ControllerBase
{
    private readonly IPickTasksService _pickTasksService;

    public PickTasksController(IPickTasksService pickTasksService)
    {
        _pickTasksService = pickTasksService;
    }

    /// <summary>
    /// Get all pick tasks with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PickTaskDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks(
        Guid tenantId,
        [FromQuery] PickTaskFilters? filters,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = await _pickTasksService.GetTasksAsync(tenantId, filters, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get a pick task by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PickTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTask(Guid tenantId, Guid id)
    {
        var result = await _pickTasksService.GetTaskByIdAsync(tenantId, id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get a pick task by task number
    /// </summary>
    [HttpGet("by-number/{taskNumber}")]
    [ProducesResponseType(typeof(ApiResponse<PickTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskByNumber(Guid tenantId, string taskNumber)
    {
        var result = await _pickTasksService.GetTaskByNumberAsync(tenantId, taskNumber);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new pick task
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PickTaskDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask(Guid tenantId, [FromBody] CreatePickTaskRequest request)
    {
        var result = await _pickTasksService.CreateTaskAsync(tenantId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(
            nameof(GetTask),
            new { tenantId, id = result.Data!.Id },
            result);
    }

    /// <summary>
    /// Delete a pick task
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid tenantId, Guid id)
    {
        var result = await _pickTasksService.DeleteTaskAsync(tenantId, id);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Assign a pick task to a user
    /// </summary>
    [HttpPost("{id:guid}/assign")]
    [ProducesResponseType(typeof(ApiResponse<PickTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTask(Guid tenantId, Guid id, [FromBody] AssignPickTaskRequest request)
    {
        var result = await _pickTasksService.AssignTaskAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Start working on a pick task
    /// </summary>
    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(typeof(ApiResponse<PickTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartTask(Guid tenantId, Guid id, [FromBody] StartPickTaskRequest? request = null)
    {
        var result = await _pickTasksService.StartTaskAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Confirm a pick (mark quantity as picked)
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(typeof(ApiResponse<PickTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmPick(Guid tenantId, Guid id, [FromBody] ConfirmPickRequest request)
    {
        var result = await _pickTasksService.ConfirmPickAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Report a short pick (item not available)
    /// </summary>
    [HttpPost("{id:guid}/short-pick")]
    [ProducesResponseType(typeof(ApiResponse<PickTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ShortPick(Guid tenantId, Guid id, [FromBody] ShortPickRequest request)
    {
        var result = await _pickTasksService.ShortPickAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel a pick task
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<PickTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelTask(Guid tenantId, Guid id, [FromQuery] string? reason = null)
    {
        var result = await _pickTasksService.CancelTaskAsync(tenantId, id, reason);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all tasks assigned to a specific user
    /// </summary>
    [HttpGet("my-tasks/{userId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PickTaskDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTasks(Guid tenantId, Guid userId)
    {
        var result = await _pickTasksService.GetMyTasksAsync(tenantId, userId);
        return Ok(result);
    }

    /// <summary>
    /// Get the next available task for a user
    /// </summary>
    [HttpGet("next/{userId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PickTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNextTask(Guid tenantId, Guid userId, [FromQuery] Guid? warehouseId = null)
    {
        var result = await _pickTasksService.GetNextTaskAsync(tenantId, userId, warehouseId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
