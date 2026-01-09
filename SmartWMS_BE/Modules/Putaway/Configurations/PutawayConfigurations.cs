using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Putaway.Models;

namespace SmartWMS.API.Modules.Putaway.Configurations;

public class PutawayTaskConfiguration : IEntityTypeConfiguration<PutawayTask>
{
    public void Configure(EntityTypeBuilder<PutawayTask> builder)
    {
        builder.ToTable("PutawayTasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TaskNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.BatchNumber)
            .HasMaxLength(100);

        builder.Property(t => t.SerialNumber)
            .HasMaxLength(100);

        builder.Property(t => t.Notes)
            .HasMaxLength(1000);

        builder.Property(t => t.QuantityToPutaway)
            .HasPrecision(18, 4);

        builder.Property(t => t.QuantityPutaway)
            .HasPrecision(18, 4);

        // Indexes
        builder.HasIndex(t => t.TenantId);
        builder.HasIndex(t => t.TaskNumber);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.AssignedToUserId);
        builder.HasIndex(t => new { t.TenantId, t.Status });

        // Relationships
        builder.HasOne(t => t.GoodsReceipt)
            .WithMany()
            .HasForeignKey(t => t.GoodsReceiptId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.GoodsReceiptLine)
            .WithMany()
            .HasForeignKey(t => t.GoodsReceiptLineId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Product)
            .WithMany()
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.FromLocation)
            .WithMany()
            .HasForeignKey(t => t.FromLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.SuggestedLocation)
            .WithMany()
            .HasForeignKey(t => t.SuggestedLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.ActualLocation)
            .WithMany()
            .HasForeignKey(t => t.ActualLocationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
