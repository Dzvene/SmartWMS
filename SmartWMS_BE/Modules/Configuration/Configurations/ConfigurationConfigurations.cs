using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Configuration.Models;

namespace SmartWMS.API.Modules.Configuration.Configurations;

public class BarcodePrefixConfiguration : IEntityTypeConfiguration<BarcodePrefix>
{
    public void Configure(EntityTypeBuilder<BarcodePrefix> builder)
    {
        builder.ToTable("BarcodePrefixes");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Prefix)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Pattern)
            .HasMaxLength(200);

        builder.Property(p => p.Notes)
            .HasMaxLength(500);

        builder.HasIndex(p => p.TenantId);
        builder.HasIndex(p => new { p.TenantId, p.Prefix }).IsUnique();
    }
}

public class ReasonCodeConfiguration : IEntityTypeConfiguration<ReasonCode>
{
    public void Configure(EntityTypeBuilder<ReasonCode> builder)
    {
        builder.ToTable("ReasonCodes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.Notes)
            .HasMaxLength(500);

        builder.HasIndex(r => r.TenantId);
        builder.HasIndex(r => new { r.TenantId, r.Code, r.ReasonType }).IsUnique();
        builder.HasIndex(r => r.ReasonType);
    }
}

public class NumberSequenceConfiguration : IEntityTypeConfiguration<NumberSequence>
{
    public void Configure(EntityTypeBuilder<NumberSequence> builder)
    {
        builder.ToTable("NumberSequences");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SequenceType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Prefix)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.Suffix)
            .HasMaxLength(20);

        builder.Property(s => s.DateFormat)
            .HasMaxLength(20);

        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => new { s.TenantId, s.SequenceType }).IsUnique();
    }
}

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => new { s.TenantId, s.Category, s.Key }).IsUnique();
        builder.HasIndex(s => s.Category);
    }
}
