using SmartWMS.API.Modules.Sessions.Models;

namespace SmartWMS.API.Modules.Sessions.DTOs;

#region Session DTOs

public record UserSessionDto(
    Guid Id,
    Guid UserId,
    string? DeviceId,
    string? DeviceName,
    string? DeviceType,
    string? Browser,
    string? OperatingSystem,
    string? IpAddress,
    string? Location,
    SessionStatus Status,
    DateTime? LastActivityAt,
    DateTime ExpiresAt,
    bool IsTrustedDevice,
    bool IsCurrent,
    DateTime CreatedAt
);

public record SessionSummaryDto(
    Guid Id,
    string? DeviceName,
    string? DeviceType,
    string? IpAddress,
    string? Location,
    SessionStatus Status,
    DateTime? LastActivityAt,
    bool IsCurrent
);

public record ActiveSessionsDto(
    int TotalActive,
    int TotalDevices,
    SessionSummaryDto? CurrentSession,
    List<SessionSummaryDto> OtherSessions
);

#endregion

#region Login Attempt DTOs

public record LoginAttemptDto(
    Guid Id,
    Guid? UserId,
    string? UserName,
    bool Success,
    string? FailureReason,
    string? IpAddress,
    string? Location,
    DateTime CreatedAt
);

public record LoginStatsDto(
    int TotalAttempts,
    int SuccessfulAttempts,
    int FailedAttempts,
    double SuccessRate,
    List<LoginAttemptDto> RecentFailures
);

#endregion

#region Trusted Device DTOs

public record TrustedDeviceDto(
    Guid Id,
    Guid UserId,
    string DeviceId,
    string DeviceName,
    string? DeviceType,
    DateTime? LastUsedAt,
    string? LastIpAddress,
    bool IsActive,
    DateTime CreatedAt
);

public record RegisterDeviceRequest(
    string DeviceId,
    string DeviceName,
    string? DeviceType
);

#endregion

#region Request/Response DTOs

public record CreateSessionRequest(
    string? DeviceId,
    string? DeviceName,
    string? DeviceType,
    string? Browser,
    string? OperatingSystem,
    string? AppVersion,
    string? IpAddress
);

public record RefreshSessionRequest(
    string RefreshToken
);

public record RevokeSessionRequest(
    string? Reason
);

public record UpdateSessionActivityRequest(
    DateTime ActivityAt
);

#endregion

#region Query Parameters

public record SessionQueryParams(
    int Page = 1,
    int PageSize = 20,
    SessionStatus? Status = null,
    string? DeviceType = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
);

public record LoginAttemptQueryParams(
    int Page = 1,
    int PageSize = 50,
    bool? Success = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
);

#endregion
