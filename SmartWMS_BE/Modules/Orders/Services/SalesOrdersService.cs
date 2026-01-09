using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Orders.DTOs;
using SmartWMS.API.Modules.Orders.Models;
using SmartWMS.API.Modules.Automation.Services;

namespace SmartWMS.API.Modules.Orders.Services;

public class SalesOrdersService : ISalesOrdersService
{
    private readonly ApplicationDbContext _context;
    private readonly IAutomationEventPublisher _automationEvents;

    public SalesOrdersService(ApplicationDbContext context, IAutomationEventPublisher automationEvents)
    {
        _context = context;
        _automationEvents = automationEvents;
    }

    #region Orders

    public async Task<ApiResponse<PaginatedResult<SalesOrderDto>>> GetOrdersAsync(
        Guid tenantId,
        SalesOrderFilters? filters = null,
        int page = 1,
        int pageSize = 25)
    {
        var query = _context.SalesOrders
            .Include(o => o.Customer)
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
                    o.Customer.Name.ToLower().Contains(search));
            }

            if (filters.Status.HasValue)
                query = query.Where(o => o.Status == filters.Status.Value);

            if (filters.Priority.HasValue)
                query = query.Where(o => o.Priority == filters.Priority.Value);

            if (filters.CustomerId.HasValue)
                query = query.Where(o => o.CustomerId == filters.CustomerId.Value);

            if (filters.WarehouseId.HasValue)
                query = query.Where(o => o.WarehouseId == filters.WarehouseId.Value);

            if (filters.OrderDateFrom.HasValue)
                query = query.Where(o => o.OrderDate >= filters.OrderDateFrom.Value);

            if (filters.OrderDateTo.HasValue)
                query = query.Where(o => o.OrderDate <= filters.OrderDateTo.Value);

            if (filters.RequiredDateFrom.HasValue)
                query = query.Where(o => o.RequiredDate >= filters.RequiredDateFrom.Value);

            if (filters.RequiredDateTo.HasValue)
                query = query.Where(o => o.RequiredDate <= filters.RequiredDateTo.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .ThenBy(o => o.OrderNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => MapToDto(o, false))
            .ToListAsync();

        var result = new PaginatedResult<SalesOrderDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<SalesOrderDto>>.Ok(result);
    }

    public async Task<ApiResponse<SalesOrderDto>> GetOrderByIdAsync(Guid tenantId, Guid id, bool includeLines = true)
    {
        var query = _context.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Warehouse)
            .AsQueryable();

        if (includeLines)
        {
            query = query.Include(o => o.Lines)
                .ThenInclude(l => l.Product);
        }

        var order = await query.FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id);

        if (order == null)
            return ApiResponse<SalesOrderDto>.Fail("Sales order not found");

        return ApiResponse<SalesOrderDto>.Ok(MapToDto(order, includeLines));
    }

    public async Task<ApiResponse<SalesOrderDto>> GetOrderByNumberAsync(Guid tenantId, string orderNumber, bool includeLines = true)
    {
        var query = _context.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Warehouse)
            .AsQueryable();

        if (includeLines)
        {
            query = query.Include(o => o.Lines)
                .ThenInclude(l => l.Product);
        }

        var order = await query.FirstOrDefaultAsync(o => o.TenantId == tenantId && o.OrderNumber == orderNumber);

        if (order == null)
            return ApiResponse<SalesOrderDto>.Fail("Sales order not found");

        return ApiResponse<SalesOrderDto>.Ok(MapToDto(order, includeLines));
    }

    public async Task<ApiResponse<SalesOrderDto>> CreateOrderAsync(Guid tenantId, CreateSalesOrderRequest request)
    {
        // Validate customer
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == request.CustomerId);

        if (customer == null)
            return ApiResponse<SalesOrderDto>.Fail("Customer not found");

        // Validate warehouse
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == request.WarehouseId);

        if (warehouse == null)
            return ApiResponse<SalesOrderDto>.Fail("Warehouse not found");

        // Generate order number if not provided
        var orderNumber = request.OrderNumber;
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            orderNumber = await GenerateOrderNumberAsync(tenantId);
        }
        else
        {
            // Check for duplicate
            var exists = await _context.SalesOrders
                .AnyAsync(o => o.TenantId == tenantId && o.OrderNumber == orderNumber);

            if (exists)
                return ApiResponse<SalesOrderDto>.Fail($"Order number '{orderNumber}' already exists");
        }

        var order = new SalesOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderNumber = orderNumber,
            ExternalReference = request.ExternalReference,
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            Status = SalesOrderStatus.Draft,
            Priority = request.Priority,
            OrderDate = DateTime.UtcNow,
            RequiredDate = request.RequiredDate,
            ShipToName = request.ShipToName ?? customer.Name,
            ShipToAddressLine1 = request.ShipToAddressLine1 ?? customer.AddressLine1,
            ShipToAddressLine2 = request.ShipToAddressLine2 ?? customer.AddressLine2,
            ShipToCity = request.ShipToCity ?? customer.City,
            ShipToRegion = request.ShipToRegion ?? customer.Region,
            ShipToPostalCode = request.ShipToPostalCode ?? customer.PostalCode,
            ShipToCountryCode = request.ShipToCountryCode ?? customer.CountryCode,
            CarrierCode = request.CarrierCode,
            ServiceLevel = request.ServiceLevel,
            ShippingInstructions = request.ShippingInstructions,
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
                    return ApiResponse<SalesOrderDto>.Fail($"Product not found: {lineRequest.ProductId}");

                var line = new SalesOrderLine
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OrderId = order.Id,
                    LineNumber = lineNumber++,
                    ProductId = lineRequest.ProductId,
                    Sku = product.Sku,
                    QuantityOrdered = lineRequest.QuantityOrdered,
                    RequiredBatchNumber = lineRequest.RequiredBatchNumber,
                    RequiredExpiryDate = lineRequest.RequiredExpiryDate,
                    Notes = lineRequest.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                order.Lines.Add(line);
            }

            // Update totals
            UpdateOrderTotals(order);
        }

        _context.SalesOrders.Add(order);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(order).Reference(o => o.Customer).LoadAsync();
        await _context.Entry(order).Reference(o => o.Warehouse).LoadAsync();

        // Publish automation event
        await _automationEvents.PublishEntityCreatedAsync(tenantId, order);

        return ApiResponse<SalesOrderDto>.Ok(MapToDto(order, true), "Sales order created successfully");
    }

    public async Task<ApiResponse<SalesOrderDto>> UpdateOrderAsync(Guid tenantId, Guid id, UpdateSalesOrderRequest request)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Warehouse)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id);

        if (order == null)
            return ApiResponse<SalesOrderDto>.Fail("Sales order not found");

        // Only allow updates in Draft or Pending status
        if (order.Status != SalesOrderStatus.Draft && order.Status != SalesOrderStatus.Pending)
            return ApiResponse<SalesOrderDto>.Fail($"Cannot update order in '{order.Status}' status");

        // Validate customer if changing
        if (request.CustomerId.HasValue && request.CustomerId.Value != order.CustomerId)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == request.CustomerId.Value);

            if (customer == null)
                return ApiResponse<SalesOrderDto>.Fail("Customer not found");

            order.CustomerId = request.CustomerId.Value;
        }

        // Validate warehouse if changing
        if (request.WarehouseId.HasValue && request.WarehouseId.Value != order.WarehouseId)
        {
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == request.WarehouseId.Value);

            if (warehouse == null)
                return ApiResponse<SalesOrderDto>.Fail("Warehouse not found");

            order.WarehouseId = request.WarehouseId.Value;
        }

        // Update fields
        if (request.ExternalReference != null) order.ExternalReference = request.ExternalReference;
        if (request.Priority.HasValue) order.Priority = request.Priority.Value;
        if (request.RequiredDate.HasValue) order.RequiredDate = request.RequiredDate;
        if (request.ShipToName != null) order.ShipToName = request.ShipToName;
        if (request.ShipToAddressLine1 != null) order.ShipToAddressLine1 = request.ShipToAddressLine1;
        if (request.ShipToAddressLine2 != null) order.ShipToAddressLine2 = request.ShipToAddressLine2;
        if (request.ShipToCity != null) order.ShipToCity = request.ShipToCity;
        if (request.ShipToRegion != null) order.ShipToRegion = request.ShipToRegion;
        if (request.ShipToPostalCode != null) order.ShipToPostalCode = request.ShipToPostalCode;
        if (request.ShipToCountryCode != null) order.ShipToCountryCode = request.ShipToCountryCode;
        if (request.CarrierCode != null) order.CarrierCode = request.CarrierCode;
        if (request.ServiceLevel != null) order.ServiceLevel = request.ServiceLevel;
        if (request.ShippingInstructions != null) order.ShippingInstructions = request.ShippingInstructions;
        if (request.Notes != null) order.Notes = request.Notes;
        if (request.InternalNotes != null) order.InternalNotes = request.InternalNotes;

        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<SalesOrderDto>.Ok(MapToDto(order, false), "Sales order updated successfully");
    }

    public async Task<ApiResponse<SalesOrderDto>> UpdateOrderStatusAsync(Guid tenantId, Guid id, UpdateSalesOrderStatusRequest request)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Warehouse)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id);

        if (order == null)
            return ApiResponse<SalesOrderDto>.Fail("Sales order not found");

        // Validate status transition
        if (!IsValidStatusTransition(order.Status, request.Status))
            return ApiResponse<SalesOrderDto>.Fail($"Cannot transition from '{order.Status}' to '{request.Status}'");

        var previousStatus = order.Status.ToString();
        order.Status = request.Status;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            order.InternalNotes = string.IsNullOrWhiteSpace(order.InternalNotes)
                ? request.Notes
                : $"{order.InternalNotes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] {request.Notes}";
        }

        // Update shipped date if transitioning to Shipped
        if (request.Status == SalesOrderStatus.Shipped)
        {
            order.ShippedDate = DateTime.UtcNow;
        }

        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Publish automation event for status change
        await _automationEvents.PublishStatusChangedAsync(tenantId, order, previousStatus, request.Status.ToString());

        return ApiResponse<SalesOrderDto>.Ok(MapToDto(order, false), "Order status updated successfully");
    }

    public async Task<ApiResponse> DeleteOrderAsync(Guid tenantId, Guid id)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id);

        if (order == null)
            return ApiResponse.Fail("Sales order not found");

        // Only allow deletion in Draft status
        if (order.Status != SalesOrderStatus.Draft)
            return ApiResponse.Fail("Can only delete orders in Draft status. Cancel the order instead.");

        _context.SalesOrderLines.RemoveRange(order.Lines);
        _context.SalesOrders.Remove(order);
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Sales order deleted successfully");
    }

    #endregion

    #region Order Lines

    public async Task<ApiResponse<SalesOrderLineDto>> AddLineAsync(Guid tenantId, Guid orderId, AddSalesOrderLineRequest request)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == orderId);

        if (order == null)
            return ApiResponse<SalesOrderLineDto>.Fail("Sales order not found");

        // Only allow line modifications in Draft or Pending status
        if (order.Status != SalesOrderStatus.Draft && order.Status != SalesOrderStatus.Pending)
            return ApiResponse<SalesOrderLineDto>.Fail($"Cannot modify lines in '{order.Status}' status");

        // Validate product
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ProductId);

        if (product == null)
            return ApiResponse<SalesOrderLineDto>.Fail("Product not found");

        var maxLineNumber = order.Lines.Any() ? order.Lines.Max(l => l.LineNumber) : 0;

        var line = new SalesOrderLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderId = orderId,
            LineNumber = maxLineNumber + 1,
            ProductId = request.ProductId,
            Sku = product.Sku,
            QuantityOrdered = request.QuantityOrdered,
            RequiredBatchNumber = request.RequiredBatchNumber,
            RequiredExpiryDate = request.RequiredExpiryDate,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        order.Lines.Add(line);
        UpdateOrderTotals(order);
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<SalesOrderLineDto>.Ok(MapLineToDto(line, product.Name), "Line added successfully");
    }

    public async Task<ApiResponse<SalesOrderLineDto>> UpdateLineAsync(Guid tenantId, Guid orderId, Guid lineId, UpdateSalesOrderLineRequest request)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Lines)
            .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == orderId);

        if (order == null)
            return ApiResponse<SalesOrderLineDto>.Fail("Sales order not found");

        // Only allow line modifications in Draft or Pending status
        if (order.Status != SalesOrderStatus.Draft && order.Status != SalesOrderStatus.Pending)
            return ApiResponse<SalesOrderLineDto>.Fail($"Cannot modify lines in '{order.Status}' status");

        var line = order.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse<SalesOrderLineDto>.Fail("Order line not found");

        if (request.QuantityOrdered.HasValue) line.QuantityOrdered = request.QuantityOrdered.Value;
        if (request.RequiredBatchNumber != null) line.RequiredBatchNumber = request.RequiredBatchNumber;
        if (request.RequiredExpiryDate.HasValue) line.RequiredExpiryDate = request.RequiredExpiryDate;
        if (request.Notes != null) line.Notes = request.Notes;

        line.UpdatedAt = DateTime.UtcNow;

        UpdateOrderTotals(order);
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<SalesOrderLineDto>.Ok(MapLineToDto(line, line.Product?.Name), "Line updated successfully");
    }

    public async Task<ApiResponse> RemoveLineAsync(Guid tenantId, Guid orderId, Guid lineId)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == orderId);

        if (order == null)
            return ApiResponse.Fail("Sales order not found");

        // Only allow line modifications in Draft or Pending status
        if (order.Status != SalesOrderStatus.Draft && order.Status != SalesOrderStatus.Pending)
            return ApiResponse.Fail($"Cannot modify lines in '{order.Status}' status");

        var line = order.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            return ApiResponse.Fail("Order line not found");

        order.Lines.Remove(line);
        _context.SalesOrderLines.Remove(line);

        UpdateOrderTotals(order);
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Line removed successfully");
    }

    #endregion

    #region Private Methods

    private async Task<string> GenerateOrderNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"SO-{today:yyyyMMdd}-";

        var lastOrder = await _context.SalesOrders
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

    private static bool IsValidStatusTransition(SalesOrderStatus from, SalesOrderStatus to)
    {
        // Define valid transitions
        var validTransitions = new Dictionary<SalesOrderStatus, SalesOrderStatus[]>
        {
            [SalesOrderStatus.Draft] = new[] { SalesOrderStatus.Pending, SalesOrderStatus.Cancelled },
            [SalesOrderStatus.Pending] = new[] { SalesOrderStatus.Confirmed, SalesOrderStatus.OnHold, SalesOrderStatus.Cancelled },
            [SalesOrderStatus.Confirmed] = new[] { SalesOrderStatus.Allocated, SalesOrderStatus.OnHold, SalesOrderStatus.Cancelled },
            [SalesOrderStatus.Allocated] = new[] { SalesOrderStatus.PartiallyPicked, SalesOrderStatus.Picked, SalesOrderStatus.OnHold, SalesOrderStatus.Cancelled },
            [SalesOrderStatus.PartiallyPicked] = new[] { SalesOrderStatus.Picked, SalesOrderStatus.OnHold, SalesOrderStatus.Cancelled },
            [SalesOrderStatus.Picked] = new[] { SalesOrderStatus.Packed, SalesOrderStatus.OnHold, SalesOrderStatus.Cancelled },
            [SalesOrderStatus.Packed] = new[] { SalesOrderStatus.Shipped, SalesOrderStatus.OnHold, SalesOrderStatus.Cancelled },
            [SalesOrderStatus.Shipped] = new[] { SalesOrderStatus.Delivered },
            [SalesOrderStatus.OnHold] = new[] { SalesOrderStatus.Pending, SalesOrderStatus.Confirmed, SalesOrderStatus.Allocated, SalesOrderStatus.Cancelled },
            [SalesOrderStatus.Delivered] = Array.Empty<SalesOrderStatus>(),
            [SalesOrderStatus.Cancelled] = Array.Empty<SalesOrderStatus>()
        };

        return validTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    private static void UpdateOrderTotals(SalesOrder order)
    {
        order.TotalLines = order.Lines.Count;
        order.TotalQuantity = order.Lines.Sum(l => l.QuantityOrdered);
        order.AllocatedQuantity = order.Lines.Sum(l => l.QuantityAllocated);
        order.PickedQuantity = order.Lines.Sum(l => l.QuantityPicked);
        order.ShippedQuantity = order.Lines.Sum(l => l.QuantityShipped);
    }

    private static SalesOrderDto MapToDto(SalesOrder order, bool includeLines)
    {
        var dto = new SalesOrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            ExternalReference = order.ExternalReference,
            CustomerId = order.CustomerId,
            CustomerCode = order.Customer?.Code,
            CustomerName = order.Customer?.Name,
            WarehouseId = order.WarehouseId,
            WarehouseCode = order.Warehouse?.Code,
            WarehouseName = order.Warehouse?.Name,
            Status = order.Status,
            Priority = order.Priority,
            OrderDate = order.OrderDate,
            RequiredDate = order.RequiredDate,
            ShippedDate = order.ShippedDate,
            ShipToName = order.ShipToName,
            ShipToAddressLine1 = order.ShipToAddressLine1,
            ShipToAddressLine2 = order.ShipToAddressLine2,
            ShipToCity = order.ShipToCity,
            ShipToRegion = order.ShipToRegion,
            ShipToPostalCode = order.ShipToPostalCode,
            ShipToCountryCode = order.ShipToCountryCode,
            CarrierCode = order.CarrierCode,
            ServiceLevel = order.ServiceLevel,
            ShippingInstructions = order.ShippingInstructions,
            TotalLines = order.TotalLines,
            TotalQuantity = order.TotalQuantity,
            AllocatedQuantity = order.AllocatedQuantity,
            PickedQuantity = order.PickedQuantity,
            ShippedQuantity = order.ShippedQuantity,
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

    private static SalesOrderLineDto MapLineToDto(SalesOrderLine line, string? productName)
    {
        return new SalesOrderLineDto
        {
            Id = line.Id,
            OrderId = line.OrderId,
            LineNumber = line.LineNumber,
            ProductId = line.ProductId,
            Sku = line.Sku,
            ProductName = productName,
            QuantityOrdered = line.QuantityOrdered,
            QuantityAllocated = line.QuantityAllocated,
            QuantityPicked = line.QuantityPicked,
            QuantityShipped = line.QuantityShipped,
            QuantityCancelled = line.QuantityCancelled,
            QuantityOutstanding = line.QuantityOutstanding,
            RequiredBatchNumber = line.RequiredBatchNumber,
            RequiredExpiryDate = line.RequiredExpiryDate,
            Notes = line.Notes
        };
    }

    #endregion
}
