using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.CycleCount.DTOs;
using SmartWMS.API.Modules.CycleCount.Models;

namespace SmartWMS.API.Modules.CycleCount.Services;

public class CycleCountService : ICycleCountService
{
    private readonly ApplicationDbContext _context;

    public CycleCountService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Queries

    public async Task<ApiResponse<PaginatedResult<CycleCountSessionListDto>>> GetCycleCountsAsync(
        Guid tenantId, CycleCountFilters? filters, int page, int pageSize)
    {
        var query = _context.CycleCountSessions
            .Include(s => s.Warehouse)
            .Include(s => s.Zone)
            .Where(s => s.TenantId == tenantId);

        if (filters != null)
        {
            if (filters.Status.HasValue)
                query = query.Where(s => s.Status == filters.Status.Value);

            if (filters.CountType.HasValue)
                query = query.Where(s => s.CountType == filters.CountType.Value);

            if (filters.WarehouseId.HasValue)
                query = query.Where(s => s.WarehouseId == filters.WarehouseId.Value);

            if (filters.ZoneId.HasValue)
                query = query.Where(s => s.ZoneId == filters.ZoneId.Value);

            if (filters.AssignedToUserId.HasValue)
                query = query.Where(s => s.AssignedToUserId == filters.AssignedToUserId.Value);

            if (filters.FromDate.HasValue)
                query = query.Where(s => s.ScheduledDate >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                query = query.Where(s => s.ScheduledDate <= filters.ToDate.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.ScheduledDate ?? s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new CycleCountSessionListDto
            {
                Id = s.Id,
                CountNumber = s.CountNumber,
                Description = s.Description,
                WarehouseName = s.Warehouse != null ? s.Warehouse.Name : null,
                ZoneName = s.Zone != null ? s.Zone.Name : null,
                CountType = s.CountType,
                Status = s.Status,
                ScheduledDate = s.ScheduledDate,
                TotalLocations = s.TotalLocations,
                CountedLocations = s.CountedLocations,
                VarianceCount = s.VarianceCount,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        var result = new PaginatedResult<CycleCountSessionListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<CycleCountSessionListDto>>.Ok(result);
    }

    public async Task<ApiResponse<CycleCountSessionDto>> GetCycleCountByIdAsync(Guid tenantId, Guid id)
    {
        var session = await _context.CycleCountSessions
            .Include(s => s.Warehouse)
            .Include(s => s.Zone)
            .Include(s => s.Items)
                .ThenInclude(i => i.Location)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (session == null)
            return ApiResponse<CycleCountSessionDto>.Fail("Cycle count not found");

        return ApiResponse<CycleCountSessionDto>.Ok(MapToDto(session));
    }

    public async Task<ApiResponse<List<CycleCountSessionListDto>>> GetMyCycleCountsAsync(Guid tenantId, Guid userId)
    {
        var sessions = await _context.CycleCountSessions
            .Include(s => s.Warehouse)
            .Include(s => s.Zone)
            .Where(s => s.TenantId == tenantId &&
                       s.AssignedToUserId == userId &&
                       s.Status != CycleCountStatus.Complete &&
                       s.Status != CycleCountStatus.Cancelled)
            .OrderBy(s => s.ScheduledDate ?? s.CreatedAt)
            .Select(s => new CycleCountSessionListDto
            {
                Id = s.Id,
                CountNumber = s.CountNumber,
                Description = s.Description,
                WarehouseName = s.Warehouse != null ? s.Warehouse.Name : null,
                ZoneName = s.Zone != null ? s.Zone.Name : null,
                CountType = s.CountType,
                Status = s.Status,
                ScheduledDate = s.ScheduledDate,
                TotalLocations = s.TotalLocations,
                CountedLocations = s.CountedLocations,
                VarianceCount = s.VarianceCount,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<CycleCountSessionListDto>>.Ok(sessions);
    }

    #endregion

    #region CRUD

    public async Task<ApiResponse<CycleCountSessionDto>> CreateCycleCountAsync(
        Guid tenantId, CreateCycleCountRequest request)
    {
        // Validate warehouse
        var warehouseExists = await _context.Warehouses
            .AnyAsync(w => w.TenantId == tenantId && w.Id == request.WarehouseId);

        if (!warehouseExists)
            return ApiResponse<CycleCountSessionDto>.Fail("Warehouse not found");

        var session = new CycleCountSession
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CountNumber = await GenerateCountNumberAsync(tenantId),
            Description = request.Description,
            WarehouseId = request.WarehouseId,
            ZoneId = request.ZoneId,
            CountType = request.CountType,
            CountScope = request.CountScope,
            ScheduledDate = request.ScheduledDate,
            RequireBlindCount = request.RequireBlindCount,
            AllowRecounts = request.AllowRecounts,
            MaxRecounts = request.MaxRecounts,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        // Generate count items based on scope
        await GenerateCountItemsAsync(tenantId, session, request);

        _context.CycleCountSessions.Add(session);
        await _context.SaveChangesAsync();

        return await GetCycleCountByIdAsync(tenantId, session.Id);
    }

    public async Task<ApiResponse<CycleCountSessionDto>> UpdateCycleCountAsync(
        Guid tenantId, Guid id, UpdateCycleCountRequest request)
    {
        var session = await _context.CycleCountSessions
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (session == null)
            return ApiResponse<CycleCountSessionDto>.Fail("Cycle count not found");

        if (session.Status != CycleCountStatus.Draft &&
            session.Status != CycleCountStatus.Scheduled)
            return ApiResponse<CycleCountSessionDto>.Fail("Can only update draft or scheduled counts");

        session.Description = request.Description;
        session.ScheduledDate = request.ScheduledDate;
        session.RequireBlindCount = request.RequireBlindCount;
        session.AllowRecounts = request.AllowRecounts;
        session.MaxRecounts = request.MaxRecounts;
        session.Notes = request.Notes;
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetCycleCountByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<bool>> DeleteCycleCountAsync(Guid tenantId, Guid id)
    {
        var session = await _context.CycleCountSessions
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (session == null)
            return ApiResponse<bool>.Fail("Cycle count not found");

        if (session.Status != CycleCountStatus.Draft)
            return ApiResponse<bool>.Fail("Can only delete draft counts");

        _context.CycleCountSessions.Remove(session);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Cycle count deleted");
    }

    #endregion

    #region Workflow

    public async Task<ApiResponse<CycleCountSessionDto>> AssignCycleCountAsync(
        Guid tenantId, Guid id, AssignCycleCountRequest request)
    {
        var session = await _context.CycleCountSessions
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (session == null)
            return ApiResponse<CycleCountSessionDto>.Fail("Cycle count not found");

        session.AssignedToUserId = request.UserId;
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetCycleCountByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<CycleCountSessionDto>> ScheduleCycleCountAsync(
        Guid tenantId, Guid id, DateTime scheduledDate)
    {
        var session = await _context.CycleCountSessions
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (session == null)
            return ApiResponse<CycleCountSessionDto>.Fail("Cycle count not found");

        if (session.Status != CycleCountStatus.Draft)
            return ApiResponse<CycleCountSessionDto>.Fail("Can only schedule draft counts");

        session.Status = CycleCountStatus.Scheduled;
        session.ScheduledDate = scheduledDate;
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetCycleCountByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<CycleCountSessionDto>> StartCycleCountAsync(Guid tenantId, Guid id)
    {
        var session = await _context.CycleCountSessions
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (session == null)
            return ApiResponse<CycleCountSessionDto>.Fail("Cycle count not found");

        if (session.Status != CycleCountStatus.Draft &&
            session.Status != CycleCountStatus.Scheduled)
            return ApiResponse<CycleCountSessionDto>.Fail("Cycle count must be draft or scheduled");

        session.Status = CycleCountStatus.InProgress;
        session.StartedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetCycleCountByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<CycleCountSessionDto>> RecordCountAsync(
        Guid tenantId, Guid sessionId, Guid itemId, RecordCountRequest request)
    {
        var session = await _context.CycleCountSessions
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == sessionId);

        if (session == null)
            return ApiResponse<CycleCountSessionDto>.Fail("Cycle count not found");

        if (session.Status != CycleCountStatus.InProgress)
            return ApiResponse<CycleCountSessionDto>.Fail("Cycle count must be in progress");

        var item = session.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return ApiResponse<CycleCountSessionDto>.Fail("Count item not found");

        item.CountedQuantity = request.CountedQuantity;
        item.CountedBatchNumber = request.CountedBatchNumber;
        item.CountedAt = DateTime.UtcNow;
        item.CountedByUserId = request.CountedByUserId;
        item.Status = CountItemStatus.Counted;

        if (!string.IsNullOrEmpty(request.Notes))
            item.Notes = request.Notes;

        // Check if variance exceeds threshold (e.g., 5% or any difference)
        var variance = request.CountedQuantity - item.ExpectedQuantity;
        if (variance != 0)
        {
            item.RequiresApproval = true;
            session.VarianceCount = session.Items.Count(i => i.CountedQuantity.HasValue &&
                                                            i.CountedQuantity.Value != i.ExpectedQuantity);
        }

        item.UpdatedAt = DateTime.UtcNow;

        // Update session progress
        session.CountedLocations = session.Items.Count(i => i.Status != CountItemStatus.Pending);
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetCycleCountByIdAsync(tenantId, sessionId);
    }

    public async Task<ApiResponse<CycleCountSessionDto>> RequestRecountAsync(
        Guid tenantId, Guid sessionId, Guid itemId, string? reason)
    {
        var session = await _context.CycleCountSessions
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == sessionId);

        if (session == null)
            return ApiResponse<CycleCountSessionDto>.Fail("Cycle count not found");

        var item = session.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return ApiResponse<CycleCountSessionDto>.Fail("Count item not found");

        if (!session.AllowRecounts)
            return ApiResponse<CycleCountSessionDto>.Fail("Recounts not allowed for this session");

        if (item.RecountNumber >= session.MaxRecounts)
            return ApiResponse<CycleCountSessionDto>.Fail($"Maximum recounts ({session.MaxRecounts}) reached");

        item.Status = CountItemStatus.Recounting;
        item.RecountNumber++;
        item.CountedQuantity = null;
        item.CountedAt = null;
        item.CountedByUserId = null;

        if (!string.IsNullOrEmpty(reason))
            item.Notes = string.IsNullOrEmpty(item.Notes)
                ? $"Recount requested: {reason}"
                : $"{item.Notes}\nRecount requested: {reason}";

        item.UpdatedAt = DateTime.UtcNow;
        session.CountedLocations = session.Items.Count(i => i.Status == CountItemStatus.Counted ||
                                                           i.Status == CountItemStatus.Approved);
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetCycleCountByIdAsync(tenantId, sessionId);
    }

    public async Task<ApiResponse<CycleCountSessionDto>> ApproveVarianceAsync(
        Guid tenantId, Guid sessionId, Guid itemId, ApproveVarianceRequest request)
    {
        var session = await _context.CycleCountSessions
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == sessionId);

        if (session == null)
            return ApiResponse<CycleCountSessionDto>.Fail("Cycle count not found");

        var item = session.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return ApiResponse<CycleCountSessionDto>.Fail("Count item not found");

        if (item.Status != CountItemStatus.Counted)
            return ApiResponse<CycleCountSessionDto>.Fail("Item must be counted before approval");

        item.IsApproved = true;
        item.ApprovedByUserId = request.ApprovedByUserId;
        item.ApprovedAt = DateTime.UtcNow;
        item.Status = CountItemStatus.Approved;

        if (!string.IsNullOrEmpty(request.Notes))
            item.Notes = string.IsNullOrEmpty(item.Notes)
                ? request.Notes
                : $"{item.Notes}\n{request.Notes}";

        item.UpdatedAt = DateTime.UtcNow;

        // Check if we need to move to review status
        var pendingApprovals = session.Items.Count(i => i.RequiresApproval && !i.IsApproved);
        if (pendingApprovals == 0 && session.Status == CycleCountStatus.InProgress)
        {
            session.Status = CycleCountStatus.Review;
        }

        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetCycleCountByIdAsync(tenantId, sessionId);
    }

    public async Task<ApiResponse<CycleCountSessionDto>> AdjustStockAsync(
        Guid tenantId, Guid sessionId, Guid itemId, AdjustStockRequest request)
    {
        var session = await _context.CycleCountSessions
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == sessionId);

        if (session == null)
            return ApiResponse<CycleCountSessionDto>.Fail("Cycle count not found");

        var item = session.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return ApiResponse<CycleCountSessionDto>.Fail("Count item not found");

        if (item.Status != CountItemStatus.Approved && item.Status != CountItemStatus.Counted)
            return ApiResponse<CycleCountSessionDto>.Fail("Item must be counted or approved before adjustment");

        // Stock adjustment would be done here via StockService
        // For now, just mark as adjusted
        item.Status = CountItemStatus.Adjusted;

        if (!string.IsNullOrEmpty(request.Reason))
            item.Notes = string.IsNullOrEmpty(item.Notes)
                ? $"Stock adjusted: {request.Reason}"
                : $"{item.Notes}\nStock adjusted: {request.Reason}";

        item.UpdatedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetCycleCountByIdAsync(tenantId, sessionId);
    }

    public async Task<ApiResponse<CycleCountSessionDto>> CompleteCycleCountAsync(Guid tenantId, Guid id)
    {
        var session = await _context.CycleCountSessions
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (session == null)
            return ApiResponse<CycleCountSessionDto>.Fail("Cycle count not found");

        // Check all items are processed
        var unprocessed = session.Items.Any(i => i.Status == CountItemStatus.Pending ||
                                                 i.Status == CountItemStatus.Recounting);
        if (unprocessed)
            return ApiResponse<CycleCountSessionDto>.Fail("All items must be counted");

        var pendingApprovals = session.Items.Any(i => i.RequiresApproval && !i.IsApproved);
        if (pendingApprovals)
            return ApiResponse<CycleCountSessionDto>.Fail("All variances must be approved");

        session.Status = CycleCountStatus.Complete;
        session.CompletedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetCycleCountByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<CycleCountSessionDto>> CancelCycleCountAsync(
        Guid tenantId, Guid id, string? reason)
    {
        var session = await _context.CycleCountSessions
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (session == null)
            return ApiResponse<CycleCountSessionDto>.Fail("Cycle count not found");

        if (session.Status == CycleCountStatus.Complete)
            return ApiResponse<CycleCountSessionDto>.Fail("Cannot cancel completed count");

        session.Status = CycleCountStatus.Cancelled;
        session.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(reason))
            session.Notes = string.IsNullOrEmpty(session.Notes)
                ? $"Cancelled: {reason}"
                : $"{session.Notes}\nCancelled: {reason}";

        await _context.SaveChangesAsync();
        return await GetCycleCountByIdAsync(tenantId, id);
    }

    #endregion

    #region Helpers

    private async Task<string> GenerateCountNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"CC-{today}-";

        var lastCount = await _context.CycleCountSessions
            .Where(s => s.TenantId == tenantId && s.CountNumber.StartsWith(prefix))
            .OrderByDescending(s => s.CountNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastCount != null)
        {
            var lastNumberStr = lastCount.CountNumber.Replace(prefix, "");
            if (int.TryParse(lastNumberStr, out var lastNumber))
                nextNumber = lastNumber + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private async Task GenerateCountItemsAsync(
        Guid tenantId, CycleCountSession session, CreateCycleCountRequest request)
    {
        // Get stock levels for the scope
        var stockQuery = _context.StockLevels
            .Include(s => s.Location)
            .Include(s => s.Product)
            .Where(s => s.TenantId == tenantId &&
                       s.Location != null &&
                       s.Location.Warehouse != null &&
                       s.Location.Warehouse.Id == request.WarehouseId);

        if (request.ZoneId.HasValue)
            stockQuery = stockQuery.Where(s => s.Location!.ZoneId == request.ZoneId.Value);

        if (request.LocationIds != null && request.LocationIds.Any())
            stockQuery = stockQuery.Where(s => request.LocationIds.Contains(s.LocationId));

        if (request.ProductIds != null && request.ProductIds.Any())
            stockQuery = stockQuery.Where(s => request.ProductIds.Contains(s.ProductId));

        var stockLevels = await stockQuery.ToListAsync();

        foreach (var stock in stockLevels)
        {
            var item = new CycleCountItem
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CycleCountSessionId = session.Id,
                LocationId = stock.LocationId,
                ProductId = stock.ProductId,
                Sku = stock.Sku,
                ExpectedQuantity = stock.QuantityOnHand,
                ExpectedBatchNumber = stock.BatchNumber,
                CreatedAt = DateTime.UtcNow
            };
            session.Items.Add(item);
        }

        session.TotalLocations = session.Items.Select(i => i.LocationId).Distinct().Count();
    }

    private static CycleCountSessionDto MapToDto(CycleCountSession session)
    {
        return new CycleCountSessionDto
        {
            Id = session.Id,
            CountNumber = session.CountNumber,
            Description = session.Description,
            WarehouseId = session.WarehouseId,
            WarehouseName = session.Warehouse?.Name,
            ZoneId = session.ZoneId,
            ZoneName = session.Zone?.Name,
            CountType = session.CountType,
            CountScope = session.CountScope,
            Status = session.Status,
            ScheduledDate = session.ScheduledDate,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt,
            AssignedToUserId = session.AssignedToUserId,
            TotalLocations = session.TotalLocations,
            CountedLocations = session.CountedLocations,
            VarianceCount = session.VarianceCount,
            RequireBlindCount = session.RequireBlindCount,
            AllowRecounts = session.AllowRecounts,
            MaxRecounts = session.MaxRecounts,
            Notes = session.Notes,
            Items = session.Items.Select(i => new CycleCountItemDto
            {
                Id = i.Id,
                CycleCountSessionId = i.CycleCountSessionId,
                LocationId = i.LocationId,
                LocationCode = i.Location?.Code,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name,
                Sku = i.Sku,
                ExpectedQuantity = i.ExpectedQuantity,
                ExpectedBatchNumber = i.ExpectedBatchNumber,
                CountedQuantity = i.CountedQuantity,
                CountedBatchNumber = i.CountedBatchNumber,
                CountedAt = i.CountedAt,
                CountedByUserId = i.CountedByUserId,
                Variance = i.Variance,
                VariancePercent = i.VariancePercent,
                Status = i.Status,
                RecountNumber = i.RecountNumber,
                RequiresApproval = i.RequiresApproval,
                IsApproved = i.IsApproved,
                Notes = i.Notes
            }).ToList(),
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt
        };
    }

    #endregion
}
