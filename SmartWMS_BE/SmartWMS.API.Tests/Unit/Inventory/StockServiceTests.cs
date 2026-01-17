using Microsoft.EntityFrameworkCore;
using Moq;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Automation.Services;
using SmartWMS.API.Modules.Inventory.DTOs;
using SmartWMS.API.Modules.Inventory.Models;
using SmartWMS.API.Modules.Inventory.Services;
using SmartWMS.API.Modules.Warehouse.Models;
using Xunit;

namespace SmartWMS.API.Tests.Unit.Inventory;

public class StockServiceTests
{
    private readonly Mock<IAutomationEventPublisher> _automationEventsMock = new();

    private ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    private StockService CreateService(ApplicationDbContext context)
    {
        return new StockService(context, _automationEventsMock.Object);
    }

    private async Task<(Guid tenantId, Guid warehouseId, Guid zoneId, Guid locationId, Guid productId)> SeedTestDataAsync(ApplicationDbContext context)
    {
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        // Category
        context.ProductCategories.Add(new ProductCategory
        {
            Id = categoryId,
            TenantId = tenantId,
            Name = "Test Category",
            Code = "CAT001",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Warehouse
        context.Warehouses.Add(new Warehouse
        {
            Id = warehouseId,
            TenantId = tenantId,
            Name = "Test Warehouse",
            Code = "WH001",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Zone
        context.Zones.Add(new Zone
        {
            Id = zoneId,
            TenantId = tenantId,
            WarehouseId = warehouseId,
            Name = "Test Zone",
            Code = "Z001",
            ZoneType = ZoneType.Storage,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Location
        context.Locations.Add(new Location
        {
            Id = locationId,
            TenantId = tenantId,
            ZoneId = zoneId,
            Code = "A-01-01",
            Aisle = "A",
            Rack = "01",
            Level = "01",
            LocationType = LocationType.Bulk,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Product
        context.Products.Add(new Product
        {
            Id = productId,
            TenantId = tenantId,
            CategoryId = categoryId,
            Sku = "SKU001",
            Name = "Test Product",
            UnitOfMeasure = "EA",
            MinStockLevel = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        return (tenantId, warehouseId, zoneId, locationId, productId);
    }

    private StockLevel CreateStockLevel(Guid tenantId, Guid productId, Guid locationId, decimal onHand, decimal reserved = 0, string? batch = null)
    {
        return new StockLevel
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductId = productId,
            Sku = "SKU001",
            LocationId = locationId,
            QuantityOnHand = onHand,
            QuantityReserved = reserved,
            BatchNumber = batch,
            CreatedAt = DateTime.UtcNow
        };
    }

    #region Stock Level Queries

    [Fact]
    public async Task GetStockLevelsAsync_ReturnsEmptyWhenNoStock()
    {
        // Arrange
        var context = CreateContext(nameof(GetStockLevelsAsync_ReturnsEmptyWhenNoStock));
        var service = CreateService(context);
        var tenantId = Guid.NewGuid();

        // Act
        var result = await service.GetStockLevelsAsync(tenantId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Items);
    }

    [Fact]
    public async Task GetStockLevelsAsync_ReturnsStockWithFilters()
    {
        // Arrange
        var context = CreateContext(nameof(GetStockLevelsAsync_ReturnsStockWithFilters));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        // Add stock level
        context.StockLevels.Add(CreateStockLevel(tenantId, productId, locationId, 100, 20));
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetStockLevelsAsync(tenantId, new StockLevelFilters
        {
            ProductId = productId,
            HasAvailableStock = true
        });

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Data!.Items);
        Assert.Equal(100, result.Data.Items.First().QuantityOnHand);
    }

    [Fact]
    public async Task GetStockLevelByIdAsync_ReturnsNotFoundForInvalidId()
    {
        // Arrange
        var context = CreateContext(nameof(GetStockLevelByIdAsync_ReturnsNotFoundForInvalidId));
        var service = CreateService(context);
        var tenantId = Guid.NewGuid();

        // Act
        var result = await service.GetStockLevelByIdAsync(tenantId, Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message);
    }

    [Fact]
    public async Task GetProductStockSummaryAsync_ReturnsAggregatedData()
    {
        // Arrange
        var context = CreateContext(nameof(GetProductStockSummaryAsync_ReturnsAggregatedData));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        // Add second location
        var location2Id = Guid.NewGuid();
        context.Locations.Add(new Location
        {
            Id = location2Id,
            TenantId = tenantId,
            ZoneId = zoneId,
            Code = "A-01-02",
            Aisle = "A",
            Rack = "01",
            Level = "02",
            LocationType = LocationType.Bulk,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Add stock in both locations
        context.StockLevels.AddRange(
            CreateStockLevel(tenantId, productId, locationId, 50, 10),
            CreateStockLevel(tenantId, productId, location2Id, 30, 5)
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetProductStockSummaryAsync(tenantId, productId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(80, result.Data!.TotalOnHand);
        Assert.Equal(15, result.Data.TotalReserved);
        Assert.Equal(65, result.Data.TotalAvailable);
        Assert.Equal(2, result.Data.LocationCount);
    }

    [Fact]
    public async Task GetLowStockProductsAsync_ReturnsProductsBelowMinimum()
    {
        // Arrange
        var context = CreateContext(nameof(GetLowStockProductsAsync_ReturnsProductsBelowMinimum));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        // Add stock below minimum (minimum is 10)
        context.StockLevels.Add(CreateStockLevel(tenantId, productId, locationId, 5));
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetLowStockProductsAsync(tenantId);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Data!);
        Assert.Equal(5, result.Data!.First().TotalOnHand);
    }

    [Fact]
    public async Task GetAvailableQuantityAsync_ReturnsCorrectQuantity()
    {
        // Arrange
        var context = CreateContext(nameof(GetAvailableQuantityAsync_ReturnsCorrectQuantity));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        context.StockLevels.Add(CreateStockLevel(tenantId, productId, locationId, 100, 25));
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAvailableQuantityAsync(tenantId, productId, locationId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(75, result.Data);
    }

    #endregion

    #region Stock Operations

    [Fact]
    public async Task ReceiveStockAsync_CreatesNewStockLevel()
    {
        // Arrange
        var context = CreateContext(nameof(ReceiveStockAsync_CreatesNewStockLevel));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        var request = new ReceiveStockRequest
        {
            ProductId = productId,
            LocationId = locationId,
            Quantity = 50,
            BatchNumber = "BATCH001",
            Notes = "Initial receipt"
        };

        // Act
        var result = await service.ReceiveStockAsync(tenantId, request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(MovementType.Receipt, result.Data!.MovementType);
        Assert.Equal(50, result.Data.Quantity);

        // Verify stock level created
        var stockLevel = await context.StockLevels.FirstOrDefaultAsync(s => s.ProductId == productId);
        Assert.NotNull(stockLevel);
        Assert.Equal(50, stockLevel.QuantityOnHand);
    }

    [Fact]
    public async Task ReceiveStockAsync_UpdatesExistingStockLevel()
    {
        // Arrange
        var context = CreateContext(nameof(ReceiveStockAsync_UpdatesExistingStockLevel));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        // Add existing stock
        context.StockLevels.Add(CreateStockLevel(tenantId, productId, locationId, 30, 0, "BATCH001"));
        await context.SaveChangesAsync();

        var request = new ReceiveStockRequest
        {
            ProductId = productId,
            LocationId = locationId,
            Quantity = 20,
            BatchNumber = "BATCH001"
        };

        // Act
        var result = await service.ReceiveStockAsync(tenantId, request);

        // Assert
        Assert.True(result.Success);

        var stockLevel = await context.StockLevels.FirstOrDefaultAsync(s => s.ProductId == productId);
        Assert.Equal(50, stockLevel!.QuantityOnHand);
    }

    [Fact]
    public async Task ReceiveStockAsync_FailsWithZeroQuantity()
    {
        // Arrange
        var context = CreateContext(nameof(ReceiveStockAsync_FailsWithZeroQuantity));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        // Act
        var result = await service.ReceiveStockAsync(tenantId, new ReceiveStockRequest
        {
            ProductId = productId,
            LocationId = locationId,
            Quantity = 0
        });

        // Assert
        Assert.False(result.Success);
        Assert.Contains("greater than zero", result.Message);
    }

    [Fact]
    public async Task IssueStockAsync_DeductsFromStockLevel()
    {
        // Arrange
        var context = CreateContext(nameof(IssueStockAsync_DeductsFromStockLevel));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        context.StockLevels.Add(CreateStockLevel(tenantId, productId, locationId, 100));
        await context.SaveChangesAsync();

        var request = new IssueStockRequest
        {
            ProductId = productId,
            LocationId = locationId,
            Quantity = 30,
            ReferenceType = "SalesOrder",
            ReferenceNumber = "SO-001"
        };

        // Act
        var result = await service.IssueStockAsync(tenantId, request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(MovementType.Issue, result.Data!.MovementType);

        var stockLevel = await context.StockLevels.FirstOrDefaultAsync(s => s.ProductId == productId);
        Assert.Equal(70, stockLevel!.QuantityOnHand);
    }

    [Fact]
    public async Task IssueStockAsync_FailsWhenInsufficientStock()
    {
        // Arrange
        var context = CreateContext(nameof(IssueStockAsync_FailsWhenInsufficientStock));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        context.StockLevels.Add(CreateStockLevel(tenantId, productId, locationId, 10));
        await context.SaveChangesAsync();

        // Act
        var result = await service.IssueStockAsync(tenantId, new IssueStockRequest
        {
            ProductId = productId,
            LocationId = locationId,
            Quantity = 50
        });

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Insufficient", result.Message);
    }

    [Fact]
    public async Task TransferStockAsync_MovesStockBetweenLocations()
    {
        // Arrange
        var context = CreateContext(nameof(TransferStockAsync_MovesStockBetweenLocations));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        // Add destination location
        var toLocationId = Guid.NewGuid();
        context.Locations.Add(new Location
        {
            Id = toLocationId,
            TenantId = tenantId,
            ZoneId = zoneId,
            Code = "B-01-01",
            Aisle = "B",
            Rack = "01",
            Level = "01",
            LocationType = LocationType.Bulk,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Add source stock
        context.StockLevels.Add(CreateStockLevel(tenantId, productId, locationId, 100));
        await context.SaveChangesAsync();

        var request = new TransferStockRequest
        {
            ProductId = productId,
            FromLocationId = locationId,
            ToLocationId = toLocationId,
            Quantity = 40,
            ReasonCode = "REORG"
        };

        // Act
        var result = await service.TransferStockAsync(tenantId, request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(MovementType.Transfer, result.Data!.MovementType);

        var sourceStock = await context.StockLevels.FirstOrDefaultAsync(s => s.LocationId == locationId);
        var destStock = await context.StockLevels.FirstOrDefaultAsync(s => s.LocationId == toLocationId);

        Assert.Equal(60, sourceStock!.QuantityOnHand);
        Assert.Equal(40, destStock!.QuantityOnHand);
    }

    [Fact]
    public async Task TransferStockAsync_FailsWhenSameLocation()
    {
        // Arrange
        var context = CreateContext(nameof(TransferStockAsync_FailsWhenSameLocation));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        // Add source stock
        context.StockLevels.Add(CreateStockLevel(tenantId, productId, locationId, 100));
        await context.SaveChangesAsync();

        // Act
        var result = await service.TransferStockAsync(tenantId, new TransferStockRequest
        {
            ProductId = productId,
            FromLocationId = locationId,
            ToLocationId = locationId,
            Quantity = 10
        });

        // Assert
        Assert.False(result.Success);
        Assert.Contains("must be different", result.Message);
    }

    [Fact]
    public async Task AdjustStockAsync_IncreasesQuantity()
    {
        // Arrange
        var context = CreateContext(nameof(AdjustStockAsync_IncreasesQuantity));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        context.StockLevels.Add(CreateStockLevel(tenantId, productId, locationId, 50));
        await context.SaveChangesAsync();

        var request = new AdjustStockRequest
        {
            ProductId = productId,
            LocationId = locationId,
            NewQuantity = 75,
            ReasonCode = "CYCLE_COUNT",
            Notes = "Found additional stock"
        };

        // Act
        var result = await service.AdjustStockAsync(tenantId, request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(MovementType.Adjustment, result.Data!.MovementType);
        Assert.Equal(25, result.Data.Quantity); // Difference

        var stockLevel = await context.StockLevels.FirstOrDefaultAsync(s => s.ProductId == productId);
        Assert.Equal(75, stockLevel!.QuantityOnHand);
    }

    [Fact]
    public async Task AdjustStockAsync_DecreasesQuantity()
    {
        // Arrange
        var context = CreateContext(nameof(AdjustStockAsync_DecreasesQuantity));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        context.StockLevels.Add(CreateStockLevel(tenantId, productId, locationId, 50));
        await context.SaveChangesAsync();

        // Act
        var result = await service.AdjustStockAsync(tenantId, new AdjustStockRequest
        {
            ProductId = productId,
            LocationId = locationId,
            NewQuantity = 30,
            ReasonCode = "DAMAGE"
        });

        // Assert
        Assert.True(result.Success);
        Assert.Equal(20, result.Data!.Quantity); // Difference

        var stockLevel = await context.StockLevels.FirstOrDefaultAsync(s => s.ProductId == productId);
        Assert.Equal(30, stockLevel!.QuantityOnHand);
    }

    [Fact]
    public async Task AdjustStockAsync_FailsWithNegativeQuantity()
    {
        // Arrange
        var context = CreateContext(nameof(AdjustStockAsync_FailsWithNegativeQuantity));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        // Add existing stock
        context.StockLevels.Add(CreateStockLevel(tenantId, productId, locationId, 50));
        await context.SaveChangesAsync();

        // Act
        var result = await service.AdjustStockAsync(tenantId, new AdjustStockRequest
        {
            ProductId = productId,
            LocationId = locationId,
            NewQuantity = -10
        });

        // Assert
        Assert.False(result.Success);
        Assert.Contains("negative", result.Message);
    }

    #endregion

    #region Reservation Operations

    [Fact]
    public async Task ReserveStockAsync_IncreasesReservedQuantity()
    {
        // Arrange
        var context = CreateContext(nameof(ReserveStockAsync_IncreasesReservedQuantity));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        context.StockLevels.Add(CreateStockLevel(tenantId, productId, locationId, 100));
        await context.SaveChangesAsync();

        var request = new ReserveStockRequest
        {
            ProductId = productId,
            LocationId = locationId,
            Quantity = 30,
            ReferenceType = "SalesOrder",
            ReferenceNumber = "SO-001"
        };

        // Act
        var result = await service.ReserveStockAsync(tenantId, request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(100, result.Data!.QuantityOnHand);
        Assert.Equal(30, result.Data.QuantityReserved);
        Assert.Equal(70, result.Data.QuantityAvailable);
    }

    [Fact]
    public async Task ReserveStockAsync_FailsWhenInsufficientAvailable()
    {
        // Arrange
        var context = CreateContext(nameof(ReserveStockAsync_FailsWhenInsufficientAvailable));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        context.StockLevels.Add(CreateStockLevel(tenantId, productId, locationId, 50, 40));
        await context.SaveChangesAsync();

        // Act
        var result = await service.ReserveStockAsync(tenantId, new ReserveStockRequest
        {
            ProductId = productId,
            LocationId = locationId,
            Quantity = 20
        });

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Insufficient available", result.Message);
    }

    [Fact]
    public async Task ReleaseReservationAsync_DecreasesReservedQuantity()
    {
        // Arrange
        var context = CreateContext(nameof(ReleaseReservationAsync_DecreasesReservedQuantity));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        context.StockLevels.Add(CreateStockLevel(tenantId, productId, locationId, 100, 30));
        await context.SaveChangesAsync();

        var request = new ReleaseReservationRequest
        {
            ProductId = productId,
            LocationId = locationId,
            Quantity = 20
        };

        // Act
        var result = await service.ReleaseReservationAsync(tenantId, request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(100, result.Data!.QuantityOnHand);
        Assert.Equal(10, result.Data.QuantityReserved);
        Assert.Equal(90, result.Data.QuantityAvailable);
    }

    [Fact]
    public async Task ReleaseReservationAsync_FailsWhenReleasingMoreThanReserved()
    {
        // Arrange
        var context = CreateContext(nameof(ReleaseReservationAsync_FailsWhenReleasingMoreThanReserved));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        context.StockLevels.Add(CreateStockLevel(tenantId, productId, locationId, 100, 10));
        await context.SaveChangesAsync();

        // Act - trying to release more than reserved
        var result = await service.ReleaseReservationAsync(tenantId, new ReleaseReservationRequest
        {
            ProductId = productId,
            LocationId = locationId,
            Quantity = 50
        });

        // Assert - should fail because can't release more than reserved
        Assert.False(result.Success);
        Assert.Contains("Cannot release more than reserved", result.Message);
    }

    #endregion

    #region Stock Movement Queries

    [Fact]
    public async Task GetStockMovementsAsync_ReturnsFilteredMovements()
    {
        // Arrange
        var context = CreateContext(nameof(GetStockMovementsAsync_ReturnsFilteredMovements));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        // Add movements
        context.StockMovements.AddRange(
            new StockMovement
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                MovementNumber = "SM-001",
                MovementType = MovementType.Receipt,
                ProductId = productId,
                Sku = "SKU001",
                ToLocationId = locationId,
                Quantity = 100,
                MovementDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new StockMovement
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                MovementNumber = "SM-002",
                MovementType = MovementType.Issue,
                ProductId = productId,
                Sku = "SKU001",
                FromLocationId = locationId,
                Quantity = 30,
                MovementDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetStockMovementsAsync(tenantId, new StockMovementFilters
        {
            MovementType = MovementType.Receipt
        });

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Data!.Items);
        Assert.Equal(MovementType.Receipt, result.Data.Items.First().MovementType);
    }

    [Fact]
    public async Task GetProductMovementHistoryAsync_ReturnsOrderedHistory()
    {
        // Arrange
        var context = CreateContext(nameof(GetProductMovementHistoryAsync_ReturnsOrderedHistory));
        var (tenantId, warehouseId, zoneId, locationId, productId) = await SeedTestDataAsync(context);
        var service = CreateService(context);

        // Add movements at different times
        context.StockMovements.AddRange(
            new StockMovement
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                MovementNumber = "SM-001",
                MovementType = MovementType.Receipt,
                ProductId = productId,
                Sku = "SKU001",
                ToLocationId = locationId,
                Quantity = 100,
                MovementDate = DateTime.UtcNow.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new StockMovement
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                MovementNumber = "SM-002",
                MovementType = MovementType.Issue,
                ProductId = productId,
                Sku = "SKU001",
                FromLocationId = locationId,
                Quantity = 30,
                MovementDate = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new StockMovement
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                MovementNumber = "SM-003",
                MovementType = MovementType.Transfer,
                ProductId = productId,
                Sku = "SKU001",
                FromLocationId = locationId,
                ToLocationId = Guid.NewGuid(),
                Quantity = 20,
                MovementDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetProductMovementHistoryAsync(tenantId, productId, 10);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(3, result.Data!.Count());

        // Should be ordered by date descending (newest first)
        var items = result.Data!.ToList();
        Assert.Equal("SM-003", items[0].MovementNumber);
        Assert.Equal("SM-002", items[1].MovementNumber);
        Assert.Equal("SM-001", items[2].MovementNumber);
    }

    #endregion
}
