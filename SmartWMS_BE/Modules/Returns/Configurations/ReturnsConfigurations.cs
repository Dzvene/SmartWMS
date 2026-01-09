using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Returns.Models;

namespace SmartWMS.API.Modules.Returns.Configurations;

public class ReturnOrderConfiguration : IEntityTypeConfiguration<ReturnOrder>
{
    public void Configure(EntityTypeBuilder<ReturnOrder> builder)
    {
        builder.ToTable("ReturnOrders");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReturnNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.ReasonDescription)
            .HasMaxLength(500);

        builder.Property(r => r.RmaNumber)
            .HasMaxLength(50);

        builder.Property(r => r.CarrierCode)
            .HasMaxLength(50);

        builder.Property(r => r.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(r => r.Notes)
            .HasMaxLength(1000);

        builder.Property(r => r.InternalNotes)
            .HasMaxLength(1000);

        builder.Property(r => r.TotalQuantityExpected)
            .HasPrecision(18, 4);

        builder.Property(r => r.TotalQuantityReceived)
            .HasPrecision(18, 4);

        // Indexes
        builder.HasIndex(r => r.TenantId);
        builder.HasIndex(r => r.ReturnNumber);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.RmaNumber);
        builder.HasIndex(r => new { r.TenantId, r.Status });
        builder.HasIndex(r => new { r.TenantId, r.CustomerId });

        // Relationships
        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.OriginalSalesOrder)
            .WithMany()
            .HasForeignKey(r => r.OriginalSalesOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.ReceivingLocation)
            .WithMany()
            .HasForeignKey(r => r.ReceivingLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Lines collection
        builder.HasMany(r => r.Lines)
            .WithOne(l => l.ReturnOrder)
            .HasForeignKey(l => l.ReturnOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReturnOrderLineConfiguration : IEntityTypeConfiguration<ReturnOrderLine>
{
    public void Configure(EntityTypeBuilder<ReturnOrderLine> builder)
    {
        builder.ToTable("ReturnOrderLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.BatchNumber)
            .HasMaxLength(100);

        builder.Property(l => l.SerialNumber)
            .HasMaxLength(100);

        builder.Property(l => l.ConditionNotes)
            .HasMaxLength(500);

        builder.Property(l => l.ReasonDescription)
            .HasMaxLength(500);

        builder.Property(l => l.Notes)
            .HasMaxLength(500);

        builder.Property(l => l.QuantityExpected)
            .HasPrecision(18, 4);

        builder.Property(l => l.QuantityReceived)
            .HasPrecision(18, 4);

        builder.Property(l => l.QuantityAccepted)
            .HasPrecision(18, 4);

        builder.Property(l => l.QuantityRejected)
            .HasPrecision(18, 4);

        // Indexes
        builder.HasIndex(l => l.TenantId);
        builder.HasIndex(l => l.ReturnOrderId);
        builder.HasIndex(l => l.ProductId);

        // Relationships
        builder.HasOne(l => l.Product)
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.DispositionLocation)
            .WithMany()
            .HasForeignKey(l => l.DispositionLocationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
