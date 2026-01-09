using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Adjustments.DTOs;
using SmartWMS.API.Modules.Adjustments.Models;

namespace SmartWMS.API.Modules.Adjustments.Services;

public class AdjustmentsService : IAdjustmentsService
{
    private readonly ApplicationDbContext _context;

    public AdjustmentsService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Stock Adjustments CRUD

    public async Task<ApiResponse<PaginatedResult<StockAdjustmentSummaryDto>>> GetAdjustmentsAsync(
        Guid tenantId, AdjustmentFilterRequest filter)
    {
        var query = _context.StockAdjustments
            .Include(a => a.Warehouse)
            .Include(a => a.ReasonCode)
            .Where(a => a.TenantId == tenantId);

        // Apply filters
        if (filter.WarehouseId.HasValue)
            query = query.Where(a => a.WarehouseId == filter.WarehouseId.Value);

        if (filter.Status.HasValue)
            query = query.Where(a => a.Status == filter.Status.Value);

        if (filter.AdjustmentType.HasValue)
            query = query.Where(a => a.AdjustmentType == filter.AdjustmentType.Value);

        if (filter.ReasonCodeId.HasValue)
            query = query.Where(a => a.ReasonCodeId == filter.ReasonCodeId.Value);

        if (filter.DateFrom.HasValue)
            query = query.Where(a => a.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(a => a.CreatedAt <= filter.DateTo.Value);

        if (filter.CreatedByUserId.HasValue)
            query = query.Where(a => a.CreatedByUserId == filter.CreatedByUserId.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(a =>
                a.AdjustmentNumber.ToLower().Contains(term) ||
                (a.Notes != null && a.Notes.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new StockAdjustmentSummaryDto(
                a.Id,
                a.AdjustmentNumber,
                a.Warehouse.Name,
                a.Status,
                a.AdjustmentType,
                a.ReasonCode != null ? a.ReasonCode.Name : null,
                a.TotalLines,
                a.TotalQuantityChange,
                null, // CreatedByUserName - would need user join
                a.CreatedAt
            ))
            .ToListAsync();

        var result = new PaginatedResult<StockAdjustmentSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        return ApiResponse<PaginatedResult<StockAdjustmentSummaryDto>>.Ok(result);
    }

    public async Task<ApiResponse<StockAdjustmentDto>> GetAdjustmentByIdAsync(Guid tenantId, Guid id)
    {
        var adjustment = await _context.StockAdjustments
            .Include(a => a.Warehouse)
            .Include(a => a.ReasonCode)
            .Include(a => a.Lines)
                .ThenInclude(l => l.Product)
            .Include(a => a.Lines)
                .ThenInclude(l => l.Location)
            .Include(a => a.Lines)
                .ThenInclude(l => l.ReasonCode)
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id);

        if (adjustment == null)
            return ApiResponse<StockAdjustmentDto>.Fail("Adjustment not found");

        return ApiResponse<StockAdjustmentDto>.Ok(MapToDto(adjustment));
    }

    public async Task<ApiResponse<StockAdjustmentDto>> GetAdjustmentByNumberAsync(Guid tenantId, string adjustmentNumber)
    {
        var adjustment = await _context.StockAdjustments
            .Include(a => a.Warehouse)
            .Include(a => a.ReasonCode)
            .Include(a => a.Lines)
                .ThenInclude(l => l.Product)
            .Include(a => a.Lines)
                .ThenInclude(l => l.Location)
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.AdjustmentNumber == adjustmentNumber);

        if (adjustment == null)
            return ApiResponse<StockAdjustmentDto>.Fail("Adjustment not found");

        return ApiResponse<StockAdjustmentDto>.Ok(MapToDto(adjustment));
    }

    public async Task<ApiResponse<StockAdjustmentDto>> CreateAdjustmentAsync(
        Guid tenantId, Guid userId, CreateStockAdjustmentRequest request)
    {
        // Validate warehouse
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == request.WarehouseId);

        if (warehouse == null)
            return ApiResponse<StockAdjustmentDto>.Fail("Warehouse not found");

        // Generate adjustment number
        var adjustmentNumber = await GenerateAdjustmentNumberAsync(tenantId);

        var adjustment = new StockAdjustment
        {
            TenantId = tenantId,
            AdjustmentNumber = adjustmentNumber,
            WarehouseId = request.WarehouseId,
            AdjustmentType = request.AdjustmentType,
            ReasonCodeId = request.ReasonCodeId,
            ReasonNotes = request.ReasonNotes,
            SourceDocumentType = request.SourceDocumentType,
            SourceDocumentId = request.SourceDocumentId,
            SourceDocumentNumber = request.SourceDocumentNumber,
            Notes = request.Notes,
            CreatedByUserId = userId,
            Status = AdjustmentStatus.Draft
        };

        _context.StockAdjustments.Add(adjustment);

        // Add lines if provided
        if (request.Lines?.Any() == true)
        {
            int lineNumber = 1;
            foreach (var lineRequest in request.Lines)
            {
                var line = await CreateLineFromRequest(tenantId, adjustment.Id, lineNumber++, lineRequest);
                if (line == null)
                    return ApiResponse<StockAdjustmentDto>.Fail($"Invalid product or location in line {lineNumber - 1}");

                adjustment.Lines.Add(line);
            }

            UpdateAdjustmentTotals(adjustment);
        }

        await _context.SaveChangesAsync();

        return await GetAdjustmentByIdAsync(tenantId, adjustment.Id);
    }

