using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Orders.Models;

namespace SmartWMS.API.Modules.Orders.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ContactName)
            .HasMaxLength(100);

        builder.Property(c => c.Email)
            .HasMaxLength(100);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.AddressLine1)
            .HasMaxLength(200);

        builder.Property(c => c.AddressLine2)
            .HasMaxLength(200);

        builder.Property(c => c.City)
            .HasMaxLength(100);

        builder.Property(c => c.Region)
            .HasMaxLength(100);

        builder.Property(c => c.PostalCode)
            .HasMaxLength(20);

        builder.Property(c => c.CountryCode)
            .HasMaxLength(3);

        builder.Property(c => c.TaxId)
            .HasMaxLength(50);

        builder.Property(c => c.PaymentTerms)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(c => new { c.TenantId, c.Code }).IsUnique();
        builder.HasIndex(c => c.Name);
    }
}

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.ContactName)
            .HasMaxLength(100);

        builder.Property(s => s.Email)
            .HasMaxLength(100);

        builder.Property(s => s.Phone)
            .HasMaxLength(50);

        builder.Property(s => s.AddressLine1)
            .HasMaxLength(200);

        builder.Property(s => s.AddressLine2)
            .HasMaxLength(200);

        builder.Property(s => s.City)
            .HasMaxLength(100);

        builder.Property(s => s.Region)
            .HasMaxLength(100);

        builder.Property(s => s.PostalCode)
            .HasMaxLength(20);

        builder.Property(s => s.CountryCode)
            .HasMaxLength(3);

        builder.Property(s => s.TaxId)
            .HasMaxLength(50);

        builder.Property(s => s.PaymentTerms)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(s => new { s.TenantId, s.Code }).IsUnique();
        builder.HasIndex(s => s.Name);
    }
}

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.ExternalReference)
            .HasMaxLength(100);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(o => o.ShipToName)
            .HasMaxLength(200);

        builder.Property(o => o.ShipToAddressLine1)
            .HasMaxLength(200);

        builder.Property(o => o.ShipToAddressLine2)
            .HasMaxLength(200);

        builder.Property(o => o.ShipToCity)
            .HasMaxLength(100);

        builder.Property(o => o.ShipToRegion)
            .HasMaxLength(100);

        builder.Property(o => o.ShipToPostalCode)
            .HasMaxLength(20);

        builder.Property(o => o.ShipToCountryCode)
            .HasMaxLength(3);

        builder.Property(o => o.CarrierCode)
            .HasMaxLength(50);

        builder.Property(o => o.ServiceLevel)
            .HasMaxLength(50);

        builder.Property(o => o.ShippingInstructions)
            .HasMaxLength(500);

        builder.Property(o => o.TotalQuantity)
            .HasPrecision(18, 4);

        builder.Property(o => o.AllocatedQuantity)
            .HasPrecision(18, 4);

        builder.Property(o => o.PickedQuantity)
            .HasPrecision(18, 4);

        builder.Property(o => o.ShippedQuantity)
            .HasPrecision(18, 4);

        builder.Property(o => o.Notes)
            .HasMaxLength(1000);

        builder.Property(o => o.InternalNotes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(o => o.Customer)
            .WithMany(c => c.SalesOrders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Warehouse)
            .WithMany()
            .HasForeignKey(o => o.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(o => new { o.TenantId, o.OrderNumber }).IsUnique();
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.OrderDate);
        builder.HasIndex(o => o.RequiredDate);
    }
}

public class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.ToTable("SalesOrderLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.QuantityOrdered)
            .HasPrecision(18, 4);

        builder.Property(l => l.QuantityAllocated)
            .HasPrecision(18, 4);

        builder.Property(l => l.QuantityPicked)
            .HasPrecision(18, 4);

        builder.Property(l => l.QuantityShipped)
            .HasPrecision(18, 4);

        builder.Property(l => l.QuantityCancelled)
            .HasPrecision(18, 4);

        builder.Property(l => l.RequiredBatchNumber)
            .HasMaxLength(100);

        builder.Property(l => l.Notes)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(l => l.Order)
            .WithMany(o => o.Lines)
            .HasForeignKey(l => l.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Product)
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(l => new { l.OrderId, l.LineNumber }).IsUnique();
    }
}

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.ExternalReference)
            .HasMaxLength(100);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.TotalQuantity)
            .HasPrecision(18, 4);

        builder.Property(o => o.ReceivedQuantity)
            .HasPrecision(18, 4);

        builder.Property(o => o.Notes)
            .HasMaxLength(1000);

        builder.Property(o => o.InternalNotes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(o => o.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(o => o.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Warehouse)
            .WithMany()
            .HasForeignKey(o => o.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.ReceivingDock)
            .WithMany()
            .HasForeignKey(o => o.ReceivingDockId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(o => new { o.TenantId, o.OrderNumber }).IsUnique();
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.ExpectedDate);
    }
}

public class PurchaseOrderLineConfiguration : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.ToTable("PurchaseOrderLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.QuantityOrdered)
            .HasPrecision(18, 4);

        builder.Property(l => l.QuantityReceived)
            .HasPrecision(18, 4);

        builder.Property(l => l.QuantityCancelled)
            .HasPrecision(18, 4);

        builder.Property(l => l.ExpectedBatchNumber)
            .HasMaxLength(100);

        builder.Property(l => l.Notes)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(l => l.Order)
            .WithMany(o => o.Lines)
            .HasForeignKey(l => l.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Product)
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(l => new { l.OrderId, l.LineNumber }).IsUnique();
    }
}
