using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Fulfillment.DTOs;
using SmartWMS.API.Modules.Fulfillment.Models;

namespace SmartWMS.API.Modules.Fulfillment.Services;

public class PickTasksService : IPickTasksService
{
    private readonly ApplicationDbContext _context;

    public PickTasksService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PaginatedResult<PickTaskDto>>> GetTasksAsync(
        Guid tenantId,
        PickTaskFilters? filters = null,
        int page = 1,
        int pageSize = 25)
    {
        var query = _context.PickTasks
            .Include(t => t.Batch)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.ToLocation)
            .Where(t => t.TenantId == tenantId)
            .AsQueryable();

        if (filters != null)
        {
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var search = filters.Search.ToLower();
                query = query.Where(t =>
                    t.TaskNumber.ToLower().Contains(search) ||
                    t.Sku.ToLower().Contains(search));
            }

            if (filters.Status.HasValue)
                query = query.Where(t => t.Status == filters.Status.Value);

            if (filters.BatchId.HasValue)
                query = query.Where(t => t.BatchId == filters.BatchId.Value);

            if (filters.OrderId.HasValue)
                query = query.Where(t => t.OrderId == filters.OrderId.Value);

            if (filters.AssignedToUserId.HasValue)
                query = query.Where(t => t.AssignedToUserId == filters.AssignedToUserId.Value);

            if (filters.FromLocationId.HasValue)
                query = query.Where(t => t.FromLocationId == filters.FromLocationId.Value);

            if (filters.WarehouseId.HasValue)
                query = query.Where(t => t.FromLocation.WarehouseId == filters.WarehouseId.Value);

