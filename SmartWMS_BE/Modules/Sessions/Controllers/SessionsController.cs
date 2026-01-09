using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Sessions.DTOs;
using SmartWMS.API.Modules.Sessions.Services;

namespace SmartWMS.API.Modules.Sessions.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/sessions")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly ISessionsService _sessionsService;

    public SessionsController(ISessionsService sessionsService)
    {
        _sessionsService = sessionsService;
    }

    #region Sessions

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserSessions(
        Guid tenantId, Guid userId, [FromQuery] SessionQueryParams query)
    {
        var result = await _sessionsService.GetUserSessionsAsync(tenantId, userId, query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetSessionById(Guid tenantId, Guid sessionId)
    {
        var result = await _sessionsService.GetSessionByIdAsync(tenantId, sessionId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("user/{userId}/active")]
    public async Task<IActionResult> GetActiveSessions(
        Guid tenantId, Guid userId, [FromQuery] Guid? currentSessionId = null)
    {
        var result = await _sessionsService.GetActiveSessionsAsync(tenantId, userId, currentSessionId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("user/{userId}")]
    public async Task<IActionResult> CreateSession(
        Guid tenantId, Guid userId, [FromBody] CreateSessionRequest request)
    {
        var result = await _sessionsService.CreateSessionAsync(tenantId, userId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetSessionById), new { tenantId, sessionId = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPost("{sessionId}/refresh")]
    public async Task<IActionResult> RefreshSession(
        Guid tenantId, Guid sessionId, [FromBody] RefreshSessionRequest request)
    {
        var result = await _sessionsService.RefreshSessionAsync(tenantId, sessionId, request.RefreshToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{sessionId}/activity")]
    public async Task<IActionResult> UpdateActivity(Guid tenantId, Guid sessionId)
    {
        var result = await _sessionsService.UpdateActivityAsync(tenantId, sessionId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{sessionId}/revoke")]
    public async Task<IActionResult> RevokeSession(
        Guid tenantId, Guid sessionId, [FromBody] RevokeSessionRequest? request = null)
    {
        var result = await _sessionsService.RevokeSessionAsync(tenantId, sessionId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("user/{userId}/revoke-all")]
    public async Task<IActionResult> RevokeAllSessions(
        Guid tenantId, Guid userId, [FromQuery] Guid? exceptSessionId = null)
    {
        var result = await _sessionsService.RevokeAllSessionsAsync(tenantId, userId, exceptSessionId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("user/{userId}/revoke-device/{deviceId}")]
    public async Task<IActionResult> RevokeSessionsByDevice(Guid tenantId, Guid userId, string deviceId)
    {
        var result = await _sessionsService.RevokeSessionsByDeviceAsync(tenantId, userId, deviceId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Login Attempts

    [HttpGet("login-attempts")]
    public async Task<IActionResult> GetLoginAttempts(
        Guid tenantId, [FromQuery] Guid? userId, [FromQuery] LoginAttemptQueryParams query)
    {
        var result = await _sessionsService.GetLoginAttemptsAsync(tenantId, userId, query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("user/{userId}/login-stats")]
    public async Task<IActionResult> GetLoginStats(Guid tenantId, Guid userId, [FromQuery] int days = 30)
    {
        var result = await _sessionsService.GetLoginStatsAsync(tenantId, userId, days);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Trusted Devices

    [HttpGet("user/{userId}/trusted-devices")]
    public async Task<IActionResult> GetTrustedDevices(Guid tenantId, Guid userId)
    {
        var result = await _sessionsService.GetTrustedDevicesAsync(tenantId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("user/{userId}/trusted-devices")]
    public async Task<IActionResult> RegisterTrustedDevice(
        Guid tenantId, Guid userId, [FromBody] RegisterDeviceRequest request)
    {
        var result = await _sessionsService.RegisterTrustedDeviceAsync(tenantId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("trusted-devices/{deviceId}")]
    public async Task<IActionResult> RemoveTrustedDevice(Guid tenantId, Guid deviceId)
    {
        var result = await _sessionsService.RemoveTrustedDeviceAsync(tenantId, deviceId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("user/{userId}/trusted-devices/{deviceId}/check")]
    public async Task<IActionResult> IsTrustedDevice(Guid tenantId, Guid userId, string deviceId)
    {
        var result = await _sessionsService.IsTrustedDeviceAsync(tenantId, userId, deviceId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Cleanup

    [HttpPost("cleanup-expired")]
    public async Task<IActionResult> CleanupExpiredSessions(Guid tenantId)
    {
        var result = await _sessionsService.CleanupExpiredSessionsAsync(tenantId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("purge-login-attempts")]
    public async Task<IActionResult> PurgeOldLoginAttempts(Guid tenantId, [FromQuery] int daysOld = 90)
    {
        var result = await _sessionsService.PurgeOldLoginAttemptsAsync(tenantId, daysOld);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
