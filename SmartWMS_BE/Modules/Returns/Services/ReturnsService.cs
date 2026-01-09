using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Returns.DTOs;
using SmartWMS.API.Modules.Returns.Models;

namespace SmartWMS.API.Modules.Returns.Services;

public class ReturnsService : IReturnsService
{
    private readonly ApplicationDbContext _context;

    public ReturnsService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Queries

    public async Task<ApiResponse<PaginatedResult<ReturnOrderListDto>>> GetReturnOrdersAsync(
        Guid tenantId, ReturnOrderFilters? filters, int page, int pageSize)
    {
        var query = _context.ReturnOrders
            .Include(r => r.Customer)
            .Include(r => r.OriginalSalesOrder)
            .Where(r => r.TenantId == tenantId);

        if (filters != null)
        {
            if (filters.Status.HasValue)
                query = query.Where(r => r.Status == filters.Status.Value);

            if (filters.ReturnType.HasValue)
                query = query.Where(r => r.ReturnType == filters.ReturnType.Value);

            if (filters.CustomerId.HasValue)
                query = query.Where(r => r.CustomerId == filters.CustomerId.Value);

            if (filters.OriginalSalesOrderId.HasValue)
                query = query.Where(r => r.OriginalSalesOrderId == filters.OriginalSalesOrderId.Value);

            if (filters.AssignedToUserId.HasValue)
                query = query.Where(r => r.AssignedToUserId == filters.AssignedToUserId.Value);

            if (!string.IsNullOrEmpty(filters.RmaNumber))
                query = query.Where(r => r.RmaNumber != null && r.RmaNumber.Contains(filters.RmaNumber));

            if (filters.FromDate.HasValue)
                query = query.Where(r => r.CreatedAt >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                query = query.Where(r => r.CreatedAt <= filters.ToDate.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReturnOrderListDto
            {
                Id = r.Id,
                ReturnNumber = r.ReturnNumber,
                OriginalSalesOrderNumber = r.OriginalSalesOrder != null ? r.OriginalSalesOrder.OrderNumber : null,
                CustomerName = r.Customer != null ? r.Customer.Name : null,
                Status = r.Status,
                ReturnType = r.ReturnType,
                RmaNumber = r.RmaNumber,
                TotalLines = r.TotalLines,
                TotalQuantityExpected = r.TotalQuantityExpected,
                TotalQuantityReceived = r.TotalQuantityReceived,
                RequestedDate = r.RequestedDate,
                ReceivedDate = r.ReceivedDate,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var result = new PaginatedResult<ReturnOrderListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<ReturnOrderListDto>>.Ok(result);
    }

    public async Task<ApiResponse<ReturnOrderDto>> GetReturnOrderByIdAsync(Guid tenantId, Guid id)
    {
        var returnOrder = await _context.ReturnOrders
            .Include(r => r.Customer)
            .Include(r => r.OriginalSalesOrder)
            .Include(r => r.ReceivingLocation)
            .Include(r => r.Lines)
                .ThenInclude(l => l.Product)
            .Include(r => r.Lines)
                .ThenInclude(l => l.DispositionLocation)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (returnOrder == null)
            return ApiResponse<ReturnOrderDto>.Fail("Return order not found");

        return ApiResponse<ReturnOrderDto>.Ok(MapToDto(returnOrder));
    }

    public async Task<ApiResponse<ReturnOrderDto>> GetReturnOrderByNumberAsync(Guid tenantId, string returnNumber)
    {
        var returnOrder = await _context.ReturnOrders
            .Include(r => r.Customer)
            .Include(r => r.OriginalSalesOrder)
            .Include(r => r.ReceivingLocation)
            .Include(r => r.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.ReturnNumber == returnNumber);

        if (returnOrder == null)
            return ApiResponse<ReturnOrderDto>.Fail("Return order not found");

        return ApiResponse<ReturnOrderDto>.Ok(MapToDto(returnOrder));
    }

    public async Task<ApiResponse<List<ReturnOrderListDto>>> GetMyReturnsAsync(Guid tenantId, Guid userId)
    {
        var returns = await _context.ReturnOrders
            .Include(r => r.Customer)
            .Include(r => r.OriginalSalesOrder)
            .Where(r => r.TenantId == tenantId &&
                       r.AssignedToUserId == userId &&
                       r.Status != ReturnOrderStatus.Complete &&
                       r.Status != ReturnOrderStatus.Cancelled)
            .OrderBy(r => r.RequestedDate)
            .ThenBy(r => r.CreatedAt)
            .Select(r => new ReturnOrderListDto
            {
                Id = r.Id,
                ReturnNumber = r.ReturnNumber,
                OriginalSalesOrderNumber = r.OriginalSalesOrder != null ? r.OriginalSalesOrder.OrderNumber : null,
                CustomerName = r.Customer != null ? r.Customer.Name : null,
                Status = r.Status,
                ReturnType = r.ReturnType,
                RmaNumber = r.RmaNumber,
                TotalLines = r.TotalLines,
                TotalQuantityExpected = r.TotalQuantityExpected,
                TotalQuantityReceived = r.TotalQuantityReceived,
                RequestedDate = r.RequestedDate,
                ReceivedDate = r.ReceivedDate,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<ReturnOrderListDto>>.Ok(returns);
    }

    #endregion

    #region Return Order CRUD

    public async Task<ApiResponse<ReturnOrderDto>> CreateReturnOrderAsync(
        Guid tenantId, CreateReturnOrderRequest request)
    {
        // Validate customer
        var customerExists = await _context.Customers
            .AnyAsync(c => c.TenantId == tenantId && c.Id == request.CustomerId);

        if (!customerExists)
            return ApiResponse<ReturnOrderDto>.Fail("Customer not found");

        // Validate original sales order if provided
        if (request.OriginalSalesOrderId.HasValue)
        {
            var orderExists = await _context.SalesOrders
                .AnyAsync(o => o.TenantId == tenantId && o.Id == request.OriginalSalesOrderId.Value);

            if (!orderExists)
                return ApiResponse<ReturnOrderDto>.Fail("Original sales order not found");
        }

        var returnOrder = new ReturnOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReturnNumber = await GenerateReturnNumberAsync(tenantId),
            CustomerId = request.CustomerId,
            OriginalSalesOrderId = request.OriginalSalesOrderId,
            ReturnType = request.ReturnType,
            ReasonCodeId = request.ReasonCodeId,
            ReasonDescription = request.ReasonDescription,
            ReceivingLocationId = request.ReceivingLocationId,
            RequestedDate = request.RequestedDate ?? DateTime.UtcNow,
            RmaNumber = request.RmaNumber,
            RmaExpiryDate = request.RmaExpiryDate,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        // Add lines
        var lineNumber = 1;
        foreach (var lineRequest in request.Lines)
        {
            var line = new ReturnOrderLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ReturnOrderId = returnOrder.Id,
                LineNumber = lineNumber++,
                ProductId = lineRequest.ProductId,
                Sku = lineRequest.Sku,
                QuantityExpected = lineRequest.QuantityExpected,
                BatchNumber = lineRequest.BatchNumber,
                SerialNumber = lineRequest.SerialNumber,
                OriginalOrderLineId = lineRequest.OriginalOrderLineId,
                ReasonCodeId = lineRequest.ReasonCodeId,
                ReasonDescription = lineRequest.ReasonDescription,
                Notes = lineRequest.Notes,
                CreatedAt = DateTime.UtcNow
            };
            returnOrder.Lines.Add(line);
        }

        returnOrder.TotalLines = returnOrder.Lines.Count;
        returnOrder.TotalQuantityExpected = returnOrder.Lines.Sum(l => l.QuantityExpected);

        _context.ReturnOrders.Add(returnOrder);
        await _context.SaveChangesAsync();

        return await GetReturnOrderByIdAsync(tenantId, returnOrder.Id);
    }

    public async Task<ApiResponse<ReturnOrderDto>> UpdateReturnOrderAsync(
        Guid tenantId, Guid id, UpdateReturnOrderRequest request)
    {
        var returnOrder = await _context.ReturnOrders
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (returnOrder == null)
            return ApiResponse<ReturnOrderDto>.Fail("Return order not found");

        if (returnOrder.Status == ReturnOrderStatus.Complete ||
            returnOrder.Status == ReturnOrderStatus.Cancelled)
            return ApiResponse<ReturnOrderDto>.Fail("Cannot update completed or cancelled return");

        returnOrder.ReasonCodeId = request.ReasonCodeId;
        returnOrder.ReasonDescription = request.ReasonDescription;
        returnOrder.ReceivingLocationId = request.ReceivingLocationId;
        returnOrder.RmaNumber = request.RmaNumber;
        returnOrder.RmaExpiryDate = request.RmaExpiryDate;
        returnOrder.CarrierCode = request.CarrierCode;
        returnOrder.TrackingNumber = request.TrackingNumber;
        returnOrder.Notes = request.Notes;
        returnOrder.InternalNotes = request.InternalNotes;
        returnOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetReturnOrderByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<bool>> DeleteReturnOrderAsync(Guid tenantId, Guid id)
    {
        var returnOrder = await _context.ReturnOrders
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (returnOrder == null)
            return ApiResponse<bool>.Fail("Return order not found");

        if (returnOrder.Status != ReturnOrderStatus.Pending)
            return ApiResponse<bool>.Fail("Can only delete pending returns");

        _context.ReturnOrders.Remove(returnOrder);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Return order deleted");
    }

    #endregion

    #region Return Order Lines

    public async Task<ApiResponse<ReturnOrderDto>> AddLineAsync(
        Guid tenantId, Guid returnOrderId, AddReturnLineRequest request)
    {
        var returnOrder = await _context.ReturnOrders
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == returnOrderId);

        if (returnOrder == null)
            return ApiResponse<ReturnOrderDto>.Fail("Return order not found");

        if (returnOrder.Status != ReturnOrderStatus.Pending &&
            returnOrder.Status != ReturnOrderStatus.InTransit)
            return ApiResponse<ReturnOrderDto>.Fail("Cannot add lines to return in this status");

        // Validate product
        var productExists = await _context.Products
            .AnyAsync(p => p.TenantId == tenantId && p.Id == request.ProductId);

        if (!productExists)
            return ApiResponse<ReturnOrderDto>.Fail("Product not found");

        var nextLineNumber = returnOrder.Lines.Count > 0
            ? returnOrder.Lines.Max(l => l.LineNumber) + 1
            : 1;

        var line = new ReturnOrderLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReturnOrderId = returnOrderId,
            LineNumber = nextLineNumber,
            ProductId = request.ProductId,
            Sku = request.Sku,
            QuantityExpected = request.QuantityExpected,
            BatchNumber = request.BatchNumber,
            SerialNumber = request.SerialNumber,
            ReasonCodeId = request.ReasonCodeId,
            ReasonDescription = request.ReasonDescription,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.ReturnOrderLines.Add(line);

        returnOrder.TotalLines = returnOrder.Lines.Count + 1;
        returnOrder.TotalQuantityExpected += request.QuantityExpected;
        returnOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetReturnOrderByIdAsync(tenantId, returnOrderId);
    }

    public async Task<ApiResponse<ReturnOrderDto>> RemoveLineAsync(
        Guid tenantId, Guid returnOrderId, Guid lineId)
    {
        var returnOrder = await _context.ReturnOrders
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == returnOrderId);

        if (returnOrder == null)
            return ApiResponse<ReturnOrderDto>.Fail("Return order not found");

        var line = returnOrder.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse<ReturnOrderDto>.Fail("Line not found");

        if (line.QuantityReceived > 0)
            return ApiResponse<ReturnOrderDto>.Fail("Cannot remove line with received quantity");

        _context.ReturnOrderLines.Remove(line);

        returnOrder.TotalLines = returnOrder.Lines.Count - 1;
        returnOrder.TotalQuantityExpected -= line.QuantityExpected;
        returnOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetReturnOrderByIdAsync(tenantId, returnOrderId);
    }

    #endregion

    #region Workflow

    public async Task<ApiResponse<ReturnOrderDto>> AssignReturnAsync(
        Guid tenantId, Guid id, AssignReturnRequest request)
    {
        var returnOrder = await _context.ReturnOrders
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (returnOrder == null)
            return ApiResponse<ReturnOrderDto>.Fail("Return order not found");

        returnOrder.AssignedToUserId = request.UserId;
        returnOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetReturnOrderByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<ReturnOrderDto>> MarkInTransitAsync(
        Guid tenantId, Guid id, SetReturnShippingRequest request)
    {
        var returnOrder = await _context.ReturnOrders
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (returnOrder == null)
            return ApiResponse<ReturnOrderDto>.Fail("Return order not found");

        if (returnOrder.Status != ReturnOrderStatus.Pending)
            return ApiResponse<ReturnOrderDto>.Fail("Return must be in Pending status");

        returnOrder.Status = ReturnOrderStatus.InTransit;
        returnOrder.CarrierCode = request.CarrierCode;
        returnOrder.TrackingNumber = request.TrackingNumber;
        returnOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetReturnOrderByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<ReturnOrderDto>> StartReceivingAsync(Guid tenantId, Guid id)
    {
        var returnOrder = await _context.ReturnOrders
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (returnOrder == null)
            return ApiResponse<ReturnOrderDto>.Fail("Return order not found");

        if (returnOrder.Status != ReturnOrderStatus.Pending &&
            returnOrder.Status != ReturnOrderStatus.InTransit)
            return ApiResponse<ReturnOrderDto>.Fail("Cannot start receiving in current status");

        returnOrder.Status = ReturnOrderStatus.Received;
        returnOrder.ReceivedDate = DateTime.UtcNow;
        returnOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetReturnOrderByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<ReturnOrderDto>> ReceiveLineAsync(
        Guid tenantId, Guid returnOrderId, Guid lineId, ReceiveReturnLineRequest request)
    {
        var returnOrder = await _context.ReturnOrders
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == returnOrderId);

        if (returnOrder == null)
            return ApiResponse<ReturnOrderDto>.Fail("Return order not found");

        if (returnOrder.Status != ReturnOrderStatus.Received &&
            returnOrder.Status != ReturnOrderStatus.InProgress)
            return ApiResponse<ReturnOrderDto>.Fail("Return must be in Received or InProgress status");

        var line = returnOrder.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse<ReturnOrderDto>.Fail("Line not found");

        line.QuantityReceived += request.QuantityReceived;
        line.Condition = request.Condition;
        line.ConditionNotes = request.ConditionNotes;

        if (!string.IsNullOrEmpty(request.BatchNumber))
            line.BatchNumber = request.BatchNumber;
        if (!string.IsNullOrEmpty(request.SerialNumber))
            line.SerialNumber = request.SerialNumber;

        line.UpdatedAt = DateTime.UtcNow;

        // Update totals
        returnOrder.TotalQuantityReceived = returnOrder.Lines.Sum(l => l.QuantityReceived);
        returnOrder.Status = ReturnOrderStatus.InProgress;
        returnOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetReturnOrderByIdAsync(tenantId, returnOrderId);
    }

    public async Task<ApiResponse<ReturnOrderDto>> ProcessLineAsync(
        Guid tenantId, Guid returnOrderId, Guid lineId, ProcessReturnLineRequest request)
    {
        var returnOrder = await _context.ReturnOrders
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == returnOrderId);

        if (returnOrder == null)
            return ApiResponse<ReturnOrderDto>.Fail("Return order not found");

        if (returnOrder.Status != ReturnOrderStatus.InProgress)
            return ApiResponse<ReturnOrderDto>.Fail("Return must be in InProgress status");

        var line = returnOrder.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse<ReturnOrderDto>.Fail("Line not found");

        if (line.QuantityReceived == 0)
            return ApiResponse<ReturnOrderDto>.Fail("Line must be received before processing");

        var totalProcessed = request.QuantityAccepted + request.QuantityRejected;
        if (totalProcessed > line.QuantityReceived)
            return ApiResponse<ReturnOrderDto>.Fail("Total processed cannot exceed received quantity");

        line.QuantityAccepted = request.QuantityAccepted;
        line.QuantityRejected = request.QuantityRejected;
        line.Disposition = request.Disposition;
        line.DispositionLocationId = request.DispositionLocationId;

        if (!string.IsNullOrEmpty(request.Notes))
            line.Notes = string.IsNullOrEmpty(line.Notes)
                ? request.Notes
                : $"{line.Notes}\n{request.Notes}";

        line.UpdatedAt = DateTime.UtcNow;
        returnOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetReturnOrderByIdAsync(tenantId, returnOrderId);
    }

    public async Task<ApiResponse<ReturnOrderDto>> CompleteReturnAsync(Guid tenantId, Guid id)
    {
        var returnOrder = await _context.ReturnOrders
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (returnOrder == null)
            return ApiResponse<ReturnOrderDto>.Fail("Return order not found");

        if (returnOrder.Status != ReturnOrderStatus.InProgress)
            return ApiResponse<ReturnOrderDto>.Fail("Return must be in InProgress status");

        // Check all lines are processed
        var allProcessed = returnOrder.Lines.All(l =>
            l.Disposition != ReturnDisposition.Pending ||
            l.QuantityReceived == 0);

        if (!allProcessed)
            return ApiResponse<ReturnOrderDto>.Fail("All received lines must be processed");

        returnOrder.Status = ReturnOrderStatus.Complete;
        returnOrder.ProcessedDate = DateTime.UtcNow;
        returnOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetReturnOrderByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<ReturnOrderDto>> CancelReturnAsync(Guid tenantId, Guid id, string? reason)
    {
        var returnOrder = await _context.ReturnOrders
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (returnOrder == null)
            return ApiResponse<ReturnOrderDto>.Fail("Return order not found");

        if (returnOrder.Status == ReturnOrderStatus.Complete)
            return ApiResponse<ReturnOrderDto>.Fail("Cannot cancel completed return");

        returnOrder.Status = ReturnOrderStatus.Cancelled;
        returnOrder.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(reason))
            returnOrder.InternalNotes = string.IsNullOrEmpty(returnOrder.InternalNotes)
                ? $"Cancelled: {reason}"
                : $"{returnOrder.InternalNotes}\nCancelled: {reason}";

        await _context.SaveChangesAsync();
        return await GetReturnOrderByIdAsync(tenantId, id);
    }

    #endregion

    #region Helpers

    private async Task<string> GenerateReturnNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"RET-{today}-";

        var lastReturn = await _context.ReturnOrders
            .Where(r => r.TenantId == tenantId && r.ReturnNumber.StartsWith(prefix))
            .OrderByDescending(r => r.ReturnNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastReturn != null)
        {
            var lastNumberStr = lastReturn.ReturnNumber.Replace(prefix, "");
            if (int.TryParse(lastNumberStr, out var lastNumber))
                nextNumber = lastNumber + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private static ReturnOrderDto MapToDto(ReturnOrder returnOrder)
    {
        return new ReturnOrderDto
        {
            Id = returnOrder.Id,
            ReturnNumber = returnOrder.ReturnNumber,
            OriginalSalesOrderId = returnOrder.OriginalSalesOrderId,
            OriginalSalesOrderNumber = returnOrder.OriginalSalesOrder?.OrderNumber,
            CustomerId = returnOrder.CustomerId,
            CustomerName = returnOrder.Customer?.Name,
            Status = returnOrder.Status,
            ReturnType = returnOrder.ReturnType,
            ReasonCodeId = returnOrder.ReasonCodeId,
            ReasonDescription = returnOrder.ReasonDescription,
            ReceivingLocationId = returnOrder.ReceivingLocationId,
            ReceivingLocationCode = returnOrder.ReceivingLocation?.Code,
            RequestedDate = returnOrder.RequestedDate,
            ReceivedDate = returnOrder.ReceivedDate,
            ProcessedDate = returnOrder.ProcessedDate,
            RmaNumber = returnOrder.RmaNumber,
            RmaExpiryDate = returnOrder.RmaExpiryDate,
            CarrierCode = returnOrder.CarrierCode,
            TrackingNumber = returnOrder.TrackingNumber,
            AssignedToUserId = returnOrder.AssignedToUserId,
            TotalLines = returnOrder.TotalLines,
            TotalQuantityExpected = returnOrder.TotalQuantityExpected,
            TotalQuantityReceived = returnOrder.TotalQuantityReceived,
            Notes = returnOrder.Notes,
            InternalNotes = returnOrder.InternalNotes,
            Lines = returnOrder.Lines.Select(l => new ReturnOrderLineDto
            {
                Id = l.Id,
                ReturnOrderId = l.ReturnOrderId,
                LineNumber = l.LineNumber,
                ProductId = l.ProductId,
                ProductName = l.Product?.Name,
                Sku = l.Sku,
                QuantityExpected = l.QuantityExpected,
                QuantityReceived = l.QuantityReceived,
                QuantityAccepted = l.QuantityAccepted,
                QuantityRejected = l.QuantityRejected,
                Condition = l.Condition,
                ConditionNotes = l.ConditionNotes,
                Disposition = l.Disposition,
                DispositionLocationId = l.DispositionLocationId,
                DispositionLocationCode = l.DispositionLocation?.Code,
                BatchNumber = l.BatchNumber,
                SerialNumber = l.SerialNumber,
                OriginalOrderLineId = l.OriginalOrderLineId,
                ReasonCodeId = l.ReasonCodeId,
                ReasonDescription = l.ReasonDescription,
                Notes = l.Notes
            }).ToList(),
            CreatedAt = returnOrder.CreatedAt,
            UpdatedAt = returnOrder.UpdatedAt
        };
    }

    #endregion
}
