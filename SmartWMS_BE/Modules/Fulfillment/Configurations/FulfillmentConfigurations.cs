using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Fulfillment.Models;

namespace SmartWMS.API.Modules.Fulfillment.Configurations;

public class FulfillmentBatchConfiguration : IEntityTypeConfiguration<FulfillmentBatch>
{
    public void Configure(EntityTypeBuilder<FulfillmentBatch> builder)
    {
        builder.ToTable("FulfillmentBatches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BatchNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.Name)
            .HasMaxLength(200);

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(b => b.BatchType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(b => b.TotalQuantity)
            .HasPrecision(18, 4);

        builder.Property(b => b.PickedQuantity)
            .HasPrecision(18, 4);

        builder.Property(b => b.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(b => b.Warehouse)
            .WithMany()
            .HasForeignKey(b => b.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Zone)
            .WithMany()
            .HasForeignKey(b => b.ZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(b => new { b.TenantId, b.BatchNumber }).IsUnique();
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.CreatedAt);
    }
}

public class FulfillmentOrderConfiguration : IEntityTypeConfiguration<FulfillmentOrder>
{
    public void Configure(EntityTypeBuilder<FulfillmentOrder> builder)
    {
        builder.ToTable("FulfillmentOrders");

        builder.HasKey(fo => fo.Id);

        // Relationships
        builder.HasOne(fo => fo.Batch)
            .WithMany(b => b.FulfillmentOrders)
            .HasForeignKey(fo => fo.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fo => fo.Order)
            .WithMany()
            .HasForeignKey(fo => fo.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes - prevent duplicate order-batch combinations
        builder.HasIndex(fo => new { fo.BatchId, fo.OrderId }).IsUnique();
    }
}

public class PickTaskConfiguration : IEntityTypeConfiguration<PickTask>
{
    public void Configure(EntityTypeBuilder<PickTask> builder)
    {
        builder.ToTable("PickTasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TaskNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.QuantityRequired)
            .HasPrecision(18, 4);

        builder.Property(t => t.QuantityPicked)
            .HasPrecision(18, 4);

        builder.Property(t => t.QuantityShortPicked)
            .HasPrecision(18, 4);

        builder.Property(t => t.PickedBatchNumber)
            .HasMaxLength(100);

        builder.Property(t => t.PickedSerialNumber)
            .HasMaxLength(100);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.ShortPickReason)
            .HasMaxLength(200);

        // Relationships
        builder.HasOne(t => t.Batch)
            .WithMany(b => b.PickTasks)
            .HasForeignKey(t => t.BatchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Order)
            .WithMany()
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.OrderLine)
            .WithMany()
            .HasForeignKey(t => t.OrderLineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Product)
            .WithMany()
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.FromLocation)
            .WithMany()
            .HasForeignKey(t => t.FromLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ToLocation)
            .WithMany()
            .HasForeignKey(t => t.ToLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(t => new { t.TenantId, t.TaskNumber }).IsUnique();
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.AssignedToUserId);
        builder.HasIndex(t => new { t.BatchId, t.Sequence });
    }
}

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ShipmentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.CarrierCode)
            .HasMaxLength(50);

        builder.Property(s => s.CarrierName)
            .HasMaxLength(100);

        builder.Property(s => s.ServiceLevel)
            .HasMaxLength(50);

        builder.Property(s => s.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(s => s.TrackingUrl)
            .HasMaxLength(500);

        builder.Property(s => s.TotalWeightKg)
            .HasPrecision(18, 4);

        builder.Property(s => s.ShipToName)
            .HasMaxLength(200);

        builder.Property(s => s.ShipToAddressLine1)
            .HasMaxLength(200);

        builder.Property(s => s.ShipToAddressLine2)
            .HasMaxLength(200);

        builder.Property(s => s.ShipToCity)
            .HasMaxLength(100);

        builder.Property(s => s.ShipToRegion)
            .HasMaxLength(100);

        builder.Property(s => s.ShipToPostalCode)
            .HasMaxLength(20);

        builder.Property(s => s.ShipToCountryCode)
            .HasMaxLength(3);

        builder.Property(s => s.ShippingCost)
            .HasPrecision(18, 4);

        builder.Property(s => s.CurrencyCode)
            .HasMaxLength(3);

        builder.Property(s => s.LabelUrl)
            .HasMaxLength(500);

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(s => s.Order)
            .WithMany()
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Warehouse)
            .WithMany()
            .HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(s => new { s.TenantId, s.ShipmentNumber }).IsUnique();
        builder.HasIndex(s => s.TrackingNumber);
        builder.HasIndex(s => s.Status);
    }
}
