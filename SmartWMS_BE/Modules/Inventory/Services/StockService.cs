using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Inventory.DTOs;
using SmartWMS.API.Modules.Inventory.Models;
using SmartWMS.API.Modules.Automation.Services;

namespace SmartWMS.API.Modules.Inventory.Services;

public class StockService : IStockService
{
    private readonly ApplicationDbContext _context;
    private readonly IAutomationEventPublisher _automationEvents;

    public StockService(ApplicationDbContext context, IAutomationEventPublisher automationEvents)
    {
        _context = context;
        _automationEvents = automationEvents;
    }

    #region Stock Level Queries

    public async Task<ApiResponse<PaginatedResult<StockLevelDto>>> GetStockLevelsAsync(
        Guid tenantId,
        StockLevelFilters? filters = null,
        int page = 1,
        int pageSize = 25)
    {
        var query = _context.StockLevels
            .Include(s => s.Product)
            .Include(s => s.Location)
                .ThenInclude(l => l.Zone)
                    .ThenInclude(z => z!.Warehouse)
            .Where(s => s.TenantId == tenantId)
            .AsQueryable();

        if (filters != null)
        {
            if (filters.ProductId.HasValue)
                query = query.Where(s => s.ProductId == filters.ProductId.Value);

            if (filters.LocationId.HasValue)
                query = query.Where(s => s.LocationId == filters.LocationId.Value);

            if (filters.WarehouseId.HasValue)
                query = query.Where(s => s.Location.Zone != null && s.Location.Zone.WarehouseId == filters.WarehouseId.Value);

            if (filters.ZoneId.HasValue)
                query = query.Where(s => s.Location.ZoneId == filters.ZoneId.Value);

            if (!string.IsNullOrWhiteSpace(filters.Sku))
                query = query.Where(s => s.Sku.Contains(filters.Sku));

            if (!string.IsNullOrWhiteSpace(filters.BatchNumber))
                query = query.Where(s => s.BatchNumber == filters.BatchNumber);

            if (filters.HasAvailableStock == true)
                query = query.Where(s => s.QuantityOnHand - s.QuantityReserved > 0);

            if (filters.IsExpiringSoon == true && filters.ExpiringWithinDays.HasValue)
            {
                var expiryThreshold = DateTime.UtcNow.AddDays(filters.ExpiringWithinDays.Value);
                query = query.Where(s => s.ExpiryDate.HasValue && s.ExpiryDate <= expiryThreshold);
            }
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(s => s.Location.Code)
            .ThenBy(s => s.Sku)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => MapStockLevelToDto(s))
            .ToListAsync();

        var result = new PaginatedResult<StockLevelDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<StockLevelDto>>.Ok(result);
    }

    public async Task<ApiResponse<StockLevelDto>> GetStockLevelByIdAsync(Guid tenantId, Guid id)
    {
        var stockLevel = await _context.StockLevels
            .Include(s => s.Product)
            .Include(s => s.Location)
                .ThenInclude(l => l.Zone)
                    .ThenInclude(z => z!.Warehouse)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (stockLevel == null)
            return ApiResponse<StockLevelDto>.Fail("Stock level not found");

        return ApiResponse<StockLevelDto>.Ok(MapStockLevelToDto(stockLevel));
    }

    public async Task<ApiResponse<ProductStockSummaryDto>> GetProductStockSummaryAsync(Guid tenantId, Guid productId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == productId);

        if (product == null)
            return ApiResponse<ProductStockSummaryDto>.Fail("Product not found");

        var stockLevels = await _context.StockLevels
            .Include(s => s.Location)
                .ThenInclude(l => l.Zone)
                    .ThenInclude(z => z!.Warehouse)
            .Where(s => s.TenantId == tenantId && s.ProductId == productId && s.QuantityOnHand > 0)
            .ToListAsync();

        var summary = new ProductStockSummaryDto
        {
            ProductId = productId,
            Sku = product.Sku,
            ProductName = product.Name,
            TotalOnHand = stockLevels.Sum(s => s.QuantityOnHand),
            TotalReserved = stockLevels.Sum(s => s.QuantityReserved),
            TotalAvailable = stockLevels.Sum(s => s.QuantityAvailable),
            LocationCount = stockLevels.Count,
            Locations = stockLevels.Select(MapStockLevelToDto).ToList()
        };

