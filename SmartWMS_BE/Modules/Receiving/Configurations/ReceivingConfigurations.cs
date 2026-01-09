using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Receiving.Models;

namespace SmartWMS.API.Modules.Receiving.Configurations;

public class GoodsReceiptConfiguration : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("GoodsReceipts");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReceiptNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.CarrierName)
            .HasMaxLength(100);

        builder.Property(r => r.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(r => r.DeliveryNote)
            .HasMaxLength(100);

        builder.Property(r => r.TotalQuantityExpected)
            .HasPrecision(18, 4);

        builder.Property(r => r.TotalQuantityReceived)
            .HasPrecision(18, 4);

        builder.Property(r => r.Notes)
            .HasMaxLength(1000);

        builder.Property(r => r.InternalNotes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(r => r.PurchaseOrder)
            .WithMany()
            .HasForeignKey(r => r.PurchaseOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Supplier)
            .WithMany()
            .HasForeignKey(r => r.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Warehouse)
            .WithMany()
            .HasForeignKey(r => r.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReceivingLocation)
            .WithMany()
            .HasForeignKey(r => r.ReceivingLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(r => new { r.TenantId, r.ReceiptNumber }).IsUnique();
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.ReceiptDate);
        builder.HasIndex(r => r.PurchaseOrderId);
    }
}

public class GoodsReceiptLineConfiguration : IEntityTypeConfiguration<GoodsReceiptLine>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptLine> builder)
    {
        builder.ToTable("GoodsReceiptLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.QuantityExpected)
            .HasPrecision(18, 4);

        builder.Property(l => l.QuantityReceived)
            .HasPrecision(18, 4);

        builder.Property(l => l.QuantityRejected)
            .HasPrecision(18, 4);

        builder.Property(l => l.BatchNumber)
            .HasMaxLength(100);

        builder.Property(l => l.LotNumber)
            .HasMaxLength(100);

        builder.Property(l => l.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.QualityStatus)
            .HasMaxLength(50);

        builder.Property(l => l.RejectionReason)
            .HasMaxLength(500);

        builder.Property(l => l.Notes)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(l => l.Receipt)
            .WithMany(r => r.Lines)
            .HasForeignKey(l => l.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Product)
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.PutawayLocation)
            .WithMany()
            .HasForeignKey(l => l.PutawayLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.PurchaseOrderLine)
            .WithMany()
            .HasForeignKey(l => l.PurchaseOrderLineId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(l => new { l.ReceiptId, l.LineNumber }).IsUnique();
        builder.HasIndex(l => l.ProductId);
    }
}
