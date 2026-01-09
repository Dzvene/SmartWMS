using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartWMS.API.Modules.Equipment.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Models.Equipment>
{
    public void Configure(EntityTypeBuilder<Models.Equipment> builder)
    {
        builder.ToTable("Equipment");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.AssignedToUserName)
            .HasMaxLength(200);

        builder.Property(e => e.MaintenanceNotes)
            .HasMaxLength(1000);

        // JSON column for type-specific specifications
        builder.Property(e => e.Specifications)
            .HasColumnType("jsonb");

        builder.Property(e => e.SerialNumber)
            .HasMaxLength(100);

        builder.Property(e => e.Manufacturer)
            .HasMaxLength(200);

        builder.Property(e => e.Model)
            .HasMaxLength(200);

        // Indexes
        builder.HasIndex(e => new { e.TenantId, e.Code })
            .IsUnique();

        builder.HasIndex(e => new { e.TenantId, e.Type });

        builder.HasIndex(e => new { e.TenantId, e.Status });

        builder.HasIndex(e => new { e.TenantId, e.WarehouseId });

        builder.HasIndex(e => new { e.TenantId, e.AssignedToUserId });

        builder.HasIndex(e => e.SerialNumber);

        // Relationships
        builder.HasOne(e => e.Warehouse)
            .WithMany()
            .HasForeignKey(e => e.WarehouseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Zone)
            .WithMany()
            .HasForeignKey(e => e.ZoneId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
