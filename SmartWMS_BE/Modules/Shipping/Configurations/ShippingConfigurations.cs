using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Shipping.Models;

namespace SmartWMS.API.Modules.Shipping.Configurations;

public class DeliveryRouteConfiguration : IEntityTypeConfiguration<DeliveryRoute>
{
    public void Configure(EntityTypeBuilder<DeliveryRoute> builder)
    {
        builder.ToTable("DeliveryRoutes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RouteNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.RouteName)
            .HasMaxLength(200);

        builder.Property(r => r.DriverName)
            .HasMaxLength(100);

        builder.Property(r => r.VehicleNumber)
            .HasMaxLength(50);

        builder.Property(r => r.VehicleType)
            .HasMaxLength(50);

        builder.Property(r => r.EstimatedDistance)
            .HasPrecision(18, 2);

        builder.Property(r => r.MaxWeight)
            .HasPrecision(18, 4);

        builder.Property(r => r.MaxVolume)
            .HasPrecision(18, 4);

        builder.Property(r => r.CurrentWeight)
            .HasPrecision(18, 4);

        builder.Property(r => r.CurrentVolume)
            .HasPrecision(18, 4);

        builder.Property(r => r.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(r => r.TenantId);
        builder.HasIndex(r => new { r.TenantId, r.RouteNumber }).IsUnique();
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.PlannedDate);
        builder.HasIndex(r => r.WarehouseId);
        builder.HasIndex(r => r.CarrierId);

        // Relationships
        builder.HasOne(r => r.Warehouse)
            .WithMany()
            .HasForeignKey(r => r.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Carrier)
            .WithMany()
            .HasForeignKey(r => r.CarrierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.CarrierService)
            .WithMany()
            .HasForeignKey(r => r.CarrierServiceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Stops)
            .WithOne(s => s.Route)
            .HasForeignKey(s => s.RouteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DeliveryStopConfiguration : IEntityTypeConfiguration<DeliveryStop>
{
    public void Configure(EntityTypeBuilder<DeliveryStop> builder)
    {
        builder.ToTable("DeliveryStops");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CustomerName)
            .HasMaxLength(200);

        builder.Property(s => s.AddressLine1)
            .HasMaxLength(500);

        builder.Property(s => s.AddressLine2)
            .HasMaxLength(500);

        builder.Property(s => s.City)
            .HasMaxLength(100);

        builder.Property(s => s.State)
            .HasMaxLength(100);

        builder.Property(s => s.PostalCode)
            .HasMaxLength(20);

        builder.Property(s => s.Country)
            .HasMaxLength(100);

        builder.Property(s => s.ContactName)
            .HasMaxLength(100);

        builder.Property(s => s.ContactPhone)
            .HasMaxLength(50);

        builder.Property(s => s.DeliveryInstructions)
            .HasMaxLength(1000);

        builder.Property(s => s.SignedBy)
            .HasMaxLength(100);

        builder.Property(s => s.ProofOfDeliveryUrl)
            .HasMaxLength(500);

        builder.Property(s => s.IssueNotes)
            .HasMaxLength(500);

        builder.Property(s => s.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.RouteId);
        builder.HasIndex(s => s.ShipmentId);
        builder.HasIndex(s => s.CustomerId);
        builder.HasIndex(s => s.Status);

        // Relationships
        builder.HasOne(s => s.Shipment)
            .WithMany()
            .HasForeignKey(s => s.ShipmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.Customer)
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class DeliveryTrackingEventConfiguration : IEntityTypeConfiguration<DeliveryTrackingEvent>
{
    public void Configure(EntityTypeBuilder<DeliveryTrackingEvent> builder)
    {
        builder.ToTable("DeliveryTrackingEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventDescription)
            .HasMaxLength(500);

        builder.Property(e => e.Latitude)
            .HasPrecision(10, 7);

        builder.Property(e => e.Longitude)
            .HasPrecision(10, 7);

        builder.Property(e => e.LocationDescription)
            .HasMaxLength(200);

        builder.Property(e => e.PerformedBy)
            .HasMaxLength(100);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.RouteId);
        builder.HasIndex(e => e.StopId);
        builder.HasIndex(e => e.ShipmentId);
        builder.HasIndex(e => e.EventTime);
        builder.HasIndex(e => e.EventType);

        // Relationships
        builder.HasOne(e => e.Route)
            .WithMany()
            .HasForeignKey(e => e.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Stop)
            .WithMany()
            .HasForeignKey(e => e.StopId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
