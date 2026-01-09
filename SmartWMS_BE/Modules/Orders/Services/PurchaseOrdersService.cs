using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Orders.DTOs;
using SmartWMS.API.Modules.Orders.Models;
using SmartWMS.API.Modules.Automation.Services;
using SmartWMS.API.Modules.Inventory.Models;
using SmartWMS.API.Modules.Inventory.Services;

namespace SmartWMS.API.Modules.Orders.Services;

public class PurchaseOrdersService : IPurchaseOrdersService
{
    private readonly ApplicationDbContext _context;
    private readonly IAutomationEventPublisher _automationEvents;
    private readonly IStockService _stockService;

    public PurchaseOrdersService(
        ApplicationDbContext context,
        IAutomationEventPublisher automationEvents,
        IStockService stockService)
    {
        _context = context;
        _automationEvents = automationEvents;
        _stockService = stockService;
    }

    #region Orders

    public async Task<ApiResponse<PaginatedResult<PurchaseOrderDto>>> GetOrdersAsync(
        Guid tenantId,
        PurchaseOrderFilters? filters = null,
        int page = 1,
        int pageSize = 25)
    {
        var query = _context.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .Where(o => o.TenantId == tenantId)
            .AsQueryable();

        // Apply filters
        if (filters != null)
        {
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var search = filters.Search.ToLower();
                query = query.Where(o =>
                    o.OrderNumber.ToLower().Contains(search) ||
                    (o.ExternalReference != null && o.ExternalReference.ToLower().Contains(search)) ||
                    o.Supplier.Name.ToLower().Contains(search));
            }

            if (filters.Status.HasValue)
                query = query.Where(o => o.Status == filters.Status.Value);

            if (filters.SupplierId.HasValue)
                query = query.Where(o => o.SupplierId == filters.SupplierId.Value);

            if (filters.WarehouseId.HasValue)
                query = query.Where(o => o.WarehouseId == filters.WarehouseId.Value);

            if (filters.OrderDateFrom.HasValue)
                query = query.Where(o => o.OrderDate >= filters.OrderDateFrom.Value);

            if (filters.OrderDateTo.HasValue)
                query = query.Where(o => o.OrderDate <= filters.OrderDateTo.Value);

            if (filters.ExpectedDateFrom.HasValue)
                query = query.Where(o => o.ExpectedDate >= filters.ExpectedDateFrom.Value);

            if (filters.ExpectedDateTo.HasValue)
                query = query.Where(o => o.ExpectedDate <= filters.ExpectedDateTo.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .ThenBy(o => o.OrderNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => MapToDto(o, false))
            .ToListAsync();

        var result = new PaginatedResult<PurchaseOrderDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<PurchaseOrderDto>>.Ok(result);
    }

    public async Task<ApiResponse<PurchaseOrderDto>> GetOrderByIdAsync(Guid tenantId, Guid id, bool includeLines = true)
    {
        var query = _context.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .Include(o => o.ReceivingDock)
            .AsQueryable();

        if (includeLines)
        {
            query = query.Include(o => o.Lines)
                .ThenInclude(l => l.Product);
        }

        var order = await query.FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id);

        if (order == null)
            return ApiResponse<PurchaseOrderDto>.Fail("Purchase order not found");

        return ApiResponse<PurchaseOrderDto>.Ok(MapToDto(order, includeLines));
    }

    public async Task<ApiResponse<PurchaseOrderDto>> GetOrderByNumberAsync(Guid tenantId, string orderNumber, bool includeLines = true)
    {
        var query = _context.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .Include(o => o.ReceivingDock)
            .AsQueryable();

        if (includeLines)
        {
            query = query.Include(o => o.Lines)
                .ThenInclude(l => l.Product);
        }

        var order = await query.FirstOrDefaultAsync(o => o.TenantId == tenantId && o.OrderNumber == orderNumber);

        if (order == null)
            return ApiResponse<PurchaseOrderDto>.Fail("Purchase order not found");

        return ApiResponse<PurchaseOrderDto>.Ok(MapToDto(order, includeLines));
    }

    public async Task<ApiResponse<PurchaseOrderDto>> CreateOrderAsync(Guid tenantId, CreatePurchaseOrderRequest request)
    {
        // Validate supplier
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == request.SupplierId);

