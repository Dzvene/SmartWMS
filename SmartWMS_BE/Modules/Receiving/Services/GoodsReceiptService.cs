using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Receiving.DTOs;
using SmartWMS.API.Modules.Receiving.Models;
using SmartWMS.API.Modules.Automation.Services;

namespace SmartWMS.API.Modules.Receiving.Services;

public class GoodsReceiptService : IGoodsReceiptService
{
    private readonly ApplicationDbContext _context;
    private readonly IAutomationEventPublisher _automationEvents;

    public GoodsReceiptService(ApplicationDbContext context, IAutomationEventPublisher automationEvents)
    {
        _context = context;
        _automationEvents = automationEvents;
    }

    public async Task<ApiResponse<PaginatedResult<GoodsReceiptDto>>> GetReceiptsAsync(
        Guid tenantId,
        GoodsReceiptFilters? filters = null,
        int page = 1,
        int pageSize = 25)
    {
        var query = _context.GoodsReceipts
            .Include(r => r.Warehouse)
            .Include(r => r.Supplier)
            .Include(r => r.PurchaseOrder)
            .Where(r => r.TenantId == tenantId)
            .AsQueryable();

        if (filters != null)
        {
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var search = filters.Search.ToLower();
                query = query.Where(r =>
                    r.ReceiptNumber.ToLower().Contains(search) ||
                    (r.Supplier != null && r.Supplier.Name.ToLower().Contains(search)) ||
                    (r.PurchaseOrder != null && r.PurchaseOrder.OrderNumber.ToLower().Contains(search)));
            }

            if (filters.Status.HasValue)
                query = query.Where(r => r.Status == filters.Status.Value);

            if (filters.WarehouseId.HasValue)
                query = query.Where(r => r.WarehouseId == filters.WarehouseId.Value);

            if (filters.SupplierId.HasValue)
                query = query.Where(r => r.SupplierId == filters.SupplierId.Value);

            if (filters.PurchaseOrderId.HasValue)
                query = query.Where(r => r.PurchaseOrderId == filters.PurchaseOrderId.Value);

            if (filters.ReceiptDateFrom.HasValue)
                query = query.Where(r => r.ReceiptDate >= filters.ReceiptDateFrom.Value);

            if (filters.ReceiptDateTo.HasValue)
                query = query.Where(r => r.ReceiptDate <= filters.ReceiptDateTo.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.ReceiptDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => MapToDto(r, false))
            .ToListAsync();

        var result = new PaginatedResult<GoodsReceiptDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<GoodsReceiptDto>>.Ok(result);
    }

    public async Task<ApiResponse<GoodsReceiptDto>> GetReceiptByIdAsync(Guid tenantId, Guid id, bool includeLines = true)
    {
        var query = _context.GoodsReceipts
            .Include(r => r.Warehouse)
            .Include(r => r.Supplier)
            .Include(r => r.PurchaseOrder)
            .Include(r => r.ReceivingLocation)
            .AsQueryable();

        if (includeLines)
        {
            query = query.Include(r => r.Lines)
                .ThenInclude(l => l.Product);
        }

        var receipt = await query.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (receipt == null)
            return ApiResponse<GoodsReceiptDto>.Fail("Goods receipt not found");

        return ApiResponse<GoodsReceiptDto>.Ok(MapToDto(receipt, includeLines));
    }

    public async Task<ApiResponse<GoodsReceiptDto>> GetReceiptByNumberAsync(Guid tenantId, string receiptNumber, bool includeLines = true)
    {
        var query = _context.GoodsReceipts
            .Include(r => r.Warehouse)
            .Include(r => r.Supplier)
            .Include(r => r.PurchaseOrder)
            .Include(r => r.ReceivingLocation)
            .AsQueryable();

        if (includeLines)
        {
            query = query.Include(r => r.Lines)
                .ThenInclude(l => l.Product);
        }

        var receipt = await query.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.ReceiptNumber == receiptNumber);

        if (receipt == null)
            return ApiResponse<GoodsReceiptDto>.Fail("Goods receipt not found");

