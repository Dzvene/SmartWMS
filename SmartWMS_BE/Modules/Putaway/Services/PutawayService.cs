using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Inventory.Models;
using SmartWMS.API.Modules.Putaway.DTOs;
using SmartWMS.API.Modules.Putaway.Models;
using SmartWMS.API.Modules.Receiving.Models;

namespace SmartWMS.API.Modules.Putaway.Services;

public class PutawayService : IPutawayService
{
    private readonly ApplicationDbContext _context;

    public PutawayService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Queries

    public async Task<ApiResponse<PaginatedResult<PutawayTaskDto>>> GetTasksAsync(
        Guid tenantId,
        PutawayTaskFilters? filters = null,
        int page = 1,
        int pageSize = 25)
    {
        var query = _context.PutawayTasks
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.SuggestedLocation)
            .Include(t => t.ActualLocation)
            .Include(t => t.GoodsReceipt)
            .Where(t => t.TenantId == tenantId)
            .AsQueryable();

        if (filters != null)
        {
            if (filters.Status.HasValue)
                query = query.Where(t => t.Status == filters.Status.Value);

            if (filters.AssignedToUserId.HasValue)
                query = query.Where(t => t.AssignedToUserId == filters.AssignedToUserId.Value);

            if (filters.ProductId.HasValue)
                query = query.Where(t => t.ProductId == filters.ProductId.Value);

            if (filters.FromLocationId.HasValue)
                query = query.Where(t => t.FromLocationId == filters.FromLocationId.Value);

            if (filters.GoodsReceiptId.HasValue)
                query = query.Where(t => t.GoodsReceiptId == filters.GoodsReceiptId.Value);

            if (filters.Priority.HasValue)
                query = query.Where(t => t.Priority == filters.Priority.Value);

            if (filters.Unassigned == true)
                query = query.Where(t => t.AssignedToUserId == null);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => MapToDto(t))
            .ToListAsync();

