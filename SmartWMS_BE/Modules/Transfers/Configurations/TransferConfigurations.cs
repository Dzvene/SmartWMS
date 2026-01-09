using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Transfers.Models;

namespace SmartWMS.API.Modules.Transfers.Configurations;

public class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable("StockTransfers");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TransferNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.ReasonNotes)
            .HasMaxLength(500);

        builder.Property(t => t.SourceDocumentType)
            .HasMaxLength(50);

        builder.Property(t => t.SourceDocumentNumber)
            .HasMaxLength(50);

        builder.Property(t => t.Notes)
            .HasMaxLength(1000);

        builder.Property(t => t.TotalQuantity)
            .HasPrecision(18, 4);

        // Indexes
        builder.HasIndex(t => t.TenantId);
        builder.HasIndex(t => new { t.TenantId, t.TransferNumber }).IsUnique();
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.TransferType);
        builder.HasIndex(t => t.FromWarehouseId);
        builder.HasIndex(t => t.ToWarehouseId);
        builder.HasIndex(t => t.ScheduledDate);
        builder.HasIndex(t => t.Priority);
        builder.HasIndex(t => t.AssignedToUserId);

        // Relationships
        builder.HasOne(t => t.FromWarehouse)
            .WithMany()
            .HasForeignKey(t => t.FromWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.FromZone)
            .WithMany()
            .HasForeignKey(t => t.FromZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.ToWarehouse)
            .WithMany()
            .HasForeignKey(t => t.ToWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ToZone)
            .WithMany()
            .HasForeignKey(t => t.ToZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.ReasonCode)
            .WithMany()
            .HasForeignKey(t => t.ReasonCodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Lines)
            .WithOne(l => l.Transfer)
            .HasForeignKey(l => l.TransferId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StockTransferLineConfiguration : IEntityTypeConfiguration<StockTransferLine>
{
    public void Configure(EntityTypeBuilder<StockTransferLine> builder)
    {
        builder.ToTable("StockTransferLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.BatchNumber)
            .HasMaxLength(100);

        builder.Property(l => l.SerialNumber)
            .HasMaxLength(100);

        builder.Property(l => l.QuantityRequested)
            .HasPrecision(18, 4);

        builder.Property(l => l.QuantityPicked)
            .HasPrecision(18, 4);

        builder.Property(l => l.QuantityReceived)
            .HasPrecision(18, 4);

        builder.Property(l => l.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(l => l.TenantId);
        builder.HasIndex(l => l.TransferId);
        builder.HasIndex(l => l.ProductId);
        builder.HasIndex(l => l.FromLocationId);
        builder.HasIndex(l => l.ToLocationId);
        builder.HasIndex(l => l.Status);

        // Relationships
        builder.HasOne(l => l.Product)
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.FromLocation)
            .WithMany()
            .HasForeignKey(l => l.FromLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.ToLocation)
            .WithMany()
            .HasForeignKey(l => l.ToLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
