using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.CycleCount.Models;

namespace SmartWMS.API.Modules.CycleCount.Configurations;

public class CycleCountSessionConfiguration : IEntityTypeConfiguration<CycleCountSession>
{
    public void Configure(EntityTypeBuilder<CycleCountSession> builder)
    {
        builder.ToTable("CycleCountSessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CountNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.CountNumber);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => new { s.TenantId, s.Status });
        builder.HasIndex(s => new { s.TenantId, s.WarehouseId });

        // Relationships
        builder.HasOne(s => s.Warehouse)
            .WithMany()
            .HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Zone)
            .WithMany()
            .HasForeignKey(s => s.ZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        // Items collection
        builder.HasMany(s => s.Items)
            .WithOne(i => i.CycleCountSession)
            .HasForeignKey(i => i.CycleCountSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CycleCountItemConfiguration : IEntityTypeConfiguration<CycleCountItem>
{
    public void Configure(EntityTypeBuilder<CycleCountItem> builder)
    {
        builder.ToTable("CycleCountItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.ExpectedBatchNumber)
            .HasMaxLength(100);

        builder.Property(i => i.CountedBatchNumber)
            .HasMaxLength(100);

        builder.Property(i => i.Notes)
            .HasMaxLength(500);

        builder.Property(i => i.ExpectedQuantity)
            .HasPrecision(18, 4);

        builder.Property(i => i.CountedQuantity)
            .HasPrecision(18, 4);

        // Ignore computed properties
        builder.Ignore(i => i.Variance);
        builder.Ignore(i => i.VariancePercent);

        // Indexes
        builder.HasIndex(i => i.TenantId);
        builder.HasIndex(i => i.CycleCountSessionId);
        builder.HasIndex(i => i.LocationId);
        builder.HasIndex(i => i.ProductId);
        builder.HasIndex(i => i.Status);

        // Relationships
        builder.HasOne(i => i.Location)
            .WithMany()
            .HasForeignKey(i => i.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
