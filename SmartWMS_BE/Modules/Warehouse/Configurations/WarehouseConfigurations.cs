using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Warehouse.Models;

namespace SmartWMS.API.Modules.Warehouse.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Models.Warehouse>
{
    public void Configure(EntityTypeBuilder<Models.Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .HasMaxLength(500);

        builder.Property(w => w.AddressLine1)
            .HasMaxLength(200);

        builder.Property(w => w.AddressLine2)
            .HasMaxLength(200);

        builder.Property(w => w.City)
            .HasMaxLength(100);

        builder.Property(w => w.Region)
            .HasMaxLength(100);

        builder.Property(w => w.PostalCode)
            .HasMaxLength(20);

        builder.Property(w => w.CountryCode)
            .HasMaxLength(3);

        builder.Property(w => w.Timezone)
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(w => new { w.TenantId, w.Code }).IsUnique();
    }
}

public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.ToTable("Zones");

        builder.HasKey(z => z.Id);

        builder.Property(z => z.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(z => z.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(z => z.Description)
            .HasMaxLength(500);

        builder.Property(z => z.ZoneType)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(z => z.Warehouse)
            .WithMany(w => w.Zones)
            .HasForeignKey(z => z.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(z => new { z.TenantId, z.WarehouseId, z.Code }).IsUnique();
    }
}

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.Name)
            .HasMaxLength(200);

        builder.Property(l => l.Aisle)
            .HasMaxLength(20);

        builder.Property(l => l.Rack)
            .HasMaxLength(20);

        builder.Property(l => l.Level)
            .HasMaxLength(20);

        builder.Property(l => l.Position)
            .HasMaxLength(20);

        builder.Property(l => l.LocationType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.MaxWeight)
            .HasPrecision(18, 4);

        builder.Property(l => l.MaxVolume)
            .HasPrecision(18, 4);

        // Relationships
        builder.HasOne(l => l.Warehouse)
            .WithMany(w => w.Locations)
            .HasForeignKey(l => l.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Zone)
            .WithMany(z => z.Locations)
            .HasForeignKey(l => l.ZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(l => new { l.TenantId, l.WarehouseId, l.Code }).IsUnique();
        builder.HasIndex(l => l.LocationType);
        builder.HasIndex(l => l.IsPickLocation);
        builder.HasIndex(l => l.PickSequence);
    }
}
