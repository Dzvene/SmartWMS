using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Infrastructure.Services.Email;
using SmartWMS.API.Modules.Automation.Models;
using SmartWMS.API.Modules.Notifications.Models;
using SmartWMS.API.Modules.Fulfillment.Services;
using SmartWMS.API.Modules.Fulfillment.DTOs;
using SmartWMS.API.Modules.Putaway.Services;
using SmartWMS.API.Modules.Putaway.DTOs;
using SmartWMS.API.Modules.Adjustments.Services;
using SmartWMS.API.Modules.Adjustments.DTOs;
using SmartWMS.API.Modules.Adjustments.Models;
using SmartWMS.API.Modules.Transfers.Services;
using SmartWMS.API.Modules.Transfers.DTOs;
using SmartWMS.API.Modules.Transfers.Models;
using SmartWMS.API.Modules.CycleCount.Services;
using SmartWMS.API.Modules.CycleCount.DTOs;
using SmartWMS.API.Modules.CycleCount.Models;
using SmartWMS.API.Modules.Reports.Services;
using SmartWMS.API.Modules.Reports.DTOs;
using SmartWMS.API.Modules.Integrations.Services;

namespace SmartWMS.API.Modules.Automation.Services;

/// <summary>
/// Action Executor - executes the actions defined in automation rules
/// </summary>
public interface IActionExecutor
{
    Task<ActionResult> ExecuteAsync(
        AutomationRule rule,
        RuleExecution execution,
        object? triggerData,
        CancellationToken cancellationToken = default);
}

