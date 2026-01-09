using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Sessions.DTOs;

namespace SmartWMS.API.Modules.Sessions.Services;

public interface ISessionsService
{
    // Sessions
    Task<ApiResponse<PaginatedResult<UserSessionDto>>> GetUserSessionsAsync(
        Guid tenantId, Guid userId, SessionQueryParams query);
    Task<ApiResponse<UserSessionDto>> GetSessionByIdAsync(Guid tenantId, Guid sessionId);
    Task<ApiResponse<ActiveSessionsDto>> GetActiveSessionsAsync(Guid tenantId, Guid userId, Guid? currentSessionId = null);
    Task<ApiResponse<UserSessionDto>> CreateSessionAsync(Guid tenantId, Guid userId, CreateSessionRequest request);
    Task<ApiResponse<UserSessionDto>> RefreshSessionAsync(Guid tenantId, Guid sessionId, string refreshToken);
    Task<ApiResponse<bool>> UpdateActivityAsync(Guid tenantId, Guid sessionId);
    Task<ApiResponse<bool>> RevokeSessionAsync(Guid tenantId, Guid sessionId, RevokeSessionRequest? request = null);
    Task<ApiResponse<int>> RevokeAllSessionsAsync(Guid tenantId, Guid userId, Guid? exceptSessionId = null);
    Task<ApiResponse<int>> RevokeSessionsByDeviceAsync(Guid tenantId, Guid userId, string deviceId);

    // Login Attempts
    Task<ApiResponse<PaginatedResult<LoginAttemptDto>>> GetLoginAttemptsAsync(
        Guid tenantId, Guid? userId, LoginAttemptQueryParams query);
    Task<ApiResponse<LoginStatsDto>> GetLoginStatsAsync(Guid tenantId, Guid userId, int days = 30);
    Task<ApiResponse<bool>> RecordLoginAttemptAsync(Guid tenantId, LoginAttemptDto attempt);

    // Trusted Devices
    Task<ApiResponse<List<TrustedDeviceDto>>> GetTrustedDevicesAsync(Guid tenantId, Guid userId);
    Task<ApiResponse<TrustedDeviceDto>> RegisterTrustedDeviceAsync(Guid tenantId, Guid userId, RegisterDeviceRequest request);
    Task<ApiResponse<bool>> RemoveTrustedDeviceAsync(Guid tenantId, Guid deviceId);
    Task<ApiResponse<bool>> IsTrustedDeviceAsync(Guid tenantId, Guid userId, string deviceId);

    // Cleanup
    Task<ApiResponse<int>> CleanupExpiredSessionsAsync(Guid tenantId);
    Task<ApiResponse<int>> PurgeOldLoginAttemptsAsync(Guid tenantId, int daysOld);
}
