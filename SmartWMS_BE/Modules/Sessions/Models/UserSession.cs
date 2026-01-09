using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Sessions.Models;

public class UserSession : TenantEntity
{
    public Guid UserId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }

    // Device/Client info
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public string? DeviceType { get; set; } // Desktop, Mobile, Tablet, Scanner
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? AppVersion { get; set; }

    // Location
    public string? IpAddress { get; set; }
    public string? Location { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }

    // Session status
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public DateTime? LastActivityAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }

    // Security
    public bool IsTrustedDevice { get; set; }
    public int FailedAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
}

public class LoginAttempt : TenantEntity
{
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }

    public bool Success { get; set; }
    public string? FailureReason { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Location { get; set; }
}

public class TrustedDevice : TenantEntity
{
    public Guid UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string? DeviceType { get; set; }

    public DateTime? LastUsedAt { get; set; }
    public string? LastIpAddress { get; set; }
    public bool IsActive { get; set; } = true;
}

#region Enums

public enum SessionStatus
{
    Active,
    Expired,
    Revoked,
    Locked
}

#endregion
