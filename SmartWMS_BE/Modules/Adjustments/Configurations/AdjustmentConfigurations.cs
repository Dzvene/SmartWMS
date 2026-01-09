using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Adjustments.Models;

namespace SmartWMS.API.Modules.Adjustments.Configurations;

public class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ToTable("StockAdjustments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AdjustmentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.ReasonNotes)
            .HasMaxLength(500);

        builder.Property(a => a.SourceDocumentType)
            .HasMaxLength(50);

        builder.Property(a => a.SourceDocumentNumber)
            .HasMaxLength(50);

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.Property(a => a.TotalQuantityChange)
            .HasPrecision(18, 4);

        builder.Property(a => a.TotalValueChange)
            .HasPrecision(18, 4);

        // Indexes
        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => new { a.TenantId, a.AdjustmentNumber }).IsUnique();
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.AdjustmentType);
        builder.HasIndex(a => a.WarehouseId);
        builder.HasIndex(a => a.CreatedAt);

        // Relationships
        builder.HasOne(a => a.Warehouse)
            .WithMany()
            .HasForeignKey(a => a.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.ReasonCode)
            .WithMany()
            .HasForeignKey(a => a.ReasonCodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(a => a.Lines)
            .WithOne(l => l.Adjustment)
            .HasForeignKey(l => l.AdjustmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StockAdjustmentLineConfiguration : IEntityTypeConfiguration<StockAdjustmentLine>
{
    public void Configure(EntityTypeBuilder<StockAdjustmentLine> builder)
    {
        builder.ToTable("StockAdjustmentLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.BatchNumber)
            .HasMaxLength(100);

        builder.Property(l => l.SerialNumber)
            .HasMaxLength(100);

        builder.Property(l => l.QuantityBefore)
            .HasPrecision(18, 4);

        builder.Property(l => l.QuantityAdjustment)
            .HasPrecision(18, 4);

        builder.Property(l => l.UnitCost)
            .HasPrecision(18, 4);

        builder.Property(l => l.ReasonNotes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(l => l.TenantId);
        builder.HasIndex(l => l.AdjustmentId);
        builder.HasIndex(l => l.ProductId);
        builder.HasIndex(l => l.LocationId);

        // Relationships
        builder.HasOne(l => l.Product)
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Location)
            .WithMany()
            .HasForeignKey(l => l.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.ReasonCode)
            .WithMany()
            .HasForeignKey(l => l.ReasonCodeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
