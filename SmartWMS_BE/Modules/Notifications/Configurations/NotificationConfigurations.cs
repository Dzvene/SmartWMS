using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Notifications.Models;

namespace SmartWMS.API.Modules.Notifications.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(n => n.Category)
            .HasMaxLength(100);

        builder.Property(n => n.EntityType)
            .HasMaxLength(100);

        builder.Property(n => n.ActionUrl)
            .HasMaxLength(500);

        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.Priority)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(n => new { n.TenantId, n.UserId });
        builder.HasIndex(n => new { n.TenantId, n.UserId, n.IsRead });
        builder.HasIndex(n => new { n.TenantId, n.UserId, n.IsArchived });
        builder.HasIndex(n => n.ExpiresAt);
        builder.HasIndex(n => n.CreatedAt);
    }
}

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.MinimumPriority)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(p => new { p.TenantId, p.UserId, p.Category }).IsUnique();
    }
}

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.TitleTemplate)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.MessageTemplate)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(t => t.EmailSubjectTemplate)
            .HasMaxLength(200);

        builder.Property(t => t.EmailBodyTemplate)
            .HasMaxLength(5000);

        builder.Property(t => t.PushTitleTemplate)
            .HasMaxLength(100);

        builder.Property(t => t.PushBodyTemplate)
            .HasMaxLength(500);

        builder.Property(t => t.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasIndex(t => t.Category);
    }
}
