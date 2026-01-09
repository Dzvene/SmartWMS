using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Packing.DTOs;
using SmartWMS.API.Modules.Packing.Models;

namespace SmartWMS.API.Modules.Packing.Services;

public class PackingService : IPackingService
{
    private readonly ApplicationDbContext _context;

    public PackingService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Packing Tasks - Queries

    public async Task<ApiResponse<PaginatedResult<PackingTaskListDto>>> GetTasksAsync(
        Guid tenantId, PackingTaskFilters filters, int page, int pageSize)
    {
        var query = _context.PackingTasks
            .Include(t => t.SalesOrder)
                .ThenInclude(so => so!.Customer)
            .Include(t => t.PackingStation)
            .Where(t => t.TenantId == tenantId);

        // Apply filters
        if (filters.Status.HasValue)
            query = query.Where(t => t.Status == filters.Status.Value);

        if (filters.AssignedToUserId.HasValue)
            query = query.Where(t => t.AssignedToUserId == filters.AssignedToUserId.Value);

        if (filters.SalesOrderId.HasValue)
            query = query.Where(t => t.SalesOrderId == filters.SalesOrderId.Value);

        if (filters.FulfillmentBatchId.HasValue)
            query = query.Where(t => t.FulfillmentBatchId == filters.FulfillmentBatchId.Value);

        if (filters.PackingStationId.HasValue)
            query = query.Where(t => t.PackingStationId == filters.PackingStationId.Value);

        if (filters.Priority.HasValue)
            query = query.Where(t => t.Priority == filters.Priority.Value);

        if (filters.Unassigned == true)
            query = query.Where(t => t.AssignedToUserId == null);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new PackingTaskListDto
            {
                Id = t.Id,
                TaskNumber = t.TaskNumber,
                SalesOrderNumber = t.SalesOrder != null ? t.SalesOrder.OrderNumber : null,
                CustomerName = t.SalesOrder != null && t.SalesOrder.Customer != null
                    ? t.SalesOrder.Customer.Name : null,
                PackingStationCode = t.PackingStation != null ? t.PackingStation.Code : null,
                AssignedToUserName = null,
                Status = t.Status,
                TotalItems = t.TotalItems,
                PackedItems = t.PackedItems,
                BoxCount = t.BoxCount,
                Priority = t.Priority,
                StartedAt = t.StartedAt,
                CompletedAt = t.CompletedAt,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        var result = new PaginatedResult<PackingTaskListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<PackingTaskListDto>>.Ok(result);
    }

    public async Task<ApiResponse<PackingTaskDto>> GetTaskByIdAsync(Guid tenantId, Guid id)
    {
        var task = await _context.PackingTasks
            .Include(t => t.SalesOrder)
                .ThenInclude(so => so!.Customer)
            .Include(t => t.FulfillmentBatch)
            .Include(t => t.PackingStation)
            .Include(t => t.Packages)
                .ThenInclude(p => p.Items)
                    .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (task == null)
            return ApiResponse<PackingTaskDto>.Fail("Packing task not found");

        return ApiResponse<PackingTaskDto>.Ok(MapToDto(task));
    }

    public async Task<ApiResponse<List<PackingTaskListDto>>> GetMyTasksAsync(Guid tenantId, Guid userId)
    {
        var tasks = await _context.PackingTasks
            .Include(t => t.SalesOrder)
                .ThenInclude(so => so!.Customer)
            .Include(t => t.PackingStation)
            .Where(t => t.TenantId == tenantId &&
                       t.AssignedToUserId == userId &&
                       (t.Status == PackingTaskStatus.Assigned || t.Status == PackingTaskStatus.InProgress))
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.AssignedAt)
            .Select(t => new PackingTaskListDto
            {
                Id = t.Id,
                TaskNumber = t.TaskNumber,
                SalesOrderNumber = t.SalesOrder != null ? t.SalesOrder.OrderNumber : null,
                CustomerName = t.SalesOrder != null && t.SalesOrder.Customer != null
                    ? t.SalesOrder.Customer.Name : null,
                PackingStationCode = t.PackingStation != null ? t.PackingStation.Code : null,
                AssignedToUserName = null,
                Status = t.Status,
                TotalItems = t.TotalItems,
                PackedItems = t.PackedItems,
                BoxCount = t.BoxCount,
                Priority = t.Priority,
                StartedAt = t.StartedAt,
                CompletedAt = t.CompletedAt,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<PackingTaskListDto>>.Ok(tasks);
    }

    public async Task<ApiResponse<PackingTaskDto?>> GetNextTaskAsync(Guid tenantId, Guid userId)
    {
        // First check for already assigned in-progress tasks
        var inProgressTask = await _context.PackingTasks
            .Include(t => t.SalesOrder)
                .ThenInclude(so => so!.Customer)
            .Include(t => t.PackingStation)
            .Include(t => t.Packages)
                .ThenInclude(p => p.Items)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId &&
                                     t.AssignedToUserId == userId &&
                                     t.Status == PackingTaskStatus.InProgress);

        if (inProgressTask != null)
            return ApiResponse<PackingTaskDto?>.Ok(MapToDto(inProgressTask));

        // Then check for assigned pending tasks
        var assignedTask = await _context.PackingTasks
            .Include(t => t.SalesOrder)
                .ThenInclude(so => so!.Customer)
            .Include(t => t.PackingStation)
            .Include(t => t.Packages)
                .ThenInclude(p => p.Items)
            .Where(t => t.TenantId == tenantId &&
                       t.AssignedToUserId == userId &&
                       t.Status == PackingTaskStatus.Assigned)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.AssignedAt)
            .FirstOrDefaultAsync();

        if (assignedTask != null)
            return ApiResponse<PackingTaskDto?>.Ok(MapToDto(assignedTask));

        return ApiResponse<PackingTaskDto?>.Ok(null, "No tasks available");
    }

