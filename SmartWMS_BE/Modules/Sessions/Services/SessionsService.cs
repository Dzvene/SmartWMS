using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Sessions.DTOs;
using SmartWMS.API.Modules.Sessions.Models;

namespace SmartWMS.API.Modules.Sessions.Services;

public class SessionsService : ISessionsService
{
    private readonly ApplicationDbContext _context;

    public SessionsService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Sessions

    public async Task<ApiResponse<PaginatedResult<UserSessionDto>>> GetUserSessionsAsync(
        Guid tenantId, Guid userId, SessionQueryParams query)
    {
        var queryable = _context.UserSessions
            .Where(s => s.TenantId == tenantId && s.UserId == userId);

        if (query.Status.HasValue)
            queryable = queryable.Where(s => s.Status == query.Status.Value);

        if (!string.IsNullOrEmpty(query.DeviceType))
            queryable = queryable.Where(s => s.DeviceType == query.DeviceType);

        if (query.FromDate.HasValue)
            queryable = queryable.Where(s => s.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            queryable = queryable.Where(s => s.CreatedAt <= query.ToDate.Value);

        var totalCount = await queryable.CountAsync();

        var items = await queryable
            .OrderByDescending(s => s.LastActivityAt ?? s.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(s => new UserSessionDto(
                s.Id,
                s.UserId,
                s.DeviceId,
                s.DeviceName,
                s.DeviceType,
                s.Browser,
                s.OperatingSystem,
                s.IpAddress,
                s.Location,
                s.Status,
                s.LastActivityAt,
                s.ExpiresAt,
                s.IsTrustedDevice,
                false, // IsCurrent set below
                s.CreatedAt
            ))
            .ToListAsync();

        var result = new PaginatedResult<UserSessionDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };

        return ApiResponse<PaginatedResult<UserSessionDto>>.Ok(result);
    }

    public async Task<ApiResponse<UserSessionDto>> GetSessionByIdAsync(Guid tenantId, Guid sessionId)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == sessionId);

        if (session == null)
            return ApiResponse<UserSessionDto>.Fail("Session not found");

