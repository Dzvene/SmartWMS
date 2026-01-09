using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SmartWMS.API.Hubs;

/// <summary>
/// SignalR hub for real-time warehouse updates.
/// Clients join tenant-specific groups to receive updates.
/// </summary>
[Authorize]
public class WarehouseHub : Hub
{
    private readonly ILogger<WarehouseHub> _logger;

    public WarehouseHub(ILogger<WarehouseHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.User?.FindFirst("tenant_id")?.Value;
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(tenantId))
        {
            // Join tenant group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
            _logger.LogInformation("Client {ConnectionId} joined tenant group {TenantId}",
                Context.ConnectionId, tenantId);
        }

        if (!string.IsNullOrEmpty(userId))
        {
            // Join user-specific group for personal notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var tenantId = Context.User?.FindFirst("tenant_id")?.Value;

        if (!string.IsNullOrEmpty(tenantId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
            _logger.LogInformation("Client {ConnectionId} left tenant group {TenantId}",
                Context.ConnectionId, tenantId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to specific entity updates (e.g., "order_123")
    /// </summary>
    public async Task SubscribeToEntity(string entityType, string entityId)
    {
        var groupName = $"{entityType}_{entityId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("Client {ConnectionId} subscribed to {GroupName}",
            Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Unsubscribe from entity updates
    /// </summary>
    public async Task UnsubscribeFromEntity(string entityType, string entityId)
    {
        var groupName = $"{entityType}_{entityId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}

/// <summary>
/// Service for sending real-time updates to connected clients
/// </summary>
public interface IWarehouseNotifier
{
    // Order updates
    Task NotifyOrderCreatedAsync(Guid tenantId, object order);
    Task NotifyOrderUpdatedAsync(Guid tenantId, object order);
    Task NotifyOrderStatusChangedAsync(Guid tenantId, Guid orderId, string oldStatus, string newStatus);

    // Task updates
    Task NotifyTaskAssignedAsync(Guid tenantId, Guid userId, object task);
    Task NotifyTaskCompletedAsync(Guid tenantId, object task);

    // Stock updates
    Task NotifyStockChangedAsync(Guid tenantId, Guid productId, Guid locationId, decimal newQuantity);
    Task NotifyLowStockAlertAsync(Guid tenantId, Guid productId, string sku, decimal currentQty, decimal minQty);

    // Notification
    Task SendNotificationAsync(Guid userId, object notification);
    Task BroadcastToTenantAsync(Guid tenantId, string eventType, object data);
}

/// <summary>
/// Implementation of real-time notifier using SignalR
/// </summary>
public class WarehouseNotifier : IWarehouseNotifier
{
    private readonly IHubContext<WarehouseHub> _hubContext;
    private readonly ILogger<WarehouseNotifier> _logger;

    public WarehouseNotifier(IHubContext<WarehouseHub> hubContext, ILogger<WarehouseNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyOrderCreatedAsync(Guid tenantId, object order)
    {
        await BroadcastToTenantAsync(tenantId, "OrderCreated", order);
    }

    public async Task NotifyOrderUpdatedAsync(Guid tenantId, object order)
    {
        await BroadcastToTenantAsync(tenantId, "OrderUpdated", order);
    }

    public async Task NotifyOrderStatusChangedAsync(Guid tenantId, Guid orderId, string oldStatus, string newStatus)
    {
        await BroadcastToTenantAsync(tenantId, "OrderStatusChanged", new
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyTaskAssignedAsync(Guid tenantId, Guid userId, object task)
    {
        // Notify the specific user
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("TaskAssigned", task);

        // Also broadcast to tenant for dashboard updates
        await BroadcastToTenantAsync(tenantId, "TaskAssigned", task);
    }

    public async Task NotifyTaskCompletedAsync(Guid tenantId, object task)
    {
        await BroadcastToTenantAsync(tenantId, "TaskCompleted", task);
    }

    public async Task NotifyStockChangedAsync(Guid tenantId, Guid productId, Guid locationId, decimal newQuantity)
    {
        await BroadcastToTenantAsync(tenantId, "StockChanged", new
        {
            ProductId = productId,
            LocationId = locationId,
            NewQuantity = newQuantity,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyLowStockAlertAsync(Guid tenantId, Guid productId, string sku, decimal currentQty, decimal minQty)
    {
        await BroadcastToTenantAsync(tenantId, "LowStockAlert", new
        {
            ProductId = productId,
            Sku = sku,
            CurrentQuantity = currentQty,
            MinimumQuantity = minQty,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task SendNotificationAsync(Guid userId, object notification)
    {
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("Notification", notification);
    }

    public async Task BroadcastToTenantAsync(Guid tenantId, string eventType, object data)
    {
        try
        {
            await _hubContext.Clients.Group($"tenant_{tenantId}")
                .SendAsync(eventType, data);

            _logger.LogDebug("Broadcast {EventType} to tenant {TenantId}", eventType, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast {EventType} to tenant {TenantId}", eventType, tenantId);
        }
    }
}