            if (filters.IsAssigned.HasValue)
                query = filters.IsAssigned.Value
                    ? query.Where(t => t.AssignedToUserId != null)
                    : query.Where(t => t.AssignedToUserId == null);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.Sequence)
            .ThenBy(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => MapToDto(t))
            .ToListAsync();

        var result = new PaginatedResult<PickTaskDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<PickTaskDto>>.Ok(result);
    }

    public async Task<ApiResponse<PickTaskDto>> GetTaskByIdAsync(Guid tenantId, Guid id)
    {
        var task = await _context.PickTasks
            .Include(t => t.Batch)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.ToLocation)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (task == null)
            return ApiResponse<PickTaskDto>.Fail("Pick task not found");

        return ApiResponse<PickTaskDto>.Ok(MapToDto(task));
    }

    public async Task<ApiResponse<PickTaskDto>> GetTaskByNumberAsync(Guid tenantId, string taskNumber)
    {
        var task = await _context.PickTasks
            .Include(t => t.Batch)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.ToLocation)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.TaskNumber == taskNumber);

        if (task == null)
            return ApiResponse<PickTaskDto>.Fail("Pick task not found");

        return ApiResponse<PickTaskDto>.Ok(MapToDto(task));
    }

    public async Task<ApiResponse<PickTaskDto>> CreateTaskAsync(Guid tenantId, CreatePickTaskRequest request)
    {
        // Validate order
        var order = await _context.SalesOrders
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == request.OrderId);

        if (order == null)
            return ApiResponse<PickTaskDto>.Fail("Order not found");

        // Validate order line
        var orderLine = await _context.SalesOrderLines
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.OrderLineId && l.OrderId == request.OrderId);

        if (orderLine == null)
            return ApiResponse<PickTaskDto>.Fail("Order line not found");

        // Validate product
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ProductId);

        if (product == null)
            return ApiResponse<PickTaskDto>.Fail("Product not found");

        // Validate from location
        var fromLocation = await _context.Locations
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.FromLocationId);

        if (fromLocation == null)
            return ApiResponse<PickTaskDto>.Fail("From location not found");

        // Validate to location if provided
        if (request.ToLocationId.HasValue)
        {
            var toLocation = await _context.Locations
                .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.ToLocationId.Value);

            if (toLocation == null)
                return ApiResponse<PickTaskDto>.Fail("To location not found");
        }

        // Validate batch if provided
        if (request.BatchId.HasValue)
        {
            var batch = await _context.FulfillmentBatches
                .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == request.BatchId.Value);

            if (batch == null)
                return ApiResponse<PickTaskDto>.Fail("Batch not found");
        }

        var taskNumber = await GenerateTaskNumberAsync(tenantId);

        var task = new PickTask
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TaskNumber = taskNumber,
            BatchId = request.BatchId,
            OrderId = request.OrderId,
            OrderLineId = request.OrderLineId,
            ProductId = request.ProductId,
            Sku = product.Sku,
            FromLocationId = request.FromLocationId,
            ToLocationId = request.ToLocationId,
            QuantityRequired = request.QuantityRequired,
            Status = PickTaskStatus.Pending,
            Priority = request.Priority,
            AssignedToUserId = request.AssignedToUserId,
            CreatedAt = DateTime.UtcNow
        };

        if (request.AssignedToUserId.HasValue)
        {
            task.Status = PickTaskStatus.Assigned;
        }

        _context.PickTasks.Add(task);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(task).Reference(t => t.Product).LoadAsync();
        await _context.Entry(task).Reference(t => t.FromLocation).LoadAsync();

        return ApiResponse<PickTaskDto>.Ok(MapToDto(task), "Pick task created successfully");
    }

    public async Task<ApiResponse> DeleteTaskAsync(Guid tenantId, Guid id)
    {
        var task = await _context.PickTasks
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (task == null)
            return ApiResponse.Fail("Pick task not found");

        if (task.Status != PickTaskStatus.Pending && task.Status != PickTaskStatus.Assigned)
            return ApiResponse.Fail("Can only delete tasks in Pending or Assigned status");

        _context.PickTasks.Remove(task);
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Pick task deleted successfully");
    }

    public async Task<ApiResponse<PickTaskDto>> AssignTaskAsync(Guid tenantId, Guid id, AssignPickTaskRequest request)
    {
        var task = await _context.PickTasks
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (task == null)
            return ApiResponse<PickTaskDto>.Fail("Pick task not found");

        if (task.Status != PickTaskStatus.Pending && task.Status != PickTaskStatus.Assigned)
            return ApiResponse<PickTaskDto>.Fail($"Cannot assign task in '{task.Status}' status");

        if (request.UserId.HasValue)
        {
            var user = await _context.Users.FindAsync(request.UserId.Value);
            if (user == null)
                return ApiResponse<PickTaskDto>.Fail("User not found");

            task.AssignedToUserId = request.UserId;
            task.Status = PickTaskStatus.Assigned;
        }
        else
        {
            task.AssignedToUserId = null;
            task.Status = PickTaskStatus.Pending;
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<PickTaskDto>.Ok(MapToDto(task), request.UserId.HasValue ? "Task assigned" : "Task unassigned");
    }

    public async Task<ApiResponse<PickTaskDto>> StartTaskAsync(Guid tenantId, Guid id, StartPickTaskRequest? request = null)
    {
        var task = await _context.PickTasks
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.Batch)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (task == null)
            return ApiResponse<PickTaskDto>.Fail("Pick task not found");

        if (task.Status != PickTaskStatus.Assigned)
            return ApiResponse<PickTaskDto>.Fail($"Cannot start task in '{task.Status}' status");

        task.Status = PickTaskStatus.InProgress;
        task.StartedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        // Update batch status if applicable
        if (task.BatchId.HasValue && task.Batch != null)
        {
            if (task.Batch.Status == FulfillmentStatus.Released)
            {
                task.Batch.Status = FulfillmentStatus.InProgress;
                task.Batch.StartedAt = DateTime.UtcNow;
                task.Batch.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return ApiResponse<PickTaskDto>.Ok(MapToDto(task), "Task started");
    }

    public async Task<ApiResponse<PickTaskDto>> ConfirmPickAsync(Guid tenantId, Guid id, ConfirmPickRequest request)
    {
        var task = await _context.PickTasks
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.Batch)
            .Include(t => t.OrderLine)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (task == null)
            return ApiResponse<PickTaskDto>.Fail("Pick task not found");

        if (task.Status != PickTaskStatus.InProgress)
            return ApiResponse<PickTaskDto>.Fail($"Cannot confirm pick for task in '{task.Status}' status");

        if (request.QuantityPicked > task.QuantityRequired - task.QuantityPicked)
            return ApiResponse<PickTaskDto>.Fail("Picked quantity exceeds required quantity");

        task.QuantityPicked += request.QuantityPicked;
        task.PickedBatchNumber = request.BatchNumber;
        task.PickedSerialNumber = request.SerialNumber;

        if (request.ToLocationId.HasValue)
            task.ToLocationId = request.ToLocationId;

        // Check if complete
        if (task.QuantityPicked >= task.QuantityRequired)
        {
            task.Status = PickTaskStatus.Complete;
            task.CompletedAt = DateTime.UtcNow;
        }

        task.UpdatedAt = DateTime.UtcNow;

        // Update order line picked quantity
        if (task.OrderLine != null)
        {
            task.OrderLine.QuantityPicked += request.QuantityPicked;
            task.OrderLine.UpdatedAt = DateTime.UtcNow;
        }

        // Update batch picked quantity
        if (task.Batch != null)
        {
            task.Batch.PickedQuantity += request.QuantityPicked;
            task.Batch.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return ApiResponse<PickTaskDto>.Ok(MapToDto(task), "Pick confirmed");
    }

    public async Task<ApiResponse<PickTaskDto>> ShortPickAsync(Guid tenantId, Guid id, ShortPickRequest request)
    {
        var task = await _context.PickTasks
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.Batch)
            .Include(t => t.OrderLine)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (task == null)
            return ApiResponse<PickTaskDto>.Fail("Pick task not found");

        if (task.Status != PickTaskStatus.InProgress)
            return ApiResponse<PickTaskDto>.Fail($"Cannot short pick task in '{task.Status}' status");

        if (request.QuantityPicked + request.QuantityShortPicked > task.QuantityRequired - task.QuantityPicked)
            return ApiResponse<PickTaskDto>.Fail("Total quantity exceeds required");

        task.QuantityPicked += request.QuantityPicked;
        task.QuantityShortPicked += request.QuantityShortPicked;
        task.ShortPickReason = request.Reason;
        task.Status = PickTaskStatus.ShortPicked;
        task.CompletedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        // Update order line picked quantity
        if (task.OrderLine != null)
        {
            task.OrderLine.QuantityPicked += request.QuantityPicked;
            task.OrderLine.UpdatedAt = DateTime.UtcNow;
        }

        // Update batch
        if (task.Batch != null)
        {
            task.Batch.PickedQuantity += request.QuantityPicked;
            task.Batch.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return ApiResponse<PickTaskDto>.Ok(MapToDto(task), "Short pick recorded");
    }

    public async Task<ApiResponse<PickTaskDto>> CancelTaskAsync(Guid tenantId, Guid id, string? reason = null)
    {
        var task = await _context.PickTasks
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (task == null)
            return ApiResponse<PickTaskDto>.Fail("Pick task not found");

        if (task.Status == PickTaskStatus.Complete || task.Status == PickTaskStatus.Cancelled)
            return ApiResponse<PickTaskDto>.Fail($"Cannot cancel task in '{task.Status}' status");

        task.Status = PickTaskStatus.Cancelled;
        task.ShortPickReason = reason;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<PickTaskDto>.Ok(MapToDto(task), "Task cancelled");
    }

    public async Task<ApiResponse<IEnumerable<PickTaskDto>>> GetMyTasksAsync(Guid tenantId, Guid userId)
    {
        var tasks = await _context.PickTasks
            .Include(t => t.Batch)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.ToLocation)
            .Where(t => t.TenantId == tenantId &&
                        t.AssignedToUserId == userId &&
                        (t.Status == PickTaskStatus.Assigned || t.Status == PickTaskStatus.InProgress))
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.Sequence)
            .Select(t => MapToDto(t))
            .ToListAsync();

        return ApiResponse<IEnumerable<PickTaskDto>>.Ok(tasks);
    }

    public async Task<ApiResponse<PickTaskDto>> GetNextTaskAsync(Guid tenantId, Guid userId, Guid? warehouseId = null)
    {
        var query = _context.PickTasks
            .Include(t => t.Batch)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.ToLocation)
            .Where(t => t.TenantId == tenantId &&
                        t.AssignedToUserId == userId &&
                        t.Status == PickTaskStatus.Assigned);

        if (warehouseId.HasValue)
        {
            query = query.Where(t => t.FromLocation.WarehouseId == warehouseId.Value);
        }

        var task = await query
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.Sequence)
            .FirstOrDefaultAsync();

        if (task == null)
            return ApiResponse<PickTaskDto>.Fail("No tasks available");

        return ApiResponse<PickTaskDto>.Ok(MapToDto(task));
    }

    #region Private Methods

    private async Task<string> GenerateTaskNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"PT-{today:yyyyMMdd}-";

        var lastTask = await _context.PickTasks
            .Where(t => t.TenantId == tenantId && t.TaskNumber.StartsWith(prefix))
            .OrderByDescending(t => t.TaskNumber)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (lastTask != null)
        {
            var lastNumber = lastTask.TaskNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumber, out var parsed))
            {
                sequence = parsed + 1;
            }
        }

        return $"{prefix}{sequence:D5}";
    }

    private static PickTaskDto MapToDto(PickTask task)
    {
        return new PickTaskDto
        {
            Id = task.Id,
            TaskNumber = task.TaskNumber,
            BatchId = task.BatchId,
            BatchNumber = task.Batch?.BatchNumber,
            OrderId = task.OrderId,
            OrderNumber = task.Order?.OrderNumber,
            OrderLineId = task.OrderLineId,
            ProductId = task.ProductId,
            Sku = task.Sku,
            ProductName = task.Product?.Name,
            FromLocationId = task.FromLocationId,
            FromLocationCode = task.FromLocation?.Code,
            ToLocationId = task.ToLocationId,
            ToLocationCode = task.ToLocation?.Code,
            QuantityRequired = task.QuantityRequired,
            QuantityPicked = task.QuantityPicked,
            QuantityShortPicked = task.QuantityShortPicked,
            PickedBatchNumber = task.PickedBatchNumber,
            PickedSerialNumber = task.PickedSerialNumber,
            Status = task.Status,
            Priority = task.Priority,
            Sequence = task.Sequence,
            AssignedToUserId = task.AssignedToUserId,
            StartedAt = task.StartedAt,
            CompletedAt = task.CompletedAt,
            ShortPickReason = task.ShortPickReason,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }

    #endregion
}
