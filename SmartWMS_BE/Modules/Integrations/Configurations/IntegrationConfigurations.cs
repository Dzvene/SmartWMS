using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Integrations.Models;

namespace SmartWMS.API.Modules.Integrations.Configurations;

public class IntegrationConfiguration : IEntityTypeConfiguration<Integration>
{
    public void Configure(EntityTypeBuilder<Integration> builder)
    {
        builder.ToTable("Integrations");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Description)
            .HasMaxLength(500);

        builder.Property(i => i.Provider)
            .HasMaxLength(100);

        builder.Property(i => i.BaseUrl)
            .HasMaxLength(500);

        builder.Property(i => i.ApiKey)
            .HasMaxLength(500);

        builder.Property(i => i.ApiSecret)
            .HasMaxLength(500);

        builder.Property(i => i.Username)
            .HasMaxLength(200);

        builder.Property(i => i.Password)
            .HasMaxLength(500);

        builder.Property(i => i.AccessToken)
            .HasMaxLength(2000);

        builder.Property(i => i.RefreshToken)
            .HasMaxLength(2000);

        builder.Property(i => i.ConfigurationJson)
            .HasMaxLength(10000);

        builder.Property(i => i.LastError)
            .HasMaxLength(2000);

        builder.Property(i => i.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(i => new { i.TenantId, i.Code }).IsUnique();
        builder.HasIndex(i => new { i.TenantId, i.IsActive });
    }
}

public class IntegrationLogConfiguration : IEntityTypeConfiguration<IntegrationLog>
{
    public void Configure(EntityTypeBuilder<IntegrationLog> builder)
    {
        builder.ToTable("IntegrationLogs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Direction)
            .HasMaxLength(10);

        builder.Property(l => l.EntityType)
            .HasMaxLength(100);

        builder.Property(l => l.ExternalId)
            .HasMaxLength(200);

        builder.Property(l => l.RequestData)
            .HasMaxLength(50000);

        builder.Property(l => l.ResponseData)
            .HasMaxLength(50000);

        builder.Property(l => l.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(l => l.LogType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasOne(l => l.Integration)
            .WithMany(i => i.Logs)
            .HasForeignKey(l => l.IntegrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => new { l.TenantId, l.IntegrationId, l.CreatedAt });
        builder.HasIndex(l => new { l.TenantId, l.IntegrationId, l.Success });
    }
}

public class IntegrationMappingConfiguration : IEntityTypeConfiguration<IntegrationMapping>
{
    public void Configure(EntityTypeBuilder<IntegrationMapping> builder)
    {
        builder.ToTable("IntegrationMappings");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.LocalEntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.ExternalEntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.ExternalEntityId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.LastSyncDirection)
            .HasMaxLength(10);

        builder.Property(m => m.SyncStatus)
            .HasMaxLength(50);

        builder.HasOne(m => m.Integration)
            .WithMany(i => i.Mappings)
            .HasForeignKey(m => m.IntegrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.TenantId, m.IntegrationId, m.LocalEntityType, m.LocalEntityId }).IsUnique();
        builder.HasIndex(m => new { m.TenantId, m.IntegrationId, m.ExternalEntityType, m.ExternalEntityId });
    }
}

public class WebhookEndpointConfiguration : IEntityTypeConfiguration<WebhookEndpoint>
{
    public void Configure(EntityTypeBuilder<WebhookEndpoint> builder)
    {
        builder.ToTable("WebhookEndpoints");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(w => w.Secret)
            .HasMaxLength(200);

        builder.HasOne(w => w.Integration)
            .WithMany()
            .HasForeignKey(w => w.IntegrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => new { w.TenantId, w.IntegrationId });
    }
}
