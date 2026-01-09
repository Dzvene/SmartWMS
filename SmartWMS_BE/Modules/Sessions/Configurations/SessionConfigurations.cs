using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Sessions.Models;

namespace SmartWMS.API.Modules.Sessions.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SessionToken)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.RefreshToken)
            .HasMaxLength(500);

        builder.Property(s => s.DeviceId)
            .HasMaxLength(200);

        builder.Property(s => s.DeviceName)
            .HasMaxLength(200);

        builder.Property(s => s.DeviceType)
            .HasMaxLength(50);

        builder.Property(s => s.Browser)
            .HasMaxLength(100);

        builder.Property(s => s.OperatingSystem)
            .HasMaxLength(100);

        builder.Property(s => s.AppVersion)
            .HasMaxLength(50);

        builder.Property(s => s.IpAddress)
            .HasMaxLength(50);

        builder.Property(s => s.Location)
            .HasMaxLength(200);

        builder.Property(s => s.Country)
            .HasMaxLength(100);

        builder.Property(s => s.City)
            .HasMaxLength(100);

        builder.Property(s => s.RevokedReason)
            .HasMaxLength(500);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(s => new { s.TenantId, s.UserId });
        builder.HasIndex(s => new { s.TenantId, s.UserId, s.Status });
        builder.HasIndex(s => s.SessionToken);
        builder.HasIndex(s => s.ExpiresAt);
    }
}

public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
{
    public void Configure(EntityTypeBuilder<LoginAttempt> builder)
    {
        builder.ToTable("LoginAttempts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserName)
            .HasMaxLength(200);

        builder.Property(a => a.Email)
            .HasMaxLength(200);

        builder.Property(a => a.FailureReason)
            .HasMaxLength(500);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50);

        builder.Property(a => a.UserAgent)
            .HasMaxLength(500);

        builder.Property(a => a.Location)
            .HasMaxLength(200);

        builder.HasIndex(a => new { a.TenantId, a.UserId, a.CreatedAt });
        builder.HasIndex(a => new { a.TenantId, a.IpAddress, a.CreatedAt });
        builder.HasIndex(a => a.CreatedAt);
    }
}

public class TrustedDeviceConfiguration : IEntityTypeConfiguration<TrustedDevice>
{
    public void Configure(EntityTypeBuilder<TrustedDevice> builder)
    {
        builder.ToTable("TrustedDevices");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DeviceId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.DeviceName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.DeviceType)
            .HasMaxLength(50);

        builder.Property(d => d.LastIpAddress)
            .HasMaxLength(50);

        builder.HasIndex(d => new { d.TenantId, d.UserId, d.DeviceId }).IsUnique();
        builder.HasIndex(d => new { d.TenantId, d.UserId, d.IsActive });
    }
}
