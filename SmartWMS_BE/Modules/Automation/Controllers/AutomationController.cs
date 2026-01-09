using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Automation.DTOs;
using SmartWMS.API.Modules.Automation.Services;

namespace SmartWMS.API.Modules.Automation.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/automation")]
[Authorize]
public class AutomationController : ControllerBase
{
    private readonly IAutomationService _automationService;

    public AutomationController(IAutomationService automationService)
    {
        _automationService = automationService;
    }

    #region Rules CRUD

    /// <summary>
    /// Get all automation rules with optional filtering
    /// </summary>
    [HttpGet("rules")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<AutomationRuleDto>>>> GetRules(
        Guid tenantId,
        [FromQuery] AutomationRuleFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _automationService.GetRulesAsync(tenantId, filters, cancellationToken);
        return Ok(ApiResponse<PaginatedResult<AutomationRuleDto>>.Ok(result));
    }

    /// <summary>
    /// Get a specific automation rule by ID
    /// </summary>
    [HttpGet("rules/{id}")]
    public async Task<ActionResult<ApiResponse<AutomationRuleDetailDto>>> GetRuleById(
        Guid tenantId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _automationService.GetRuleByIdAsync(tenantId, id, cancellationToken);

        if (result == null)
            return NotFound(ApiResponse<AutomationRuleDetailDto>.Fail("Rule not found"));

        return Ok(ApiResponse<AutomationRuleDetailDto>.Ok(result));
    }

    /// <summary>
    /// Create a new automation rule
    /// </summary>
    [HttpPost("rules")]
    public async Task<ActionResult<ApiResponse<AutomationRuleDto>>> CreateRule(
        Guid tenantId,
        [FromBody] CreateAutomationRuleRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        var result = await _automationService.CreateRuleAsync(tenantId, request, userId, cancellationToken);

        return CreatedAtAction(
            nameof(GetRuleById),
            new { tenantId, id = result.Id },
            ApiResponse<AutomationRuleDto>.Ok(result, "Rule created successfully"));
    }

    /// <summary>
    /// Update an existing automation rule
    /// </summary>
    [HttpPut("rules/{id}")]
    public async Task<ActionResult<ApiResponse<AutomationRuleDto>>> UpdateRule(
        Guid tenantId,
        Guid id,
        [FromBody] UpdateAutomationRuleRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        var result = await _automationService.UpdateRuleAsync(tenantId, id, request, userId, cancellationToken);

        if (result == null)
            return NotFound(ApiResponse<AutomationRuleDto>.Fail("Rule not found"));

        return Ok(ApiResponse<AutomationRuleDto>.Ok(result, "Rule updated successfully"));
    }

    /// <summary>
    /// Delete an automation rule
    /// </summary>
    [HttpDelete("rules/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRule(
        Guid tenantId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var success = await _automationService.DeleteRuleAsync(tenantId, id, cancellationToken);

        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Rule not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Rule deleted successfully"));
    }

    /// <summary>
    /// Toggle rule active status
    /// </summary>
    [HttpPost("rules/{id}/toggle")]
    public async Task<ActionResult<ApiResponse<AutomationRuleDto>>> ToggleRule(
        Guid tenantId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var rule = await _automationService.GetRuleByIdAsync(tenantId, id, cancellationToken);
        if (rule == null)
            return NotFound(ApiResponse<AutomationRuleDto>.Fail("Rule not found"));

        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        var result = await _automationService.UpdateRuleAsync(tenantId, id,
            new UpdateAutomationRuleRequest { IsActive = !rule.IsActive },
            userId, cancellationToken);

        return Ok(ApiResponse<AutomationRuleDto>.Ok(result!, $"Rule {(result!.IsActive ? "activated" : "deactivated")}"));
    }

    #endregion

    #region Rule Execution

    /// <summary>
    /// Manually trigger a rule
    /// </summary>
    [HttpPost("rules/{id}/trigger")]
    public async Task<ActionResult<ApiResponse<RuleExecutionDto>>> TriggerRule(
        Guid tenantId,
        Guid id,
        [FromBody] TriggerRuleRequest? request,
        CancellationToken cancellationToken)
    {
        try
        {
            var execution = await _automationService.TriggerRuleAsync(tenantId, id, request, cancellationToken);

            var dto = new RuleExecutionDto
            {
                Id = execution.Id,
                RuleId = execution.RuleId,
                StartedAt = execution.StartedAt,
                CompletedAt = execution.CompletedAt,
                DurationMs = execution.DurationMs,
                Status = execution.Status,
                ConditionsMet = execution.ConditionsMet,
                ResultData = execution.ResultData,
                ErrorMessage = execution.ErrorMessage
            };

            return Ok(ApiResponse<RuleExecutionDto>.Ok(dto, "Rule triggered"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<RuleExecutionDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Test a rule without actually executing the action
    /// </summary>
    [HttpPost("rules/{id}/test")]
    public async Task<ActionResult<ApiResponse<TestRuleResponse>>> TestRule(
        Guid tenantId,
        Guid id,
        [FromBody] TestRuleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _automationService.TestRuleAsync(tenantId, id, request, cancellationToken);
            return Ok(ApiResponse<TestRuleResponse>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<TestRuleResponse>.Fail(ex.Message));
        }
    }

    #endregion

    #region Executions History

    /// <summary>
    /// Get execution history with optional filtering
    /// </summary>
    [HttpGet("executions")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<RuleExecutionDto>>>> GetExecutions(
        Guid tenantId,
        [FromQuery] RuleExecutionFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _automationService.GetExecutionsAsync(tenantId, filters, cancellationToken);
        return Ok(ApiResponse<PaginatedResult<RuleExecutionDto>>.Ok(result));
    }

    /// <summary>
    /// Get executions for a specific rule
    /// </summary>
    [HttpGet("rules/{id}/executions")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<RuleExecutionDto>>>> GetRuleExecutions(
        Guid tenantId,
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var filters = new RuleExecutionFilters { RuleId = id, Page = page, PageSize = pageSize };
        var result = await _automationService.GetExecutionsAsync(tenantId, filters, cancellationToken);
        return Ok(ApiResponse<PaginatedResult<RuleExecutionDto>>.Ok(result));
    }

    #endregion

    #region Stats

    /// <summary>
    /// Get automation statistics and dashboard data
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<AutomationStatsDto>>> GetStats(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var result = await _automationService.GetStatsAsync(tenantId, cancellationToken);
        return Ok(ApiResponse<AutomationStatsDto>.Ok(result));
    }

    #endregion

    #region Trigger Types & Action Types (for UI dropdowns)

    /// <summary>
    /// Get available trigger types
    /// </summary>
    [HttpGet("trigger-types")]
    public ActionResult<ApiResponse<List<EnumOption>>> GetTriggerTypes()
    {
        var types = Enum.GetValues<Models.TriggerType>()
            .Select(t => new EnumOption { Value = (int)t, Name = t.ToString() })
            .ToList();

        return Ok(ApiResponse<List<EnumOption>>.Ok(types));
    }

    /// <summary>
    /// Get available action types
    /// </summary>
    [HttpGet("action-types")]
    public ActionResult<ApiResponse<List<EnumOption>>> GetActionTypes()
    {
        var types = Enum.GetValues<Models.ActionType>()
            .Select(t => new EnumOption { Value = (int)t, Name = t.ToString() })
            .ToList();

        return Ok(ApiResponse<List<EnumOption>>.Ok(types));
    }

    /// <summary>
    /// Get available condition operators
    /// </summary>
    [HttpGet("condition-operators")]
    public ActionResult<ApiResponse<List<EnumOption>>> GetConditionOperators()
    {
        var operators = Enum.GetValues<Models.ConditionOperator>()
            .Select(o => new EnumOption { Value = (int)o, Name = o.ToString() })
            .ToList();

        return Ok(ApiResponse<List<EnumOption>>.Ok(operators));
    }

    #endregion
}

public record EnumOption
{
    public int Value { get; init; }
    public string Name { get; init; } = string.Empty;
}
