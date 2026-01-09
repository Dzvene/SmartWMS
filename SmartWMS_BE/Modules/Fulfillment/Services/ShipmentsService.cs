using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Fulfillment.DTOs;
using SmartWMS.API.Modules.Fulfillment.Models;

namespace SmartWMS.API.Modules.Fulfillment.Services;

public class ShipmentsService : IShipmentsService
{
    private readonly ApplicationDbContext _context;

    public ShipmentsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PaginatedResult<ShipmentDto>>> GetShipmentsAsync(
        Guid tenantId,
        ShipmentFilters? filters = null,
        int page = 1,
        int pageSize = 25)
    {
        var query = _context.Shipments
            .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
            .Include(s => s.Warehouse)
            .Where(s => s.TenantId == tenantId)
            .AsQueryable();

        if (filters != null)
        {
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var search = filters.Search.ToLower();
                query = query.Where(s =>
                    s.ShipmentNumber.ToLower().Contains(search) ||
                    (s.TrackingNumber != null && s.TrackingNumber.ToLower().Contains(search)) ||
                    s.Order.OrderNumber.ToLower().Contains(search));
            }

            if (filters.Status.HasValue)
                query = query.Where(s => s.Status == filters.Status.Value);

            if (filters.OrderId.HasValue)
                query = query.Where(s => s.OrderId == filters.OrderId.Value);

            if (filters.WarehouseId.HasValue)
                query = query.Where(s => s.WarehouseId == filters.WarehouseId.Value);

            if (!string.IsNullOrWhiteSpace(filters.CarrierCode))
                query = query.Where(s => s.CarrierCode == filters.CarrierCode);

            if (filters.ShippedFrom.HasValue)
                query = query.Where(s => s.ShippedAt >= filters.ShippedFrom.Value);

            if (filters.ShippedTo.HasValue)
                query = query.Where(s => s.ShippedAt <= filters.ShippedTo.Value);

            if (filters.CreatedFrom.HasValue)
                query = query.Where(s => s.CreatedAt >= filters.CreatedFrom.Value);

            if (filters.CreatedTo.HasValue)
                query = query.Where(s => s.CreatedAt <= filters.CreatedTo.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => MapToDto(s))
            .ToListAsync();

        var result = new PaginatedResult<ShipmentDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<ShipmentDto>>.Ok(result);
    }

    public async Task<ApiResponse<ShipmentDto>> GetShipmentByIdAsync(Guid tenantId, Guid id)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (shipment == null)
            return ApiResponse<ShipmentDto>.Fail("Shipment not found");