public class ActionExecutor : IActionExecutor
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ActionExecutor> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ActionExecutor(
        ApplicationDbContext dbContext,
        ILogger<ActionExecutor> logger,
        IServiceProvider serviceProvider,
        IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ActionResult> ExecuteAsync(
        AutomationRule rule,
        RuleExecution execution,
        object? triggerData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing action {ActionType} for rule {RuleId} ({RuleName})",
                rule.ActionType, rule.Id, rule.Name);

            return rule.ActionType switch
            {
                ActionType.SendNotification => await ExecuteSendNotificationAsync(rule, execution, triggerData, cancellationToken),
                ActionType.SendEmail => await ExecuteSendEmailAsync(rule, execution, triggerData, cancellationToken),
                ActionType.SendWebhook => await ExecuteSendWebhookAsync(rule, execution, triggerData, cancellationToken),
                ActionType.CreateTask => await ExecuteCreateTaskAsync(rule, execution, triggerData, cancellationToken),
                ActionType.UpdateEntityStatus => await ExecuteUpdateEntityStatusAsync(rule, execution, triggerData, cancellationToken),
                ActionType.UpdateEntityField => await ExecuteUpdateEntityFieldAsync(rule, execution, triggerData, cancellationToken),
                ActionType.GenerateReport => await ExecuteGenerateReportAsync(rule, execution, triggerData, cancellationToken),
                ActionType.TriggerSync => await ExecuteTriggerSyncAsync(rule, execution, triggerData, cancellationToken),
                ActionType.CreateAdjustment => await ExecuteCreateAdjustmentAsync(rule, execution, triggerData, cancellationToken),
                ActionType.CreateTransfer => await ExecuteCreateTransferAsync(rule, execution, triggerData, cancellationToken),
                _ => ActionResult.Failure($"Unsupported action type: {rule.ActionType}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing action {ActionType} for rule {RuleId}", rule.ActionType, rule.Id);
            return ActionResult.Failure(ex.Message);
        }
    }

    #region SendNotification

    private async Task<ActionResult> ExecuteSendNotificationAsync(
        AutomationRule rule,
        RuleExecution execution,
        object? triggerData,
        CancellationToken cancellationToken)
    {
        var config = DeserializeConfig<SendNotificationConfig>(rule.ActionConfigJson);
        if (config == null)
            return ActionResult.Failure("Invalid notification configuration");

        var title = ReplacePlaceholders(config.Title ?? rule.Name, triggerData);
        var message = ReplacePlaceholders(config.Message ?? "", triggerData);

        // Determine recipients
        var userIds = new List<Guid>();

        if (config.UserIds?.Any() == true)
            userIds.AddRange(config.UserIds);

        if (config.RoleIds?.Any() == true)
        {
            var usersInRoles = await _dbContext.Users
                .Where(u => _dbContext.UserRoles
                    .Any(ur => ur.UserId == u.Id && config.RoleIds.Contains(ur.RoleId)))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);
            userIds.AddRange(usersInRoles);
        }

        // If no specific recipients, send to all users in tenant with appropriate role
        if (!userIds.Any() && config.SendToAllAdmins == true)
        {
            var adminRoles = await _dbContext.Roles
                .Where(r => r.Name != null && (r.Name.Contains("Admin") || r.Name.Contains("Manager")))
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

            var adminUsers = await _dbContext.Users
                .Where(u => u.TenantId == rule.TenantId &&
                    _dbContext.UserRoles.Any(ur => ur.UserId == u.Id && adminRoles.Contains(ur.RoleId)))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);
            userIds.AddRange(adminUsers);
        }

        userIds = userIds.Distinct().ToList();

        if (!userIds.Any())
        {
            _logger.LogWarning("No recipients for notification from rule {RuleId}", rule.Id);
            return ActionResult.Success("No recipients found - notification skipped");
        }

        // Create notifications
        var priority = Enum.TryParse<NotificationPriority>(config.Priority, out var p) ? p : NotificationPriority.Normal;
        var notificationType = Enum.TryParse<NotificationType>(config.NotificationType, out var nt) ? nt : NotificationType.Alert;

        var notifications = new List<Notification>();
        foreach (var userId in userIds)
        {
            var notification = new Notification
            {
                TenantId = rule.TenantId,
                UserId = userId,
                Type = notificationType,
                Priority = priority,
                Title = title,
                Message = message,
                EntityType = execution.TriggerEntityType,
                EntityId = execution.TriggerEntityId,
                ActionUrl = config.ActionUrl ?? $"/automation/rules/{rule.Id}"
            };

            notifications.Add(notification);
            _dbContext.Notifications.Add(notification);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created {Count} notifications for rule {RuleId}", notifications.Count, rule.Id);

        return ActionResult.Success($"Sent {userIds.Count} notification(s)");
    }

    #endregion

    #region SendEmail

    private async Task<ActionResult> ExecuteSendEmailAsync(
        AutomationRule rule,
        RuleExecution execution,
        object? triggerData,
        CancellationToken cancellationToken)
    {
        var config = DeserializeConfig<SendEmailConfig>(rule.ActionConfigJson);
        if (config == null || (config.ToAddresses?.Any() != true && config.ToUserIds?.Any() != true && config.ToRoleIds?.Any() != true))
            return ActionResult.Failure("Invalid email configuration - no recipients specified");

        var recipients = new List<string>();

        // Add direct email addresses
        if (config.ToAddresses?.Any() == true)
            recipients.AddRange(config.ToAddresses);

        // Resolve user IDs to emails
        if (config.ToUserIds?.Any() == true)
        {
            var userEmails = await _dbContext.Users
                .Where(u => config.ToUserIds.Contains(u.Id) && !string.IsNullOrEmpty(u.Email))
                .Select(u => u.Email!)
                .ToListAsync(cancellationToken);
            recipients.AddRange(userEmails);
        }

        // Resolve role IDs to user emails
        if (config.ToRoleIds?.Any() == true)
        {
            var roleUserEmails = await _dbContext.Users
                .Where(u => u.TenantId == rule.TenantId &&
                    _dbContext.UserRoles.Any(ur => ur.UserId == u.Id && config.ToRoleIds.Contains(ur.RoleId)) &&
                    !string.IsNullOrEmpty(u.Email))
                .Select(u => u.Email!)
                .ToListAsync(cancellationToken);
            recipients.AddRange(roleUserEmails);
        }

        recipients = recipients.Distinct().ToList();

        if (!recipients.Any())
            return ActionResult.Failure("No valid email recipients found");

        var subject = ReplacePlaceholders(config.Subject ?? $"[SmartWMS] {rule.Name}", triggerData);
        var body = ReplacePlaceholders(config.BodyTemplate ?? config.Body ?? "", triggerData);

        // Use the email service to send the email
        var emailService = _serviceProvider.GetRequiredService<IEmailService>();

        var emailMessage = new EmailMessage
        {
            To = recipients,
            Cc = config.CcAddresses,
            Subject = subject,
            Body = body,
            IsHtml = config.IsHtml
        };

        var emailResult = await emailService.SendEmailAsync(emailMessage, cancellationToken);

        // Store result in execution
        var emailRecord = new
        {
            To = recipients,
            Cc = config.CcAddresses ?? new List<string>(),
            Subject = subject,
            BodyLength = body.Length,
            IsHtml = config.IsHtml,
            RuleId = rule.Id,
            ExecutionId = execution.Id,
            SentAt = DateTime.UtcNow,
            Success = emailResult.Success,
            MessageId = emailResult.MessageId,
            ErrorMessage = emailResult.ErrorMessage
        };

        execution.ResultData = JsonSerializer.Serialize(emailRecord, JsonOptions);

        if (emailResult.Success)
        {
            _logger.LogInformation("Email sent successfully for rule {RuleId}: To={Recipients}, Subject={Subject}, MessageId={MessageId}",
                rule.Id, string.Join(", ", recipients), subject, emailResult.MessageId);
            return ActionResult.Success($"Email sent to {recipients.Count} recipient(s): {string.Join(", ", recipients.Take(3))}{(recipients.Count > 3 ? "..." : "")}");
        }
        else
        {
            _logger.LogWarning("Failed to send email for rule {RuleId}: {Error}",
                rule.Id, emailResult.ErrorMessage);
            return ActionResult.Failure($"Failed to send email: {emailResult.ErrorMessage}");
        }
    }

    #endregion

    #region SendWebhook

    private async Task<ActionResult> ExecuteSendWebhookAsync(
        AutomationRule rule,
        RuleExecution execution,
        object? triggerData,
        CancellationToken cancellationToken)
    {
        var config = DeserializeConfig<SendWebhookConfig>(rule.ActionConfigJson);
        if (config == null || string.IsNullOrEmpty(config.Url))
            return ActionResult.Failure("Invalid webhook configuration - URL is required");

        try
        {
            using var httpClient = _httpClientFactory.CreateClient("AutomationWebhook");
            httpClient.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds ?? 30);

            // Add headers
            if (config.Headers != null)
            {
                foreach (var header in config.Headers)
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Add authentication if configured
            if (!string.IsNullOrEmpty(config.AuthType))
            {
                switch (config.AuthType.ToLower())
                {
                    case "bearer":
                        httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", config.AuthToken);
                        break;
                    case "basic":
                        var credentials = Convert.ToBase64String(
                            System.Text.Encoding.UTF8.GetBytes($"{config.AuthUsername}:{config.AuthPassword}"));
                        httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Basic", credentials);
                        break;
                    case "apikey":
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                            config.ApiKeyHeader ?? "X-API-Key", config.AuthToken);
                        break;
                }
            }

            // Prepare payload
            var payload = string.IsNullOrEmpty(config.PayloadTemplate)
                ? JsonSerializer.Serialize(new
                {
                    ruleId = rule.Id,
                    ruleName = rule.Name,
                    executionId = execution.Id,
                    triggerType = rule.TriggerType.ToString(),
                    entityType = execution.TriggerEntityType,
                    entityId = execution.TriggerEntityId,
                    data = triggerData,
                    timestamp = DateTime.UtcNow
                }, JsonOptions)
                : ReplacePlaceholders(config.PayloadTemplate, triggerData);

            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            var method = (config.Method ?? "POST").ToUpperInvariant();

            response = method switch
            {
                "POST" => await httpClient.PostAsync(config.Url, content, cancellationToken),
                "PUT" => await httpClient.PutAsync(config.Url, content, cancellationToken),
                "PATCH" => await httpClient.PatchAsync(config.Url, content, cancellationToken),
                "DELETE" => await httpClient.DeleteAsync(config.Url, cancellationToken),
                _ => await httpClient.GetAsync(config.Url, cancellationToken)
            };

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            // Store response in execution
            execution.ResultData = JsonSerializer.Serialize(new
            {
                statusCode = (int)response.StatusCode,
                reasonPhrase = response.ReasonPhrase,
                responseBody = responseBody.Length > 1000 ? responseBody[..1000] + "..." : responseBody
            }, JsonOptions);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Webhook succeeded for rule {RuleId}: {StatusCode}", rule.Id, response.StatusCode);
                return ActionResult.Success($"Webhook called successfully: {response.StatusCode}");
            }
            else
            {
                _logger.LogWarning("Webhook failed for rule {RuleId}: {StatusCode} - {Response}",
                    rule.Id, response.StatusCode, responseBody.Length > 200 ? responseBody[..200] : responseBody);
                return ActionResult.Failure($"Webhook failed: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        catch (TaskCanceledException)
        {
            return ActionResult.Failure("Webhook timed out");
        }
        catch (HttpRequestException ex)
        {
            return ActionResult.Failure($"Webhook request failed: {ex.Message}");
        }
    }

    #endregion

    #region CreateTask

    private async Task<ActionResult> ExecuteCreateTaskAsync(
        AutomationRule rule,
        RuleExecution execution,
        object? triggerData,
        CancellationToken cancellationToken)
    {
        var config = DeserializeConfig<CreateTaskConfig>(rule.ActionConfigJson);
        if (config == null || string.IsNullOrEmpty(config.TaskType))
            return ActionResult.Failure("Invalid task configuration - TaskType is required");

        var taskType = config.TaskType.ToLowerInvariant();

        try
        {
            switch (taskType)
            {
                case "pick":
                case "picking":
                    return await CreatePickTaskAsync(rule, config, triggerData, cancellationToken);

                case "putaway":
                    return await CreatePutawayTaskAsync(rule, config, triggerData, cancellationToken);

                case "count":
                case "cyclecount":
                    return await CreateCycleCountTaskAsync(rule, config, triggerData, cancellationToken);

                default:
                    _logger.LogWarning("Unknown task type '{TaskType}' for rule {RuleId}", taskType, rule.Id);
                    return ActionResult.Failure($"Unknown task type: {taskType}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create task for rule {RuleId}", rule.Id);
            return ActionResult.Failure($"Failed to create task: {ex.Message}");
        }
    }

    private async Task<ActionResult> CreatePickTaskAsync(
        AutomationRule rule,
        CreateTaskConfig config,
        object? triggerData,
        CancellationToken cancellationToken)
    {
        // Extract data from trigger
        var data = ExtractTriggerData(triggerData);

        if (!data.TryGetValue("orderId", out var orderIdStr) || !Guid.TryParse(orderIdStr, out var orderId))
            return ActionResult.Failure("Cannot create pick task: orderId not found in trigger data");

        if (!data.TryGetValue("productId", out var productIdStr) || !Guid.TryParse(productIdStr, out var productId))
            return ActionResult.Failure("Cannot create pick task: productId not found in trigger data");

        // Get pick task service
        var pickTaskService = _serviceProvider.GetRequiredService<IPickTasksService>();

        // Find best location for picking
        var stockLevel = await _dbContext.StockLevels
            .Where(s => s.TenantId == rule.TenantId && s.ProductId == productId && s.QuantityAvailable > 0)
            .OrderByDescending(s => s.QuantityAvailable)
            .FirstOrDefaultAsync(cancellationToken);

        if (stockLevel == null)
            return ActionResult.Failure($"No available stock found for product {productId}");

        decimal quantity = 1;
        if (data.TryGetValue("quantity", out var qtyStr) && decimal.TryParse(qtyStr, out var qty))
            quantity = qty;

        var request = new CreatePickTaskRequest
        {
            OrderId = orderId,
            OrderLineId = data.TryGetValue("orderLineId", out var lineIdStr) && Guid.TryParse(lineIdStr, out var lineId) ? lineId : Guid.Empty,
            ProductId = productId,
            FromLocationId = stockLevel.LocationId,
            QuantityRequired = quantity,
            Priority = config.Priority ?? 0,
            AssignedToUserId = config.AssignToUserId
        };

        var result = await pickTaskService.CreateTaskAsync(rule.TenantId, request);

        if (result.Success && result.Data != null)
        {
            _logger.LogInformation("Created pick task {TaskNumber} for rule {RuleId}", result.Data.TaskNumber, rule.Id);
            return ActionResult.Success($"Created pick task: {result.Data.TaskNumber}", "PickTask", result.Data.Id);
        }

        return ActionResult.Failure($"Failed to create pick task: {result.Message}");
    }

    private async Task<ActionResult> CreatePutawayTaskAsync(
        AutomationRule rule,
        CreateTaskConfig config,
        object? triggerData,
        CancellationToken cancellationToken)
    {
        var data = ExtractTriggerData(triggerData);

        // Putaway task requires productId, fromLocationId, and quantity
        if (!data.TryGetValue("productId", out var productIdStr) || !Guid.TryParse(productIdStr, out var productId))
            return ActionResult.Failure("Cannot create putaway task: productId not found in trigger data");

        if (!data.TryGetValue("fromLocationId", out var fromLocIdStr) || !Guid.TryParse(fromLocIdStr, out var fromLocationId))
        {
            // Try to get from receiving location
            if (!data.TryGetValue("locationId", out fromLocIdStr) || !Guid.TryParse(fromLocIdStr, out fromLocationId))
                return ActionResult.Failure("Cannot create putaway task: fromLocationId/locationId not found in trigger data");
        }

        decimal quantity = 1;
        if (data.TryGetValue("quantity", out var qtyStr) && decimal.TryParse(qtyStr, out var qty))
            quantity = qty;

        var putawayService = _serviceProvider.GetRequiredService<IPutawayService>();

        var request = new CreatePutawayTaskRequest
        {
            ProductId = productId,
            FromLocationId = fromLocationId,
            Quantity = quantity,
            BatchNumber = data.TryGetValue("batchNumber", out var batch) ? batch : null,
            SerialNumber = data.TryGetValue("serialNumber", out var serial) ? serial : null,
            Priority = config.Priority ?? 5,
            Notes = $"Auto-created by rule: {rule.Name}"
        };

        var result = await putawayService.CreateTaskAsync(rule.TenantId, request);

        if (result.Success && result.Data != null)
        {
            _logger.LogInformation("Created putaway task {TaskNumber} for rule {RuleId}", result.Data.TaskNumber, rule.Id);
            return ActionResult.Success($"Created putaway task: {result.Data.TaskNumber}", "PutawayTask", result.Data.Id);
        }

        return ActionResult.Failure($"Failed to create putaway task: {result.Message}");
    }

    private async Task<ActionResult> CreateCycleCountTaskAsync(
        AutomationRule rule,
        CreateTaskConfig config,
        object? triggerData,
        CancellationToken cancellationToken)
    {
        var data = ExtractTriggerData(triggerData);

        // Get warehouse from trigger data or config
        Guid warehouseId = Guid.Empty;
        if (data.TryGetValue("warehouseId", out var warehouseIdStr))
            Guid.TryParse(warehouseIdStr, out warehouseId);

        if (warehouseId == Guid.Empty)
        {
            // Try to get default warehouse for tenant
            var defaultWarehouse = await _dbContext.Warehouses
                .Where(w => w.TenantId == rule.TenantId && w.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            if (defaultWarehouse == null)
                return ActionResult.Failure("Cannot create cycle count: no warehouse found");

            warehouseId = defaultWarehouse.Id;
        }

        // Optional zone filter
        Guid? zoneId = null;
        if (data.TryGetValue("zoneId", out var zoneIdStr) && Guid.TryParse(zoneIdStr, out var parsedZoneId))
            zoneId = parsedZoneId;

        // Get location IDs if specified
        List<Guid>? locationIds = null;
        if (data.TryGetValue("locationId", out var locationIdStr) && Guid.TryParse(locationIdStr, out var locationId))
        {
            locationIds = new List<Guid> { locationId };
        }

        // Get product IDs if specified
        List<Guid>? productIds = null;
        if (data.TryGetValue("productId", out var productIdStr) && Guid.TryParse(productIdStr, out var productId))
        {
            productIds = new List<Guid> { productId };
        }

        // Determine count type based on trigger
        var countType = CountType.Scheduled;
        if (data.TryGetValue("countType", out var countTypeStr))
        {
            Enum.TryParse<CountType>(countTypeStr, out countType);
        }

        var cycleCountService = _serviceProvider.GetRequiredService<ICycleCountService>();

        var request = new CreateCycleCountRequest
        {
            Description = $"Auto-created by automation rule: {rule.Name}",
            WarehouseId = warehouseId,
            ZoneId = zoneId,
            CountType = countType,
            CountScope = locationIds?.Any() == true ? CountScope.Location : CountScope.Zone,
            ScheduledDate = DateTime.UtcNow,
            RequireBlindCount = false,
            AllowRecounts = true,
            MaxRecounts = 3,
            Notes = $"Created by automation rule '{rule.Name}' at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
            LocationIds = locationIds,
            ProductIds = productIds
        };

        var result = await cycleCountService.CreateCycleCountAsync(rule.TenantId, request);

        if (result.Success && result.Data != null)
        {
            _logger.LogInformation("Created cycle count session {CountNumber} for rule {RuleId}",
                result.Data.CountNumber, rule.Id);
            return ActionResult.Success($"Created cycle count: {result.Data.CountNumber}", "CycleCountSession", result.Data.Id);
        }

        return ActionResult.Failure($"Failed to create cycle count: {result.Message}");
    }

    #endregion

    #region UpdateEntity

    private async Task<ActionResult> ExecuteUpdateEntityStatusAsync(
        AutomationRule rule,
        RuleExecution execution,
        object? triggerData,
        CancellationToken cancellationToken)
    {
        var config = DeserializeConfig<UpdateEntityConfig>(rule.ActionConfigJson);
        if (config == null || string.IsNullOrEmpty(config.EntityType))
            return ActionResult.Failure("Invalid update configuration");

        var entityId = execution.TriggerEntityId;
        if (!entityId.HasValue)
        {
            var data = ExtractTriggerData(triggerData);
            if (data.TryGetValue("id", out var idStr) && Guid.TryParse(idStr, out var id))
                entityId = id;
        }

        if (!entityId.HasValue)
            return ActionResult.Failure("Cannot update entity: entityId not found");

        var newStatus = ReplacePlaceholders(config.Value ?? "", triggerData);

        // Update based on entity type
        var entityType = config.EntityType.ToLowerInvariant();
        var updated = false;

        switch (entityType)
        {
            case "salesorder":
                var order = await _dbContext.SalesOrders
                    .FirstOrDefaultAsync(o => o.TenantId == rule.TenantId && o.Id == entityId.Value, cancellationToken);
                if (order != null && Enum.TryParse<Common.Enums.SalesOrderStatus>(newStatus, out var orderStatus))
                {
                    order.Status = orderStatus;
                    order.UpdatedAt = DateTime.UtcNow;
                    updated = true;
                }
                break;

            case "purchaseorder":
                var po = await _dbContext.PurchaseOrders
                    .FirstOrDefaultAsync(o => o.TenantId == rule.TenantId && o.Id == entityId.Value, cancellationToken);
                if (po != null && Enum.TryParse<Common.Enums.PurchaseOrderStatus>(newStatus, out var poStatus))
                {
                    po.Status = poStatus;
                    po.UpdatedAt = DateTime.UtcNow;
                    updated = true;
                }
                break;

            case "picktask":
                var task = await _dbContext.PickTasks
                    .FirstOrDefaultAsync(t => t.TenantId == rule.TenantId && t.Id == entityId.Value, cancellationToken);
                if (task != null && Enum.TryParse<Common.Enums.PickTaskStatus>(newStatus, out var taskStatus))
                {
                    task.Status = taskStatus;
                    task.UpdatedAt = DateTime.UtcNow;
                    updated = true;
                }
                break;

            default:
                return ActionResult.Failure($"Unsupported entity type for status update: {entityType}");
        }

        if (updated)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated {EntityType} {EntityId} status to {NewStatus} for rule {RuleId}",
                entityType, entityId, newStatus, rule.Id);
            return ActionResult.Success($"Updated {entityType} status to {newStatus}");
        }

        return ActionResult.Failure($"Failed to update {entityType} status");
    }

    private async Task<ActionResult> ExecuteUpdateEntityFieldAsync(
        AutomationRule rule,
        RuleExecution execution,
        object? triggerData,
        CancellationToken cancellationToken)
    {
        var config = DeserializeConfig<UpdateEntityConfig>(rule.ActionConfigJson);
        if (config == null || string.IsNullOrEmpty(config.EntityType) || string.IsNullOrEmpty(config.Field))
            return ActionResult.Failure("Invalid update configuration");

        var entityId = execution.TriggerEntityId;
        if (!entityId.HasValue)
        {
            var data = ExtractTriggerData(triggerData);
            if (data.TryGetValue("id", out var idStr) && Guid.TryParse(idStr, out var id))
                entityId = id;
        }

        if (!entityId.HasValue)
            return ActionResult.Failure("Cannot update entity: entityId not found");

        var newValue = ReplacePlaceholders(config.Value ?? "", triggerData);
        var entityType = config.EntityType.ToLowerInvariant();
        var field = config.Field;

        // For safety, only allow updating specific fields
        var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Priority", "Notes", "InternalNotes", "Tags", "CustomField1", "CustomField2"
        };

        if (!allowedFields.Contains(field))
        {
            _logger.LogWarning("Attempted to update restricted field {Field} for rule {RuleId}", field, rule.Id);
            return ActionResult.Failure($"Field '{field}' cannot be updated via automation");
        }

        // Generic field update using reflection (simplified)
        object? entity = entityType switch
        {
            "salesorder" => await _dbContext.SalesOrders
                .FirstOrDefaultAsync(o => o.TenantId == rule.TenantId && o.Id == entityId.Value, cancellationToken),
            "purchaseorder" => await _dbContext.PurchaseOrders
                .FirstOrDefaultAsync(o => o.TenantId == rule.TenantId && o.Id == entityId.Value, cancellationToken),
            "product" => await _dbContext.Products
                .FirstOrDefaultAsync(p => p.TenantId == rule.TenantId && p.Id == entityId.Value, cancellationToken),
            _ => null
        };

        if (entity == null)
            return ActionResult.Failure($"Entity {entityType} with id {entityId} not found");

        var property = entity.GetType().GetProperty(field);
        if (property == null || !property.CanWrite)
            return ActionResult.Failure($"Property {field} not found or is read-only");

        try
        {
            var convertedValue = Convert.ChangeType(newValue, property.PropertyType);
            property.SetValue(entity, convertedValue);

            // Update timestamp
            var updatedAtProp = entity.GetType().GetProperty("UpdatedAt");
            updatedAtProp?.SetValue(entity, DateTime.UtcNow);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated {EntityType}.{Field} to {Value} for rule {RuleId}",
                entityType, field, newValue, rule.Id);
            return ActionResult.Success($"Updated {entityType}.{field}");
        }
        catch (Exception ex)
        {
            return ActionResult.Failure($"Failed to update field: {ex.Message}");
        }
    }

    #endregion

    #region CreateAdjustment

    private async Task<ActionResult> ExecuteCreateAdjustmentAsync(
        AutomationRule rule,
        RuleExecution execution,
        object? triggerData,
        CancellationToken cancellationToken)
    {
        var config = DeserializeConfig<CreateAdjustmentConfig>(rule.ActionConfigJson);
        if (config == null)
            return ActionResult.Failure("Invalid adjustment configuration");

        var data = ExtractTriggerData(triggerData);

        if (!data.TryGetValue("productId", out var productIdStr) || !Guid.TryParse(productIdStr, out var productId))
            return ActionResult.Failure("Cannot create adjustment: productId not found");

        if (!data.TryGetValue("locationId", out var locationIdStr) || !Guid.TryParse(locationIdStr, out var locationId))
            return ActionResult.Failure("Cannot create adjustment: locationId not found");

        if (!data.TryGetValue("warehouseId", out var warehouseIdStr) || !Guid.TryParse(warehouseIdStr, out var warehouseId))
        {
            // Try to get warehouse from location
            var location = await _dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == locationId, cancellationToken);
            if (location == null)
                return ActionResult.Failure("Cannot create adjustment: warehouseId not found and location not found");
            warehouseId = location.WarehouseId;
        }

        decimal quantity = config.Quantity ?? 0;
        if (data.TryGetValue("adjustmentQuantity", out var qtyStr) && decimal.TryParse(qtyStr, out var qty))
            quantity = qty;

        if (quantity == 0)
            return ActionResult.Failure("Adjustment quantity is zero");

        // Get product SKU
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
            return ActionResult.Failure("Cannot create adjustment: product not found");

        var adjustmentsService = _serviceProvider.GetRequiredService<IAdjustmentsService>();

        // Get a system user for automation
        var systemUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.TenantId == rule.TenantId && u.UserName == "system", cancellationToken);
        var userId = systemUser?.Id ?? Guid.Empty;

        data.TryGetValue("batchNumber", out var batchNumber);
        data.TryGetValue("serialNumber", out var serialNumber);

        var adjustmentLine = new CreateAdjustmentLineRequest(
            ProductId: productId,
            Sku: product.Sku,
            LocationId: locationId,
            BatchNumber: batchNumber,
            SerialNumber: serialNumber,
            QuantityAdjustment: quantity,
            UnitCost: null,
            ReasonCodeId: null,
            ReasonNotes: $"Auto-created by rule: {rule.Name}"
        );

        var request = new CreateStockAdjustmentRequest(
            WarehouseId: warehouseId,
            AdjustmentType: AdjustmentType.Correction,
            ReasonCodeId: null,
            ReasonNotes: config.ReasonCode ?? "AUTOMATION",
            SourceDocumentType: "AutomationRule",
            SourceDocumentId: rule.Id,
            SourceDocumentNumber: rule.Name,
            Notes: $"Auto-created by rule: {rule.Name}",
            Lines: new List<CreateAdjustmentLineRequest> { adjustmentLine }
        );

        var result = await adjustmentsService.QuickAdjustAsync(rule.TenantId, userId, request);

        if (result.Success && result.Data != null)
        {
            _logger.LogInformation("Created adjustment {AdjustmentNumber} for rule {RuleId}",
                result.Data.AdjustmentNumber, rule.Id);
            return ActionResult.Success($"Created adjustment: {result.Data.AdjustmentNumber}", "StockAdjustment", result.Data.Id);
        }

        return ActionResult.Failure($"Failed to create adjustment: {result.Message}");
    }

    #endregion

    #region CreateTransfer

    private async Task<ActionResult> ExecuteCreateTransferAsync(
        AutomationRule rule,
        RuleExecution execution,
        object? triggerData,
        CancellationToken cancellationToken)
    {
        var config = DeserializeConfig<CreateTransferConfig>(rule.ActionConfigJson);
        if (config == null)
            return ActionResult.Failure("Invalid transfer configuration");

        var data = ExtractTriggerData(triggerData);

        if (!data.TryGetValue("productId", out var productIdStr) || !Guid.TryParse(productIdStr, out var productId))
            return ActionResult.Failure("Cannot create transfer: productId not found");

        Guid fromLocationId = config.FromLocationId ?? Guid.Empty;
        Guid toLocationId = config.ToLocationId ?? Guid.Empty;

        if (fromLocationId == Guid.Empty && data.TryGetValue("fromLocationId", out var fromStr))
            Guid.TryParse(fromStr, out fromLocationId);
        if (toLocationId == Guid.Empty && data.TryGetValue("toLocationId", out var toStr))
            Guid.TryParse(toStr, out toLocationId);

        if (fromLocationId == Guid.Empty || toLocationId == Guid.Empty)
            return ActionResult.Failure("Cannot create transfer: fromLocationId or toLocationId not found");

        decimal quantity = config.Quantity ?? 0;
        if (data.TryGetValue("quantity", out var qtyStr) && decimal.TryParse(qtyStr, out var qty))
            quantity = qty;

        if (quantity <= 0)
            return ActionResult.Failure("Transfer quantity must be greater than zero");

        // Get warehouse IDs from locations
        var fromLocation = await _dbContext.Locations.FirstOrDefaultAsync(l => l.Id == fromLocationId, cancellationToken);
        var toLocation = await _dbContext.Locations.FirstOrDefaultAsync(l => l.Id == toLocationId, cancellationToken);

        if (fromLocation == null || toLocation == null)
            return ActionResult.Failure("Cannot create transfer: source or destination location not found");

        // Get product SKU
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
            return ActionResult.Failure("Cannot create transfer: product not found");

        var transfersService = _serviceProvider.GetRequiredService<ITransfersService>();

        var systemUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.TenantId == rule.TenantId && u.UserName == "system", cancellationToken);
        var userId = systemUser?.Id ?? Guid.Empty;

        data.TryGetValue("batchNumber", out var batchNumber);
        data.TryGetValue("serialNumber", out var serialNumber);

        var transferLine = new CreateTransferLineRequest(
            ProductId: productId,
            Sku: product.Sku,
            FromLocationId: fromLocationId,
            ToLocationId: toLocationId,
            BatchNumber: batchNumber,
            SerialNumber: serialNumber,
            QuantityRequested: quantity,
            Notes: null
        );

        var request = new CreateStockTransferRequest(
            TransferType: fromLocation.WarehouseId == toLocation.WarehouseId
                ? TransferType.Internal
                : TransferType.InterWarehouse,
            FromWarehouseId: fromLocation.WarehouseId,
            FromZoneId: fromLocation.ZoneId,
            ToWarehouseId: toLocation.WarehouseId,
            ToZoneId: toLocation.ZoneId,
            Priority: TransferPriority.Normal,
            ScheduledDate: null,
            RequiredByDate: null,
            ReasonCodeId: null,
            ReasonNotes: null,
            SourceDocumentType: "AutomationRule",
            SourceDocumentId: rule.Id,
            SourceDocumentNumber: rule.Name,
            Notes: $"Auto-created by rule: {rule.Name}",
            Lines: new List<CreateTransferLineRequest> { transferLine }
        );

        var result = await transfersService.CreateTransferAsync(rule.TenantId, userId, request);

        if (result.Success && result.Data != null)
        {
            _logger.LogInformation("Created transfer {TransferNumber} for rule {RuleId}",
                result.Data.TransferNumber, rule.Id);
            return ActionResult.Success($"Created transfer: {result.Data.TransferNumber}", "StockTransfer", result.Data.Id);
        }

        return ActionResult.Failure($"Failed to create transfer: {result.Message}");
    }

    #endregion

    #region Report & Sync

    private async Task<ActionResult> ExecuteGenerateReportAsync(
        AutomationRule rule,
        RuleExecution execution,
        object? triggerData,
        CancellationToken cancellationToken)
    {
        var config = DeserializeConfig<GenerateReportConfig>(rule.ActionConfigJson);
        if (config == null || string.IsNullOrEmpty(config.ReportType))
            return ActionResult.Failure("Invalid report configuration - ReportType is required");

        var data = ExtractTriggerData(triggerData);
        var reportsService = _serviceProvider.GetRequiredService<IReportsService>();

        // Get warehouse ID from config or trigger data
        Guid? warehouseId = null;
        if (config.Parameters?.TryGetValue("warehouseId", out var whIdStr) == true && Guid.TryParse(whIdStr, out var whId))
            warehouseId = whId;
        else if (data.TryGetValue("warehouseId", out var dataWhIdStr) && Guid.TryParse(dataWhIdStr, out var dataWhId))
            warehouseId = dataWhId;

        // Get date range from config
        var dateFrom = DateTime.UtcNow.AddDays(-30);
        var dateTo = DateTime.UtcNow;

        if (config.Parameters?.TryGetValue("dateFrom", out var dateFromStr) == true && DateTime.TryParse(dateFromStr, out var parsedDateFrom))
            dateFrom = parsedDateFrom;
        if (config.Parameters?.TryGetValue("dateTo", out var dateToStr) == true && DateTime.TryParse(dateToStr, out var parsedDateTo))
            dateTo = parsedDateTo;

        object? reportData = null;
        var reportType = config.ReportType.ToLowerInvariant();

        try
        {
            // Generate report based on type
            switch (reportType)
            {
                case "inventorysummary":
                case "inventory":
                    var invResult = await reportsService.GetInventorySummaryAsync(rule.TenantId, warehouseId);
                    if (invResult.Success) reportData = invResult.Data;
                    break;

                case "stockmovement":
                case "movement":
                    var movResult = await reportsService.GetStockMovementReportAsync(
                        rule.TenantId,
                        new ReportDateRangeFilter { DateFrom = dateFrom, DateTo = dateTo, WarehouseId = warehouseId });
                    if (movResult.Success) reportData = movResult.Data;
                    break;

                case "orderfulfillment":
                case "fulfillment":
                    var fulfResult = await reportsService.GetOrderFulfillmentReportAsync(
                        rule.TenantId,
                        new ReportDateRangeFilter { DateFrom = dateFrom, DateTo = dateTo, WarehouseId = warehouseId });
                    if (fulfResult.Success) reportData = fulfResult.Data;
                    break;

                case "receiving":
                    var recResult = await reportsService.GetReceivingReportAsync(
                        rule.TenantId,
                        new ReportDateRangeFilter { DateFrom = dateFrom, DateTo = dateTo, WarehouseId = warehouseId });
                    if (recResult.Success) reportData = recResult.Data;
                    break;

                case "warehouseutilization":
                case "utilization":
                    if (!warehouseId.HasValue)
                    {
                        // Get first warehouse
                        var wh = await _dbContext.Warehouses
                            .Where(w => w.TenantId == rule.TenantId && w.IsActive)
                            .FirstOrDefaultAsync(cancellationToken);
                        warehouseId = wh?.Id;
                    }

                    if (warehouseId.HasValue)
                    {
                        var utilResult = await reportsService.GetWarehouseUtilizationAsync(rule.TenantId, warehouseId.Value);
                        if (utilResult.Success) reportData = utilResult.Data;
                    }
                    break;

                default:
                    return ActionResult.Failure($"Unknown report type: {config.ReportType}");
            }

            if (reportData == null)
            {
                return ActionResult.Failure($"Failed to generate report: {config.ReportType}");
            }

            // Serialize report data
            var reportJson = JsonSerializer.Serialize(reportData, JsonOptions);

            // Store in execution result
            execution.ResultData = JsonSerializer.Serialize(new
            {
                reportType = config.ReportType,
                generatedAt = DateTime.UtcNow,
                dateRange = new { from = dateFrom, to = dateTo },
                warehouseId,
                dataLength = reportJson.Length
            }, JsonOptions);

            // If email is configured, send the report
            if (config.EmailReport && config.EmailAddresses?.Any() == true)
            {
                var emailService = _serviceProvider.GetRequiredService<IEmailService>();
                var emailMessage = new EmailMessage
                {
                    To = config.EmailAddresses,
                    Subject = $"[SmartWMS] {config.ReportType} Report - {DateTime.UtcNow:yyyy-MM-dd}",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2>{config.ReportType} Report</h2>
                            <p>Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                            <p>Period: {dateFrom:yyyy-MM-dd} - {dateTo:yyyy-MM-dd}</p>
                            <hr/>
                            <p>Report generated by automation rule: <strong>{rule.Name}</strong></p>
                            <pre style='background: #f5f5f5; padding: 10px; overflow: auto;'>{reportJson[..Math.Min(reportJson.Length, 5000)]}{(reportJson.Length > 5000 ? "\n... (truncated)" : "")}</pre>
                        </body>
                        </html>",
                    IsHtml = true
                };

                await emailService.SendEmailAsync(emailMessage, cancellationToken);
                _logger.LogInformation("Report {ReportType} emailed to {Recipients} for rule {RuleId}",
                    config.ReportType, string.Join(", ", config.EmailAddresses), rule.Id);
            }

            _logger.LogInformation("Generated report {ReportType} for rule {RuleId}", config.ReportType, rule.Id);
            return ActionResult.Success($"Report generated: {config.ReportType}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate report {ReportType} for rule {RuleId}", config.ReportType, rule.Id);
            return ActionResult.Failure($"Failed to generate report: {ex.Message}");
        }
    }

    private async Task<ActionResult> ExecuteTriggerSyncAsync(
        AutomationRule rule,
        RuleExecution execution,
        object? triggerData,
        CancellationToken cancellationToken)
    {
        var config = DeserializeConfig<TriggerSyncConfig>(rule.ActionConfigJson);
        if (config == null || config.IntegrationId == Guid.Empty)
            return ActionResult.Failure("Invalid sync configuration - IntegrationId is required");

        var integrationsService = _serviceProvider.GetRequiredService<IIntegrationsService>();

        try
        {
            // Verify integration exists and is active
            var integrationResult = await integrationsService.GetIntegrationByIdAsync(rule.TenantId, config.IntegrationId);

            if (!integrationResult.Success || integrationResult.Data == null)
            {
                return ActionResult.Failure($"Integration not found: {config.IntegrationId}");
            }

            var integration = integrationResult.Data;

            if (!integration.IsActive)
            {
                _logger.LogWarning("Cannot trigger sync for inactive integration {IntegrationId} in rule {RuleId}",
                    config.IntegrationId, rule.Id);
                return ActionResult.Failure("Integration is not active");
            }

            // Trigger the sync
            var syncResult = await integrationsService.TriggerSyncAsync(rule.TenantId, config.IntegrationId);

            if (syncResult.Success)
            {
                _logger.LogInformation(
                    "Sync triggered for integration {IntegrationId} ({IntegrationName}), EntityType={EntityType}, Direction={Direction}, Rule={RuleId}",
                    config.IntegrationId, integration.Name, config.EntityType, config.Direction, rule.Id);

                execution.ResultData = JsonSerializer.Serialize(new
                {
                    integrationId = config.IntegrationId,
                    integrationName = integration.Name,
                    entityType = config.EntityType,
                    direction = config.Direction,
                    triggeredAt = DateTime.UtcNow
                }, JsonOptions);

                return ActionResult.Success($"Sync triggered: {integration.Name} - {config.EntityType} ({config.Direction})");
            }
            else
            {
                return ActionResult.Failure($"Failed to trigger sync: {syncResult.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger sync for integration {IntegrationId} in rule {RuleId}",
                config.IntegrationId, rule.Id);
            return ActionResult.Failure($"Failed to trigger sync: {ex.Message}");
        }
    }

    #endregion

    #region Helper Methods

    private T? DeserializeConfig<T>(string? json) where T : class
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize config: {Json}", json);
            return null;
        }
    }

    private string ReplacePlaceholders(string template, object? data)
    {
        if (string.IsNullOrEmpty(template) || data == null)
            return template;

        var dict = ExtractTriggerData(data);

        foreach (var kvp in dict)
        {
            template = template.Replace($"{{{{{kvp.Key}}}}}", kvp.Value, StringComparison.OrdinalIgnoreCase);
            template = template.Replace($"${{{kvp.Key}}}", kvp.Value, StringComparison.OrdinalIgnoreCase);
            template = template.Replace($"{{{kvp.Key}}}", kvp.Value, StringComparison.OrdinalIgnoreCase);
        }

        return template;
    }

    private Dictionary<string, string> ExtractTriggerData(object? data)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (data == null)
            return dict;

        if (data is JsonElement jsonElement)
        {
            ExtractFromJsonElement(jsonElement, dict, "");
        }
        else if (data is string jsonString)
        {
            try
            {
                var element = JsonSerializer.Deserialize<JsonElement>(jsonString);
                ExtractFromJsonElement(element, dict, "");
            }
            catch { }
        }
        else
        {
            // Use reflection for objects
            foreach (var prop in data.GetType().GetProperties())
            {
                var value = prop.GetValue(data);
                if (value != null)
                {
                    dict[prop.Name] = value.ToString() ?? "";
                }
            }
        }

        return dict;
    }

    private void ExtractFromJsonElement(JsonElement element, Dictionary<string, string> dict, string prefix)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                    if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        ExtractFromJsonElement(prop.Value, dict, key);
                    }
                    else
                    {
                        dict[key] = prop.Value.ToString();
                        // Also add without prefix for simple access
                        if (!dict.ContainsKey(prop.Name))
                            dict[prop.Name] = prop.Value.ToString();
                    }
                }
                break;

            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    ExtractFromJsonElement(item, dict, $"{prefix}[{index}]");
                    index++;
                }
                break;

            default:
                if (!string.IsNullOrEmpty(prefix))
                    dict[prefix] = element.ToString();
                break;
        }
    }

    #endregion
}

#region Action Result

public class ActionResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? CreatedEntityType { get; init; }
    public Guid? CreatedEntityId { get; init; }
    public object? Data { get; init; }

    public static ActionResult Success(string message, string? createdEntityType = null, Guid? createdEntityId = null)
        => new() { IsSuccess = true, Message = message, CreatedEntityType = createdEntityType, CreatedEntityId = createdEntityId };

    public static ActionResult Failure(string message)
        => new() { IsSuccess = false, Message = message };
}

#endregion