        return ApiResponse<GoodsReceiptDto>.Ok(MapToDto(receipt, includeLines));
    }

    public async Task<ApiResponse<GoodsReceiptDto>> CreateReceiptAsync(Guid tenantId, CreateGoodsReceiptRequest request)
    {
        // Validate warehouse
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == request.WarehouseId);

        if (warehouse == null)
            return ApiResponse<GoodsReceiptDto>.Fail("Warehouse not found");

        // Validate supplier if provided
        if (request.SupplierId.HasValue)
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == request.SupplierId.Value);

            if (supplier == null)
                return ApiResponse<GoodsReceiptDto>.Fail("Supplier not found");
        }

        // Validate PO if provided
        if (request.PurchaseOrderId.HasValue)
        {
            var po = await _context.PurchaseOrders
                .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.PurchaseOrderId.Value);

            if (po == null)
                return ApiResponse<GoodsReceiptDto>.Fail("Purchase order not found");
        }

        // Generate receipt number if not provided
        var receiptNumber = request.ReceiptNumber;
        if (string.IsNullOrWhiteSpace(receiptNumber))
        {
            receiptNumber = await GenerateReceiptNumberAsync(tenantId);
        }
        else
        {
            var exists = await _context.GoodsReceipts
                .AnyAsync(r => r.TenantId == tenantId && r.ReceiptNumber == receiptNumber);

            if (exists)
                return ApiResponse<GoodsReceiptDto>.Fail($"Receipt number '{receiptNumber}' already exists");
        }

        var receipt = new GoodsReceipt
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReceiptNumber = receiptNumber,
            PurchaseOrderId = request.PurchaseOrderId,
            SupplierId = request.SupplierId,
            WarehouseId = request.WarehouseId,
            ReceivingLocationId = request.ReceivingLocationId,
            ReceiptDate = request.ReceiptDate ?? DateTime.UtcNow,
            CarrierName = request.CarrierName,
            TrackingNumber = request.TrackingNumber,
            DeliveryNote = request.DeliveryNote,
            Notes = request.Notes,
            Status = GoodsReceiptStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        // Add lines if provided
        if (request.Lines?.Any() == true)
        {
            var lineNumber = 1;
            foreach (var lineRequest in request.Lines)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == lineRequest.ProductId);

                if (product == null)
                    return ApiResponse<GoodsReceiptDto>.Fail($"Product not found: {lineRequest.ProductId}");

                var line = new GoodsReceiptLine
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReceiptId = receipt.Id,
                    LineNumber = lineNumber++,
                    ProductId = lineRequest.ProductId,
                    Sku = product.Sku,
                    QuantityExpected = lineRequest.QuantityExpected,
                    BatchNumber = lineRequest.BatchNumber,
                    LotNumber = lineRequest.LotNumber,
                    ExpirationDate = lineRequest.ExpirationDate,
                    PurchaseOrderLineId = lineRequest.PurchaseOrderLineId,
                    Notes = lineRequest.Notes,
                    Status = GoodsReceiptLineStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                receipt.Lines.Add(line);
                receipt.TotalLines++;
                receipt.TotalQuantityExpected += lineRequest.QuantityExpected;
            }
        }

        _context.GoodsReceipts.Add(receipt);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(receipt).Reference(r => r.Warehouse).LoadAsync();
        if (receipt.SupplierId.HasValue)
            await _context.Entry(receipt).Reference(r => r.Supplier).LoadAsync();

        // Publish automation event
        await _automationEvents.PublishEntityCreatedAsync(tenantId, receipt);

        return ApiResponse<GoodsReceiptDto>.Ok(MapToDto(receipt, true), "Goods receipt created successfully");
    }

    public async Task<ApiResponse<GoodsReceiptDto>> UpdateReceiptAsync(Guid tenantId, Guid id, UpdateGoodsReceiptRequest request)
    {
        var receipt = await _context.GoodsReceipts
            .Include(r => r.Warehouse)
            .Include(r => r.Supplier)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (receipt == null)
            return ApiResponse<GoodsReceiptDto>.Fail("Goods receipt not found");

        if (receipt.Status != GoodsReceiptStatus.Draft)
            return ApiResponse<GoodsReceiptDto>.Fail($"Cannot update receipt in '{receipt.Status}' status");

        if (request.ReceivingLocationId.HasValue)
            receipt.ReceivingLocationId = request.ReceivingLocationId;

        if (request.CarrierName != null)
            receipt.CarrierName = request.CarrierName;

        if (request.TrackingNumber != null)
            receipt.TrackingNumber = request.TrackingNumber;

        if (request.DeliveryNote != null)
            receipt.DeliveryNote = request.DeliveryNote;

        if (request.Notes != null)
            receipt.Notes = request.Notes;

        receipt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<GoodsReceiptDto>.Ok(MapToDto(receipt, false), "Receipt updated successfully");
    }

    public async Task<ApiResponse> DeleteReceiptAsync(Guid tenantId, Guid id)
    {
        var receipt = await _context.GoodsReceipts
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (receipt == null)
            return ApiResponse.Fail("Goods receipt not found");

        if (receipt.Status != GoodsReceiptStatus.Draft)
            return ApiResponse.Fail("Can only delete receipts in Draft status");

        _context.GoodsReceiptLines.RemoveRange(receipt.Lines);
        _context.GoodsReceipts.Remove(receipt);
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Goods receipt deleted successfully");
    }

    public async Task<ApiResponse<GoodsReceiptDto>> AddLineAsync(Guid tenantId, Guid receiptId, AddGoodsReceiptLineRequest request)
    {
        var receipt = await _context.GoodsReceipts
            .Include(r => r.Lines)
            .Include(r => r.Warehouse)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == receiptId);

        if (receipt == null)
            return ApiResponse<GoodsReceiptDto>.Fail("Goods receipt not found");

        if (receipt.Status != GoodsReceiptStatus.Draft && receipt.Status != GoodsReceiptStatus.InProgress)
            return ApiResponse<GoodsReceiptDto>.Fail($"Cannot add lines to receipt in '{receipt.Status}' status");

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ProductId);

        if (product == null)
            return ApiResponse<GoodsReceiptDto>.Fail("Product not found");

        var maxLineNumber = receipt.Lines.Any() ? receipt.Lines.Max(l => l.LineNumber) : 0;

        var line = new GoodsReceiptLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReceiptId = receiptId,
            LineNumber = maxLineNumber + 1,
            ProductId = request.ProductId,
            Sku = product.Sku,
            QuantityExpected = request.QuantityExpected,
            BatchNumber = request.BatchNumber,
            LotNumber = request.LotNumber,
            ExpirationDate = request.ExpirationDate,
            PurchaseOrderLineId = request.PurchaseOrderLineId,
            Notes = request.Notes,
            Status = GoodsReceiptLineStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        receipt.Lines.Add(line);
        receipt.TotalLines++;
        receipt.TotalQuantityExpected += request.QuantityExpected;
        receipt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<GoodsReceiptDto>.Ok(MapToDto(receipt, true), "Line added successfully");
    }

    public async Task<ApiResponse> RemoveLineAsync(Guid tenantId, Guid receiptId, Guid lineId)
    {
        var receipt = await _context.GoodsReceipts
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == receiptId);

        if (receipt == null)
            return ApiResponse.Fail("Goods receipt not found");

        if (receipt.Status != GoodsReceiptStatus.Draft)
            return ApiResponse.Fail("Can only remove lines from Draft receipts");

        var line = receipt.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse.Fail("Line not found");

        receipt.Lines.Remove(line);
        _context.GoodsReceiptLines.Remove(line);
        receipt.TotalLines--;
        receipt.TotalQuantityExpected -= line.QuantityExpected;
        receipt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Line removed successfully");
    }

    public async Task<ApiResponse<GoodsReceiptDto>> StartReceivingAsync(Guid tenantId, Guid id)
    {
        var receipt = await _context.GoodsReceipts
            .Include(r => r.Lines)
            .Include(r => r.Warehouse)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (receipt == null)
            return ApiResponse<GoodsReceiptDto>.Fail("Goods receipt not found");

        if (receipt.Status != GoodsReceiptStatus.Draft)
            return ApiResponse<GoodsReceiptDto>.Fail($"Cannot start receiving from '{receipt.Status}' status");

        if (!receipt.Lines.Any())
            return ApiResponse<GoodsReceiptDto>.Fail("Cannot start receiving empty receipt");

        var previousStatus = GoodsReceiptStatus.Draft.ToString();
        receipt.Status = GoodsReceiptStatus.InProgress;
        receipt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Publish automation event for status change
        await _automationEvents.PublishStatusChangedAsync(tenantId, receipt, previousStatus, receipt.Status.ToString());

        return ApiResponse<GoodsReceiptDto>.Ok(MapToDto(receipt, true), "Receiving started");
    }

    public async Task<ApiResponse<GoodsReceiptLineDto>> ReceiveLineAsync(Guid tenantId, Guid receiptId, Guid lineId, ReceiveLineRequest request)
    {
        var receipt = await _context.GoodsReceipts
            .Include(r => r.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == receiptId);

        if (receipt == null)
            return ApiResponse<GoodsReceiptLineDto>.Fail("Goods receipt not found");

        if (receipt.Status != GoodsReceiptStatus.InProgress)
            return ApiResponse<GoodsReceiptLineDto>.Fail($"Cannot receive in '{receipt.Status}' status. Start receiving first.");

        var line = receipt.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse<GoodsReceiptLineDto>.Fail("Line not found");

        // Validate quantity
        var totalReceiving = request.QuantityReceived + request.QuantityRejected;
        var remaining = line.QuantityExpected - line.QuantityReceived - line.QuantityRejected;

        if (totalReceiving > remaining)
            return ApiResponse<GoodsReceiptLineDto>.Fail($"Total quantity ({totalReceiving}) exceeds remaining ({remaining})");

        // Update line
        line.QuantityReceived += request.QuantityReceived;
        line.QuantityRejected += request.QuantityRejected;

        if (request.BatchNumber != null) line.BatchNumber = request.BatchNumber;
        if (request.LotNumber != null) line.LotNumber = request.LotNumber;
        if (request.ExpirationDate.HasValue) line.ExpirationDate = request.ExpirationDate;
        if (request.ManufactureDate.HasValue) line.ManufactureDate = request.ManufactureDate;
        if (request.PutawayLocationId.HasValue) line.PutawayLocationId = request.PutawayLocationId;
        if (request.QualityStatus != null) line.QualityStatus = request.QualityStatus;
        if (request.RejectionReason != null) line.RejectionReason = request.RejectionReason;
        if (request.Notes != null) line.Notes = request.Notes;

        // Update line status
        if (line.QuantityReceived + line.QuantityRejected >= line.QuantityExpected)
        {
            line.Status = line.QuantityRejected > 0 && line.QuantityReceived == 0
                ? GoodsReceiptLineStatus.Rejected
                : GoodsReceiptLineStatus.Received;
        }
        else if (line.QuantityReceived > 0)
        {
            line.Status = GoodsReceiptLineStatus.PartiallyReceived;
        }

        line.UpdatedAt = DateTime.UtcNow;

        // Update receipt totals
        receipt.TotalQuantityReceived = receipt.Lines.Sum(l => l.QuantityReceived);
        receipt.UpdatedAt = DateTime.UtcNow;

        // Update linked PO line if exists
        if (line.PurchaseOrderLineId.HasValue)
        {
            var poLine = await _context.PurchaseOrderLines
                .Include(l => l.Order)
                .FirstOrDefaultAsync(l => l.Id == line.PurchaseOrderLineId.Value);

            if (poLine != null)
            {
                poLine.QuantityReceived += request.QuantityReceived;
                poLine.UpdatedAt = DateTime.UtcNow;

                // Update PO totals
                poLine.Order!.ReceivedQuantity = poLine.Order.Lines.Sum(l => l.QuantityReceived);

                // Update PO status
                if (poLine.Order.ReceivedQuantity >= poLine.Order.TotalQuantity)
                {
                    poLine.Order.Status = PurchaseOrderStatus.Received;
                }
                else if (poLine.Order.ReceivedQuantity > 0)
                {
                    poLine.Order.Status = PurchaseOrderStatus.PartiallyReceived;
                }

                poLine.Order.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return ApiResponse<GoodsReceiptLineDto>.Ok(MapLineToDto(line), "Line received successfully");
    }

    public async Task<ApiResponse<GoodsReceiptDto>> CompleteReceiptAsync(Guid tenantId, Guid id)
    {
        var receipt = await _context.GoodsReceipts
            .Include(r => r.Lines)
            .Include(r => r.Warehouse)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (receipt == null)
            return ApiResponse<GoodsReceiptDto>.Fail("Goods receipt not found");

        if (receipt.Status != GoodsReceiptStatus.InProgress)
            return ApiResponse<GoodsReceiptDto>.Fail($"Cannot complete receipt in '{receipt.Status}' status");

        // Check if all lines have been processed
        var pendingLines = receipt.Lines.Count(l => l.Status == GoodsReceiptLineStatus.Pending);
        if (pendingLines > 0)
            return ApiResponse<GoodsReceiptDto>.Fail($"Cannot complete receipt with {pendingLines} pending lines");

        // Determine final status
        var allReceived = receipt.Lines.All(l =>
            l.Status == GoodsReceiptLineStatus.Received || l.Status == GoodsReceiptLineStatus.PutAway);

        var previousStatus = receipt.Status.ToString();
        receipt.Status = allReceived ? GoodsReceiptStatus.Complete : GoodsReceiptStatus.PartiallyComplete;
        receipt.CompletedAt = DateTime.UtcNow;
        receipt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Publish automation event for status change
        await _automationEvents.PublishStatusChangedAsync(tenantId, receipt, previousStatus, receipt.Status.ToString());

        return ApiResponse<GoodsReceiptDto>.Ok(MapToDto(receipt, true), "Receipt completed");
    }

    public async Task<ApiResponse<GoodsReceiptDto>> CancelReceiptAsync(Guid tenantId, Guid id, string? reason = null)
    {
        var receipt = await _context.GoodsReceipts
            .Include(r => r.Lines)
            .Include(r => r.Warehouse)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (receipt == null)
            return ApiResponse<GoodsReceiptDto>.Fail("Goods receipt not found");

        if (receipt.Status == GoodsReceiptStatus.Complete || receipt.Status == GoodsReceiptStatus.Cancelled)
            return ApiResponse<GoodsReceiptDto>.Fail($"Cannot cancel receipt in '{receipt.Status}' status");

        // If any receiving has happened, don't allow cancel
        if (receipt.TotalQuantityReceived > 0)
            return ApiResponse<GoodsReceiptDto>.Fail("Cannot cancel receipt with received quantities. Use Complete instead.");

        var previousStatus = receipt.Status.ToString();
        receipt.Status = GoodsReceiptStatus.Cancelled;

        if (!string.IsNullOrWhiteSpace(reason))
        {
            receipt.Notes = string.IsNullOrWhiteSpace(receipt.Notes)
                ? $"[Cancelled] {reason}"
                : $"{receipt.Notes}\n[Cancelled] {reason}";
        }

        receipt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Publish automation event for status change
        await _automationEvents.PublishStatusChangedAsync(tenantId, receipt, previousStatus, receipt.Status.ToString());

        return ApiResponse<GoodsReceiptDto>.Ok(MapToDto(receipt, false), "Receipt cancelled");
    }

    public async Task<ApiResponse<GoodsReceiptDto>> CreateFromPurchaseOrderAsync(
        Guid tenantId, Guid purchaseOrderId, Guid warehouseId, Guid? receivingLocationId = null)
    {
        var po = await _context.PurchaseOrders
            .Include(p => p.Lines)
                .ThenInclude(l => l.Product)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == purchaseOrderId);

        if (po == null)
            return ApiResponse<GoodsReceiptDto>.Fail("Purchase order not found");

        if (po.Status != PurchaseOrderStatus.Confirmed && po.Status != PurchaseOrderStatus.PartiallyReceived)
            return ApiResponse<GoodsReceiptDto>.Fail($"Cannot receive PO in '{po.Status}' status");

        // Check for remaining quantities
        var hasRemainingQty = po.Lines.Any(l => l.QuantityOrdered - l.QuantityReceived - l.QuantityCancelled > 0);
        if (!hasRemainingQty)
            return ApiResponse<GoodsReceiptDto>.Fail("No remaining quantities to receive on this PO");

        var receiptNumber = await GenerateReceiptNumberAsync(tenantId);

        var receipt = new GoodsReceipt
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReceiptNumber = receiptNumber,
            PurchaseOrderId = purchaseOrderId,
            SupplierId = po.SupplierId,
            WarehouseId = warehouseId,
            ReceivingLocationId = receivingLocationId,
            ReceiptDate = DateTime.UtcNow,
            Status = GoodsReceiptStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        var lineNumber = 1;
        foreach (var poLine in po.Lines)
        {
            var remainingQty = poLine.QuantityOrdered - poLine.QuantityReceived - poLine.QuantityCancelled;
            if (remainingQty <= 0) continue;

            var line = new GoodsReceiptLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ReceiptId = receipt.Id,
                LineNumber = lineNumber++,
                ProductId = poLine.ProductId,
                Sku = poLine.Sku,
                QuantityExpected = remainingQty,
                BatchNumber = poLine.ExpectedBatchNumber,
                ExpirationDate = poLine.ExpectedExpiryDate,
                PurchaseOrderLineId = poLine.Id,
                Status = GoodsReceiptLineStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            receipt.Lines.Add(line);
            receipt.TotalLines++;
            receipt.TotalQuantityExpected += remainingQty;
        }

        _context.GoodsReceipts.Add(receipt);
        await _context.SaveChangesAsync();

        // Reload with nav props
        await _context.Entry(receipt).Reference(r => r.Warehouse).LoadAsync();
        await _context.Entry(receipt).Reference(r => r.Supplier).LoadAsync();
        await _context.Entry(receipt).Reference(r => r.PurchaseOrder).LoadAsync();

        return ApiResponse<GoodsReceiptDto>.Ok(MapToDto(receipt, true), "Goods receipt created from PO");
    }

    #region Private Methods

    private async Task<string> GenerateReceiptNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"GR-{today:yyyyMMdd}-";

        var lastReceipt = await _context.GoodsReceipts
            .Where(r => r.TenantId == tenantId && r.ReceiptNumber.StartsWith(prefix))
            .OrderByDescending(r => r.ReceiptNumber)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (lastReceipt != null)
        {
            var lastNumber = lastReceipt.ReceiptNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumber, out var parsed))
            {
                sequence = parsed + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }

    private static GoodsReceiptDto MapToDto(GoodsReceipt receipt, bool includeLines)
    {
        var dto = new GoodsReceiptDto
        {
            Id = receipt.Id,
            ReceiptNumber = receipt.ReceiptNumber,
            PurchaseOrderId = receipt.PurchaseOrderId,
            PurchaseOrderNumber = receipt.PurchaseOrder?.OrderNumber,
            SupplierId = receipt.SupplierId,
            SupplierName = receipt.Supplier?.Name,
            WarehouseId = receipt.WarehouseId,
            WarehouseCode = receipt.Warehouse?.Code,
            WarehouseName = receipt.Warehouse?.Name,
            ReceivingLocationId = receipt.ReceivingLocationId,
            ReceivingLocationCode = receipt.ReceivingLocation?.Code,
            Status = receipt.Status,
            ReceiptDate = receipt.ReceiptDate,
            CompletedAt = receipt.CompletedAt,
            CarrierName = receipt.CarrierName,
            TrackingNumber = receipt.TrackingNumber,
            DeliveryNote = receipt.DeliveryNote,
            TotalLines = receipt.TotalLines,
            TotalQuantityExpected = receipt.TotalQuantityExpected,
            TotalQuantityReceived = receipt.TotalQuantityReceived,
            Notes = receipt.Notes,
            CreatedAt = receipt.CreatedAt,
            UpdatedAt = receipt.UpdatedAt
        };

        if (includeLines && receipt.Lines?.Any() == true)
        {
            dto.Lines = receipt.Lines
                .OrderBy(l => l.LineNumber)
                .Select(MapLineToDto)
                .ToList();
        }

        return dto;
    }

    private static GoodsReceiptLineDto MapLineToDto(GoodsReceiptLine line)
    {
        return new GoodsReceiptLineDto
        {
            Id = line.Id,
            ReceiptId = line.ReceiptId,
            LineNumber = line.LineNumber,
            ProductId = line.ProductId,
            Sku = line.Sku,
            ProductName = line.Product?.Name,
            QuantityExpected = line.QuantityExpected,
            QuantityReceived = line.QuantityReceived,
            QuantityRejected = line.QuantityRejected,
            BatchNumber = line.BatchNumber,
            LotNumber = line.LotNumber,
            ExpirationDate = line.ExpirationDate,
            PutawayLocationId = line.PutawayLocationId,
            PutawayLocationCode = line.PutawayLocation?.Code,
            Status = line.Status,
            QualityStatus = line.QualityStatus,
            RejectionReason = line.RejectionReason,
            PurchaseOrderLineId = line.PurchaseOrderLineId,
            Notes = line.Notes
        };
    }

    #endregion
}