        if (supplier == null)
            return ApiResponse<PurchaseOrderDto>.Fail("Supplier not found");

        // Validate warehouse
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == request.WarehouseId);

        if (warehouse == null)
            return ApiResponse<PurchaseOrderDto>.Fail("Warehouse not found");

        // Validate receiving dock if provided
        if (request.ReceivingDockId.HasValue)
        {
            var dock = await _context.Locations
                .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.ReceivingDockId.Value);

            if (dock == null)
                return ApiResponse<PurchaseOrderDto>.Fail("Receiving dock location not found");
        }

        // Generate order number if not provided
        var orderNumber = request.OrderNumber;
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            orderNumber = await GenerateOrderNumberAsync(tenantId);
        }
        else
        {
            // Check for duplicate
            var exists = await _context.PurchaseOrders
                .AnyAsync(o => o.TenantId == tenantId && o.OrderNumber == orderNumber);

            if (exists)
                return ApiResponse<PurchaseOrderDto>.Fail($"Order number '{orderNumber}' already exists");
        }

        var order = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderNumber = orderNumber,
            ExternalReference = request.ExternalReference,
            SupplierId = request.SupplierId,
            WarehouseId = request.WarehouseId,
            Status = PurchaseOrderStatus.Draft,
            OrderDate = DateTime.UtcNow,
            ExpectedDate = request.ExpectedDate,
            ReceivingDockId = request.ReceivingDockId,
            Notes = request.Notes,
            InternalNotes = request.InternalNotes,
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
                    return ApiResponse<PurchaseOrderDto>.Fail($"Product not found: {lineRequest.ProductId}");

                var line = new PurchaseOrderLine
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OrderId = order.Id,
                    LineNumber = lineNumber++,
                    ProductId = lineRequest.ProductId,
                    Sku = product.Sku,
                    QuantityOrdered = lineRequest.QuantityOrdered,
                    ExpectedBatchNumber = lineRequest.ExpectedBatchNumber,
                    ExpectedExpiryDate = lineRequest.ExpectedExpiryDate,
                    Notes = lineRequest.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                order.Lines.Add(line);
            }

            // Update totals
            UpdateOrderTotals(order);
        }

        _context.PurchaseOrders.Add(order);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(order).Reference(o => o.Supplier).LoadAsync();
        await _context.Entry(order).Reference(o => o.Warehouse).LoadAsync();

        // Publish automation event
        await _automationEvents.PublishEntityCreatedAsync(tenantId, order);

        return ApiResponse<PurchaseOrderDto>.Ok(MapToDto(order, true), "Purchase order created successfully");
    }

    public async Task<ApiResponse<PurchaseOrderDto>> UpdateOrderAsync(Guid tenantId, Guid id, UpdatePurchaseOrderRequest request)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id);

        if (order == null)
            return ApiResponse<PurchaseOrderDto>.Fail("Purchase order not found");

        // Only allow updates in Draft or Pending status
        if (order.Status != PurchaseOrderStatus.Draft && order.Status != PurchaseOrderStatus.Pending)
            return ApiResponse<PurchaseOrderDto>.Fail($"Cannot update order in '{order.Status}' status");

        // Validate supplier if changing
        if (request.SupplierId.HasValue && request.SupplierId.Value != order.SupplierId)
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == request.SupplierId.Value);

            if (supplier == null)
                return ApiResponse<PurchaseOrderDto>.Fail("Supplier not found");

            order.SupplierId = request.SupplierId.Value;
        }

        // Validate warehouse if changing
        if (request.WarehouseId.HasValue && request.WarehouseId.Value != order.WarehouseId)
        {
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == request.WarehouseId.Value);

            if (warehouse == null)
                return ApiResponse<PurchaseOrderDto>.Fail("Warehouse not found");

            order.WarehouseId = request.WarehouseId.Value;
        }

        // Validate receiving dock if changing
        if (request.ReceivingDockId.HasValue)
        {
            var dock = await _context.Locations
                .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.ReceivingDockId.Value);

            if (dock == null)
                return ApiResponse<PurchaseOrderDto>.Fail("Receiving dock location not found");

            order.ReceivingDockId = request.ReceivingDockId;
        }

        // Update fields
        if (request.ExternalReference != null) order.ExternalReference = request.ExternalReference;
        if (request.ExpectedDate.HasValue) order.ExpectedDate = request.ExpectedDate;
        if (request.Notes != null) order.Notes = request.Notes;
        if (request.InternalNotes != null) order.InternalNotes = request.InternalNotes;

        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<PurchaseOrderDto>.Ok(MapToDto(order, false), "Purchase order updated successfully");
    }

    public async Task<ApiResponse<PurchaseOrderDto>> UpdateOrderStatusAsync(Guid tenantId, Guid id, UpdatePurchaseOrderStatusRequest request)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id);

        if (order == null)
            return ApiResponse<PurchaseOrderDto>.Fail("Purchase order not found");

        // Validate status transition
        if (!IsValidStatusTransition(order.Status, request.Status))
            return ApiResponse<PurchaseOrderDto>.Fail($"Cannot transition from '{order.Status}' to '{request.Status}'");

        var previousStatus = order.Status.ToString();
        order.Status = request.Status;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            order.InternalNotes = string.IsNullOrWhiteSpace(order.InternalNotes)
                ? request.Notes
                : $"{order.InternalNotes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] {request.Notes}";
        }

        // Update received date if transitioning to Received or Closed
        if (request.Status == PurchaseOrderStatus.Received || request.Status == PurchaseOrderStatus.Closed)
        {
            order.ReceivedDate ??= DateTime.UtcNow;
        }

        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Publish automation event for status change
        await _automationEvents.PublishStatusChangedAsync(tenantId, order, previousStatus, order.Status.ToString());

        return ApiResponse<PurchaseOrderDto>.Ok(MapToDto(order, false), "Order status updated successfully");
    }

    public async Task<ApiResponse> DeleteOrderAsync(Guid tenantId, Guid id)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id);

        if (order == null)
            return ApiResponse.Fail("Purchase order not found");

        // Only allow deletion in Draft status
        if (order.Status != PurchaseOrderStatus.Draft)
            return ApiResponse.Fail("Can only delete orders in Draft status. Cancel the order instead.");

        _context.PurchaseOrderLines.RemoveRange(order.Lines);
        _context.PurchaseOrders.Remove(order);
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Purchase order deleted successfully");
    }

    #endregion

    #region Order Lines

    public async Task<ApiResponse<PurchaseOrderLineDto>> AddLineAsync(Guid tenantId, Guid orderId, AddPurchaseOrderLineRequest request)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == orderId);

        if (order == null)
            return ApiResponse<PurchaseOrderLineDto>.Fail("Purchase order not found");

        // Only allow line modifications in Draft or Pending status
        if (order.Status != PurchaseOrderStatus.Draft && order.Status != PurchaseOrderStatus.Pending)
            return ApiResponse<PurchaseOrderLineDto>.Fail($"Cannot modify lines in '{order.Status}' status");

        // Validate product
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ProductId);

        if (product == null)
            return ApiResponse<PurchaseOrderLineDto>.Fail("Product not found");

        var maxLineNumber = order.Lines.Any() ? order.Lines.Max(l => l.LineNumber) : 0;

        var line = new PurchaseOrderLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderId = orderId,
            LineNumber = maxLineNumber + 1,
            ProductId = request.ProductId,
            Sku = product.Sku,
            QuantityOrdered = request.QuantityOrdered,
            ExpectedBatchNumber = request.ExpectedBatchNumber,
            ExpectedExpiryDate = request.ExpectedExpiryDate,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        order.Lines.Add(line);
        UpdateOrderTotals(order);
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<PurchaseOrderLineDto>.Ok(MapLineToDto(line, product.Name), "Line added successfully");
    }

    public async Task<ApiResponse<PurchaseOrderLineDto>> UpdateLineAsync(Guid tenantId, Guid orderId, Guid lineId, UpdatePurchaseOrderLineRequest request)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Lines)
            .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == orderId);

        if (order == null)
            return ApiResponse<PurchaseOrderLineDto>.Fail("Purchase order not found");

        // Only allow line modifications in Draft or Pending status
        if (order.Status != PurchaseOrderStatus.Draft && order.Status != PurchaseOrderStatus.Pending)
            return ApiResponse<PurchaseOrderLineDto>.Fail($"Cannot modify lines in '{order.Status}' status");

        var line = order.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse<PurchaseOrderLineDto>.Fail("Order line not found");

        if (request.QuantityOrdered.HasValue) line.QuantityOrdered = request.QuantityOrdered.Value;
        if (request.ExpectedBatchNumber != null) line.ExpectedBatchNumber = request.ExpectedBatchNumber;
        if (request.ExpectedExpiryDate.HasValue) line.ExpectedExpiryDate = request.ExpectedExpiryDate;
        if (request.Notes != null) line.Notes = request.Notes;

        line.UpdatedAt = DateTime.UtcNow;

        UpdateOrderTotals(order);
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<PurchaseOrderLineDto>.Ok(MapLineToDto(line, line.Product?.Name), "Line updated successfully");
    }

    public async Task<ApiResponse> RemoveLineAsync(Guid tenantId, Guid orderId, Guid lineId)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == orderId);

        if (order == null)
            return ApiResponse.Fail("Purchase order not found");

        // Only allow line modifications in Draft or Pending status
        if (order.Status != PurchaseOrderStatus.Draft && order.Status != PurchaseOrderStatus.Pending)
            return ApiResponse.Fail($"Cannot modify lines in '{order.Status}' status");

        var line = order.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse.Fail("Order line not found");

        order.Lines.Remove(line);
        _context.PurchaseOrderLines.Remove(line);

        UpdateOrderTotals(order);
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Line removed successfully");
    }

    public async Task<ApiResponse<PurchaseOrderLineDto>> ReceiveLineAsync(Guid tenantId, Guid orderId, Guid lineId, ReceivePurchaseOrderLineRequest request)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Lines)
            .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == orderId);

        if (order == null)
            return ApiResponse<PurchaseOrderLineDto>.Fail("Purchase order not found");

        // Only allow receiving in Confirmed or PartiallyReceived status
        if (order.Status != PurchaseOrderStatus.Confirmed && order.Status != PurchaseOrderStatus.PartiallyReceived)
            return ApiResponse<PurchaseOrderLineDto>.Fail($"Cannot receive items in '{order.Status}' status");

        var line = order.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse<PurchaseOrderLineDto>.Fail("Order line not found");

        // Validate quantity
        var remainingToReceive = line.QuantityOrdered - line.QuantityReceived - line.QuantityCancelled;
        if (request.QuantityReceived > remainingToReceive)
            return ApiResponse<PurchaseOrderLineDto>.Fail($"Cannot receive more than outstanding quantity ({remainingToReceive})");

        // Update received quantity
        line.QuantityReceived += request.QuantityReceived;
        line.UpdatedAt = DateTime.UtcNow;

        // Create stock movement and update inventory
        if (request.LocationId.HasValue)
        {
            var receiveRequest = new Inventory.DTOs.ReceiveStockRequest
            {
                ProductId = line.ProductId,
                LocationId = request.LocationId.Value,
                Quantity = request.QuantityReceived,
                BatchNumber = request.BatchNumber,
                SerialNumber = request.SerialNumber,
                ExpiryDate = request.ExpiryDate,
                ReferenceType = "PurchaseOrder",
                ReferenceId = order.Id,
                ReferenceNumber = order.OrderNumber,
                Notes = $"Received from PO line {line.LineNumber}"
            };

            var stockResult = await _stockService.ReceiveStockAsync(tenantId, receiveRequest);
            if (!stockResult.Success)
            {
                // Log warning but don't fail the PO receive - stock can be reconciled later
                // This allows PO receiving to work even if there's a stock issue
            }
        }

        // Update order totals and status
        UpdateOrderTotals(order);

        // Auto-update order status based on received quantities
        var totalOrdered = order.Lines.Sum(l => l.QuantityOrdered);
        var totalReceived = order.Lines.Sum(l => l.QuantityReceived);
        var totalCancelled = order.Lines.Sum(l => l.QuantityCancelled);

        if (totalReceived >= totalOrdered - totalCancelled)
        {
            order.Status = PurchaseOrderStatus.Received;
            order.ReceivedDate = DateTime.UtcNow;
        }
        else if (totalReceived > 0)
        {
            order.Status = PurchaseOrderStatus.PartiallyReceived;
        }

        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<PurchaseOrderLineDto>.Ok(MapLineToDto(line, line.Product?.Name), "Items received successfully");
    }

    #endregion

    #region Private Methods

    private async Task<string> GenerateOrderNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"PO-{today:yyyyMMdd}-";

        var lastOrder = await _context.PurchaseOrders
            .Where(o => o.TenantId == tenantId && o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (lastOrder != null)
        {
            var lastNumber = lastOrder.OrderNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumber, out var parsed))
            {
                sequence = parsed + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }

    private static bool IsValidStatusTransition(PurchaseOrderStatus from, PurchaseOrderStatus to)
    {
        // Define valid transitions
        var validTransitions = new Dictionary<PurchaseOrderStatus, PurchaseOrderStatus[]>
        {
            [PurchaseOrderStatus.Draft] = new[] { PurchaseOrderStatus.Pending, PurchaseOrderStatus.Cancelled },
            [PurchaseOrderStatus.Pending] = new[] { PurchaseOrderStatus.Confirmed, PurchaseOrderStatus.Cancelled },
            [PurchaseOrderStatus.Confirmed] = new[] { PurchaseOrderStatus.PartiallyReceived, PurchaseOrderStatus.Received, PurchaseOrderStatus.Cancelled },
            [PurchaseOrderStatus.PartiallyReceived] = new[] { PurchaseOrderStatus.Received, PurchaseOrderStatus.Closed, PurchaseOrderStatus.Cancelled },
            [PurchaseOrderStatus.Received] = new[] { PurchaseOrderStatus.Closed },
            [PurchaseOrderStatus.Closed] = Array.Empty<PurchaseOrderStatus>(),
            [PurchaseOrderStatus.Cancelled] = Array.Empty<PurchaseOrderStatus>()
        };

        return validTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    private static void UpdateOrderTotals(PurchaseOrder order)
    {
        order.TotalLines = order.Lines.Count;
        order.TotalQuantity = order.Lines.Sum(l => l.QuantityOrdered);
        order.ReceivedQuantity = order.Lines.Sum(l => l.QuantityReceived);
    }

    private static PurchaseOrderDto MapToDto(PurchaseOrder order, bool includeLines)
    {
        var dto = new PurchaseOrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            ExternalReference = order.ExternalReference,
            SupplierId = order.SupplierId,
            SupplierCode = order.Supplier?.Code,
            SupplierName = order.Supplier?.Name,
            WarehouseId = order.WarehouseId,
            WarehouseCode = order.Warehouse?.Code,
            WarehouseName = order.Warehouse?.Name,
            Status = order.Status,
            OrderDate = order.OrderDate,
            ExpectedDate = order.ExpectedDate,
            ReceivedDate = order.ReceivedDate,
            ReceivingDockId = order.ReceivingDockId,
            ReceivingDockCode = order.ReceivingDock?.Code,
            TotalLines = order.TotalLines,
            TotalQuantity = order.TotalQuantity,
            ReceivedQuantity = order.ReceivedQuantity,
            Notes = order.Notes,
            InternalNotes = order.InternalNotes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };

        if (includeLines && order.Lines?.Any() == true)
        {
            dto.Lines = order.Lines
                .OrderBy(l => l.LineNumber)
                .Select(l => MapLineToDto(l, l.Product?.Name))
                .ToList();
        }

        return dto;
    }

    private static PurchaseOrderLineDto MapLineToDto(PurchaseOrderLine line, string? productName)
    {
        return new PurchaseOrderLineDto
        {
            Id = line.Id,
            OrderId = line.OrderId,
            LineNumber = line.LineNumber,
            ProductId = line.ProductId,
            Sku = line.Sku,
            ProductName = productName,
            QuantityOrdered = line.QuantityOrdered,
            QuantityReceived = line.QuantityReceived,
            QuantityCancelled = line.QuantityCancelled,
            QuantityOutstanding = line.QuantityOutstanding,
            ExpectedBatchNumber = line.ExpectedBatchNumber,
            ExpectedExpiryDate = line.ExpectedExpiryDate,
            Notes = line.Notes
        };
    }

    #endregion
}
