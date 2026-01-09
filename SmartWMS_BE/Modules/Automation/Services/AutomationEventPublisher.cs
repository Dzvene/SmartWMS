using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartWMS.API.Modules.Automation.Models;

namespace SmartWMS.API.Modules.Automation.Services;

/// <summary>
/// Event Publisher - publishes events to the automation system from other modules
/// This is the main integration point for other services to trigger automation rules
/// </summary>
public interface IAutomationEventPublisher
{
    Task PublishEntityCreatedAsync<T>(Guid tenantId, T entity, CancellationToken cancellationToken = default) where T : class;
    Task PublishEntityUpdatedAsync<T>(Guid tenantId, T entity, T? previousState = null, CancellationToken cancellationToken = default) where T : class;
    Task PublishEntityDeletedAsync<T>(Guid tenantId, Guid entityId, CancellationToken cancellationToken = default) where T : class;
    Task PublishStatusChangedAsync<T>(Guid tenantId, T entity, string previousStatus, string newStatus, CancellationToken cancellationToken = default) where T : class;
    Task PublishThresholdCrossedAsync(Guid tenantId, string entityType, Guid entityId, string field, decimal previousValue, decimal newValue, decimal threshold, CancellationToken cancellationToken = default);
    Task PublishCustomEventAsync(Guid tenantId, string eventType, object eventData, CancellationToken cancellationToken = default);
}

public class AutomationEventPublisher : IAutomationEventPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutomationEventPublisher> _logger;

    public AutomationEventPublisher(IServiceProvider serviceProvider, ILogger<AutomationEventPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishEntityCreatedAsync<T>(Guid tenantId, T entity, CancellationToken cancellationToken = default) where T : class
    {
        var entityType = typeof(T).Name;
        var entityId = GetEntityId(entity);

        _logger.LogDebug("Publishing EntityCreated event: {EntityType} {EntityId}", entityType, entityId);

        await ProcessEventAsync(tenantId, TriggerType.EntityCreated, entityType, "Created", entityId, entity, cancellationToken);
    }

    public async Task PublishEntityUpdatedAsync<T>(Guid tenantId, T entity, T? previousState = null, CancellationToken cancellationToken = default) where T : class
    {
        var entityType = typeof(T).Name;
        var entityId = GetEntityId(entity);

        _logger.LogDebug("Publishing EntityUpdated event: {EntityType} {EntityId}", entityType, entityId);

        var eventData = new
        {
            current = entity,
            previous = previousState
        };

        await ProcessEventAsync(tenantId, TriggerType.EntityUpdated, entityType, "Updated", entityId, eventData, cancellationToken);
    }

    public async Task PublishEntityDeletedAsync<T>(Guid tenantId, Guid entityId, CancellationToken cancellationToken = default) where T : class
    {
        var entityType = typeof(T).Name;

        _logger.LogDebug("Publishing EntityDeleted event: {EntityType} {EntityId}", entityType, entityId);

        await ProcessEventAsync(tenantId, TriggerType.EntityDeleted, entityType, "Deleted", entityId, new { entityId }, cancellationToken);
    }

    public async Task PublishStatusChangedAsync<T>(Guid tenantId, T entity, string previousStatus, string newStatus, CancellationToken cancellationToken = default) where T : class
    {
        var entityType = typeof(T).Name;
        var entityId = GetEntityId(entity);

        _logger.LogDebug("Publishing StatusChanged event: {EntityType} {EntityId} {PreviousStatus} -> {NewStatus}",
            entityType, entityId, previousStatus, newStatus);

        var eventData = new
        {
            entity,
            previousStatus,
            newStatus
        };

        await ProcessEventAsync(tenantId, TriggerType.StatusChanged, entityType, $"StatusChanged:{newStatus}", entityId, eventData, cancellationToken);
    }

    public async Task PublishThresholdCrossedAsync(
        Guid tenantId,
        string entityType,
        Guid entityId,
        string field,
        decimal previousValue,
        decimal newValue,
        decimal threshold,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Publishing ThresholdCrossed event: {EntityType} {EntityId} {Field} crossed {Threshold}",
            entityType, entityId, field, threshold);

        var direction = newValue < threshold ? "Below" : "Above";
        var eventData = new
        {
            entityId,
            field,
            previousValue,
            newValue,
            threshold,
            direction
        };

        await ProcessEventAsync(tenantId, TriggerType.ThresholdCrossed, entityType, $"ThresholdCrossed:{field}:{direction}", entityId, eventData, cancellationToken);
    }

    public async Task PublishCustomEventAsync(
        Guid tenantId,
        string eventType,
        object eventData,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Publishing custom event: {EventType}", eventType);

        await ProcessEventAsync(tenantId, TriggerType.WebhookReceived, "Custom", eventType, null, eventData, cancellationToken);
    }

    private async Task ProcessEventAsync(
        Guid tenantId,
        TriggerType triggerType,
        string entityType,
        string eventName,
        Guid? entityId,
        object? eventData,
        CancellationToken cancellationToken)
    {
        try
        {
            // Use a scope to get the AutomationService
            using var scope = _serviceProvider.CreateScope();
            var automationService = scope.ServiceProvider.GetRequiredService<IAutomationService>();

            await automationService.ProcessEventAsync(tenantId, triggerType, entityType, eventName, entityId, eventData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing automation event {TriggerType} {EntityType} {EventName}",
                triggerType, entityType, eventName);
            // Don't rethrow - automation failures shouldn't break the main operation
        }
    }

    private Guid? GetEntityId<T>(T entity) where T : class
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null && idProperty.PropertyType == typeof(Guid))
        {
            return (Guid?)idProperty.GetValue(entity);
        }
        return null;
    }
}

/// <summary>
/// Extension methods for easy integration with other services
/// </summary>
public static class AutomationEventPublisherExtensions
{
    /// <summary>
    /// Publish stock low event when quantity falls below minimum
    /// </summary>
    public static async Task PublishStockLowAsync(
        this IAutomationEventPublisher publisher,
        Guid tenantId,
        Guid productId,
        string sku,
        Guid locationId,
        string locationCode,
        decimal currentQuantity,
        decimal minQuantity,
        CancellationToken cancellationToken = default)
    {
        await publisher.PublishThresholdCrossedAsync(
            tenantId,
            "StockLevel",
            productId,
            "quantity",
            minQuantity + 1, // Previous was above
            currentQuantity,
            minQuantity,
            cancellationToken);
    }
}
