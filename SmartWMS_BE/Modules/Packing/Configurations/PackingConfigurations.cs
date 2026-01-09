using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Packing.Models;

namespace SmartWMS.API.Modules.Packing.Configurations;

public class PackingTaskConfiguration : IEntityTypeConfiguration<PackingTask>
{
    public void Configure(EntityTypeBuilder<PackingTask> builder)
    {
        builder.ToTable("PackingTasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TaskNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Notes)
            .HasMaxLength(1000);

        builder.Property(t => t.TotalWeightKg)
            .HasPrecision(18, 4);

        // Indexes
        builder.HasIndex(t => t.TenantId);
        builder.HasIndex(t => t.TaskNumber);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.AssignedToUserId);
        builder.HasIndex(t => new { t.TenantId, t.Status });
        builder.HasIndex(t => new { t.TenantId, t.SalesOrderId });

        // Relationships
        builder.HasOne(t => t.SalesOrder)
            .WithMany()
            .HasForeignKey(t => t.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.FulfillmentBatch)
            .WithMany()
            .HasForeignKey(t => t.FulfillmentBatchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.PackingStation)
            .WithMany()
            .HasForeignKey(t => t.PackingStationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Packages collection
        builder.HasMany(t => t.Packages)
            .WithOne(p => p.PackingTask)
            .HasForeignKey(p => p.PackingTaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.ToTable("Packages");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PackageNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.PackagingType)
            .HasMaxLength(50);

        builder.Property(p => p.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(p => p.LabelUrl)
            .HasMaxLength(500);

        builder.Property(p => p.WeightKg)
            .HasPrecision(18, 4);

        // Indexes
        builder.HasIndex(p => p.TenantId);
        builder.HasIndex(p => p.PackageNumber);
        builder.HasIndex(p => p.TrackingNumber);
        builder.HasIndex(p => p.PackingTaskId);

        // Items collection
        builder.HasMany(p => p.Items)
            .WithOne(i => i.Package)
            .HasForeignKey(i => i.PackageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PackageItemConfiguration : IEntityTypeConfiguration<PackageItem>
{
    public void Configure(EntityTypeBuilder<PackageItem> builder)
    {
        builder.ToTable("PackageItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.BatchNumber)
            .HasMaxLength(100);

        builder.Property(i => i.SerialNumber)
            .HasMaxLength(100);

        builder.Property(i => i.Quantity)
            .HasPrecision(18, 4);

        // Indexes
        builder.HasIndex(i => i.TenantId);
        builder.HasIndex(i => i.PackageId);
        builder.HasIndex(i => i.ProductId);

        // Relationships
        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PackingStationConfiguration : IEntityTypeConfiguration<PackingStation>
{
    public void Configure(EntityTypeBuilder<PackingStation> builder)
    {
        builder.ToTable("PackingStations");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => new { s.TenantId, s.Code }).IsUnique();
        builder.HasIndex(s => s.WarehouseId);

        // Relationships
        builder.HasOne(s => s.Warehouse)
            .WithMany()
            .HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
