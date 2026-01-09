using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Automation.Models;
using NCrontab;

namespace SmartWMS.API.Modules.Automation.Services;

/// <summary>
/// Background service that executes scheduled automation rules based on cron expressions
/// </summary>
public class AutomationSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutomationSchedulerService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public AutomationSchedulerService(
        IServiceProvider serviceProvider,
        ILogger<AutomationSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Automation Scheduler Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessScheduledRulesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled rules");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Automation Scheduler Service stopped");
    }

    private async Task ProcessScheduledRulesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var automationService = scope.ServiceProvider.GetRequiredService<IAutomationService>();

        // Get all active scheduled rules
        var scheduledRules = await dbContext.AutomationRules
            .Where(r => r.IsActive && r.TriggerType == TriggerType.Schedule && !string.IsNullOrEmpty(r.CronExpression))
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var rule in scheduledRules)
        {
            try
            {
                if (ShouldExecute(rule, now))
                {
                    _logger.LogInformation("Executing scheduled rule: {RuleId} {RuleName}", rule.Id, rule.Name);

                    await automationService.TriggerRuleAsync(rule.TenantId, rule.Id, null, cancellationToken);

                    // Update last executed time
                    rule.LastExecutedAt = now;
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing scheduled rule {RuleId}", rule.Id);
            }
        }
    }

    private bool ShouldExecute(AutomationRule rule, DateTime now)
    {
        if (string.IsNullOrEmpty(rule.CronExpression))
            return false;

        try
        {
            var schedule = CrontabSchedule.Parse(rule.CronExpression, new CrontabSchedule.ParseOptions
            {
                IncludingSeconds = rule.CronExpression.Split(' ').Length == 6
            });

            // Get the last scheduled time before now
            var lastScheduledTime = schedule.GetNextOccurrence(now.AddMinutes(-2));

            // If no previous trigger or last trigger was before this scheduled time
            if (!rule.LastExecutedAt.HasValue)
            {
                // First run - check if we're within the execution window
                return IsWithinExecutionWindow(lastScheduledTime, now);
            }

            // Check if this scheduled time is after the last trigger
            if (lastScheduledTime > rule.LastExecutedAt.Value)
            {
                return IsWithinExecutionWindow(lastScheduledTime, now);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Invalid cron expression for rule {RuleId}: {Cron}", rule.Id, rule.CronExpression);
            return false;
        }
    }

    private bool IsWithinExecutionWindow(DateTime scheduledTime, DateTime now)
    {
        // Execute if scheduled time is within the last 2 minutes (our check interval + buffer)
        var windowStart = now.AddMinutes(-2);
        return scheduledTime >= windowStart && scheduledTime <= now;
    }
}

/// <summary>
/// Scheduled job tracker for more precise scheduling
/// </summary>
public class ScheduledJobTracker
{
    private readonly Dictionary<Guid, DateTime> _nextExecutionTimes = new();
    private readonly object _lock = new();

    public void UpdateNextExecution(Guid ruleId, string cronExpression)
    {
        try
        {
            var schedule = CrontabSchedule.Parse(cronExpression);
            var nextTime = schedule.GetNextOccurrence(DateTime.UtcNow);

            lock (_lock)
            {
                _nextExecutionTimes[ruleId] = nextTime;
            }
        }
        catch
        {
            // Invalid cron - remove from tracking
            lock (_lock)
            {
                _nextExecutionTimes.Remove(ruleId);
            }
        }
    }

    public DateTime? GetNextExecution(Guid ruleId)
    {
        lock (_lock)
        {
            return _nextExecutionTimes.TryGetValue(ruleId, out var time) ? time : null;
        }
    }

    public IEnumerable<(Guid RuleId, DateTime NextTime)> GetDueRules(DateTime asOf)
    {
        lock (_lock)
        {
            return _nextExecutionTimes
                .Where(kvp => kvp.Value <= asOf)
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToList();
        }
    }

    public void Remove(Guid ruleId)
    {
        lock (_lock)
        {
            _nextExecutionTimes.Remove(ruleId);
        }
    }
}