        var result = new PaginatedResult<PutawayTaskDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<PutawayTaskDto>>.Ok(result);
    }

    public async Task<ApiResponse<PutawayTaskDto>> GetTaskByIdAsync(Guid tenantId, Guid id)
    {
        var task = await _context.PutawayTasks
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.SuggestedLocation)
            .Include(t => t.ActualLocation)
            .Include(t => t.GoodsReceipt)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (task == null)
            return ApiResponse<PutawayTaskDto>.Fail("Putaway task not found");

        return ApiResponse<PutawayTaskDto>.Ok(MapToDto(task));
    }

    public async Task<ApiResponse<IEnumerable<PutawayTaskDto>>> GetMyTasksAsync(Guid tenantId, Guid userId)
    {
        var tasks = await _context.PutawayTasks
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.SuggestedLocation)
            .Where(t => t.TenantId == tenantId &&
                        t.AssignedToUserId == userId &&
                        (t.Status == PutawayTaskStatus.Assigned || t.Status == PutawayTaskStatus.InProgress))
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Select(t => MapToDto(t))
            .ToListAsync();

        return ApiResponse<IEnumerable<PutawayTaskDto>>.Ok(tasks);
    }

    public async Task<ApiResponse<PutawayTaskDto>> GetNextTaskAsync(Guid tenantId, Guid userId)
    {
        // First check if user has in-progress task
        var inProgress = await _context.PutawayTasks
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.SuggestedLocation)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId &&
                                      t.AssignedToUserId == userId &&
                                      t.Status == PutawayTaskStatus.InProgress);

        if (inProgress != null)
            return ApiResponse<PutawayTaskDto>.Ok(MapToDto(inProgress));

        // Get next assigned task
        var nextAssigned = await _context.PutawayTasks
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.SuggestedLocation)
            .Where(t => t.TenantId == tenantId &&
                        t.AssignedToUserId == userId &&
                        t.Status == PutawayTaskStatus.Assigned)
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        if (nextAssigned != null)
            return ApiResponse<PutawayTaskDto>.Ok(MapToDto(nextAssigned));

        return ApiResponse<PutawayTaskDto>.Fail("No tasks available");
    }

    #endregion

    #region Task Creation

    public async Task<ApiResponse<IEnumerable<PutawayTaskDto>>> CreateFromGoodsReceiptAsync(
        Guid tenantId, CreatePutawayFromReceiptRequest request)
    {
        var receipt = await _context.GoodsReceipts
            .Include(gr => gr.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(gr => gr.TenantId == tenantId && gr.Id == request.GoodsReceiptId);

        if (receipt == null)
            return ApiResponse<IEnumerable<PutawayTaskDto>>.Fail("Goods receipt not found");

        if (receipt.Status != GoodsReceiptStatus.Complete && receipt.Status != GoodsReceiptStatus.PartiallyComplete)
            return ApiResponse<IEnumerable<PutawayTaskDto>>.Fail("Goods receipt must be completed before creating putaway tasks");

        var fromLocationId = request.DefaultFromLocationId ?? receipt.ReceivingLocationId;
        if (!fromLocationId.HasValue)
            return ApiResponse<IEnumerable<PutawayTaskDto>>.Fail("From location is required");

        var tasks = new List<PutawayTask>();

        foreach (var line in receipt.Lines.Where(l => l.QuantityReceived > 0))
        {
            // Get suggested location
            var suggestion = await GetBestLocationAsync(tenantId, line.ProductId, line.QuantityReceived, line.BatchNumber, line.ExpirationDate);

            var task = new PutawayTask
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TaskNumber = await GenerateTaskNumberAsync(tenantId),
                GoodsReceiptId = receipt.Id,
                GoodsReceiptLineId = line.Id,
                ProductId = line.ProductId,
                Sku = line.Sku,
                QuantityToPutaway = line.QuantityReceived,
                QuantityPutaway = 0,
                FromLocationId = fromLocationId.Value,
                SuggestedLocationId = suggestion?.LocationId,
                BatchNumber = line.BatchNumber,
                SerialNumber = null,
                ExpiryDate = line.ExpirationDate,
                Status = PutawayTaskStatus.Pending,
                Priority = 5,
                CreatedAt = DateTime.UtcNow
            };

            tasks.Add(task);
        }

        _context.PutawayTasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Reload with navigation
        var taskIds = tasks.Select(t => t.Id).ToList();
        var createdTasks = await _context.PutawayTasks
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.SuggestedLocation)
            .Where(t => taskIds.Contains(t.Id))
            .ToListAsync();

        return ApiResponse<IEnumerable<PutawayTaskDto>>.Ok(
            createdTasks.Select(MapToDto),
            $"Created {tasks.Count} putaway tasks");
    }

    public async Task<ApiResponse<PutawayTaskDto>> CreateTaskAsync(Guid tenantId, CreatePutawayTaskRequest request)
    {
        if (request.Quantity <= 0)
            return ApiResponse<PutawayTaskDto>.Fail("Quantity must be greater than zero");

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ProductId);

        if (product == null)
            return ApiResponse<PutawayTaskDto>.Fail("Product not found");

        var fromLocation = await _context.Locations
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.FromLocationId);

        if (fromLocation == null)
            return ApiResponse<PutawayTaskDto>.Fail("From location not found");

        // Get suggested location if not provided
        Guid? suggestedLocationId = request.SuggestedLocationId;
        if (!suggestedLocationId.HasValue)
        {
            var suggestion = await GetBestLocationAsync(tenantId, request.ProductId, request.Quantity, request.BatchNumber, request.ExpiryDate);
            suggestedLocationId = suggestion?.LocationId;
        }

        var task = new PutawayTask
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TaskNumber = await GenerateTaskNumberAsync(tenantId),
            ProductId = request.ProductId,
            Sku = product.Sku,
            QuantityToPutaway = request.Quantity,
            QuantityPutaway = 0,
            FromLocationId = request.FromLocationId,
            SuggestedLocationId = suggestedLocationId,
            BatchNumber = request.BatchNumber,
            SerialNumber = request.SerialNumber,
            ExpiryDate = request.ExpiryDate,
            Status = PutawayTaskStatus.Pending,
            Priority = request.Priority,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.PutawayTasks.Add(task);
        await _context.SaveChangesAsync();

        await _context.Entry(task).Reference(t => t.Product).LoadAsync();
        await _context.Entry(task).Reference(t => t.FromLocation).LoadAsync();
        if (task.SuggestedLocationId.HasValue)
            await _context.Entry(task).Reference(t => t.SuggestedLocation).LoadAsync();

        return ApiResponse<PutawayTaskDto>.Ok(MapToDto(task), "Putaway task created");
    }

    #endregion

    #region Task Workflow

    public async Task<ApiResponse<PutawayTaskDto>> AssignTaskAsync(Guid tenantId, Guid taskId, AssignPutawayTaskRequest request)
    {
        var task = await _context.PutawayTasks
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.SuggestedLocation)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == taskId);

        if (task == null)
            return ApiResponse<PutawayTaskDto>.Fail("Putaway task not found");

        if (task.Status != PutawayTaskStatus.Pending && task.Status != PutawayTaskStatus.Assigned)
            return ApiResponse<PutawayTaskDto>.Fail($"Cannot assign task in {task.Status} status");

        task.AssignedToUserId = request.UserId;
        task.AssignedAt = DateTime.UtcNow;
        task.Status = PutawayTaskStatus.Assigned;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<PutawayTaskDto>.Ok(MapToDto(task), "Task assigned");
    }

    public async Task<ApiResponse<PutawayTaskDto>> StartTaskAsync(Guid tenantId, Guid taskId)
    {
        var task = await _context.PutawayTasks
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.SuggestedLocation)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == taskId);

        if (task == null)
            return ApiResponse<PutawayTaskDto>.Fail("Putaway task not found");

        if (task.Status != PutawayTaskStatus.Assigned)
            return ApiResponse<PutawayTaskDto>.Fail($"Cannot start task in {task.Status} status");

        task.Status = PutawayTaskStatus.InProgress;
        task.StartedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<PutawayTaskDto>.Ok(MapToDto(task), "Task started");
    }

    public async Task<ApiResponse<PutawayTaskDto>> CompleteTaskAsync(Guid tenantId, Guid taskId, CompletePutawayTaskRequest request)
    {
        var task = await _context.PutawayTasks
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.SuggestedLocation)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == taskId);

        if (task == null)
            return ApiResponse<PutawayTaskDto>.Fail("Putaway task not found");

        if (task.Status != PutawayTaskStatus.InProgress && task.Status != PutawayTaskStatus.Assigned)
            return ApiResponse<PutawayTaskDto>.Fail($"Cannot complete task in {task.Status} status");

        if (request.QuantityPutaway <= 0)
            return ApiResponse<PutawayTaskDto>.Fail("Quantity must be greater than zero");

        if (request.QuantityPutaway > task.QuantityToPutaway - task.QuantityPutaway)
            return ApiResponse<PutawayTaskDto>.Fail("Quantity exceeds remaining quantity to putaway");

        // Verify actual location exists
        var actualLocation = await _context.Locations
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.ActualLocationId);

        if (actualLocation == null)
            return ApiResponse<PutawayTaskDto>.Fail("Actual location not found");

        // Update task
        task.ActualLocationId = request.ActualLocationId;
        task.QuantityPutaway += request.QuantityPutaway;
        task.Notes = request.Notes ?? task.Notes;

        if (task.QuantityPutaway >= task.QuantityToPutaway)
        {
            task.Status = PutawayTaskStatus.Complete;
            task.CompletedAt = DateTime.UtcNow;
        }

        task.UpdatedAt = DateTime.UtcNow;

        // Create stock movement - transfer from receiving to storage
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MovementNumber = await GenerateMovementNumberAsync(tenantId),
            MovementType = MovementType.Transfer,
            ProductId = task.ProductId,
            Sku = task.Sku,
            FromLocationId = task.FromLocationId,
            ToLocationId = request.ActualLocationId,
            Quantity = request.QuantityPutaway,
            BatchNumber = task.BatchNumber,
            SerialNumber = task.SerialNumber,
            ReferenceType = "PutawayTask",
            ReferenceId = task.Id,
            ReferenceNumber = task.TaskNumber,
            Notes = $"Putaway from {task.FromLocation?.Code} to {actualLocation.Code}",
            MovementDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.StockMovements.Add(movement);

        // Update stock levels
        await UpdateStockLevelsForPutaway(tenantId, task, request.ActualLocationId, request.QuantityPutaway);

        await _context.SaveChangesAsync();

        await _context.Entry(task).Reference(t => t.ActualLocation).LoadAsync();

        return ApiResponse<PutawayTaskDto>.Ok(MapToDto(task), "Task completed");
    }

    public async Task<ApiResponse<PutawayTaskDto>> CancelTaskAsync(Guid tenantId, Guid taskId, string? reason = null)
    {
        var task = await _context.PutawayTasks
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.SuggestedLocation)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == taskId);

        if (task == null)
            return ApiResponse<PutawayTaskDto>.Fail("Putaway task not found");

        if (task.Status == PutawayTaskStatus.Complete)
            return ApiResponse<PutawayTaskDto>.Fail("Cannot cancel completed task");

        task.Status = PutawayTaskStatus.Cancelled;
        task.Notes = reason ?? task.Notes;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<PutawayTaskDto>.Ok(MapToDto(task), "Task cancelled");
    }

    #endregion

    #region Location Suggestion

    public async Task<ApiResponse<IEnumerable<LocationSuggestionDto>>> SuggestLocationsAsync(
        Guid tenantId, SuggestLocationRequest request)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ProductId);

        if (product == null)
            return ApiResponse<IEnumerable<LocationSuggestionDto>>.Fail("Product not found");

        // Get all active storage locations
        var locationsQuery = _context.Locations
            .Include(l => l.Zone)
                .ThenInclude(z => z!.Warehouse)
            .Where(l => l.TenantId == tenantId &&
                        l.IsActive &&
                        l.Zone!.IsActive &&
                        l.LocationType == LocationType.Bulk);

        if (request.PreferredZoneId.HasValue)
        {
            locationsQuery = locationsQuery.Where(l => l.ZoneId == request.PreferredZoneId.Value);
        }

        var locations = await locationsQuery.ToListAsync();

        // Get existing stock levels for scoring
        var stockLevels = await _context.StockLevels
            .Where(s => s.TenantId == tenantId)
            .GroupBy(s => s.LocationId)
            .Select(g => new { LocationId = g.Key, TotalQuantity = g.Sum(s => s.QuantityOnHand) })
            .ToDictionaryAsync(x => x.LocationId, x => x.TotalQuantity);

        // Score each location
        var suggestions = locations
            .Select(l => new
            {
                Location = l,
                Score = CalculateLocationScore(l, product, stockLevels, request)
            })
            .OrderByDescending(x => x.Score)
            .Take(5)
            .Select(x => new LocationSuggestionDto
            {
                LocationId = x.Location.Id,
                LocationCode = x.Location.Code,
                ZoneName = x.Location.Zone?.Name,
                WarehouseName = x.Location.Zone?.Warehouse?.Name,
                Score = x.Score,
                Reason = GetSuggestionReason(x.Score)
            })
            .ToList();

        return ApiResponse<IEnumerable<LocationSuggestionDto>>.Ok(suggestions);
    }

    #endregion

    #region Private Helpers

    private async Task<string> GenerateTaskNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"PA-{today}-";

        var lastNumber = await _context.PutawayTasks
            .Where(t => t.TenantId == tenantId && t.TaskNumber.StartsWith(prefix))
            .OrderByDescending(t => t.TaskNumber)
            .Select(t => t.TaskNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastNumber != null)
        {
            var lastNumStr = lastNumber.Replace(prefix, "");
            if (int.TryParse(lastNumStr, out var lastNum))
                nextNumber = lastNum + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private async Task<string> GenerateMovementNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"SM-{today}-";

        var lastNumber = await _context.StockMovements
            .Where(m => m.TenantId == tenantId && m.MovementNumber != null && m.MovementNumber.StartsWith(prefix))
            .OrderByDescending(m => m.MovementNumber)
            .Select(m => m.MovementNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastNumber != null)
        {
            var lastNumStr = lastNumber.Replace(prefix, "");
            if (int.TryParse(lastNumStr, out var lastNum))
                nextNumber = lastNum + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private async Task<LocationSuggestionDto?> GetBestLocationAsync(
        Guid tenantId, Guid productId, decimal quantity, string? batchNumber, DateTime? expiryDate)
    {
        var suggestions = await SuggestLocationsAsync(tenantId, new SuggestLocationRequest
        {
            ProductId = productId,
            Quantity = quantity,
            BatchNumber = batchNumber,
            ExpiryDate = expiryDate
        });

        return suggestions.Data?.FirstOrDefault();
    }

    private decimal CalculateLocationScore(
        Warehouse.Models.Location location,
        Product product,
        Dictionary<Guid, decimal> stockLevels,
        SuggestLocationRequest request)
    {
        decimal score = 100;

        // Prefer empty locations
        if (!stockLevels.ContainsKey(location.Id) || stockLevels[location.Id] == 0)
            score += 50;

        // Prefer locations with same product (consolidation)
        // Would need additional query - simplified here

        // Prefer floor level for heavy items
        if (location.Level == "01" || location.Level == "A")
            score += 10;

        // Zone preference
        if (request.PreferredZoneId.HasValue && location.ZoneId == request.PreferredZoneId.Value)
            score += 30;

        return score;
    }

    private string GetSuggestionReason(decimal score)
    {
        if (score >= 150) return "Empty location, optimal for new stock";
        if (score >= 120) return "Good location with space available";
        if (score >= 100) return "Suitable location";
        return "Available location";
    }

    private async Task UpdateStockLevelsForPutaway(
        Guid tenantId, PutawayTask task, Guid toLocationId, decimal quantity)
    {
        // Decrease from source location
        var sourceStock = await _context.StockLevels
            .FirstOrDefaultAsync(s =>
                s.TenantId == tenantId &&
                s.ProductId == task.ProductId &&
                s.LocationId == task.FromLocationId &&
                (task.BatchNumber == null || s.BatchNumber == task.BatchNumber));

        if (sourceStock != null)
        {
            sourceStock.QuantityOnHand -= quantity;
            sourceStock.LastMovementAt = DateTime.UtcNow;
            sourceStock.UpdatedAt = DateTime.UtcNow;
        }

        // Increase at destination location
        var destStock = await _context.StockLevels
            .FirstOrDefaultAsync(s =>
                s.TenantId == tenantId &&
                s.ProductId == task.ProductId &&
                s.LocationId == toLocationId &&
                (task.BatchNumber == null || s.BatchNumber == task.BatchNumber));

        if (destStock != null)
        {
            destStock.QuantityOnHand += quantity;
            destStock.LastMovementAt = DateTime.UtcNow;
            destStock.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Create new stock level at destination
            _context.StockLevels.Add(new StockLevel
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductId = task.ProductId,
                Sku = task.Sku,
                LocationId = toLocationId,
                QuantityOnHand = quantity,
                QuantityReserved = 0,
                BatchNumber = task.BatchNumber,
                SerialNumber = task.SerialNumber,
                ExpiryDate = task.ExpiryDate,
                LastMovementAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private static PutawayTaskDto MapToDto(PutawayTask task)
    {
        return new PutawayTaskDto
        {
            Id = task.Id,
            TaskNumber = task.TaskNumber,
            GoodsReceiptId = task.GoodsReceiptId,
            GoodsReceiptNumber = task.GoodsReceipt?.ReceiptNumber,
            ProductId = task.ProductId,
            Sku = task.Sku,
            ProductName = task.Product?.Name,
            QuantityToPutaway = task.QuantityToPutaway,
            QuantityPutaway = task.QuantityPutaway,
            FromLocationId = task.FromLocationId,
            FromLocationCode = task.FromLocation?.Code,
            SuggestedLocationId = task.SuggestedLocationId,
            SuggestedLocationCode = task.SuggestedLocation?.Code,
            ActualLocationId = task.ActualLocationId,
            ActualLocationCode = task.ActualLocation?.Code,
            BatchNumber = task.BatchNumber,
            SerialNumber = task.SerialNumber,
            ExpiryDate = task.ExpiryDate,
            AssignedToUserId = task.AssignedToUserId,
            AssignedAt = task.AssignedAt,
            Status = task.Status,
            StartedAt = task.StartedAt,
            CompletedAt = task.CompletedAt,
            Priority = task.Priority,
            Notes = task.Notes,
            CreatedAt = task.CreatedAt
        };
    }

    #endregion
}
