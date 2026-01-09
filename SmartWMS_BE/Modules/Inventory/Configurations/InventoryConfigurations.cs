using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Inventory.Models;

namespace SmartWMS.API.Modules.Inventory.Configurations;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.Path)
            .HasMaxLength(500);

        // Self-referencing relationship
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.ChildCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(c => new { c.TenantId, c.Code }).IsUnique();
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Barcode)
            .HasMaxLength(100);

        builder.Property(p => p.AlternativeBarcodes)
            .HasMaxLength(1000); // JSON array

        builder.Property(p => p.UnitOfMeasure)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.GrossWeightKg)
            .HasPrecision(18, 4);

        builder.Property(p => p.NetWeightKg)
            .HasPrecision(18, 4);

        builder.Property(p => p.MinStockLevel)
            .HasPrecision(18, 4);

        builder.Property(p => p.MaxStockLevel)
            .HasPrecision(18, 4);

        builder.Property(p => p.ReorderPoint)
            .HasPrecision(18, 4);

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.DefaultSupplier)
            .WithMany()
            .HasForeignKey(p => p.DefaultSupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(p => new { p.TenantId, p.Sku }).IsUnique();
        builder.HasIndex(p => p.Barcode);
        builder.HasIndex(p => p.Name);
    }
}

public class StockLevelConfiguration : IEntityTypeConfiguration<StockLevel>
{
    public void Configure(EntityTypeBuilder<StockLevel> builder)
    {
        builder.ToTable("StockLevels");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.QuantityOnHand)
            .HasPrecision(18, 4);

        builder.Property(s => s.QuantityReserved)
            .HasPrecision(18, 4);

        builder.Property(s => s.BatchNumber)
            .HasMaxLength(100);

        builder.Property(s => s.SerialNumber)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(s => s.Product)
            .WithMany(p => p.StockLevels)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Location)
            .WithMany(l => l.StockLevels)
            .HasForeignKey(s => s.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes - composite for efficient lookups
        builder.HasIndex(s => new { s.TenantId, s.ProductId, s.LocationId, s.BatchNumber }).IsUnique();
        builder.HasIndex(s => new { s.TenantId, s.LocationId });
        builder.HasIndex(s => s.Sku);
    }
}

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.MovementNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.MovementType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(m => m.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Quantity)
            .HasPrecision(18, 4);

        builder.Property(m => m.BatchNumber)
            .HasMaxLength(100);

        builder.Property(m => m.SerialNumber)
            .HasMaxLength(100);

        builder.Property(m => m.ReferenceType)
            .HasMaxLength(50);

        builder.Property(m => m.ReferenceNumber)
            .HasMaxLength(50);

        builder.Property(m => m.ReasonCode)
            .HasMaxLength(50);

        builder.Property(m => m.Notes)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(m => m.Product)
            .WithMany()
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.FromLocation)
            .WithMany()
            .HasForeignKey(m => m.FromLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.ToLocation)
            .WithMany()
            .HasForeignKey(m => m.ToLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(m => new { m.TenantId, m.MovementNumber }).IsUnique();
        builder.HasIndex(m => m.MovementDate);
        builder.HasIndex(m => new { m.ReferenceType, m.ReferenceId });
    }
}
