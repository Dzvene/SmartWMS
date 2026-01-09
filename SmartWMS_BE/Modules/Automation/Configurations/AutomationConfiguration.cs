using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWMS.API.Modules.Automation.Models;

namespace SmartWMS.API.Modules.Automation.Configurations;

public class AutomationRuleConfiguration : IEntityTypeConfiguration<AutomationRule>
{
    public void Configure(EntityTypeBuilder<AutomationRule> builder)
    {
        builder.ToTable("AutomationRules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.TriggerEntityType)
            .HasMaxLength(100);

        builder.Property(x => x.TriggerEvent)
            .HasMaxLength(100);

        builder.Property(x => x.CronExpression)
            .HasMaxLength(100);

        builder.Property(x => x.Timezone)
            .HasMaxLength(50);

        builder.Property(x => x.ConditionsJson)
            .HasColumnType("jsonb");

        builder.Property(x => x.ActionConfigJson)
            .HasColumnType("jsonb");

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => new { x.TenantId, x.TriggerType, x.IsActive });
        builder.HasIndex(x => new { x.TenantId, x.NextScheduledAt })
            .HasFilter("\"IsActive\" = true AND \"TriggerType\" = 5"); // Schedule

        builder.HasMany(x => x.Executions)
            .WithOne(x => x.Rule)
            .HasForeignKey(x => x.RuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RuleConditionConfiguration : IEntityTypeConfiguration<RuleCondition>
{
    public void Configure(EntityTypeBuilder<RuleCondition> builder)
    {
        builder.ToTable("AutomationRuleConditions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Field)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.ValueType)
            .HasMaxLength(50);

        builder.HasIndex(x => x.RuleId);

        builder.HasOne(x => x.Rule)
            .WithMany()
            .HasForeignKey(x => x.RuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RuleExecutionConfiguration : IEntityTypeConfiguration<RuleExecution>
{
    public void Configure(EntityTypeBuilder<RuleExecution> builder)
    {
        builder.ToTable("AutomationRuleExecutions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TriggerEntityType)
            .HasMaxLength(100);

        builder.Property(x => x.TriggerEventData)
            .HasColumnType("jsonb");

        builder.Property(x => x.ResultData)
            .HasColumnType("jsonb");

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedEntityType)
            .HasMaxLength(100);

        builder.HasIndex(x => x.RuleId);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.StartedAt);
        builder.HasIndex(x => new { x.TenantId, x.StartedAt })
            .IsDescending(false, true);
    }
}

public class ActionTemplateConfiguration : IEntityTypeConfiguration<ActionTemplate>
{
    public void Configure(EntityTypeBuilder<ActionTemplate> builder)
    {
        builder.ToTable("AutomationActionTemplates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.ConfigJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.HasIndex(x => new { x.TenantId, x.Code })
            .IsUnique();
    }
}

public class ScheduledJobConfiguration : IEntityTypeConfiguration<ScheduledJob>
{
    public void Configure(EntityTypeBuilder<ScheduledJob> builder)
    {
        builder.ToTable("AutomationScheduledJobs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.RuleId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ScheduledFor);
        builder.HasIndex(x => new { x.Status, x.ScheduledFor })
            .HasFilter("\"Status\" = 0"); // Pending
    }
}