    #endregion

    #region Packing Tasks - Creation

    public async Task<ApiResponse<PackingTaskDto>> CreateTaskAsync(Guid tenantId, CreatePackingTaskRequest request)
    {
        // Validate sales order
        var salesOrder = await _context.SalesOrders
            .Include(so => so.Lines)
            .FirstOrDefaultAsync(so => so.TenantId == tenantId && so.Id == request.SalesOrderId);

        if (salesOrder == null)
            return ApiResponse<PackingTaskDto>.Fail("Sales order not found");

        // Calculate total items
        var totalItems = (int)salesOrder.Lines.Sum(l => l.QuantityOrdered);

        var task = new PackingTask
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TaskNumber = await GenerateTaskNumberAsync(tenantId),
            SalesOrderId = request.SalesOrderId,
            FulfillmentBatchId = request.FulfillmentBatchId,
            PackingStationId = request.PackingStationId,
            TotalItems = totalItems,
            Priority = request.Priority,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        if (request.AssignToUserId.HasValue)
        {
            task.AssignedToUserId = request.AssignToUserId.Value;
            task.AssignedAt = DateTime.UtcNow;
            task.Status = PackingTaskStatus.Assigned;
        }

        _context.PackingTasks.Add(task);
        await _context.SaveChangesAsync();

        return await GetTaskByIdAsync(tenantId, task.Id);
    }

    public async Task<ApiResponse<List<PackingTaskDto>>> CreateFromFulfillmentAsync(
        Guid tenantId, CreatePackingFromFulfillmentRequest request)
    {
        var batch = await _context.FulfillmentBatches
            .Include(b => b.FulfillmentOrders)
                .ThenInclude(fo => fo.Order)
                    .ThenInclude(so => so!.Lines)
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == request.FulfillmentBatchId);

        if (batch == null)
            return ApiResponse<List<PackingTaskDto>>.Fail("Fulfillment batch not found");

        var tasks = new List<PackingTask>();

        foreach (var fulfillmentOrder in batch.FulfillmentOrders)
        {
            var totalItems = (int)(fulfillmentOrder.Order?.Lines.Sum(l => l.QuantityOrdered) ?? 0);

            var task = new PackingTask
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TaskNumber = await GenerateTaskNumberAsync(tenantId),
                SalesOrderId = fulfillmentOrder.OrderId,
                FulfillmentBatchId = batch.Id,
                PackingStationId = request.PackingStationId,
                TotalItems = totalItems,
                Priority = 5,
                CreatedAt = DateTime.UtcNow
            };