    public async Task<ApiResponse<StockAdjustmentDto>> UpdateAdjustmentAsync(
        Guid tenantId, Guid id, UpdateStockAdjustmentRequest request)
    {
        var adjustment = await _context.StockAdjustments
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id);

        if (adjustment == null)
            return ApiResponse<StockAdjustmentDto>.Fail("Adjustment not found");

        if (adjustment.Status != AdjustmentStatus.Draft)
            return ApiResponse<StockAdjustmentDto>.Fail("Can only update adjustments in Draft status");

        adjustment.ReasonCodeId = request.ReasonCodeId;
        adjustment.ReasonNotes = request.ReasonNotes;
        adjustment.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return await GetAdjustmentByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<bool>> DeleteAdjustmentAsync(Guid tenantId, Guid id)
    {
        var adjustment = await _context.StockAdjustments
            .Include(a => a.Lines)
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id);

        if (adjustment == null)
            return ApiResponse<bool>.Fail("Adjustment not found");

        if (adjustment.Status != AdjustmentStatus.Draft)
            return ApiResponse<bool>.Fail("Can only delete adjustments in Draft status");

        _context.StockAdjustmentLines.RemoveRange(adjustment.Lines);
        _context.StockAdjustments.Remove(adjustment);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true);
    }

    #endregion

    #region Adjustment Lines

    public async Task<ApiResponse<StockAdjustmentLineDto>> AddLineAsync(
        Guid tenantId, Guid adjustmentId, AddAdjustmentLineRequest request)
    {
        var adjustment = await _context.StockAdjustments
            .Include(a => a.Lines)
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == adjustmentId);

        if (adjustment == null)
            return ApiResponse<StockAdjustmentLineDto>.Fail("Adjustment not found");

        if (adjustment.Status != AdjustmentStatus.Draft)
            return ApiResponse<StockAdjustmentLineDto>.Fail("Can only add lines to Draft adjustments");

        var lineNumber = adjustment.Lines.Count > 0 ? adjustment.Lines.Max(l => l.LineNumber) + 1 : 1;

        var line = await CreateLineFromRequest(tenantId, adjustmentId, lineNumber, new CreateAdjustmentLineRequest(
            request.ProductId,
            request.Sku,
            request.LocationId,
            request.BatchNumber,
            request.SerialNumber,
            request.QuantityAdjustment,
            request.UnitCost,
            request.ReasonCodeId,
            request.ReasonNotes
        ));

        if (line == null)
            return ApiResponse<StockAdjustmentLineDto>.Fail("Invalid product or location");

        _context.StockAdjustmentLines.Add(line);
        adjustment.Lines.Add(line);
        UpdateAdjustmentTotals(adjustment);

        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(line).Reference(l => l.Product).LoadAsync();
        await _context.Entry(line).Reference(l => l.Location).LoadAsync();

        return ApiResponse<StockAdjustmentLineDto>.Ok(MapLineToDto(line));
    }

    public async Task<ApiResponse<StockAdjustmentLineDto>> UpdateLineAsync(
        Guid tenantId, Guid adjustmentId, Guid lineId, UpdateAdjustmentLineRequest request)
    {
        var adjustment = await _context.StockAdjustments
            .Include(a => a.Lines)
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == adjustmentId);

        if (adjustment == null)
            return ApiResponse<StockAdjustmentLineDto>.Fail("Adjustment not found");

        if (adjustment.Status != AdjustmentStatus.Draft)
            return ApiResponse<StockAdjustmentLineDto>.Fail("Can only update lines in Draft adjustments");

        var line = adjustment.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse<StockAdjustmentLineDto>.Fail("Line not found");

        line.QuantityAdjustment = request.QuantityAdjustment;
        line.UnitCost = request.UnitCost;
        line.ReasonCodeId = request.ReasonCodeId;
        line.ReasonNotes = request.ReasonNotes;

        UpdateAdjustmentTotals(adjustment);

        await _context.SaveChangesAsync();

        await _context.Entry(line).Reference(l => l.Product).LoadAsync();
        await _context.Entry(line).Reference(l => l.Location).LoadAsync();

        return ApiResponse<StockAdjustmentLineDto>.Ok(MapLineToDto(line));
    }

    public async Task<ApiResponse<bool>> RemoveLineAsync(Guid tenantId, Guid adjustmentId, Guid lineId)
    {
        var adjustment = await _context.StockAdjustments
            .Include(a => a.Lines)
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == adjustmentId);

        if (adjustment == null)
            return ApiResponse<bool>.Fail("Adjustment not found");

        if (adjustment.Status != AdjustmentStatus.Draft)
            return ApiResponse<bool>.Fail("Can only remove lines from Draft adjustments");

        var line = adjustment.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse<bool>.Fail("Line not found");

        adjustment.Lines.Remove(line);
        _context.StockAdjustmentLines.Remove(line);
        UpdateAdjustmentTotals(adjustment);

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true);
    }

    #endregion

    #region Workflow Actions

    public async Task<ApiResponse<StockAdjustmentDto>> SubmitForApprovalAsync(
        Guid tenantId, Guid id, Guid userId, SubmitForApprovalRequest request)
    {
        var adjustment = await _context.StockAdjustments
            .Include(a => a.Lines)
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id);

        if (adjustment == null)
            return ApiResponse<StockAdjustmentDto>.Fail("Adjustment not found");

        if (adjustment.Status != AdjustmentStatus.Draft)
            return ApiResponse<StockAdjustmentDto>.Fail("Can only submit Draft adjustments");

        if (!adjustment.Lines.Any())
            return ApiResponse<StockAdjustmentDto>.Fail("Cannot submit adjustment without lines");

        adjustment.Status = AdjustmentStatus.PendingApproval;
        if (!string.IsNullOrWhiteSpace(request.Notes))
            adjustment.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return await GetAdjustmentByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<StockAdjustmentDto>> ApproveAsync(
        Guid tenantId, Guid id, Guid userId, ApproveAdjustmentRequest request)
    {
        var adjustment = await _context.StockAdjustments
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id);

        if (adjustment == null)
            return ApiResponse<StockAdjustmentDto>.Fail("Adjustment not found");

        if (adjustment.Status != AdjustmentStatus.PendingApproval)
            return ApiResponse<StockAdjustmentDto>.Fail("Can only approve adjustments pending approval");

        adjustment.Status = AdjustmentStatus.Approved;
        adjustment.ApprovedByUserId = userId;
        adjustment.ApprovedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetAdjustmentByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<StockAdjustmentDto>> RejectAsync(
        Guid tenantId, Guid id, Guid userId, RejectAdjustmentRequest request)
    {
        var adjustment = await _context.StockAdjustments
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id);

        if (adjustment == null)
            return ApiResponse<StockAdjustmentDto>.Fail("Adjustment not found");

        if (adjustment.Status != AdjustmentStatus.PendingApproval)
            return ApiResponse<StockAdjustmentDto>.Fail("Can only reject adjustments pending approval");

        adjustment.Status = AdjustmentStatus.Draft;
        adjustment.Notes = $"{adjustment.Notes}\n[Rejected: {request.RejectionReason}]";

        await _context.SaveChangesAsync();

        return await GetAdjustmentByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<StockAdjustmentDto>> PostAsync(
        Guid tenantId, Guid id, Guid userId, PostAdjustmentRequest request)
    {
        var adjustment = await _context.StockAdjustments
            .Include(a => a.Lines)
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id);

        if (adjustment == null)
            return ApiResponse<StockAdjustmentDto>.Fail("Adjustment not found");

        if (adjustment.Status != AdjustmentStatus.Approved)
            return ApiResponse<StockAdjustmentDto>.Fail("Can only post approved adjustments");

        // Apply stock changes
        foreach (var line in adjustment.Lines)
        {
            var stockLevel = await _context.StockLevels
                .FirstOrDefaultAsync(s => s.TenantId == tenantId &&
                    s.ProductId == line.ProductId &&
                    s.LocationId == line.LocationId &&
                    s.BatchNumber == line.BatchNumber);

            if (stockLevel == null)
            {
                // Create new stock level if positive adjustment
                if (line.QuantityAdjustment > 0)
                {
                    var product = await _context.Products.FindAsync(line.ProductId);
                    stockLevel = new Inventory.Models.StockLevel
                    {
                        TenantId = tenantId,
                        ProductId = line.ProductId,
                        Sku = product?.Sku ?? line.Sku,
                        LocationId = line.LocationId,
                        BatchNumber = line.BatchNumber,
                        SerialNumber = line.SerialNumber,
                        QuantityOnHand = line.QuantityAdjustment,
                        LastMovementAt = DateTime.UtcNow
                    };
                    _context.StockLevels.Add(stockLevel);
                }
                else
                {
                    return ApiResponse<StockAdjustmentDto>.Fail(
                        $"Cannot reduce stock for product {line.Sku} at location - no existing stock");
                }
            }
            else
            {
                var newQuantity = stockLevel.QuantityOnHand + line.QuantityAdjustment;
                if (newQuantity < 0)
                {
                    return ApiResponse<StockAdjustmentDto>.Fail(
                        $"Cannot reduce stock for {line.Sku} below zero. Current: {stockLevel.QuantityOnHand}, Adjustment: {line.QuantityAdjustment}");
                }

                stockLevel.QuantityOnHand = newQuantity;
                stockLevel.LastMovementAt = DateTime.UtcNow;
            }

            // Create stock movement record
            var movementNumber = $"MOV-{DateTime.UtcNow:yyyyMMddHHmmss}-{line.LineNumber:D3}";
            var movement = new Inventory.Models.StockMovement
            {
                TenantId = tenantId,
                MovementNumber = movementNumber,
                ProductId = line.ProductId,
                Sku = line.Sku,
                FromLocationId = line.QuantityAdjustment < 0 ? line.LocationId : null,
                ToLocationId = line.QuantityAdjustment >= 0 ? line.LocationId : null,
                MovementType = MovementType.Adjustment,
                Quantity = Math.Abs(line.QuantityAdjustment),
                BatchNumber = line.BatchNumber,
                ReferenceType = "StockAdjustment",
                ReferenceId = adjustment.Id,
                ReferenceNumber = adjustment.AdjustmentNumber,
                Notes = line.ReasonNotes ?? adjustment.ReasonNotes
            };
            _context.StockMovements.Add(movement);

            line.IsProcessed = true;
            line.ProcessedAt = DateTime.UtcNow;
        }

        adjustment.Status = AdjustmentStatus.Posted;
        adjustment.PostedByUserId = userId;
        adjustment.PostedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetAdjustmentByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<StockAdjustmentDto>> CancelAsync(Guid tenantId, Guid id, Guid userId)
    {
        var adjustment = await _context.StockAdjustments
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id);

        if (adjustment == null)
            return ApiResponse<StockAdjustmentDto>.Fail("Adjustment not found");

        if (adjustment.Status == AdjustmentStatus.Posted)
            return ApiResponse<StockAdjustmentDto>.Fail("Cannot cancel posted adjustments");

        if (adjustment.Status == AdjustmentStatus.Cancelled)
            return ApiResponse<StockAdjustmentDto>.Fail("Adjustment already cancelled");

        adjustment.Status = AdjustmentStatus.Cancelled;

        await _context.SaveChangesAsync();

        return await GetAdjustmentByIdAsync(tenantId, id);
    }

    #endregion

    #region Quick & Auto Adjustments

    public async Task<ApiResponse<StockAdjustmentDto>> QuickAdjustAsync(
        Guid tenantId, Guid userId, CreateStockAdjustmentRequest request)
    {
        // Create adjustment
        var createResult = await CreateAdjustmentAsync(tenantId, userId, request);
        if (!createResult.Success)
            return createResult;

        var adjustment = createResult.Data!;

        // Submit for approval
        var submitResult = await SubmitForApprovalAsync(tenantId, adjustment.Id, userId, new SubmitForApprovalRequest(null));
        if (!submitResult.Success)
            return submitResult;

        // Auto-approve
        var approveResult = await ApproveAsync(tenantId, adjustment.Id, userId, new ApproveAdjustmentRequest(null));
        if (!approveResult.Success)
            return approveResult;

        // Post
        return await PostAsync(tenantId, adjustment.Id, userId, new PostAdjustmentRequest(null));
    }

    public async Task<ApiResponse<StockAdjustmentDto>> CreateFromCycleCountAsync(
        Guid tenantId, Guid userId, Guid cycleCountSessionId)
    {
        var session = await _context.CycleCountSessions
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Items)
                .ThenInclude(i => i.Location)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == cycleCountSessionId);

        if (session == null)
            return ApiResponse<StockAdjustmentDto>.Fail("Cycle count session not found");

        // Get items with variance
        var itemsWithVariance = session.Items
            .Where(i => i.CountedQuantity.HasValue && i.Variance != 0)
            .ToList();

        if (!itemsWithVariance.Any())
            return ApiResponse<StockAdjustmentDto>.Fail("No variance items to adjust");

        var lines = itemsWithVariance.Select(i => new CreateAdjustmentLineRequest(
            i.ProductId,
            i.Product?.Sku ?? i.Sku,
            i.LocationId,
            i.ExpectedBatchNumber,
            null,
            i.Variance, // Variance = CountedQuantity - ExpectedQuantity
            null,
            null,
            $"Cycle count variance: Expected {i.ExpectedQuantity}, Counted {i.CountedQuantity}"
        )).ToList();

        var request = new CreateStockAdjustmentRequest(
            session.WarehouseId,
            AdjustmentType.CycleCount,
            null,
            null,
            "CycleCount",
            session.Id,
            session.CountNumber,
            $"Auto-generated from Cycle Count {session.CountNumber}",
            lines
        );

        return await CreateAdjustmentAsync(tenantId, userId, request);
    }

    #endregion

    #region Private Helpers

    private async Task<string> GenerateAdjustmentNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"ADJ-{today:yyyyMMdd}-";

        var lastNumber = await _context.StockAdjustments
            .Where(a => a.TenantId == tenantId && a.AdjustmentNumber.StartsWith(prefix))
            .OrderByDescending(a => a.AdjustmentNumber)
            .Select(a => a.AdjustmentNumber)
            .FirstOrDefaultAsync();

        int nextSequence = 1;
        if (lastNumber != null)
        {
            var lastSequence = lastNumber.Substring(prefix.Length);
            if (int.TryParse(lastSequence, out int seq))
                nextSequence = seq + 1;
        }

        return $"{prefix}{nextSequence:D4}";
    }

    private async Task<StockAdjustmentLine?> CreateLineFromRequest(
        Guid tenantId, Guid adjustmentId, int lineNumber, CreateAdjustmentLineRequest request)
    {
        // Validate product
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ProductId);

        if (product == null)
            return null;

        // Validate location
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.LocationId);

        if (location == null)
            return null;

        // Get current stock level
        var stockLevel = await _context.StockLevels
            .FirstOrDefaultAsync(s => s.TenantId == tenantId &&
                s.ProductId == request.ProductId &&
                s.LocationId == request.LocationId &&
                s.BatchNumber == request.BatchNumber);

        var quantityBefore = stockLevel?.QuantityOnHand ?? 0;

        return new StockAdjustmentLine
        {
            TenantId = tenantId,
            AdjustmentId = adjustmentId,
            LineNumber = lineNumber,
            ProductId = request.ProductId,
            Sku = request.Sku,
            LocationId = request.LocationId,
            BatchNumber = request.BatchNumber,
            SerialNumber = request.SerialNumber,
            QuantityBefore = quantityBefore,
            QuantityAdjustment = request.QuantityAdjustment,
            UnitCost = request.UnitCost,
            ReasonCodeId = request.ReasonCodeId,
            ReasonNotes = request.ReasonNotes
        };
    }

    private void UpdateAdjustmentTotals(StockAdjustment adjustment)
    {
        adjustment.TotalLines = adjustment.Lines.Count;
        adjustment.TotalQuantityChange = adjustment.Lines.Sum(l => l.QuantityAdjustment);
        adjustment.TotalValueChange = adjustment.Lines
            .Where(l => l.ValueChange.HasValue)
            .Sum(l => l.ValueChange);
    }

    private StockAdjustmentDto MapToDto(StockAdjustment adjustment)
    {
        return new StockAdjustmentDto(
            adjustment.Id,
            adjustment.AdjustmentNumber,
            adjustment.WarehouseId,
            adjustment.Warehouse?.Name,
            adjustment.Status,
            adjustment.AdjustmentType,
            adjustment.ReasonCodeId,
            adjustment.ReasonCode?.Name,
            adjustment.ReasonNotes,
            adjustment.SourceDocumentType,
            adjustment.SourceDocumentId,
            adjustment.SourceDocumentNumber,
            adjustment.CreatedByUserId,
            null, // CreatedByUserName
            adjustment.ApprovedByUserId,
            null, // ApprovedByUserName
            adjustment.ApprovedAt,
            adjustment.PostedByUserId,
            null, // PostedByUserName
            adjustment.PostedAt,
            adjustment.Notes,
            adjustment.TotalLines,
            adjustment.TotalQuantityChange,
            adjustment.TotalValueChange,
            adjustment.CreatedAt,
            adjustment.UpdatedAt,
            adjustment.Lines?.Select(MapLineToDto).ToList()
        );
    }

    private StockAdjustmentLineDto MapLineToDto(StockAdjustmentLine line)
    {
        return new StockAdjustmentLineDto(
            line.Id,
            line.AdjustmentId,
            line.LineNumber,
            line.ProductId,
            line.Sku,
            line.Product?.Name,
            line.LocationId,
            line.Location?.Code,
            line.BatchNumber,
            line.SerialNumber,
            line.QuantityBefore,
            line.QuantityAdjustment,
            line.QuantityAfter,
            line.UnitCost,
            line.ValueChange,
            line.ReasonCodeId,
            line.ReasonCode?.Name,
            line.ReasonNotes,
            line.IsProcessed,
            line.ProcessedAt
        );
    }

    #endregion
}