        return ApiResponse<ProductStockSummaryDto>.Ok(summary);
    }

    public async Task<ApiResponse<IEnumerable<ProductStockSummaryDto>>> GetLowStockProductsAsync(Guid tenantId, Guid? warehouseId = null)
    {
        var query = _context.StockLevels
            .Include(s => s.Product)
            .Include(s => s.Location)
                .ThenInclude(l => l.Zone)
            .Where(s => s.TenantId == tenantId);

        if (warehouseId.HasValue)
            query = query.Where(s => s.Location.Zone != null && s.Location.Zone.WarehouseId == warehouseId.Value);

        var stockByProduct = await query
            .GroupBy(s => new { s.ProductId, s.Product.Sku, s.Product.Name, s.Product.MinStockLevel })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.Sku,
                g.Key.Name,
                g.Key.MinStockLevel,
                TotalOnHand = g.Sum(s => s.QuantityOnHand),
                TotalReserved = g.Sum(s => s.QuantityReserved),
                LocationCount = g.Count()
            })
            .Where(x => x.MinStockLevel.HasValue && x.TotalOnHand - x.TotalReserved < x.MinStockLevel)
            .ToListAsync();

        var summaries = stockByProduct.Select(x => new ProductStockSummaryDto
        {
            ProductId = x.ProductId,
            Sku = x.Sku,
            ProductName = x.Name,
            TotalOnHand = x.TotalOnHand,
            TotalReserved = x.TotalReserved,
            TotalAvailable = x.TotalOnHand - x.TotalReserved,
            LocationCount = x.LocationCount
        });

        return ApiResponse<IEnumerable<ProductStockSummaryDto>>.Ok(summaries);
    }

    #endregion

    #region Stock Movement Queries

    public async Task<ApiResponse<PaginatedResult<StockMovementDto>>> GetStockMovementsAsync(
        Guid tenantId,
        StockMovementFilters? filters = null,
        int page = 1,
        int pageSize = 25)
    {
        var query = _context.StockMovements
            .Include(m => m.Product)
            .Include(m => m.FromLocation)
            .Include(m => m.ToLocation)
            .Where(m => m.TenantId == tenantId)
            .AsQueryable();

        if (filters != null)
        {
            if (filters.ProductId.HasValue)
                query = query.Where(m => m.ProductId == filters.ProductId.Value);

            if (filters.LocationId.HasValue)
                query = query.Where(m => m.FromLocationId == filters.LocationId || m.ToLocationId == filters.LocationId);

            if (filters.MovementType.HasValue)
                query = query.Where(m => m.MovementType == filters.MovementType.Value);

            if (!string.IsNullOrWhiteSpace(filters.ReferenceType))
                query = query.Where(m => m.ReferenceType == filters.ReferenceType);

            if (filters.ReferenceId.HasValue)
                query = query.Where(m => m.ReferenceId == filters.ReferenceId.Value);

            if (filters.DateFrom.HasValue)
                query = query.Where(m => m.MovementDate >= filters.DateFrom.Value);

            if (filters.DateTo.HasValue)
                query = query.Where(m => m.MovementDate <= filters.DateTo.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(m => m.MovementDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => MapMovementToDto(m))
            .ToListAsync();

        var result = new PaginatedResult<StockMovementDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PaginatedResult<StockMovementDto>>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<StockMovementDto>>> GetProductMovementHistoryAsync(
        Guid tenantId, Guid productId, int limit = 50)
    {
        var movements = await _context.StockMovements
            .Include(m => m.Product)
            .Include(m => m.FromLocation)
            .Include(m => m.ToLocation)
            .Where(m => m.TenantId == tenantId && m.ProductId == productId)
            .OrderByDescending(m => m.MovementDate)
            .Take(limit)
            .Select(m => MapMovementToDto(m))
            .ToListAsync();

        return ApiResponse<IEnumerable<StockMovementDto>>.Ok(movements);
    }

    #endregion

    #region Stock Operations

    public async Task<ApiResponse<StockMovementDto>> ReceiveStockAsync(Guid tenantId, ReceiveStockRequest request)
    {
        // Validate product
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ProductId);

        if (product == null)
            return ApiResponse<StockMovementDto>.Fail("Product not found");

        // Validate location
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.LocationId);

        if (location == null)
            return ApiResponse<StockMovementDto>.Fail("Location not found");

        if (request.Quantity <= 0)
            return ApiResponse<StockMovementDto>.Fail("Quantity must be greater than zero");

        // Find or create stock level
        var stockLevel = await FindOrCreateStockLevel(
            tenantId, request.ProductId, product.Sku, request.LocationId,
            request.BatchNumber, request.SerialNumber, request.ExpiryDate);

        // Update stock level
        stockLevel.QuantityOnHand += request.Quantity;
        stockLevel.LastMovementAt = DateTime.UtcNow;
        stockLevel.UpdatedAt = DateTime.UtcNow;

        // Create movement record
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MovementNumber = await GenerateMovementNumberAsync(tenantId),
            MovementType = MovementType.Receipt,
            ProductId = request.ProductId,
            Sku = product.Sku,
            ToLocationId = request.LocationId,
            Quantity = request.Quantity,
            BatchNumber = request.BatchNumber,
            SerialNumber = request.SerialNumber,
            ReferenceType = request.ReferenceType,
            ReferenceId = request.ReferenceId,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            MovementDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        // Reload with navigation
        await _context.Entry(movement).Reference(m => m.Product).LoadAsync();
        await _context.Entry(movement).Reference(m => m.ToLocation).LoadAsync();

        return ApiResponse<StockMovementDto>.Ok(MapMovementToDto(movement), "Stock received successfully");
    }

    public async Task<ApiResponse<StockMovementDto>> IssueStockAsync(Guid tenantId, IssueStockRequest request)
    {
        // Validate product
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ProductId);

        if (product == null)
            return ApiResponse<StockMovementDto>.Fail("Product not found");

        if (request.Quantity <= 0)
            return ApiResponse<StockMovementDto>.Fail("Quantity must be greater than zero");

        // Find stock level
        var stockLevel = await _context.StockLevels
            .FirstOrDefaultAsync(s =>
                s.TenantId == tenantId &&
                s.ProductId == request.ProductId &&
                s.LocationId == request.LocationId &&
                (request.BatchNumber == null || s.BatchNumber == request.BatchNumber) &&
                (request.SerialNumber == null || s.SerialNumber == request.SerialNumber));

        if (stockLevel == null)
            return ApiResponse<StockMovementDto>.Fail("Stock not found at specified location");

        if (stockLevel.QuantityAvailable < request.Quantity)
            return ApiResponse<StockMovementDto>.Fail($"Insufficient available stock. Available: {stockLevel.QuantityAvailable}");

        // Update stock level
        stockLevel.QuantityOnHand -= request.Quantity;
        stockLevel.LastMovementAt = DateTime.UtcNow;
        stockLevel.UpdatedAt = DateTime.UtcNow;

        // Create movement record
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MovementNumber = await GenerateMovementNumberAsync(tenantId),
            MovementType = MovementType.Issue,
            ProductId = request.ProductId,
            Sku = product.Sku,
            FromLocationId = request.LocationId,
            Quantity = request.Quantity,
            BatchNumber = request.BatchNumber,
            SerialNumber = request.SerialNumber,
            ReferenceType = request.ReferenceType,
            ReferenceId = request.ReferenceId,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            MovementDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        // Reload with navigation
        await _context.Entry(movement).Reference(m => m.Product).LoadAsync();
        await _context.Entry(movement).Reference(m => m.FromLocation).LoadAsync();

        // Check for low stock threshold and publish automation event
        if (product.MinStockLevel.HasValue && stockLevel.QuantityOnHand < product.MinStockLevel.Value)
        {
            await _automationEvents.PublishStockLowAsync(
                tenantId,
                product.Id,
                product.Sku,
                request.LocationId,
                movement.FromLocation?.Code ?? "",
                stockLevel.QuantityOnHand,
                product.MinStockLevel.Value);
        }

        return ApiResponse<StockMovementDto>.Ok(MapMovementToDto(movement), "Stock issued successfully");
    }

    public async Task<ApiResponse<StockMovementDto>> TransferStockAsync(Guid tenantId, TransferStockRequest request)
    {
        // Validate product
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ProductId);

        if (product == null)
            return ApiResponse<StockMovementDto>.Fail("Product not found");

        // Validate locations
        var fromLocation = await _context.Locations
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.FromLocationId);

        if (fromLocation == null)
            return ApiResponse<StockMovementDto>.Fail("From location not found");

        var toLocation = await _context.Locations
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.ToLocationId);

        if (toLocation == null)
            return ApiResponse<StockMovementDto>.Fail("To location not found");

        if (request.FromLocationId == request.ToLocationId)
            return ApiResponse<StockMovementDto>.Fail("From and To locations must be different");

        if (request.Quantity <= 0)
            return ApiResponse<StockMovementDto>.Fail("Quantity must be greater than zero");

        // Find source stock level
        var fromStock = await _context.StockLevels
            .FirstOrDefaultAsync(s =>
                s.TenantId == tenantId &&
                s.ProductId == request.ProductId &&
                s.LocationId == request.FromLocationId &&
                (request.BatchNumber == null || s.BatchNumber == request.BatchNumber));

        if (fromStock == null)
            return ApiResponse<StockMovementDto>.Fail("Stock not found at source location");

        if (fromStock.QuantityAvailable < request.Quantity)
            return ApiResponse<StockMovementDto>.Fail($"Insufficient available stock. Available: {fromStock.QuantityAvailable}");

        // Update source stock
        fromStock.QuantityOnHand -= request.Quantity;
        fromStock.LastMovementAt = DateTime.UtcNow;
        fromStock.UpdatedAt = DateTime.UtcNow;

        // Find or create destination stock level
        var toStock = await FindOrCreateStockLevel(
            tenantId, request.ProductId, product.Sku, request.ToLocationId,
            request.BatchNumber, request.SerialNumber, fromStock.ExpiryDate);

        // Update destination stock
        toStock.QuantityOnHand += request.Quantity;
        toStock.LastMovementAt = DateTime.UtcNow;
        toStock.UpdatedAt = DateTime.UtcNow;

        // Create movement record
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MovementNumber = await GenerateMovementNumberAsync(tenantId),
            MovementType = MovementType.Transfer,
            ProductId = request.ProductId,
            Sku = product.Sku,
            FromLocationId = request.FromLocationId,
            ToLocationId = request.ToLocationId,
            Quantity = request.Quantity,
            BatchNumber = request.BatchNumber,
            SerialNumber = request.SerialNumber,
            ReasonCode = request.ReasonCode,
            Notes = request.Notes,
            MovementDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        // Reload with navigation
        await _context.Entry(movement).Reference(m => m.Product).LoadAsync();
        await _context.Entry(movement).Reference(m => m.FromLocation).LoadAsync();
        await _context.Entry(movement).Reference(m => m.ToLocation).LoadAsync();

        return ApiResponse<StockMovementDto>.Ok(MapMovementToDto(movement), "Stock transferred successfully");
    }

    public async Task<ApiResponse<StockMovementDto>> AdjustStockAsync(Guid tenantId, AdjustStockRequest request)
    {
        // Validate product
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ProductId);

        if (product == null)
            return ApiResponse<StockMovementDto>.Fail("Product not found");

        // Validate location
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.LocationId);

        if (location == null)
            return ApiResponse<StockMovementDto>.Fail("Location not found");

        if (request.NewQuantity < 0)
            return ApiResponse<StockMovementDto>.Fail("New quantity cannot be negative");

        // Find or create stock level
        var stockLevel = await FindOrCreateStockLevel(
            tenantId, request.ProductId, product.Sku, request.LocationId,
            request.BatchNumber, request.SerialNumber, null);

        var oldQuantity = stockLevel.QuantityOnHand;
        var adjustmentQuantity = request.NewQuantity - oldQuantity;

        if (adjustmentQuantity == 0)
            return ApiResponse<StockMovementDto>.Fail("No adjustment needed - quantity unchanged");

        // Check reserved quantity
        if (request.NewQuantity < stockLevel.QuantityReserved)
            return ApiResponse<StockMovementDto>.Fail($"Cannot adjust below reserved quantity ({stockLevel.QuantityReserved})");

        // Update stock level
        stockLevel.QuantityOnHand = request.NewQuantity;
        stockLevel.LastMovementAt = DateTime.UtcNow;
        stockLevel.LastCountAt = DateTime.UtcNow;
        stockLevel.UpdatedAt = DateTime.UtcNow;

        // Create movement record
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MovementNumber = await GenerateMovementNumberAsync(tenantId),
            MovementType = MovementType.Adjustment,
            ProductId = request.ProductId,
            Sku = product.Sku,
            ToLocationId = adjustmentQuantity > 0 ? request.LocationId : null,
            FromLocationId = adjustmentQuantity < 0 ? request.LocationId : null,
            Quantity = Math.Abs(adjustmentQuantity),
            BatchNumber = request.BatchNumber,
            SerialNumber = request.SerialNumber,
            ReasonCode = request.ReasonCode,
            Notes = $"Adjusted from {oldQuantity} to {request.NewQuantity}. {request.Notes}",
            MovementDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        // Reload with navigation
        await _context.Entry(movement).Reference(m => m.Product).LoadAsync();

        return ApiResponse<StockMovementDto>.Ok(MapMovementToDto(movement), "Stock adjusted successfully");
    }

    #endregion

    #region Reservation Operations

    public async Task<ApiResponse<StockLevelDto>> ReserveStockAsync(Guid tenantId, ReserveStockRequest request)
    {
        if (request.Quantity <= 0)
            return ApiResponse<StockLevelDto>.Fail("Quantity must be greater than zero");

        // Find stock level
        var stockLevel = await _context.StockLevels
            .Include(s => s.Product)
            .Include(s => s.Location)
                .ThenInclude(l => l.Zone)
                    .ThenInclude(z => z!.Warehouse)
            .FirstOrDefaultAsync(s =>
                s.TenantId == tenantId &&
                s.ProductId == request.ProductId &&
                s.LocationId == request.LocationId &&
                (request.BatchNumber == null || s.BatchNumber == request.BatchNumber));

        if (stockLevel == null)
            return ApiResponse<StockLevelDto>.Fail("Stock not found at specified location");

        if (stockLevel.QuantityAvailable < request.Quantity)
            return ApiResponse<StockLevelDto>.Fail($"Insufficient available stock. Available: {stockLevel.QuantityAvailable}");

        // Reserve stock
        stockLevel.QuantityReserved += request.Quantity;
        stockLevel.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<StockLevelDto>.Ok(MapStockLevelToDto(stockLevel), "Stock reserved successfully");
    }

    public async Task<ApiResponse<StockLevelDto>> ReleaseReservationAsync(Guid tenantId, ReleaseReservationRequest request)
    {
        if (request.Quantity <= 0)
            return ApiResponse<StockLevelDto>.Fail("Quantity must be greater than zero");

        // Find stock level
        var stockLevel = await _context.StockLevels
            .Include(s => s.Product)
            .Include(s => s.Location)
                .ThenInclude(l => l.Zone)
                    .ThenInclude(z => z!.Warehouse)
            .FirstOrDefaultAsync(s =>
                s.TenantId == tenantId &&
                s.ProductId == request.ProductId &&
                s.LocationId == request.LocationId &&
                (request.BatchNumber == null || s.BatchNumber == request.BatchNumber));

        if (stockLevel == null)
            return ApiResponse<StockLevelDto>.Fail("Stock not found at specified location");

        if (stockLevel.QuantityReserved < request.Quantity)
            return ApiResponse<StockLevelDto>.Fail($"Cannot release more than reserved. Reserved: {stockLevel.QuantityReserved}");

        // Release reservation
        stockLevel.QuantityReserved -= request.Quantity;
        stockLevel.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<StockLevelDto>.Ok(MapStockLevelToDto(stockLevel), "Reservation released successfully");
    }

    #endregion

    #region Availability Check

    public async Task<ApiResponse<decimal>> GetAvailableQuantityAsync(
        Guid tenantId, Guid productId, Guid? locationId = null, string? batchNumber = null)
    {
        var query = _context.StockLevels
            .Where(s => s.TenantId == tenantId && s.ProductId == productId);

        if (locationId.HasValue)
            query = query.Where(s => s.LocationId == locationId.Value);

        if (!string.IsNullOrWhiteSpace(batchNumber))
            query = query.Where(s => s.BatchNumber == batchNumber);

        var totalAvailable = await query.SumAsync(s => s.QuantityOnHand - s.QuantityReserved);

        return ApiResponse<decimal>.Ok(totalAvailable);
    }

    #endregion

    #region Private Methods

    private async Task<StockLevel> FindOrCreateStockLevel(
        Guid tenantId,
        Guid productId,
        string sku,
        Guid locationId,
        string? batchNumber,
        string? serialNumber,
        DateTime? expiryDate)
    {
        var stockLevel = await _context.StockLevels
            .FirstOrDefaultAsync(s =>
                s.TenantId == tenantId &&
                s.ProductId == productId &&
                s.LocationId == locationId &&
                s.BatchNumber == batchNumber &&
                s.SerialNumber == serialNumber);

        if (stockLevel == null)
        {
            stockLevel = new StockLevel
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductId = productId,
                Sku = sku,
                LocationId = locationId,
                BatchNumber = batchNumber,
                SerialNumber = serialNumber,
                ExpiryDate = expiryDate,
                QuantityOnHand = 0,
                QuantityReserved = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.StockLevels.Add(stockLevel);
        }
        else if (expiryDate.HasValue && !stockLevel.ExpiryDate.HasValue)
        {
            stockLevel.ExpiryDate = expiryDate;
        }

        return stockLevel;
    }

    private async Task<string> GenerateMovementNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"SM-{today:yyyyMMdd}-";

        var lastMovement = await _context.StockMovements
            .Where(m => m.TenantId == tenantId && m.MovementNumber.StartsWith(prefix))
            .OrderByDescending(m => m.MovementNumber)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (lastMovement != null)
        {
            var lastNumber = lastMovement.MovementNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumber, out var parsed))
            {
                sequence = parsed + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }

    private static StockLevelDto MapStockLevelToDto(StockLevel s)
    {
        return new StockLevelDto
        {
            Id = s.Id,
            ProductId = s.ProductId,
            Sku = s.Sku,
            ProductName = s.Product?.Name,
            LocationId = s.LocationId,
            LocationCode = s.Location?.Code,
            WarehouseName = s.Location?.Zone?.Warehouse?.Name,
            ZoneName = s.Location?.Zone?.Name,
            QuantityOnHand = s.QuantityOnHand,
            QuantityReserved = s.QuantityReserved,
            QuantityAvailable = s.QuantityAvailable,
            BatchNumber = s.BatchNumber,
            SerialNumber = s.SerialNumber,
            ExpiryDate = s.ExpiryDate,
            LastMovementAt = s.LastMovementAt,
            LastCountAt = s.LastCountAt,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
    }

    private static StockMovementDto MapMovementToDto(StockMovement m)
    {
        return new StockMovementDto
        {
            Id = m.Id,
            MovementNumber = m.MovementNumber,
            MovementType = m.MovementType,
            ProductId = m.ProductId,
            Sku = m.Sku,
            ProductName = m.Product?.Name,
            FromLocationId = m.FromLocationId,
            FromLocationCode = m.FromLocation?.Code,
            ToLocationId = m.ToLocationId,
            ToLocationCode = m.ToLocation?.Code,
            Quantity = m.Quantity,
            BatchNumber = m.BatchNumber,
            SerialNumber = m.SerialNumber,
            ReferenceType = m.ReferenceType,
            ReferenceId = m.ReferenceId,
            ReferenceNumber = m.ReferenceNumber,
            ReasonCode = m.ReasonCode,
            Notes = m.Notes,
            MovementDate = m.MovementDate,
            CreatedAt = m.CreatedAt
        };
    }

    #endregion
}