            tasks.Add(task);
        }

        _context.PackingTasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        var result = new List<PackingTaskDto>();
        foreach (var task in tasks)
        {
            var response = await GetTaskByIdAsync(tenantId, task.Id);
            if (response.Data != null)
                result.Add(response.Data);
        }

        return ApiResponse<List<PackingTaskDto>>.Ok(result, $"Created {tasks.Count} packing tasks from fulfillment batch");
    }

    #endregion

    #region Packing Tasks - Workflow

    public async Task<ApiResponse<PackingTaskDto>> AssignTaskAsync(
        Guid tenantId, Guid id, AssignPackingTaskRequest request)
    {
        var task = await _context.PackingTasks
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (task == null)
            return ApiResponse<PackingTaskDto>.Fail("Packing task not found");

        if (task.Status != PackingTaskStatus.Pending && task.Status != PackingTaskStatus.Assigned)
            return ApiResponse<PackingTaskDto>.Fail($"Cannot assign task in {task.Status} status");

        task.AssignedToUserId = request.UserId;
        task.AssignedAt = DateTime.UtcNow;
        task.Status = PackingTaskStatus.Assigned;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetTaskByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<PackingTaskDto>> StartTaskAsync(Guid tenantId, Guid id)
    {
        var task = await _context.PackingTasks
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (task == null)
            return ApiResponse<PackingTaskDto>.Fail("Packing task not found");

        if (task.Status != PackingTaskStatus.Assigned)
            return ApiResponse<PackingTaskDto>.Fail($"Cannot start task in {task.Status} status. Task must be Assigned first.");

        task.Status = PackingTaskStatus.InProgress;
        task.StartedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetTaskByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<PackingTaskDto>> CompleteTaskAsync(
        Guid tenantId, Guid id, CompletePackingTaskRequest request)
    {
        var task = await _context.PackingTasks
            .Include(t => t.Packages)
                .ThenInclude(p => p.Items)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (task == null)
            return ApiResponse<PackingTaskDto>.Fail("Packing task not found");

        if (task.Status != PackingTaskStatus.InProgress)
            return ApiResponse<PackingTaskDto>.Fail($"Cannot complete task in {task.Status} status");

        if (!task.Packages.Any())
            return ApiResponse<PackingTaskDto>.Fail("Task must have at least one package");

        // Update packed items count
        task.PackedItems = (int)task.Packages.Sum(p => p.Items.Sum(i => i.Quantity));
        task.BoxCount = task.Packages.Count;
        task.TotalWeightKg = task.Packages.Sum(p => p.WeightKg);

        task.Status = PackingTaskStatus.Complete;
        task.CompletedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.Notes))
            task.Notes = string.IsNullOrEmpty(task.Notes)
                ? request.Notes
                : $"{task.Notes}\n{request.Notes}";

        await _context.SaveChangesAsync();
        return await GetTaskByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<PackingTaskDto>> CancelTaskAsync(Guid tenantId, Guid id, string? reason)
    {
        var task = await _context.PackingTasks
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (task == null)
            return ApiResponse<PackingTaskDto>.Fail("Packing task not found");

        if (task.Status == PackingTaskStatus.Complete)
            return ApiResponse<PackingTaskDto>.Fail("Cannot cancel completed task");

        task.Status = PackingTaskStatus.Cancelled;
        task.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(reason))
            task.Notes = string.IsNullOrEmpty(task.Notes)
                ? $"Cancelled: {reason}"
                : $"{task.Notes}\nCancelled: {reason}";

        await _context.SaveChangesAsync();
        return await GetTaskByIdAsync(tenantId, id);
    }

    #endregion

    #region Packages

    public async Task<ApiResponse<PackageDto>> CreatePackageAsync(
        Guid tenantId, Guid taskId, CreatePackageRequest request)
    {
        var task = await _context.PackingTasks
            .Include(t => t.Packages)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == taskId);

        if (task == null)
            return ApiResponse<PackageDto>.Fail("Packing task not found");

        if (task.Status != PackingTaskStatus.InProgress)
            return ApiResponse<PackageDto>.Fail("Task must be in progress to add packages");

        var sequenceNumber = task.Packages.Count + 1;

        var package = new Package
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PackingTaskId = taskId,
            PackageNumber = $"{task.TaskNumber}-{sequenceNumber:D2}",
            SequenceNumber = sequenceNumber,
            LengthMm = request.LengthMm,
            WidthMm = request.WidthMm,
            HeightMm = request.HeightMm,
            WeightKg = request.WeightKg,
            PackagingType = request.PackagingType,
            CreatedAt = DateTime.UtcNow
        };

        _context.Packages.Add(package);
        task.BoxCount = sequenceNumber;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<PackageDto>.Ok(MapPackageToDto(package));
    }

    public async Task<ApiResponse<PackageDto>> AddItemToPackageAsync(
        Guid tenantId, Guid taskId, Guid packageId, AddItemToPackageRequest request)
    {
        var package = await _context.Packages
            .Include(p => p.PackingTask)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.TenantId == tenantId &&
                                     p.PackingTaskId == taskId &&
                                     p.Id == packageId);

        if (package == null)
            return ApiResponse<PackageDto>.Fail("Package not found");

        if (package.PackingTask?.Status != PackingTaskStatus.InProgress)
            return ApiResponse<PackageDto>.Fail("Task must be in progress to add items");

        // Validate product
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ProductId);

        if (product == null)
            return ApiResponse<PackageDto>.Fail("Product not found");

        var item = new PackageItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PackageId = packageId,
            ProductId = request.ProductId,
            Sku = request.Sku,
            Quantity = request.Quantity,
            BatchNumber = request.BatchNumber,
            SerialNumber = request.SerialNumber,
            CreatedAt = DateTime.UtcNow
        };

        _context.PackageItems.Add(item);

        // Update task packed items
        var task = await _context.PackingTasks
            .Include(t => t.Packages)
                .ThenInclude(p => p.Items)
            .FirstAsync(t => t.Id == taskId);

        task.PackedItems = (int)task.Packages.Sum(p => p.Items.Sum(i => i.Quantity)) + (int)request.Quantity;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload package with items
        package = await _context.Packages
            .Include(p => p.Items)
                .ThenInclude(i => i.Product)
            .FirstAsync(p => p.Id == packageId);

        return ApiResponse<PackageDto>.Ok(MapPackageToDto(package));
    }

    public async Task<ApiResponse<PackageDto>> RemoveItemFromPackageAsync(
        Guid tenantId, Guid taskId, Guid packageId, Guid itemId)
    {
        var item = await _context.PackageItems
            .Include(i => i.Package)
                .ThenInclude(p => p!.PackingTask)
            .FirstOrDefaultAsync(i => i.TenantId == tenantId &&
                                     i.PackageId == packageId &&
                                     i.Id == itemId);

        if (item == null)
            return ApiResponse<PackageDto>.Fail("Item not found");

        if (item.Package?.PackingTask?.Status != PackingTaskStatus.InProgress)
            return ApiResponse<PackageDto>.Fail("Task must be in progress to remove items");

        var quantityRemoved = item.Quantity;
        _context.PackageItems.Remove(item);

        // Update task packed items
        var task = await _context.PackingTasks.FirstAsync(t => t.Id == taskId);
        task.PackedItems -= (int)quantityRemoved;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var package = await _context.Packages
            .Include(p => p.Items)
                .ThenInclude(i => i.Product)
            .FirstAsync(p => p.Id == packageId);

        return ApiResponse<PackageDto>.Ok(MapPackageToDto(package));
    }

    public async Task<ApiResponse<PackageDto>> SetTrackingAsync(
        Guid tenantId, Guid taskId, Guid packageId, SetTrackingRequest request)
    {
        var package = await _context.Packages
            .Include(p => p.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(p => p.TenantId == tenantId &&
                                     p.PackingTaskId == taskId &&
                                     p.Id == packageId);

        if (package == null)
            return ApiResponse<PackageDto>.Fail("Package not found");

        package.TrackingNumber = request.TrackingNumber;
        package.LabelUrl = request.LabelUrl;
        package.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<PackageDto>.Ok(MapPackageToDto(package));
    }

    public async Task<ApiResponse<bool>> DeletePackageAsync(Guid tenantId, Guid taskId, Guid packageId)
    {
        var package = await _context.Packages
            .Include(p => p.PackingTask)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.TenantId == tenantId &&
                                     p.PackingTaskId == taskId &&
                                     p.Id == packageId);

        if (package == null)
            return ApiResponse<bool>.Fail("Package not found");

        if (package.PackingTask?.Status == PackingTaskStatus.Complete)
            return ApiResponse<bool>.Fail("Cannot delete package from completed task");

        // Update task
        var task = await _context.PackingTasks
            .Include(t => t.Packages)
            .FirstAsync(t => t.Id == taskId);

        task.PackedItems -= (int)package.Items.Sum(i => i.Quantity);
        task.BoxCount = task.Packages.Count - 1;
        task.UpdatedAt = DateTime.UtcNow;

        _context.Packages.Remove(package);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Package deleted");
    }

    #endregion

    #region Packing Stations

    public async Task<ApiResponse<List<PackingStationDto>>> GetPackingStationsAsync(
        Guid tenantId, Guid? warehouseId = null)
    {
        var query = _context.PackingStations
            .Include(s => s.Warehouse)
            .Where(s => s.TenantId == tenantId);

        if (warehouseId.HasValue)
            query = query.Where(s => s.WarehouseId == warehouseId.Value);

        var stations = await query
            .OrderBy(s => s.Code)
            .Select(s => new PackingStationDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse != null ? s.Warehouse.Name : null,
                IsActive = s.IsActive,
                CanPrintLabels = s.CanPrintLabels,
                HasScale = s.HasScale,
                HasDimensioner = s.HasDimensioner,
                Notes = s.Notes,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<PackingStationDto>>.Ok(stations);
    }

    public async Task<ApiResponse<PackingStationDto>> GetPackingStationByIdAsync(Guid tenantId, Guid id)
    {
        var station = await _context.PackingStations
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (station == null)
            return ApiResponse<PackingStationDto>.Fail("Packing station not found");

        return ApiResponse<PackingStationDto>.Ok(MapStationToDto(station));
    }

    public async Task<ApiResponse<PackingStationDto>> CreatePackingStationAsync(
        Guid tenantId, CreatePackingStationRequest request)
    {
        // Check for duplicate code
        var exists = await _context.PackingStations
            .AnyAsync(s => s.TenantId == tenantId && s.Code == request.Code);

        if (exists)
            return ApiResponse<PackingStationDto>.Fail($"Packing station with code '{request.Code}' already exists");

        // Validate warehouse
        var warehouseExists = await _context.Warehouses
            .AnyAsync(w => w.TenantId == tenantId && w.Id == request.WarehouseId);

        if (!warehouseExists)
            return ApiResponse<PackingStationDto>.Fail("Warehouse not found");

        var station = new PackingStation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code,
            Name = request.Name,
            WarehouseId = request.WarehouseId,
            CanPrintLabels = request.CanPrintLabels,
            HasScale = request.HasScale,
            HasDimensioner = request.HasDimensioner,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.PackingStations.Add(station);
        await _context.SaveChangesAsync();

        return await GetPackingStationByIdAsync(tenantId, station.Id);
    }

    public async Task<ApiResponse<PackingStationDto>> UpdatePackingStationAsync(
        Guid tenantId, Guid id, UpdatePackingStationRequest request)
    {
        var station = await _context.PackingStations
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (station == null)
            return ApiResponse<PackingStationDto>.Fail("Packing station not found");

        station.Name = request.Name;
        station.IsActive = request.IsActive;
        station.CanPrintLabels = request.CanPrintLabels;
        station.HasScale = request.HasScale;
        station.HasDimensioner = request.HasDimensioner;
        station.Notes = request.Notes;
        station.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetPackingStationByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<bool>> DeletePackingStationAsync(Guid tenantId, Guid id)
    {
        var station = await _context.PackingStations
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (station == null)
            return ApiResponse<bool>.Fail("Packing station not found");

        // Check for active tasks
        var hasActiveTasks = await _context.PackingTasks
            .AnyAsync(t => t.PackingStationId == id &&
                          t.Status != PackingTaskStatus.Complete &&
                          t.Status != PackingTaskStatus.Cancelled);

        if (hasActiveTasks)
            return ApiResponse<bool>.Fail("Cannot delete station with active packing tasks");

        _context.PackingStations.Remove(station);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Packing station deleted");
    }

    #endregion

    #region Helpers

    private async Task<string> GenerateTaskNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"PK-{today}-";

        var lastTask = await _context.PackingTasks
            .Where(t => t.TenantId == tenantId && t.TaskNumber.StartsWith(prefix))
            .OrderByDescending(t => t.TaskNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastTask != null)
        {
            var lastNumberStr = lastTask.TaskNumber.Replace(prefix, "");
            if (int.TryParse(lastNumberStr, out var lastNumber))
                nextNumber = lastNumber + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private static PackingTaskDto MapToDto(PackingTask task)
    {
        return new PackingTaskDto
        {
            Id = task.Id,
            TaskNumber = task.TaskNumber,
            SalesOrderId = task.SalesOrderId,
            SalesOrderNumber = task.SalesOrder?.OrderNumber,
            CustomerName = task.SalesOrder?.Customer?.Name,
            FulfillmentBatchId = task.FulfillmentBatchId,
            FulfillmentBatchNumber = task.FulfillmentBatch?.BatchNumber,
            PackingStationId = task.PackingStationId,
            PackingStationCode = task.PackingStation?.Code,
            PackingStationName = task.PackingStation?.Name,
            AssignedToUserId = task.AssignedToUserId,
            AssignedAt = task.AssignedAt,
            Status = task.Status,
            TotalItems = task.TotalItems,
            PackedItems = task.PackedItems,
            BoxCount = task.BoxCount,
            TotalWeightKg = task.TotalWeightKg,
            StartedAt = task.StartedAt,
            CompletedAt = task.CompletedAt,
            Priority = task.Priority,
            Notes = task.Notes,
            Packages = task.Packages.Select(MapPackageToDto).ToList(),
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }

    private static PackageDto MapPackageToDto(Package package)
    {
        return new PackageDto
        {
            Id = package.Id,
            PackingTaskId = package.PackingTaskId,
            PackageNumber = package.PackageNumber,
            SequenceNumber = package.SequenceNumber,
            LengthMm = package.LengthMm,
            WidthMm = package.WidthMm,
            HeightMm = package.HeightMm,
            WeightKg = package.WeightKg,
            PackagingType = package.PackagingType,
            TrackingNumber = package.TrackingNumber,
            LabelUrl = package.LabelUrl,
            Items = package.Items.Select(i => new PackageItemDto
            {
                Id = i.Id,
                PackageId = i.PackageId,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name,
                Sku = i.Sku,
                Quantity = i.Quantity,
                BatchNumber = i.BatchNumber,
                SerialNumber = i.SerialNumber
            }).ToList(),
            CreatedAt = package.CreatedAt
        };
    }

    private static PackingStationDto MapStationToDto(PackingStation station)
    {
        return new PackingStationDto
        {
            Id = station.Id,
            Code = station.Code,
            Name = station.Name,
            WarehouseId = station.WarehouseId,
            WarehouseName = station.Warehouse?.Name,
            IsActive = station.IsActive,
            CanPrintLabels = station.CanPrintLabels,
            HasScale = station.HasScale,
            HasDimensioner = station.HasDimensioner,
            Notes = station.Notes,
            CreatedAt = station.CreatedAt,
            UpdatedAt = station.UpdatedAt
        };
    }

    #endregion
}
