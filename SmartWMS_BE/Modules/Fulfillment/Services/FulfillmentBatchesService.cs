using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Fulfillment.DTOs;
using SmartWMS.API.Modules.Fulfillment.Models;

namespace SmartWMS.API.Modules.Fulfillment.Services;

public class FulfillmentBatchesService : IFulfillmentBatchesService
{
    private readonly ApplicationDbContext _context;

    public FulfillmentBatchesService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PaginatedResult<FulfillmentBatchDto>>> GetBatchesAsync(
        Guid tenantId,
        FulfillmentBatchFilters? filters = null,
        int page = 1,
        int pageSize = 25)
    {
        var query = _context.FulfillmentBatches
            .Include(b => b.Warehouse)
            .Include(b => b.Zone)
            .Where(b => b.TenantId == tenantId)
            .AsQueryable();

        if (filters != null)
        {
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var search = filters.Search.ToLower();
                query = query.Where(b =>
                    b.BatchNumber.ToLower().Contains(search) ||
                    (b.Name != null && b.Name.ToLower().Contains(search)));
            }

            if (filters.Status.HasValue)
                query = query.Where(b => b.Status == filters.Status.Value);

            if (filters.BatchType.HasValue)
                query = query.Where(b => b.BatchType == filters.BatchType.Value);

            if (filters.WarehouseId.HasValue)
                query = query.Where(b => b.WarehouseId == filters.WarehouseId.Value);

            if (filters.ZoneId.HasValue)
                query = query.Where(b => b.ZoneId == filters.ZoneId.Value);

            if (filters.AssignedToUserId.HasValue)
                query = query.Where(b => b.AssignedToUserId == filters.AssignedToUserId.Value);

            if (filters.CreatedFrom.HasValue)
                query = query.Where(b => b.CreatedAt >= filters.CreatedFrom.Value);

            if (filters.CreatedTo.HasValue)
                query = query.Where(b => b.CreatedAt <= filters.CreatedTo.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.Priority)
            .ThenByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => MapToDto(b, false))
            .ToListAsync();

