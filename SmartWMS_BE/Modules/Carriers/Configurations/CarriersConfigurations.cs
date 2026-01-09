using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Carriers.Models;

namespace SmartWMS.API.Modules.Carriers.Configurations;

public class CarrierConfiguration : IEntityTypeConfiguration<Carrier>
{
    public void Configure(EntityTypeBuilder<Carrier> builder)
    {
        builder.ToTable("Carriers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.ContactName)
            .HasMaxLength(100);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.Email)
            .HasMaxLength(100);

        builder.Property(c => c.Website)
            .HasMaxLength(200);

        builder.Property(c => c.AccountNumber)
            .HasMaxLength(100);

        builder.Property(c => c.ApiEndpoint)
            .HasMaxLength(500);

        builder.Property(c => c.ApiKey)
            .HasMaxLength(500);

        builder.Property(c => c.DefaultServiceCode)
            .HasMaxLength(50);

        builder.Property(c => c.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => new { c.TenantId, c.Code }).IsUnique();
        builder.HasIndex(c => c.IsActive);

        // Services collection
        builder.HasMany(c => c.Services)
            .WithOne(s => s.Carrier)
            .HasForeignKey(s => s.CarrierId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CarrierServiceConfiguration : IEntityTypeConfiguration<CarrierService>
{
    public void Configure(EntityTypeBuilder<CarrierService> builder)
    {
        builder.ToTable("CarrierServices");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.TrackingUrlTemplate)
            .HasMaxLength(500);

        builder.Property(s => s.MaxWeightKg)
            .HasPrecision(18, 4);

        builder.Property(s => s.MaxLengthCm)
            .HasPrecision(18, 2);

        builder.Property(s => s.MaxWidthCm)
            .HasPrecision(18, 2);

        builder.Property(s => s.MaxHeightCm)
            .HasPrecision(18, 2);

        builder.Property(s => s.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.CarrierId);
        builder.HasIndex(s => new { s.TenantId, s.CarrierId, s.Code }).IsUnique();
    }
}
