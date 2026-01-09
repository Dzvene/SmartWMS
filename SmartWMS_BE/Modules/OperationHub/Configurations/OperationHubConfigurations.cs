using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.OperationHub.Models;

namespace SmartWMS.API.Modules.OperationHub.Configurations;

public class OperatorSessionConfiguration : IEntityTypeConfiguration<OperatorSession>
{
    public void Configure(EntityTypeBuilder<OperatorSession> builder)
    {
        builder.ToTable("OperatorSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DeviceId)
            .HasMaxLength(100);

        builder.Property(x => x.DeviceType)
            .HasMaxLength(50);

        builder.Property(x => x.DeviceName)
            .HasMaxLength(200);

        builder.Property(x => x.CurrentTaskType)
            .HasMaxLength(50);

        builder.Property(x => x.CurrentZone)
            .HasMaxLength(50);

        builder.Property(x => x.CurrentLocation)
            .HasMaxLength(50);

        builder.Property(x => x.ShiftCode)
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(x => new { x.TenantId, x.UserId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.WarehouseId, x.Status });
    }
}

public class OperatorProductivityConfiguration : IEntityTypeConfiguration<OperatorProductivity>
{
    public void Configure(EntityTypeBuilder<OperatorProductivity> builder)
    {
        builder.ToTable("OperatorProductivity");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TotalUnitsPicked)
            .HasPrecision(18, 4);

        builder.Property(x => x.TotalUnitsPacked)
            .HasPrecision(18, 4);

        builder.Property(x => x.TotalUnitsPutaway)
            .HasPrecision(18, 4);

        builder.Property(x => x.AccuracyRate)
            .HasPrecision(5, 2);

        builder.Property(x => x.PicksPerHour)
            .HasPrecision(10, 2);

        builder.Property(x => x.UnitsPerHour)
            .HasPrecision(10, 2);

        builder.Property(x => x.TasksPerHour)
            .HasPrecision(10, 2);

        builder.HasIndex(x => new { x.TenantId, x.UserId, x.Date }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.WarehouseId, x.Date });
    }
}

public class ScanLogConfiguration : IEntityTypeConfiguration<ScanLog>
{
    public void Configure(EntityTypeBuilder<ScanLog> builder)
    {
        builder.ToTable("ScanLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Barcode)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ScanType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Context)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.EntityType)
            .HasMaxLength(50);

        builder.Property(x => x.ResolvedSku)
            .HasMaxLength(100);

        builder.Property(x => x.ResolvedLocation)
            .HasMaxLength(50);

        builder.Property(x => x.TaskType)
            .HasMaxLength(50);

        builder.Property(x => x.ErrorCode)
            .HasMaxLength(50);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(500);

        builder.Property(x => x.DeviceId)
            .HasMaxLength(100);

        builder.HasIndex(x => new { x.TenantId, x.UserId, x.ScannedAt });
        builder.HasIndex(x => new { x.TenantId, x.WarehouseId, x.ScannedAt });
        builder.HasIndex(x => new { x.TenantId, x.SessionId });
        builder.HasIndex(x => new { x.TenantId, x.Barcode });
    }
}

public class TaskActionLogConfiguration : IEntityTypeConfiguration<TaskActionLog>
{
    public void Configure(EntityTypeBuilder<TaskActionLog> builder)
    {
        builder.ToTable("TaskActionLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TaskType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.TaskNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Action)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.FromStatus)
            .HasMaxLength(30);

        builder.Property(x => x.ToStatus)
            .HasMaxLength(30);

        builder.Property(x => x.LocationCode)
            .HasMaxLength(50);

        builder.Property(x => x.ProductSku)
            .HasMaxLength(100);

        builder.Property(x => x.Quantity)
            .HasPrecision(18, 4);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.ReasonCode)
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.TenantId, x.TaskType, x.TaskId });
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.ActionAt });
        builder.HasIndex(x => new { x.TenantId, x.WarehouseId, x.ActionAt });
    }
}