        var result = new PaginatedResult<FulfillmentBatchDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<FulfillmentBatchDto>>.Ok(result);
    }

    public async Task<ApiResponse<FulfillmentBatchDto>> GetBatchByIdAsync(Guid tenantId, Guid id, bool includeDetails = true)
    {
        var query = _context.FulfillmentBatches
            .Include(b => b.Warehouse)
            .Include(b => b.Zone)
            .AsQueryable();

        if (includeDetails)
        {
            query = query
                .Include(b => b.FulfillmentOrders)
                    .ThenInclude(fo => fo.Order)
                        .ThenInclude(o => o.Customer)
                .Include(b => b.PickTasks);
        }

        var batch = await query.FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == id);

        if (batch == null)
            return ApiResponse<FulfillmentBatchDto>.Fail("Fulfillment batch not found");

        return ApiResponse<FulfillmentBatchDto>.Ok(MapToDto(batch, includeDetails));
    }

    public async Task<ApiResponse<FulfillmentBatchDto>> GetBatchByNumberAsync(Guid tenantId, string batchNumber, bool includeDetails = true)
    {
        var query = _context.FulfillmentBatches
            .Include(b => b.Warehouse)
            .Include(b => b.Zone)
            .AsQueryable();

        if (includeDetails)
        {
            query = query
                .Include(b => b.FulfillmentOrders)
                    .ThenInclude(fo => fo.Order)
                        .ThenInclude(o => o.Customer)
                .Include(b => b.PickTasks);
        }

        var batch = await query.FirstOrDefaultAsync(b => b.TenantId == tenantId && b.BatchNumber == batchNumber);

        if (batch == null)
            return ApiResponse<FulfillmentBatchDto>.Fail("Fulfillment batch not found");

        return ApiResponse<FulfillmentBatchDto>.Ok(MapToDto(batch, includeDetails));
    }

    public async Task<ApiResponse<FulfillmentBatchDto>> CreateBatchAsync(Guid tenantId, CreateFulfillmentBatchRequest request)
    {
        // Validate warehouse
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == request.WarehouseId);

        if (warehouse == null)
            return ApiResponse<FulfillmentBatchDto>.Fail("Warehouse not found");

        // Validate zone if provided
        if (request.ZoneId.HasValue)
        {
            var zone = await _context.Zones
                .FirstOrDefaultAsync(z => z.TenantId == tenantId && z.Id == request.ZoneId.Value);

            if (zone == null)
                return ApiResponse<FulfillmentBatchDto>.Fail("Zone not found");
        }

        // Generate batch number if not provided
        var batchNumber = request.BatchNumber;
        if (string.IsNullOrWhiteSpace(batchNumber))
        {
            batchNumber = await GenerateBatchNumberAsync(tenantId);
        }
        else
        {
            var exists = await _context.FulfillmentBatches
                .AnyAsync(b => b.TenantId == tenantId && b.BatchNumber == batchNumber);

            if (exists)
                return ApiResponse<FulfillmentBatchDto>.Fail($"Batch number '{batchNumber}' already exists");
        }

        var batch = new FulfillmentBatch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BatchNumber = batchNumber,
            Name = request.Name,
            WarehouseId = request.WarehouseId,
            BatchType = request.BatchType,
            Status = FulfillmentStatus.Created,
            Priority = request.Priority,
            ZoneId = request.ZoneId,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        // Add orders if provided
        if (request.OrderIds?.Any() == true)
        {
            var sequence = 1;
            foreach (var orderId in request.OrderIds)
            {
                var order = await _context.SalesOrders
                    .Include(o => o.Lines)
                    .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == orderId);

                if (order == null)
                    return ApiResponse<FulfillmentBatchDto>.Fail($"Order not found: {orderId}");

                // Check order status (must be Confirmed or Allocated)
                if (order.Status != SalesOrderStatus.Confirmed && order.Status != SalesOrderStatus.Allocated)
                    return ApiResponse<FulfillmentBatchDto>.Fail($"Order {order.OrderNumber} must be Confirmed or Allocated to add to batch");

                batch.FulfillmentOrders.Add(new FulfillmentOrder
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    BatchId = batch.Id,
                    OrderId = orderId,
                    Sequence = sequence++,
                    CreatedAt = DateTime.UtcNow
                });

                batch.OrderCount++;
                batch.LineCount += order.Lines.Count;
                batch.TotalQuantity += order.TotalQuantity;
            }
        }

        _context.FulfillmentBatches.Add(batch);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(batch).Reference(b => b.Warehouse).LoadAsync();

        return ApiResponse<FulfillmentBatchDto>.Ok(MapToDto(batch, false), "Fulfillment batch created successfully");
    }

    public async Task<ApiResponse<FulfillmentBatchDto>> UpdateBatchAsync(Guid tenantId, Guid id, UpdateFulfillmentBatchRequest request)
    {
        var batch = await _context.FulfillmentBatches
            .Include(b => b.Warehouse)
            .Include(b => b.Zone)
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == id);

        if (batch == null)
            return ApiResponse<FulfillmentBatchDto>.Fail("Fulfillment batch not found");

        // Only allow updates in Created status
        if (batch.Status != FulfillmentStatus.Created)
            return ApiResponse<FulfillmentBatchDto>.Fail($"Cannot update batch in '{batch.Status}' status");

        // Validate zone if changing
        if (request.ZoneId.HasValue)
        {
            var zone = await _context.Zones
                .FirstOrDefaultAsync(z => z.TenantId == tenantId && z.Id == request.ZoneId.Value);

            if (zone == null)
                return ApiResponse<FulfillmentBatchDto>.Fail("Zone not found");

            batch.ZoneId = request.ZoneId;
        }

        if (request.Name != null) batch.Name = request.Name;
        if (request.Priority.HasValue) batch.Priority = request.Priority.Value;
        if (request.AssignedToUserId.HasValue) batch.AssignedToUserId = request.AssignedToUserId;
        if (request.Notes != null) batch.Notes = request.Notes;

        batch.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<FulfillmentBatchDto>.Ok(MapToDto(batch, false), "Batch updated successfully");
    }

    public async Task<ApiResponse> DeleteBatchAsync(Guid tenantId, Guid id)
    {
        var batch = await _context.FulfillmentBatches
            .Include(b => b.FulfillmentOrders)
            .Include(b => b.PickTasks)
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == id);

        if (batch == null)
            return ApiResponse.Fail("Fulfillment batch not found");

        // Only allow deletion in Created status
        if (batch.Status != FulfillmentStatus.Created)
            return ApiResponse.Fail("Can only delete batches in Created status");

        _context.FulfillmentOrders.RemoveRange(batch.FulfillmentOrders);
        _context.PickTasks.RemoveRange(batch.PickTasks);
        _context.FulfillmentBatches.Remove(batch);
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Fulfillment batch deleted successfully");
    }

    public async Task<ApiResponse<FulfillmentBatchDto>> AddOrdersToBatchAsync(Guid tenantId, Guid id, AddOrdersToBatchRequest request)
    {
        var batch = await _context.FulfillmentBatches
            .Include(b => b.FulfillmentOrders)
            .Include(b => b.Warehouse)
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == id);

        if (batch == null)
            return ApiResponse<FulfillmentBatchDto>.Fail("Fulfillment batch not found");

        if (batch.Status != FulfillmentStatus.Created)
            return ApiResponse<FulfillmentBatchDto>.Fail($"Cannot add orders to batch in '{batch.Status}' status");

        var maxSequence = batch.FulfillmentOrders.Any() ? batch.FulfillmentOrders.Max(fo => fo.Sequence) : 0;

        foreach (var orderId in request.OrderIds)
        {
            // Check if already in batch
            if (batch.FulfillmentOrders.Any(fo => fo.OrderId == orderId))
                continue;

            var order = await _context.SalesOrders
                .Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == orderId);

            if (order == null)
                return ApiResponse<FulfillmentBatchDto>.Fail($"Order not found: {orderId}");

            if (order.Status != SalesOrderStatus.Confirmed && order.Status != SalesOrderStatus.Allocated)
                return ApiResponse<FulfillmentBatchDto>.Fail($"Order {order.OrderNumber} must be Confirmed or Allocated");

            batch.FulfillmentOrders.Add(new FulfillmentOrder
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                BatchId = batch.Id,
                OrderId = orderId,
                Sequence = ++maxSequence,
                CreatedAt = DateTime.UtcNow
            });

            batch.OrderCount++;
            batch.LineCount += order.Lines.Count;
            batch.TotalQuantity += order.TotalQuantity;
        }

        batch.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<FulfillmentBatchDto>.Ok(MapToDto(batch, false), "Orders added to batch");
    }

    public async Task<ApiResponse> RemoveOrderFromBatchAsync(Guid tenantId, Guid batchId, Guid orderId)
    {
        var batch = await _context.FulfillmentBatches
            .Include(b => b.FulfillmentOrders)
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == batchId);

        if (batch == null)
            return ApiResponse.Fail("Fulfillment batch not found");

        if (batch.Status != FulfillmentStatus.Created)
            return ApiResponse.Fail($"Cannot remove orders from batch in '{batch.Status}' status");

        var fulfillmentOrder = batch.FulfillmentOrders.FirstOrDefault(fo => fo.OrderId == orderId);
        if (fulfillmentOrder == null)
            return ApiResponse.Fail("Order not found in batch");

        var order = await _context.SalesOrders
            .Include(o => o.Lines)
            .FirstAsync(o => o.Id == orderId);

        batch.FulfillmentOrders.Remove(fulfillmentOrder);
        _context.FulfillmentOrders.Remove(fulfillmentOrder);

        batch.OrderCount--;
        batch.LineCount -= order.Lines.Count;
        batch.TotalQuantity -= order.TotalQuantity;

        batch.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Order removed from batch");
    }

    public async Task<ApiResponse<FulfillmentBatchDto>> ReleaseBatchAsync(Guid tenantId, Guid id, ReleaseBatchRequest request)
    {
        var batch = await _context.FulfillmentBatches
            .Include(b => b.FulfillmentOrders)
            .Include(b => b.Warehouse)
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == id);

        if (batch == null)
            return ApiResponse<FulfillmentBatchDto>.Fail("Fulfillment batch not found");

        if (batch.Status != FulfillmentStatus.Created)
            return ApiResponse<FulfillmentBatchDto>.Fail($"Cannot release batch in '{batch.Status}' status");

        if (!batch.FulfillmentOrders.Any())
            return ApiResponse<FulfillmentBatchDto>.Fail("Cannot release empty batch");

        batch.Status = FulfillmentStatus.Released;
        batch.ReleasedAt = DateTime.UtcNow;
        batch.AssignedToUserId = request.AssignedToUserId;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            batch.Notes = string.IsNullOrWhiteSpace(batch.Notes)
                ? request.Notes
                : $"{batch.Notes}\n[Released] {request.Notes}";
        }

        batch.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<FulfillmentBatchDto>.Ok(MapToDto(batch, false), "Batch released for picking");
    }

    public async Task<ApiResponse<FulfillmentBatchDto>> CompleteBatchAsync(Guid tenantId, Guid id)
    {
        var batch = await _context.FulfillmentBatches
            .Include(b => b.PickTasks)
            .Include(b => b.Warehouse)
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == id);

        if (batch == null)
            return ApiResponse<FulfillmentBatchDto>.Fail("Fulfillment batch not found");

        if (batch.Status != FulfillmentStatus.InProgress && batch.Status != FulfillmentStatus.PartiallyComplete)
            return ApiResponse<FulfillmentBatchDto>.Fail($"Cannot complete batch in '{batch.Status}' status");

        // Check if all pick tasks are complete
        var incompleteTasks = batch.PickTasks.Count(t =>
            t.Status != PickTaskStatus.Complete && t.Status != PickTaskStatus.ShortPicked && t.Status != PickTaskStatus.Cancelled);

        if (incompleteTasks > 0)
            return ApiResponse<FulfillmentBatchDto>.Fail($"Cannot complete batch with {incompleteTasks} incomplete pick tasks");

        batch.Status = FulfillmentStatus.Complete;
        batch.CompletedAt = DateTime.UtcNow;
        batch.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<FulfillmentBatchDto>.Ok(MapToDto(batch, false), "Batch completed");
    }

    public async Task<ApiResponse<FulfillmentBatchDto>> CancelBatchAsync(Guid tenantId, Guid id, string? reason = null)
    {
        var batch = await _context.FulfillmentBatches
            .Include(b => b.PickTasks)
            .Include(b => b.Warehouse)
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == id);

        if (batch == null)
            return ApiResponse<FulfillmentBatchDto>.Fail("Fulfillment batch not found");

        if (batch.Status == FulfillmentStatus.Complete || batch.Status == FulfillmentStatus.Cancelled)
            return ApiResponse<FulfillmentBatchDto>.Fail($"Cannot cancel batch in '{batch.Status}' status");

        // Cancel all pending pick tasks
        foreach (var task in batch.PickTasks.Where(t =>
            t.Status == PickTaskStatus.Pending || t.Status == PickTaskStatus.Assigned))
        {
            task.Status = PickTaskStatus.Cancelled;
            task.UpdatedAt = DateTime.UtcNow;
        }

        batch.Status = FulfillmentStatus.Cancelled;

        if (!string.IsNullOrWhiteSpace(reason))
        {
            batch.Notes = string.IsNullOrWhiteSpace(batch.Notes)
                ? $"[Cancelled] {reason}"
                : $"{batch.Notes}\n[Cancelled] {reason}";
        }

        batch.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<FulfillmentBatchDto>.Ok(MapToDto(batch, false), "Batch cancelled");
    }

    #region Private Methods

    private async Task<string> GenerateBatchNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"FB-{today:yyyyMMdd}-";

        var lastBatch = await _context.FulfillmentBatches
            .Where(b => b.TenantId == tenantId && b.BatchNumber.StartsWith(prefix))
            .OrderByDescending(b => b.BatchNumber)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (lastBatch != null)
        {
            var lastNumber = lastBatch.BatchNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumber, out var parsed))
            {
                sequence = parsed + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }

    private static FulfillmentBatchDto MapToDto(FulfillmentBatch batch, bool includeDetails)
    {
        var dto = new FulfillmentBatchDto
        {
            Id = batch.Id,
            BatchNumber = batch.BatchNumber,
            Name = batch.Name,
            WarehouseId = batch.WarehouseId,
            WarehouseCode = batch.Warehouse?.Code,
            WarehouseName = batch.Warehouse?.Name,
            Status = batch.Status,
            BatchType = batch.BatchType,
            OrderCount = batch.OrderCount,
            LineCount = batch.LineCount,
            TotalQuantity = batch.TotalQuantity,
            PickedQuantity = batch.PickedQuantity,
            Priority = batch.Priority,
            AssignedToUserId = batch.AssignedToUserId,
            ZoneId = batch.ZoneId,
            ZoneName = batch.Zone?.Name,
            ReleasedAt = batch.ReleasedAt,
            StartedAt = batch.StartedAt,
            CompletedAt = batch.CompletedAt,
            Notes = batch.Notes,
            CreatedAt = batch.CreatedAt,
            UpdatedAt = batch.UpdatedAt
        };

        if (includeDetails)
        {
            if (batch.FulfillmentOrders?.Any() == true)
            {
                dto.Orders = batch.FulfillmentOrders
                    .OrderBy(fo => fo.Sequence)
                    .Select(fo => new FulfillmentOrderDto
                    {
                        Id = fo.Id,
                        BatchId = fo.BatchId,
                        OrderId = fo.OrderId,
                        OrderNumber = fo.Order?.OrderNumber,
                        CustomerName = fo.Order?.Customer?.Name,
                        Sequence = fo.Sequence
                    })
                    .ToList();
            }

            if (batch.PickTasks?.Any() == true)
            {
                dto.PickTasks = batch.PickTasks
                    .OrderBy(t => t.Sequence)
                    .Select(t => new PickTaskDto
                    {
                        Id = t.Id,
                        TaskNumber = t.TaskNumber,
                        BatchId = t.BatchId,
                        OrderId = t.OrderId,
                        OrderLineId = t.OrderLineId,
                        ProductId = t.ProductId,
                        Sku = t.Sku,
                        FromLocationId = t.FromLocationId,
                        ToLocationId = t.ToLocationId,
                        QuantityRequired = t.QuantityRequired,
                        QuantityPicked = t.QuantityPicked,
                        QuantityShortPicked = t.QuantityShortPicked,
                        Status = t.Status,
                        Priority = t.Priority,
                        Sequence = t.Sequence,
                        AssignedToUserId = t.AssignedToUserId,
                        StartedAt = t.StartedAt,
                        CompletedAt = t.CompletedAt,
                        ShortPickReason = t.ShortPickReason,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .ToList();
            }
        }

        return dto;
    }

    #endregion
}
