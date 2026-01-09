using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Transfers.DTOs;
using SmartWMS.API.Modules.Transfers.Models;

namespace SmartWMS.API.Modules.Transfers.Services;

public class TransfersService : ITransfersService
{
    private readonly ApplicationDbContext _context;

    public TransfersService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Stock Transfers CRUD

    public async Task<ApiResponse<PaginatedResult<StockTransferSummaryDto>>> GetTransfersAsync(
        Guid tenantId, TransferFilterRequest filter)
    {
        var query = _context.StockTransfers
            .Include(t => t.FromWarehouse)
            .Include(t => t.ToWarehouse)
            .Where(t => t.TenantId == tenantId);

        // Apply filters
        if (filter.FromWarehouseId.HasValue)
            query = query.Where(t => t.FromWarehouseId == filter.FromWarehouseId.Value);

        if (filter.ToWarehouseId.HasValue)
            query = query.Where(t => t.ToWarehouseId == filter.ToWarehouseId.Value);

        if (filter.TransferType.HasValue)
            query = query.Where(t => t.TransferType == filter.TransferType.Value);

        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status.Value);

        if (filter.Priority.HasValue)
            query = query.Where(t => t.Priority == filter.Priority.Value);

        if (filter.AssignedToUserId.HasValue)
            query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId.Value);

        if (filter.DateFrom.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(t =>
                t.TransferNumber.ToLower().Contains(term) ||
                (t.Notes != null && t.Notes.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(t => new StockTransferSummaryDto(
                t.Id,
                t.TransferNumber,
                t.TransferType,
                t.FromWarehouse.Name,
                t.ToWarehouse.Name,
                t.Status,
                t.Priority,
                t.ScheduledDate,
                t.TotalLines,
                t.TotalQuantity,
                null, // AssignedToUserName
                t.CreatedAt
            ))
            .ToListAsync();

        var result = new PaginatedResult<StockTransferSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        return ApiResponse<PaginatedResult<StockTransferSummaryDto>>.Ok(result);
    }

    public async Task<ApiResponse<StockTransferDto>> GetTransferByIdAsync(Guid tenantId, Guid id)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.FromWarehouse)
            .Include(t => t.FromZone)
            .Include(t => t.ToWarehouse)
            .Include(t => t.ToZone)
            .Include(t => t.ReasonCode)
            .Include(t => t.Lines)
                .ThenInclude(l => l.Product)
            .Include(t => t.Lines)
                .ThenInclude(l => l.FromLocation)
            .Include(t => t.Lines)
                .ThenInclude(l => l.ToLocation)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (transfer == null)
            return ApiResponse<StockTransferDto>.Fail("Transfer not found");

        return ApiResponse<StockTransferDto>.Ok(MapToDto(transfer));
    }

    public async Task<ApiResponse<StockTransferDto>> GetTransferByNumberAsync(Guid tenantId, string transferNumber)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.FromWarehouse)
            .Include(t => t.ToWarehouse)
            .Include(t => t.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.TransferNumber == transferNumber);

        if (transfer == null)
            return ApiResponse<StockTransferDto>.Fail("Transfer not found");

        return ApiResponse<StockTransferDto>.Ok(MapToDto(transfer));
    }

    public async Task<ApiResponse<StockTransferDto>> CreateTransferAsync(
        Guid tenantId, Guid userId, CreateStockTransferRequest request)
    {
        // Validate warehouses
        var fromWarehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == request.FromWarehouseId);
        if (fromWarehouse == null)
            return ApiResponse<StockTransferDto>.Fail("Source warehouse not found");

        var toWarehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == request.ToWarehouseId);
        if (toWarehouse == null)
            return ApiResponse<StockTransferDto>.Fail("Destination warehouse not found");

        // Generate transfer number
        var transferNumber = await GenerateTransferNumberAsync(tenantId);

        var transfer = new StockTransfer
        {
            TenantId = tenantId,
            TransferNumber = transferNumber,
            TransferType = request.TransferType,
            FromWarehouseId = request.FromWarehouseId,
            FromZoneId = request.FromZoneId,
            ToWarehouseId = request.ToWarehouseId,
            ToZoneId = request.ToZoneId,
            Priority = request.Priority,
            ScheduledDate = request.ScheduledDate,
            RequiredByDate = request.RequiredByDate,
            ReasonCodeId = request.ReasonCodeId,
            ReasonNotes = request.ReasonNotes,
            SourceDocumentType = request.SourceDocumentType,
            SourceDocumentId = request.SourceDocumentId,
            SourceDocumentNumber = request.SourceDocumentNumber,
            Notes = request.Notes,
            CreatedByUserId = userId,
            Status = TransferStatus.Draft
        };

        _context.StockTransfers.Add(transfer);

        // Add lines if provided
        if (request.Lines?.Any() == true)
        {
            int lineNumber = 1;
            foreach (var lineRequest in request.Lines)
            {
                var line = await CreateLineFromRequest(tenantId, transfer.Id, lineNumber++, lineRequest);
                if (line == null)
                    return ApiResponse<StockTransferDto>.Fail($"Invalid product or location in line {lineNumber - 1}");

                transfer.Lines.Add(line);
            }

            UpdateTransferTotals(transfer);
        }

        await _context.SaveChangesAsync();

        return await GetTransferByIdAsync(tenantId, transfer.Id);
    }

    public async Task<ApiResponse<StockTransferDto>> UpdateTransferAsync(
        Guid tenantId, Guid id, UpdateStockTransferRequest request)
    {
        var transfer = await _context.StockTransfers
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (transfer == null)
            return ApiResponse<StockTransferDto>.Fail("Transfer not found");

        if (transfer.Status != TransferStatus.Draft && transfer.Status != TransferStatus.Requested)
            return ApiResponse<StockTransferDto>.Fail("Can only update transfers in Draft or Requested status");

        if (request.Priority.HasValue)
            transfer.Priority = request.Priority.Value;
        if (request.ScheduledDate.HasValue)
            transfer.ScheduledDate = request.ScheduledDate;
        if (request.RequiredByDate.HasValue)
            transfer.RequiredByDate = request.RequiredByDate;
        if (request.ReasonCodeId.HasValue)
            transfer.ReasonCodeId = request.ReasonCodeId;
        if (request.ReasonNotes != null)
            transfer.ReasonNotes = request.ReasonNotes;
        if (request.Notes != null)
            transfer.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return await GetTransferByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<bool>> DeleteTransferAsync(Guid tenantId, Guid id)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (transfer == null)
            return ApiResponse<bool>.Fail("Transfer not found");

        if (transfer.Status != TransferStatus.Draft)
            return ApiResponse<bool>.Fail("Can only delete transfers in Draft status");

        _context.StockTransferLines.RemoveRange(transfer.Lines);
        _context.StockTransfers.Remove(transfer);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true);
    }

    #endregion

    #region Transfer Lines

    public async Task<ApiResponse<StockTransferLineDto>> AddLineAsync(
        Guid tenantId, Guid transferId, AddTransferLineRequest request)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == transferId);

        if (transfer == null)
            return ApiResponse<StockTransferLineDto>.Fail("Transfer not found");

        if (transfer.Status != TransferStatus.Draft && transfer.Status != TransferStatus.Requested)
            return ApiResponse<StockTransferLineDto>.Fail("Cannot add lines to transfers in progress");

        var lineNumber = transfer.Lines.Count > 0 ? transfer.Lines.Max(l => l.LineNumber) + 1 : 1;

        var line = await CreateLineFromRequest(tenantId, transferId, lineNumber, new CreateTransferLineRequest(
            request.ProductId,
            request.Sku,
            request.FromLocationId,
            request.ToLocationId,
            request.BatchNumber,
            request.SerialNumber,
            request.QuantityRequested,
            request.Notes
        ));

        if (line == null)
            return ApiResponse<StockTransferLineDto>.Fail("Invalid product or location");

        _context.StockTransferLines.Add(line);
        transfer.Lines.Add(line);
        UpdateTransferTotals(transfer);

        await _context.SaveChangesAsync();

        await _context.Entry(line).Reference(l => l.Product).LoadAsync();
        await _context.Entry(line).Reference(l => l.FromLocation).LoadAsync();
        await _context.Entry(line).Reference(l => l.ToLocation).LoadAsync();

        return ApiResponse<StockTransferLineDto>.Ok(MapLineToDto(line));
    }

    public async Task<ApiResponse<StockTransferLineDto>> UpdateLineAsync(
        Guid tenantId, Guid transferId, Guid lineId, UpdateTransferLineRequest request)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == transferId);

        if (transfer == null)
            return ApiResponse<StockTransferLineDto>.Fail("Transfer not found");

        if (transfer.Status != TransferStatus.Draft && transfer.Status != TransferStatus.Requested)
            return ApiResponse<StockTransferLineDto>.Fail("Cannot update lines in transfers in progress");

        var line = transfer.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse<StockTransferLineDto>.Fail("Line not found");

        line.QuantityRequested = request.QuantityRequested;
        line.Notes = request.Notes;

        UpdateTransferTotals(transfer);

        await _context.SaveChangesAsync();

        await _context.Entry(line).Reference(l => l.Product).LoadAsync();
        await _context.Entry(line).Reference(l => l.FromLocation).LoadAsync();
        await _context.Entry(line).Reference(l => l.ToLocation).LoadAsync();

        return ApiResponse<StockTransferLineDto>.Ok(MapLineToDto(line));
    }

    public async Task<ApiResponse<bool>> RemoveLineAsync(Guid tenantId, Guid transferId, Guid lineId)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == transferId);

        if (transfer == null)
            return ApiResponse<bool>.Fail("Transfer not found");

        if (transfer.Status != TransferStatus.Draft)
            return ApiResponse<bool>.Fail("Can only remove lines from Draft transfers");

        var line = transfer.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse<bool>.Fail("Line not found");

        transfer.Lines.Remove(line);
        _context.StockTransferLines.Remove(line);
        UpdateTransferTotals(transfer);

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true);
    }

    #endregion

    #region Workflow Actions

    public async Task<ApiResponse<StockTransferDto>> ReleaseAsync(Guid tenantId, Guid id, Guid userId)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (transfer == null)
            return ApiResponse<StockTransferDto>.Fail("Transfer not found");

        if (transfer.Status != TransferStatus.Draft && transfer.Status != TransferStatus.Requested)
            return ApiResponse<StockTransferDto>.Fail("Can only release Draft or Requested transfers");

        if (!transfer.Lines.Any())
            return ApiResponse<StockTransferDto>.Fail("Cannot release transfer without lines");

        transfer.Status = TransferStatus.Released;

        await _context.SaveChangesAsync();

        return await GetTransferByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<StockTransferDto>> AssignAsync(
        Guid tenantId, Guid id, AssignTransferRequest request)
    {
        var transfer = await _context.StockTransfers
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (transfer == null)
            return ApiResponse<StockTransferDto>.Fail("Transfer not found");

        transfer.AssignedToUserId = request.AssignedToUserId;

        await _context.SaveChangesAsync();

        return await GetTransferByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<StockTransferDto>> StartPickingAsync(Guid tenantId, Guid id, Guid userId)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (transfer == null)
            return ApiResponse<StockTransferDto>.Fail("Transfer not found");

        if (transfer.Status != TransferStatus.Released)
            return ApiResponse<StockTransferDto>.Fail("Can only start picking Released transfers");

        transfer.Status = TransferStatus.InProgress;
        transfer.PickedByUserId = userId;

        foreach (var line in transfer.Lines)
        {
            line.Status = TransferLineStatus.Allocated;
        }

        await _context.SaveChangesAsync();

        return await GetTransferByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<StockTransferLineDto>> PickLineAsync(
        Guid tenantId, Guid transferId, Guid lineId, Guid userId, PickLineRequest request)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == transferId);

        if (transfer == null)
            return ApiResponse<StockTransferLineDto>.Fail("Transfer not found");

        if (transfer.Status != TransferStatus.InProgress)
            return ApiResponse<StockTransferLineDto>.Fail("Transfer must be In Progress to pick lines");

        var line = transfer.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse<StockTransferLineDto>.Fail("Line not found");

        // Validate stock availability
        var stockLevel = await _context.StockLevels
            .FirstOrDefaultAsync(s => s.TenantId == tenantId &&
                s.ProductId == line.ProductId &&
                s.LocationId == line.FromLocationId &&
                s.BatchNumber == line.BatchNumber);

        if (stockLevel == null || stockLevel.QuantityAvailable < request.QuantityPicked)
        {
            return ApiResponse<StockTransferLineDto>.Fail(
                $"Insufficient stock available at source location. Available: {stockLevel?.QuantityAvailable ?? 0}");
        }

        line.QuantityPicked = request.QuantityPicked;
        line.PickedAt = DateTime.UtcNow;
        line.PickedByUserId = userId;
        line.Status = line.QuantityPicked >= line.QuantityRequested
            ? TransferLineStatus.Picked
            : TransferLineStatus.PartiallyPicked;
        line.Notes = request.Notes;

        // Reserve the stock
        stockLevel.QuantityReserved += request.QuantityPicked;

        // Update transfer totals
        transfer.PickedLines = transfer.Lines.Count(l => l.Status == TransferLineStatus.Picked);

        await _context.SaveChangesAsync();

        await _context.Entry(line).Reference(l => l.Product).LoadAsync();
        await _context.Entry(line).Reference(l => l.FromLocation).LoadAsync();
        await _context.Entry(line).Reference(l => l.ToLocation).LoadAsync();

        return ApiResponse<StockTransferLineDto>.Ok(MapLineToDto(line));
    }

    public async Task<ApiResponse<StockTransferDto>> CompletePickingAsync(
        Guid tenantId, Guid id, Guid userId, CompletePickingRequest request)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (transfer == null)
            return ApiResponse<StockTransferDto>.Fail("Transfer not found");

        if (transfer.Status != TransferStatus.InProgress)
            return ApiResponse<StockTransferDto>.Fail("Transfer must be In Progress to complete picking");

        var unpickedLines = transfer.Lines.Count(l => l.QuantityPicked == 0);
        if (unpickedLines > 0)
            return ApiResponse<StockTransferDto>.Fail($"{unpickedLines} lines have not been picked");

        transfer.Status = TransferStatus.Picked;
        transfer.PickedAt = DateTime.UtcNow;
        transfer.PickedByUserId = userId;

        await _context.SaveChangesAsync();

        return await GetTransferByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<StockTransferDto>> MarkInTransitAsync(Guid tenantId, Guid id, Guid userId)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (transfer == null)
            return ApiResponse<StockTransferDto>.Fail("Transfer not found");

        if (transfer.Status != TransferStatus.Picked)
            return ApiResponse<StockTransferDto>.Fail("Can only mark Picked transfers as In Transit");

        // Deduct stock from source locations
        foreach (var line in transfer.Lines.Where(l => l.QuantityPicked > 0))
        {
            var stockLevel = await _context.StockLevels
                .FirstOrDefaultAsync(s => s.TenantId == tenantId &&
                    s.ProductId == line.ProductId &&
                    s.LocationId == line.FromLocationId &&
                    s.BatchNumber == line.BatchNumber);

            if (stockLevel != null)
            {
                stockLevel.QuantityOnHand -= line.QuantityPicked;
                stockLevel.QuantityReserved -= line.QuantityPicked;
                stockLevel.LastMovementAt = DateTime.UtcNow;

                // Create outbound movement
                var movementNumber = $"MOV-{DateTime.UtcNow:yyyyMMddHHmmss}-{line.LineNumber:D3}";
                var movement = new Inventory.Models.StockMovement
                {
                    TenantId = tenantId,
                    MovementNumber = movementNumber,
                    ProductId = line.ProductId,
                    Sku = line.Sku,
                    FromLocationId = line.FromLocationId,
                    ToLocationId = null,
                    MovementType = MovementType.Transfer,
                    Quantity = line.QuantityPicked,
                    BatchNumber = line.BatchNumber,
                    ReferenceType = "StockTransfer",
                    ReferenceId = transfer.Id,
                    ReferenceNumber = transfer.TransferNumber,
                    Notes = $"Transfer out to {transfer.ToWarehouse?.Name ?? "destination"}"
                };
                _context.StockMovements.Add(movement);
            }

            line.Status = TransferLineStatus.InTransit;
        }

        transfer.Status = TransferStatus.InTransit;

        await _context.SaveChangesAsync();

        return await GetTransferByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<StockTransferLineDto>> ReceiveLineAsync(
        Guid tenantId, Guid transferId, Guid lineId, Guid userId, ReceiveLineRequest request)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == transferId);

        if (transfer == null)
            return ApiResponse<StockTransferLineDto>.Fail("Transfer not found");

        if (transfer.Status != TransferStatus.InTransit && transfer.Status != TransferStatus.Received)
            return ApiResponse<StockTransferLineDto>.Fail("Transfer must be In Transit to receive lines");

        var line = transfer.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse<StockTransferLineDto>.Fail("Line not found");

        line.QuantityReceived = request.QuantityReceived;
        line.ReceivedAt = DateTime.UtcNow;
        line.ReceivedByUserId = userId;
        line.Status = line.QuantityReceived >= line.QuantityPicked
            ? TransferLineStatus.Received
            : TransferLineStatus.PartiallyReceived;
        line.Notes = request.Notes;

        // Update transfer totals
        transfer.ReceivedLines = transfer.Lines.Count(l => l.Status == TransferLineStatus.Received);

        // Add stock to destination location
        var stockLevel = await _context.StockLevels
            .FirstOrDefaultAsync(s => s.TenantId == tenantId &&
                s.ProductId == line.ProductId &&
                s.LocationId == line.ToLocationId &&
                s.BatchNumber == line.BatchNumber);

        if (stockLevel == null)
        {
            stockLevel = new Inventory.Models.StockLevel
            {
                TenantId = tenantId,
                ProductId = line.ProductId,
                Sku = line.Sku,
                LocationId = line.ToLocationId,
                BatchNumber = line.BatchNumber,
                SerialNumber = line.SerialNumber,
                QuantityOnHand = request.QuantityReceived,
                LastMovementAt = DateTime.UtcNow
            };
            _context.StockLevels.Add(stockLevel);
        }
        else
        {
            stockLevel.QuantityOnHand += request.QuantityReceived;
            stockLevel.LastMovementAt = DateTime.UtcNow;
        }

        // Create inbound movement
        var movementNumber = $"MOV-{DateTime.UtcNow:yyyyMMddHHmmss}-R{line.LineNumber:D3}";
        var movement = new Inventory.Models.StockMovement
        {
            TenantId = tenantId,
            MovementNumber = movementNumber,
            ProductId = line.ProductId,
            Sku = line.Sku,
            FromLocationId = null,
            ToLocationId = line.ToLocationId,
            MovementType = MovementType.Transfer,
            Quantity = request.QuantityReceived,
            BatchNumber = line.BatchNumber,
            ReferenceType = "StockTransfer",
            ReferenceId = transfer.Id,
            ReferenceNumber = transfer.TransferNumber,
            Notes = $"Transfer in from {transfer.FromWarehouse?.Name ?? "source"}"
        };
        _context.StockMovements.Add(movement);

        await _context.SaveChangesAsync();

        await _context.Entry(line).Reference(l => l.Product).LoadAsync();
        await _context.Entry(line).Reference(l => l.FromLocation).LoadAsync();
        await _context.Entry(line).Reference(l => l.ToLocation).LoadAsync();

        return ApiResponse<StockTransferLineDto>.Ok(MapLineToDto(line));
    }

    public async Task<ApiResponse<StockTransferDto>> CompleteReceivingAsync(
        Guid tenantId, Guid id, Guid userId, CompleteReceivingRequest request)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (transfer == null)
            return ApiResponse<StockTransferDto>.Fail("Transfer not found");

        if (transfer.Status != TransferStatus.InTransit && transfer.Status != TransferStatus.Received)
            return ApiResponse<StockTransferDto>.Fail("Transfer must be In Transit or Received to complete");

        transfer.Status = TransferStatus.Complete;
        transfer.ReceivedAt = DateTime.UtcNow;
        transfer.ReceivedByUserId = userId;

        await _context.SaveChangesAsync();

        return await GetTransferByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<StockTransferDto>> CancelAsync(Guid tenantId, Guid id, Guid userId)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (transfer == null)
            return ApiResponse<StockTransferDto>.Fail("Transfer not found");

        if (transfer.Status == TransferStatus.Complete)
            return ApiResponse<StockTransferDto>.Fail("Cannot cancel completed transfers");

        if (transfer.Status == TransferStatus.InTransit)
            return ApiResponse<StockTransferDto>.Fail("Cannot cancel transfers in transit");

        // Release any reserved stock
        foreach (var line in transfer.Lines.Where(l => l.QuantityPicked > 0))
        {
            var stockLevel = await _context.StockLevels
                .FirstOrDefaultAsync(s => s.TenantId == tenantId &&
                    s.ProductId == line.ProductId &&
                    s.LocationId == line.FromLocationId &&
                    s.BatchNumber == line.BatchNumber);

            if (stockLevel != null && stockLevel.QuantityReserved > 0)
            {
                stockLevel.QuantityReserved -= line.QuantityPicked;
            }

            line.Status = TransferLineStatus.Cancelled;
        }

        transfer.Status = TransferStatus.Cancelled;

        await _context.SaveChangesAsync();

        return await GetTransferByIdAsync(tenantId, id);
    }

    #endregion

    #region Replenishment & Quick Transfer

    public async Task<ApiResponse<StockTransferDto>> CreateReplenishmentAsync(
        Guid tenantId, Guid userId, ReplenishmentRequest request)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ProductId);

        if (product == null)
            return ApiResponse<StockTransferDto>.Fail("Product not found");

        var createRequest = new CreateStockTransferRequest(
            TransferType.Replenishment,
            request.WarehouseId,
            null,
            request.WarehouseId,
            null,
            request.Priority,
            null,
            null,
            null,
            request.Notes,
            "Replenishment",
            null,
            null,
            null,
            new List<CreateTransferLineRequest>
            {
                new CreateTransferLineRequest(
                    request.ProductId,
                    product.Sku,
                    request.FromLocationId,
                    request.ToLocationId,
                    null,
                    null,
                    request.Quantity,
                    request.Notes
                )
            }
        );

        return await CreateTransferAsync(tenantId, userId, createRequest);
    }

    public async Task<ApiResponse<StockTransferDto>> QuickTransferAsync(
        Guid tenantId, Guid userId, CreateStockTransferRequest request)
    {
        // Create transfer
        var createResult = await CreateTransferAsync(tenantId, userId, request);
        if (!createResult.Success)
            return createResult;

        var transfer = createResult.Data!;

        // Release
        var releaseResult = await ReleaseAsync(tenantId, transfer.Id, userId);
        if (!releaseResult.Success)
            return releaseResult;

        // Start picking
        var startResult = await StartPickingAsync(tenantId, transfer.Id, userId);
        if (!startResult.Success)
            return startResult;

        // Pick all lines
        var transferEntity = await _context.StockTransfers
            .Include(t => t.Lines)
            .FirstAsync(t => t.Id == transfer.Id);

        foreach (var line in transferEntity.Lines)
        {
            await PickLineAsync(tenantId, transfer.Id, line.Id, userId,
                new PickLineRequest(line.QuantityRequested, null));
        }

        // Complete picking
        await CompletePickingAsync(tenantId, transfer.Id, userId, new CompletePickingRequest(null));

        // Mark in transit
        await MarkInTransitAsync(tenantId, transfer.Id, userId);

        // Receive all lines
        foreach (var line in transferEntity.Lines)
        {
            await ReceiveLineAsync(tenantId, transfer.Id, line.Id, userId,
                new ReceiveLineRequest(line.QuantityPicked, null));
        }

        // Complete receiving
        return await CompleteReceivingAsync(tenantId, transfer.Id, userId, new CompleteReceivingRequest(null));
    }

    #endregion

    #region Private Helpers

    private async Task<string> GenerateTransferNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"TRF-{today:yyyyMMdd}-";

        var lastNumber = await _context.StockTransfers
            .Where(t => t.TenantId == tenantId && t.TransferNumber.StartsWith(prefix))
            .OrderByDescending(t => t.TransferNumber)
            .Select(t => t.TransferNumber)
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

    private async Task<StockTransferLine?> CreateLineFromRequest(
        Guid tenantId, Guid transferId, int lineNumber, CreateTransferLineRequest request)
    {
        // Validate product
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ProductId);

        if (product == null)
            return null;

        // Validate locations
        var fromLocation = await _context.Locations
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.FromLocationId);

        if (fromLocation == null)
            return null;

        var toLocation = await _context.Locations
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.ToLocationId);

        if (toLocation == null)
            return null;

        return new StockTransferLine
        {
            TenantId = tenantId,
            TransferId = transferId,
            LineNumber = lineNumber,
            ProductId = request.ProductId,
            Sku = request.Sku,
            FromLocationId = request.FromLocationId,
            ToLocationId = request.ToLocationId,
            BatchNumber = request.BatchNumber,
            SerialNumber = request.SerialNumber,
            QuantityRequested = request.QuantityRequested,
            Notes = request.Notes
        };
    }

    private void UpdateTransferTotals(StockTransfer transfer)
    {
        transfer.TotalLines = transfer.Lines.Count;
        transfer.TotalQuantity = transfer.Lines.Sum(l => l.QuantityRequested);
        transfer.PickedLines = transfer.Lines.Count(l =>
            l.Status == TransferLineStatus.Picked ||
            l.Status == TransferLineStatus.InTransit ||
            l.Status == TransferLineStatus.Received);
        transfer.ReceivedLines = transfer.Lines.Count(l => l.Status == TransferLineStatus.Received);
    }

    private StockTransferDto MapToDto(StockTransfer transfer)
    {
        return new StockTransferDto(
            transfer.Id,
            transfer.TransferNumber,
            transfer.TransferType,
            transfer.FromWarehouseId,
            transfer.FromWarehouse?.Name,
            transfer.FromZoneId,
            transfer.FromZone?.Name,
            transfer.ToWarehouseId,
            transfer.ToWarehouse?.Name,
            transfer.ToZoneId,
            transfer.ToZone?.Name,
            transfer.Status,
            transfer.Priority,
            transfer.ScheduledDate,
            transfer.RequiredByDate,
            transfer.ReasonCodeId,
            transfer.ReasonCode?.Name,
            transfer.ReasonNotes,
            transfer.SourceDocumentType,
            transfer.SourceDocumentId,
            transfer.SourceDocumentNumber,
            transfer.CreatedByUserId,
            null, // CreatedByUserName
            transfer.AssignedToUserId,
            null, // AssignedToUserName
            transfer.PickedByUserId,
            transfer.PickedAt,
            transfer.ReceivedByUserId,
            transfer.ReceivedAt,
            transfer.Notes,
            transfer.TotalLines,
            transfer.TotalQuantity,
            transfer.PickedLines,
            transfer.ReceivedLines,
            transfer.CreatedAt,
            transfer.UpdatedAt,
            transfer.Lines?.Select(MapLineToDto).ToList()
        );
    }

    private StockTransferLineDto MapLineToDto(StockTransferLine line)
    {
        return new StockTransferLineDto(
            line.Id,
            line.TransferId,
            line.LineNumber,
            line.ProductId,
            line.Sku,
            line.Product?.Name,
            line.FromLocationId,
            line.FromLocation?.Code,
            line.ToLocationId,
            line.ToLocation?.Code,
            line.BatchNumber,
            line.SerialNumber,
            line.QuantityRequested,
            line.QuantityPicked,
            line.QuantityReceived,
            line.QuantityVariance,
            line.Status,
            line.PickedAt,
            line.ReceivedAt,
            line.Notes
        );
    }

    #endregion
}