        return ApiResponse<ShipmentDto>.Ok(MapToDto(shipment));
    }

    public async Task<ApiResponse<ShipmentDto>> GetShipmentByNumberAsync(Guid tenantId, string shipmentNumber)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.ShipmentNumber == shipmentNumber);

        if (shipment == null)
            return ApiResponse<ShipmentDto>.Fail("Shipment not found");

        return ApiResponse<ShipmentDto>.Ok(MapToDto(shipment));
    }

    public async Task<ApiResponse<ShipmentDto>> CreateShipmentAsync(Guid tenantId, CreateShipmentRequest request)
    {
        // Validate order
        var order = await _context.SalesOrders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == request.OrderId);

        if (order == null)
            return ApiResponse<ShipmentDto>.Fail("Order not found");

        // Validate warehouse
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == request.WarehouseId);

        if (warehouse == null)
            return ApiResponse<ShipmentDto>.Fail("Warehouse not found");

        // Generate shipment number if not provided
        var shipmentNumber = request.ShipmentNumber;
        if (string.IsNullOrWhiteSpace(shipmentNumber))
        {
            shipmentNumber = await GenerateShipmentNumberAsync(tenantId);
        }
        else
        {
            var exists = await _context.Shipments
                .AnyAsync(s => s.TenantId == tenantId && s.ShipmentNumber == shipmentNumber);

            if (exists)
                return ApiResponse<ShipmentDto>.Fail($"Shipment number '{shipmentNumber}' already exists");
        }

        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ShipmentNumber = shipmentNumber,
            OrderId = request.OrderId,
            WarehouseId = request.WarehouseId,
            Status = ShipmentStatus.Created,
            CarrierCode = request.CarrierCode,
            CarrierName = request.CarrierName,
            ServiceLevel = request.ServiceLevel,
            PackageCount = request.PackageCount,
            TotalWeightKg = request.TotalWeightKg,
            WidthMm = request.WidthMm,
            HeightMm = request.HeightMm,
            DepthMm = request.DepthMm,
            // Copy address from order or use provided override
            ShipToName = request.ShipToName ?? order.ShipToName,
            ShipToAddressLine1 = request.ShipToAddressLine1 ?? order.ShipToAddressLine1,
            ShipToAddressLine2 = request.ShipToAddressLine2 ?? order.ShipToAddressLine2,
            ShipToCity = request.ShipToCity ?? order.ShipToCity,
            ShipToRegion = request.ShipToRegion ?? order.ShipToRegion,
            ShipToPostalCode = request.ShipToPostalCode ?? order.ShipToPostalCode,
            ShipToCountryCode = request.ShipToCountryCode ?? order.ShipToCountryCode,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(shipment).Reference(s => s.Order).LoadAsync();
        await _context.Entry(shipment.Order).Reference(o => o.Customer).LoadAsync();
        await _context.Entry(shipment).Reference(s => s.Warehouse).LoadAsync();

        return ApiResponse<ShipmentDto>.Ok(MapToDto(shipment), "Shipment created successfully");
    }

    public async Task<ApiResponse<ShipmentDto>> UpdateShipmentAsync(Guid tenantId, Guid id, UpdateShipmentRequest request)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (shipment == null)
            return ApiResponse<ShipmentDto>.Fail("Shipment not found");

        if (shipment.Status == ShipmentStatus.InTransit || shipment.Status == ShipmentStatus.Delivered || shipment.Status == ShipmentStatus.Cancelled)
            return ApiResponse<ShipmentDto>.Fail($"Cannot update shipment in '{shipment.Status}' status");

        if (request.CarrierCode != null) shipment.CarrierCode = request.CarrierCode;
        if (request.CarrierName != null) shipment.CarrierName = request.CarrierName;
        if (request.ServiceLevel != null) shipment.ServiceLevel = request.ServiceLevel;
        if (request.PackageCount.HasValue) shipment.PackageCount = request.PackageCount.Value;
        if (request.TotalWeightKg.HasValue) shipment.TotalWeightKg = request.TotalWeightKg;
        if (request.WidthMm.HasValue) shipment.WidthMm = request.WidthMm;
        if (request.HeightMm.HasValue) shipment.HeightMm = request.HeightMm;
        if (request.DepthMm.HasValue) shipment.DepthMm = request.DepthMm;
        if (request.Notes != null) shipment.Notes = request.Notes;

        shipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<ShipmentDto>.Ok(MapToDto(shipment), "Shipment updated successfully");
    }

    public async Task<ApiResponse> DeleteShipmentAsync(Guid tenantId, Guid id)
    {
        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (shipment == null)
            return ApiResponse.Fail("Shipment not found");

        if (shipment.Status != ShipmentStatus.Created)
            return ApiResponse.Fail("Can only delete shipments in Created status");

        _context.Shipments.Remove(shipment);
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Shipment deleted successfully");
    }

    public async Task<ApiResponse<ShipmentDto>> AddTrackingAsync(Guid tenantId, Guid id, AddTrackingRequest request)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (shipment == null)
            return ApiResponse<ShipmentDto>.Fail("Shipment not found");

        shipment.TrackingNumber = request.TrackingNumber;
        shipment.TrackingUrl = request.TrackingUrl;

        if (!string.IsNullOrWhiteSpace(request.CarrierCode))
            shipment.CarrierCode = request.CarrierCode;

        if (!string.IsNullOrWhiteSpace(request.CarrierName))
            shipment.CarrierName = request.CarrierName;

        shipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<ShipmentDto>.Ok(MapToDto(shipment), "Tracking added");
    }

    public async Task<ApiResponse<ShipmentDto>> UpdateLabelAsync(Guid tenantId, Guid id, UpdateLabelRequest request)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (shipment == null)
            return ApiResponse<ShipmentDto>.Fail("Shipment not found");

        shipment.LabelUrl = request.LabelUrl;
        shipment.LabelData = request.LabelData;

        if (shipment.Status == ShipmentStatus.Created || shipment.Status == ShipmentStatus.Packed)
        {
            shipment.Status = ShipmentStatus.LabelPrinted;
        }

        shipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<ShipmentDto>.Ok(MapToDto(shipment), "Label updated");
    }

    public async Task<ApiResponse<ShipmentDto>> MarkAsPackedAsync(Guid tenantId, Guid id)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (shipment == null)
            return ApiResponse<ShipmentDto>.Fail("Shipment not found");

        if (shipment.Status != ShipmentStatus.Created)
            return ApiResponse<ShipmentDto>.Fail($"Cannot mark as packed from '{shipment.Status}' status");

        shipment.Status = ShipmentStatus.Packed;
        shipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<ShipmentDto>.Ok(MapToDto(shipment), "Shipment marked as packed");
    }

    public async Task<ApiResponse<ShipmentDto>> ShipAsync(Guid tenantId, Guid id, ShipShipmentRequest? request = null)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (shipment == null)
            return ApiResponse<ShipmentDto>.Fail("Shipment not found");

        if (shipment.Status != ShipmentStatus.Packed && shipment.Status != ShipmentStatus.LabelPrinted)
            return ApiResponse<ShipmentDto>.Fail($"Cannot ship from '{shipment.Status}' status");

        if (request != null)
        {
            if (!string.IsNullOrWhiteSpace(request.TrackingNumber))
                shipment.TrackingNumber = request.TrackingNumber;

            if (!string.IsNullOrWhiteSpace(request.TrackingUrl))
                shipment.TrackingUrl = request.TrackingUrl;

            if (request.ShippingCost.HasValue)
            {
                shipment.ShippingCost = request.ShippingCost;
                shipment.CurrencyCode = request.CurrencyCode ?? "USD";
            }

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                shipment.Notes = string.IsNullOrWhiteSpace(shipment.Notes)
                    ? request.Notes
                    : $"{shipment.Notes}\n[Shipped] {request.Notes}";
            }
        }

        shipment.Status = ShipmentStatus.InTransit;
        shipment.ShippedAt = DateTime.UtcNow;
        shipment.UpdatedAt = DateTime.UtcNow;

        // Update order status
        if (shipment.Order != null && shipment.Order.Status != SalesOrderStatus.Shipped)
        {
            shipment.Order.Status = SalesOrderStatus.Shipped;
            shipment.Order.ShippedDate = DateTime.UtcNow;
            shipment.Order.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return ApiResponse<ShipmentDto>.Ok(MapToDto(shipment), "Shipment shipped");
    }

    public async Task<ApiResponse<ShipmentDto>> MarkAsDeliveredAsync(Guid tenantId, Guid id)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (shipment == null)
            return ApiResponse<ShipmentDto>.Fail("Shipment not found");

        if (shipment.Status != ShipmentStatus.InTransit && shipment.Status != ShipmentStatus.PickedUp)
            return ApiResponse<ShipmentDto>.Fail($"Cannot mark as delivered from '{shipment.Status}' status");

        shipment.Status = ShipmentStatus.Delivered;
        shipment.DeliveredAt = DateTime.UtcNow;
        shipment.UpdatedAt = DateTime.UtcNow;

        // Update order status
        if (shipment.Order != null)
        {
            shipment.Order.Status = SalesOrderStatus.Delivered;
            shipment.Order.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return ApiResponse<ShipmentDto>.Ok(MapToDto(shipment), "Shipment delivered");
    }

    public async Task<ApiResponse<ShipmentDto>> CancelShipmentAsync(Guid tenantId, Guid id, string? reason = null)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (shipment == null)
            return ApiResponse<ShipmentDto>.Fail("Shipment not found");

        if (shipment.Status == ShipmentStatus.Delivered || shipment.Status == ShipmentStatus.Cancelled)
            return ApiResponse<ShipmentDto>.Fail($"Cannot cancel shipment in '{shipment.Status}' status");

        shipment.Status = ShipmentStatus.Cancelled;

        if (!string.IsNullOrWhiteSpace(reason))
        {
            shipment.Notes = string.IsNullOrWhiteSpace(shipment.Notes)
                ? $"[Cancelled] {reason}"
                : $"{shipment.Notes}\n[Cancelled] {reason}";
        }

        shipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<ShipmentDto>.Ok(MapToDto(shipment), "Shipment cancelled");
    }

    #region Private Methods

    private async Task<string> GenerateShipmentNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"SH-{today:yyyyMMdd}-";

        var lastShipment = await _context.Shipments
            .Where(s => s.TenantId == tenantId && s.ShipmentNumber.StartsWith(prefix))
            .OrderByDescending(s => s.ShipmentNumber)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (lastShipment != null)
        {
            var lastNumber = lastShipment.ShipmentNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumber, out var parsed))
            {
                sequence = parsed + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }

    private static ShipmentDto MapToDto(Shipment shipment)
    {
        return new ShipmentDto
        {
            Id = shipment.Id,
            ShipmentNumber = shipment.ShipmentNumber,
            OrderId = shipment.OrderId,
            OrderNumber = shipment.Order?.OrderNumber,
            CustomerName = shipment.Order?.Customer?.Name,
            WarehouseId = shipment.WarehouseId,
            WarehouseCode = shipment.Warehouse?.Code,
            Status = shipment.Status,
            CarrierCode = shipment.CarrierCode,
            CarrierName = shipment.CarrierName,
            ServiceLevel = shipment.ServiceLevel,
            TrackingNumber = shipment.TrackingNumber,
            TrackingUrl = shipment.TrackingUrl,
            PackageCount = shipment.PackageCount,
            TotalWeightKg = shipment.TotalWeightKg,
            WidthMm = shipment.WidthMm,
            HeightMm = shipment.HeightMm,
            DepthMm = shipment.DepthMm,
            ShipToName = shipment.ShipToName,
            ShipToAddressLine1 = shipment.ShipToAddressLine1,
            ShipToAddressLine2 = shipment.ShipToAddressLine2,
            ShipToCity = shipment.ShipToCity,
            ShipToRegion = shipment.ShipToRegion,
            ShipToPostalCode = shipment.ShipToPostalCode,
            ShipToCountryCode = shipment.ShipToCountryCode,
            ShippedAt = shipment.ShippedAt,
            DeliveredAt = shipment.DeliveredAt,
            ShippingCost = shipment.ShippingCost,
            CurrencyCode = shipment.CurrencyCode,
            LabelUrl = shipment.LabelUrl,
            LabelData = shipment.LabelData,
            Notes = shipment.Notes,
            CreatedAt = shipment.CreatedAt,
            UpdatedAt = shipment.UpdatedAt
        };
    }

    #endregion
}