        return ApiResponse<UserSessionDto>.Ok(MapToDto(session, false));
    }

    public async Task<ApiResponse<ActiveSessionsDto>> GetActiveSessionsAsync(
        Guid tenantId, Guid userId, Guid? currentSessionId = null)
    {
        var activeSessions = await _context.UserSessions
            .Where(s => s.TenantId == tenantId &&
                       s.UserId == userId &&
                       s.Status == SessionStatus.Active &&
                       s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.LastActivityAt ?? s.CreatedAt)
            .ToListAsync();

        var currentSession = currentSessionId.HasValue
            ? activeSessions.FirstOrDefault(s => s.Id == currentSessionId.Value)
            : null;

        var otherSessions = activeSessions
            .Where(s => currentSessionId == null || s.Id != currentSessionId.Value)
            .Select(s => new SessionSummaryDto(
                s.Id,
                s.DeviceName,
                s.DeviceType,
                s.IpAddress,
                s.Location,
                s.Status,
                s.LastActivityAt,
                false
            ))
            .ToList();

        var uniqueDevices = activeSessions
            .Where(s => !string.IsNullOrEmpty(s.DeviceId))
            .Select(s => s.DeviceId)
            .Distinct()
            .Count();

        var result = new ActiveSessionsDto(
            TotalActive: activeSessions.Count,
            TotalDevices: uniqueDevices,
            CurrentSession: currentSession != null
                ? new SessionSummaryDto(
                    currentSession.Id,
                    currentSession.DeviceName,
                    currentSession.DeviceType,
                    currentSession.IpAddress,
                    currentSession.Location,
                    currentSession.Status,
                    currentSession.LastActivityAt,
                    true)
                : null,
            OtherSessions: otherSessions
        );

        return ApiResponse<ActiveSessionsDto>.Ok(result);
    }

    public async Task<ApiResponse<UserSessionDto>> CreateSessionAsync(
        Guid tenantId, Guid userId, CreateSessionRequest request)
    {
        // Check if device is trusted
        var isTrusted = false;
        if (!string.IsNullOrEmpty(request.DeviceId))
        {
            isTrusted = await _context.TrustedDevices
                .AnyAsync(d => d.TenantId == tenantId &&
                              d.UserId == userId &&
                              d.DeviceId == request.DeviceId &&
                              d.IsActive);
        }

        var session = new UserSession
        {
            TenantId = tenantId,
            UserId = userId,
            SessionToken = GenerateToken(),
            RefreshToken = GenerateToken(),
            DeviceId = request.DeviceId,
            DeviceName = request.DeviceName,
            DeviceType = request.DeviceType,
            Browser = request.Browser,
            OperatingSystem = request.OperatingSystem,
            AppVersion = request.AppVersion,
            IpAddress = request.IpAddress,
            Status = SessionStatus.Active,
            LastActivityAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24), // Default 24h expiry
            IsTrustedDevice = isTrusted
        };

        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();

        return ApiResponse<UserSessionDto>.Ok(MapToDto(session, true));
    }

    public async Task<ApiResponse<UserSessionDto>> RefreshSessionAsync(Guid tenantId, Guid sessionId, string refreshToken)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == sessionId);

        if (session == null)
            return ApiResponse<UserSessionDto>.Fail("Session not found");

        if (session.Status != SessionStatus.Active)
            return ApiResponse<UserSessionDto>.Fail("Session is not active");

        if (session.RefreshToken != refreshToken)
            return ApiResponse<UserSessionDto>.Fail("Invalid refresh token");

        // Extend session
        session.SessionToken = GenerateToken();
        session.RefreshToken = GenerateToken();
        session.ExpiresAt = DateTime.UtcNow.AddHours(24);
        session.LastActivityAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<UserSessionDto>.Ok(MapToDto(session, true));
    }

    public async Task<ApiResponse<bool>> UpdateActivityAsync(Guid tenantId, Guid sessionId)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == sessionId);

        if (session == null)
            return ApiResponse<bool>.Fail("Session not found");

        if (session.Status != SessionStatus.Active)
            return ApiResponse<bool>.Fail("Session is not active");

        session.LastActivityAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<bool>> RevokeSessionAsync(Guid tenantId, Guid sessionId, RevokeSessionRequest? request = null)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == sessionId);

        if (session == null)
            return ApiResponse<bool>.Fail("Session not found");

        session.Status = SessionStatus.Revoked;
        session.RevokedAt = DateTime.UtcNow;
        session.RevokedReason = request?.Reason ?? "User initiated logout";

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Session revoked");
    }

    public async Task<ApiResponse<int>> RevokeAllSessionsAsync(Guid tenantId, Guid userId, Guid? exceptSessionId = null)
    {
        var sessions = await _context.UserSessions
            .Where(s => s.TenantId == tenantId &&
                       s.UserId == userId &&
                       s.Status == SessionStatus.Active)
            .ToListAsync();

        if (exceptSessionId.HasValue)
            sessions = sessions.Where(s => s.Id != exceptSessionId.Value).ToList();

        var now = DateTime.UtcNow;
        foreach (var session in sessions)
        {
            session.Status = SessionStatus.Revoked;
            session.RevokedAt = now;
            session.RevokedReason = "All sessions revoked";
        }

        await _context.SaveChangesAsync();

        return ApiResponse<int>.Ok(sessions.Count, $"{sessions.Count} sessions revoked");
    }

    public async Task<ApiResponse<int>> RevokeSessionsByDeviceAsync(Guid tenantId, Guid userId, string deviceId)
    {
        var sessions = await _context.UserSessions
            .Where(s => s.TenantId == tenantId &&
                       s.UserId == userId &&
                       s.DeviceId == deviceId &&
                       s.Status == SessionStatus.Active)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var session in sessions)
        {
            session.Status = SessionStatus.Revoked;
            session.RevokedAt = now;
            session.RevokedReason = "Device sessions revoked";
        }

        await _context.SaveChangesAsync();

        return ApiResponse<int>.Ok(sessions.Count, $"{sessions.Count} sessions revoked");
    }

    #endregion

    #region Login Attempts

    public async Task<ApiResponse<PaginatedResult<LoginAttemptDto>>> GetLoginAttemptsAsync(
        Guid tenantId, Guid? userId, LoginAttemptQueryParams query)
    {
        var queryable = _context.LoginAttempts
            .Where(a => a.TenantId == tenantId);

        if (userId.HasValue)
            queryable = queryable.Where(a => a.UserId == userId.Value);

        if (query.Success.HasValue)
            queryable = queryable.Where(a => a.Success == query.Success.Value);

        if (query.FromDate.HasValue)
            queryable = queryable.Where(a => a.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            queryable = queryable.Where(a => a.CreatedAt <= query.ToDate.Value);

        var totalCount = await queryable.CountAsync();

        var items = await queryable
            .OrderByDescending(a => a.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new LoginAttemptDto(
                a.Id,
                a.UserId,
                a.UserName,
                a.Success,
                a.FailureReason,
                a.IpAddress,
                a.Location,
                a.CreatedAt
            ))
            .ToListAsync();

        var result = new PaginatedResult<LoginAttemptDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };

        return ApiResponse<PaginatedResult<LoginAttemptDto>>.Ok(result);
    }

    public async Task<ApiResponse<LoginStatsDto>> GetLoginStatsAsync(Guid tenantId, Guid userId, int days = 30)
    {
        var since = DateTime.UtcNow.AddDays(-days);

        var attempts = await _context.LoginAttempts
            .Where(a => a.TenantId == tenantId && a.UserId == userId && a.CreatedAt >= since)
            .ToListAsync();

        var recentFailures = attempts
            .Where(a => !a.Success)
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new LoginAttemptDto(
                a.Id,
                a.UserId,
                a.UserName,
                a.Success,
                a.FailureReason,
                a.IpAddress,
                a.Location,
                a.CreatedAt
            ))
            .ToList();

        var stats = new LoginStatsDto(
            TotalAttempts: attempts.Count,
            SuccessfulAttempts: attempts.Count(a => a.Success),
            FailedAttempts: attempts.Count(a => !a.Success),
            SuccessRate: attempts.Count > 0 ? (double)attempts.Count(a => a.Success) / attempts.Count * 100 : 0,
            RecentFailures: recentFailures
        );

        return ApiResponse<LoginStatsDto>.Ok(stats);
    }

    public async Task<ApiResponse<bool>> RecordLoginAttemptAsync(Guid tenantId, LoginAttemptDto attempt)
    {
        var loginAttempt = new LoginAttempt
        {
            TenantId = tenantId,
            UserId = attempt.UserId,
            UserName = attempt.UserName,
            Success = attempt.Success,
            FailureReason = attempt.FailureReason,
            IpAddress = attempt.IpAddress,
            Location = attempt.Location
        };

        _context.LoginAttempts.Add(loginAttempt);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true);
    }

    #endregion

    #region Trusted Devices

    public async Task<ApiResponse<List<TrustedDeviceDto>>> GetTrustedDevicesAsync(Guid tenantId, Guid userId)
    {
        var devices = await _context.TrustedDevices
            .Where(d => d.TenantId == tenantId && d.UserId == userId && d.IsActive)
            .OrderByDescending(d => d.LastUsedAt ?? d.CreatedAt)
            .Select(d => new TrustedDeviceDto(
                d.Id,
                d.UserId,
                d.DeviceId,
                d.DeviceName,
                d.DeviceType,
                d.LastUsedAt,
                d.LastIpAddress,
                d.IsActive,
                d.CreatedAt
            ))
            .ToListAsync();

        return ApiResponse<List<TrustedDeviceDto>>.Ok(devices);
    }

    public async Task<ApiResponse<TrustedDeviceDto>> RegisterTrustedDeviceAsync(
        Guid tenantId, Guid userId, RegisterDeviceRequest request)
    {
        var existing = await _context.TrustedDevices
            .FirstOrDefaultAsync(d => d.TenantId == tenantId &&
                                     d.UserId == userId &&
                                     d.DeviceId == request.DeviceId);

        if (existing != null)
        {
            existing.DeviceName = request.DeviceName;
            existing.DeviceType = request.DeviceType;
            existing.IsActive = true;
            existing.LastUsedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new TrustedDevice
            {
                TenantId = tenantId,
                UserId = userId,
                DeviceId = request.DeviceId,
                DeviceName = request.DeviceName,
                DeviceType = request.DeviceType,
                IsActive = true
            };
            _context.TrustedDevices.Add(existing);
        }

        await _context.SaveChangesAsync();

        return ApiResponse<TrustedDeviceDto>.Ok(new TrustedDeviceDto(
            existing.Id,
            existing.UserId,
            existing.DeviceId,
            existing.DeviceName,
            existing.DeviceType,
            existing.LastUsedAt,
            existing.LastIpAddress,
            existing.IsActive,
            existing.CreatedAt
        ));
    }

    public async Task<ApiResponse<bool>> RemoveTrustedDeviceAsync(Guid tenantId, Guid deviceId)
    {
        var device = await _context.TrustedDevices
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == deviceId);

        if (device == null)
            return ApiResponse<bool>.Fail("Device not found");

        device.IsActive = false;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Device removed from trusted list");
    }

    public async Task<ApiResponse<bool>> IsTrustedDeviceAsync(Guid tenantId, Guid userId, string deviceId)
    {
        var isTrusted = await _context.TrustedDevices
            .AnyAsync(d => d.TenantId == tenantId &&
                          d.UserId == userId &&
                          d.DeviceId == deviceId &&
                          d.IsActive);

        return ApiResponse<bool>.Ok(isTrusted);
    }

    #endregion

    #region Cleanup

    public async Task<ApiResponse<int>> CleanupExpiredSessionsAsync(Guid tenantId)
    {
        var now = DateTime.UtcNow;

        var expiredSessions = await _context.UserSessions
            .Where(s => s.TenantId == tenantId &&
                       s.Status == SessionStatus.Active &&
                       s.ExpiresAt <= now)
            .ToListAsync();

        foreach (var session in expiredSessions)
        {
            session.Status = SessionStatus.Expired;
        }

        await _context.SaveChangesAsync();

        return ApiResponse<int>.Ok(expiredSessions.Count, $"{expiredSessions.Count} expired sessions cleaned up");
    }

    public async Task<ApiResponse<int>> PurgeOldLoginAttemptsAsync(Guid tenantId, int daysOld)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysOld);

        var oldAttempts = await _context.LoginAttempts
            .Where(a => a.TenantId == tenantId && a.CreatedAt <= cutoff)
            .ToListAsync();

        _context.LoginAttempts.RemoveRange(oldAttempts);
        await _context.SaveChangesAsync();

        return ApiResponse<int>.Ok(oldAttempts.Count, $"{oldAttempts.Count} old login attempts purged");
    }

    #endregion

    #region Private Helpers

    private static UserSessionDto MapToDto(UserSession s, bool isCurrent) => new(
        s.Id,
        s.UserId,
        s.DeviceId,
        s.DeviceName,
        s.DeviceType,
        s.Browser,
        s.OperatingSystem,
        s.IpAddress,
        s.Location,
        s.Status,
        s.LastActivityAt,
        s.ExpiresAt,
        s.IsTrustedDevice,
        isCurrent,
        s.CreatedAt
    );

    private static string GenerateToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    #endregion
}
