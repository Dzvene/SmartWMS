using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Audit.Models;

namespace SmartWMS.API.Modules.Audit.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EventType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityNumber)
            .HasMaxLength(100);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.UserName)
            .HasMaxLength(100);

        builder.Property(a => a.UserEmail)
            .HasMaxLength(256);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50);

        builder.Property(a => a.UserAgent)
            .HasMaxLength(500);

        builder.Property(a => a.ChangedFields)
            .HasMaxLength(1000);

        builder.Property(a => a.Module)
            .HasMaxLength(50);

        builder.Property(a => a.SubModule)
            .HasMaxLength(50);

        builder.Property(a => a.CorrelationId)
            .HasMaxLength(100);

        builder.Property(a => a.SessionId)
            .HasMaxLength(100);

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.Property(a => a.ErrorMessage)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => a.EventTime);
        builder.HasIndex(a => a.EventType);
        builder.HasIndex(a => a.EntityType);
        builder.HasIndex(a => a.EntityId);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Severity);
        builder.HasIndex(a => a.Module);
        builder.HasIndex(a => a.IsSuccess);
        builder.HasIndex(a => new { a.TenantId, a.EntityType, a.EntityId });
        builder.HasIndex(a => new { a.TenantId, a.EventTime });
    }
}

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.ActivityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.UserName)
            .HasMaxLength(100);

        builder.Property(a => a.Module)
            .HasMaxLength(50);

        builder.Property(a => a.RelatedEntityType)
            .HasMaxLength(100);

        builder.Property(a => a.RelatedEntityNumber)
            .HasMaxLength(100);

        builder.Property(a => a.DeviceType)
            .HasMaxLength(20);

        builder.Property(a => a.DeviceId)
            .HasMaxLength(100);

        builder.Property(a => a.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => a.ActivityTime);
        builder.HasIndex(a => a.ActivityType);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Module);
        builder.HasIndex(a => a.RelatedEntityId);
        builder.HasIndex(a => new { a.TenantId, a.UserId, a.ActivityTime });
    }
}

public class SystemEventLogConfiguration : IEntityTypeConfiguration<SystemEventLog>
{
    public void Configure(EntityTypeBuilder<SystemEventLog> builder)
    {
        builder.ToTable("SystemEventLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.Source)
            .HasMaxLength(100);

        builder.Property(e => e.SourceVersion)
            .HasMaxLength(50);

        builder.Property(e => e.MachineName)
            .HasMaxLength(100);

        builder.Property(e => e.ExceptionType)
            .HasMaxLength(500);

        builder.Property(e => e.ExceptionMessage)
            .HasMaxLength(2000);

        builder.Property(e => e.CorrelationId)
            .HasMaxLength(100);

        builder.Property(e => e.RequestId)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.EventTime);
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.Severity);
        builder.HasIndex(e => e.Source);
        builder.HasIndex(e => new { e.TenantId, e.EventTime });
    }
}
