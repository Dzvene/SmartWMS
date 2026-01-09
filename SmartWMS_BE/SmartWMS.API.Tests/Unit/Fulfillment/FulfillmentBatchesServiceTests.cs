using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Companies.Models;
using SmartWMS.API.Modules.Fulfillment.DTOs;
using SmartWMS.API.Modules.Fulfillment.Models;
using SmartWMS.API.Modules.Fulfillment.Services;
using SmartWMS.API.Modules.Orders.Models;

namespace SmartWMS.API.Tests.Unit.Fulfillment;

public class FulfillmentBatchesServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly FulfillmentBatchesService _batchesService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _warehouseId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _batchId = Guid.NewGuid();

    public FulfillmentBatchesServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _batchesService = new FulfillmentBatchesService(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        _context.Companies.Add(new Company
        {
            Id = _tenantId,
            Code = "TEST",
            Name = "Test Company",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        _context.Warehouses.Add(new Modules.Warehouse.Models.Warehouse
        {
            Id = _warehouseId,
            TenantId = _tenantId,
            Code = "WH-001",
            Name = "Test Warehouse",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        _context.Customers.Add(new Customer
        {
            Id = _customerId,
            TenantId = _tenantId,
            Code = "CUST-001",
            Name = "Test Customer",
            City = "Test City",
            CountryCode = "US",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Add confirmed order for batch tests
        _context.SalesOrders.Add(new SalesOrder
        {
            Id = _orderId,
            TenantId = _tenantId,
            OrderNumber = "SO-TEST-0001",
            CustomerId = _customerId,
            WarehouseId = _warehouseId,
            Status = SalesOrderStatus.Confirmed,
            Priority = OrderPriority.Normal,
            OrderDate = DateTime.UtcNow,
            TotalLines = 1,
            TotalQuantity = 10,
            CreatedAt = DateTime.UtcNow
        });

        // Add test batch
        _context.FulfillmentBatches.Add(new FulfillmentBatch
        {
            Id = _batchId,
            TenantId = _tenantId,
            BatchNumber = "FB-TEST-0001",
            Name = "Test Batch",
            WarehouseId = _warehouseId,
            BatchType = BatchType.Multi,
            Status = FulfillmentStatus.Created,
            Priority = 0,
            CreatedAt = DateTime.UtcNow
        });

        // Add a released batch for status tests
        _context.FulfillmentBatches.Add(new FulfillmentBatch
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            BatchNumber = "FB-TEST-0002",
            Name = "Released Batch",
            WarehouseId = _warehouseId,
            BatchType = BatchType.Single,
            Status = FulfillmentStatus.Released,
            Priority = 1,
            ReleasedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetBatches Tests

    [Fact]
    public async Task GetBatchesAsync_ReturnsAllBatches()
    {
        var result = await _batchesService.GetBatchesAsync(_tenantId);

        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBatchesAsync_WithStatusFilter_ReturnsFilteredResults()
    {
        var filters = new FulfillmentBatchFilters { Status = FulfillmentStatus.Created };
        var result = await _batchesService.GetBatchesAsync(_tenantId, filters);

        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().Status.Should().Be(FulfillmentStatus.Created);
    }

    [Fact]
    public async Task GetBatchesAsync_WithSearchFilter_ReturnsMatchingResults()
    {
        var filters = new FulfillmentBatchFilters { Search = "0001" };
        var result = await _batchesService.GetBatchesAsync(_tenantId, filters);

        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().BatchNumber.Should().Be("FB-TEST-0001");
    }

    [Fact]
    public async Task GetBatchesAsync_WithWarehouseFilter_ReturnsFilteredResults()
    {
        var filters = new FulfillmentBatchFilters { WarehouseId = _warehouseId };
        var result = await _batchesService.GetBatchesAsync(_tenantId, filters);

        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBatchesAsync_OrdersByPriorityDescending()
    {
        var result = await _batchesService.GetBatchesAsync(_tenantId);

        result.Success.Should().BeTrue();
        result.Data!.Items.First().Priority.Should().Be(1); // Higher priority first
    }

    #endregion

    #region GetBatchById Tests

    [Fact]
    public async Task GetBatchByIdAsync_WithValidId_ReturnsBatch()
    {
        var result = await _batchesService.GetBatchByIdAsync(_tenantId, _batchId);

        result.Success.Should().BeTrue();
        result.Data!.BatchNumber.Should().Be("FB-TEST-0001");
    }

    [Fact]
    public async Task GetBatchByIdAsync_WithInvalidId_ReturnsFailure()
    {
        var result = await _batchesService.GetBatchByIdAsync(_tenantId, Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    #endregion

    #region GetBatchByNumber Tests

    [Fact]
    public async Task GetBatchByNumberAsync_WithValidNumber_ReturnsBatch()
    {
        var result = await _batchesService.GetBatchByNumberAsync(_tenantId, "FB-TEST-0001");

        result.Success.Should().BeTrue();
        result.Data!.Id.Should().Be(_batchId);
    }

    [Fact]
    public async Task GetBatchByNumberAsync_WithInvalidNumber_ReturnsFailure()
    {
        var result = await _batchesService.GetBatchByNumberAsync(_tenantId, "INVALID");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    #endregion

    #region CreateBatch Tests

    [Fact]
    public async Task CreateBatchAsync_WithValidData_CreatesBatch()
    {
        var request = new CreateFulfillmentBatchRequest
        {
            Name = "New Batch",
            WarehouseId = _warehouseId,
            BatchType = BatchType.Single,
            Priority = 5
        };

        var result = await _batchesService.CreateBatchAsync(_tenantId, request);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.BatchNumber.Should().StartWith("FB-");
        result.Data.Name.Should().Be("New Batch");
        result.Data.Status.Should().Be(FulfillmentStatus.Created);
    }

    [Fact]
    public async Task CreateBatchAsync_WithCustomBatchNumber_UsesProvidedNumber()
    {
        var request = new CreateFulfillmentBatchRequest
        {
            BatchNumber = "FB-CUSTOM-001",
            WarehouseId = _warehouseId
        };

        var result = await _batchesService.CreateBatchAsync(_tenantId, request);

        result.Success.Should().BeTrue();
        result.Data!.BatchNumber.Should().Be("FB-CUSTOM-001");
    }

    [Fact]
    public async Task CreateBatchAsync_WithInvalidWarehouse_ReturnsFailure()
    {
        var request = new CreateFulfillmentBatchRequest
        {
            WarehouseId = Guid.NewGuid()
        };

        var result = await _batchesService.CreateBatchAsync(_tenantId, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Warehouse not found");
    }

    [Fact]
    public async Task CreateBatchAsync_WithDuplicateBatchNumber_ReturnsFailure()
    {
        var request = new CreateFulfillmentBatchRequest
        {
            BatchNumber = "FB-TEST-0001",
            WarehouseId = _warehouseId
        };

        var result = await _batchesService.CreateBatchAsync(_tenantId, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateBatchAsync_WithOrders_AddsOrdersToBatch()
    {
        var request = new CreateFulfillmentBatchRequest
        {
            WarehouseId = _warehouseId,
            OrderIds = new List<Guid> { _orderId }
        };

        var result = await _batchesService.CreateBatchAsync(_tenantId, request);

        result.Success.Should().BeTrue();
        result.Data!.OrderCount.Should().Be(1);
        result.Data.TotalQuantity.Should().Be(10);
    }

    #endregion

    #region UpdateBatch Tests

    [Fact]
    public async Task UpdateBatchAsync_WithValidData_UpdatesBatch()
    {
        var request = new UpdateFulfillmentBatchRequest
        {
            Name = "Updated Name",
            Priority = 10
        };

        var result = await _batchesService.UpdateBatchAsync(_tenantId, _batchId, request);

        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Updated Name");
        result.Data.Priority.Should().Be(10);
    }

    [Fact]
    public async Task UpdateBatchAsync_WithNonCreatedStatus_ReturnsFailure()
    {
        var releasedBatch = await _context.FulfillmentBatches
            .FirstAsync(b => b.Status == FulfillmentStatus.Released);

        var request = new UpdateFulfillmentBatchRequest { Name = "Try Update" };

        var result = await _batchesService.UpdateBatchAsync(_tenantId, releasedBatch.Id, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot update");
    }

    #endregion

    #region DeleteBatch Tests

    [Fact]
    public async Task DeleteBatchAsync_WithCreatedStatus_DeletesBatch()
    {
        var result = await _batchesService.DeleteBatchAsync(_tenantId, _batchId);

        result.Success.Should().BeTrue();

        var dbBatch = await _context.FulfillmentBatches.FindAsync(_batchId);
        dbBatch.Should().BeNull();
    }

    [Fact]
    public async Task DeleteBatchAsync_WithReleasedStatus_ReturnsFailure()
    {
        var releasedBatch = await _context.FulfillmentBatches
            .FirstAsync(b => b.Status == FulfillmentStatus.Released);

        var result = await _batchesService.DeleteBatchAsync(_tenantId, releasedBatch.Id);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Created status");
    }

    #endregion

    #region AddOrdersToBatch Tests

    [Fact]
    public async Task AddOrdersToBatchAsync_WithInvalidBatch_ReturnsFailure()
    {
        var request = new AddOrdersToBatchRequest
        {
            OrderIds = new List<Guid> { _orderId }
        };

        var result = await _batchesService.AddOrdersToBatchAsync(_tenantId, Guid.NewGuid(), request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task AddOrdersToBatchAsync_WithInvalidOrder_ReturnsFailure()
    {
        var request = new AddOrdersToBatchRequest
        {
            OrderIds = new List<Guid> { Guid.NewGuid() }
        };

        var result = await _batchesService.AddOrdersToBatchAsync(_tenantId, _batchId, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Order not found");
    }

    #endregion

    #region RemoveOrderFromBatch Tests

    [Fact]
    public async Task RemoveOrderFromBatchAsync_WithOrderNotInBatch_ReturnsFailure()
    {
        var result = await _batchesService.RemoveOrderFromBatchAsync(_tenantId, _batchId, Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found in batch");
    }

    #endregion

    #region ReleaseBatch Tests

    [Fact]
    public async Task ReleaseBatchAsync_WithEmptyBatch_ReturnsFailure()
    {
        var request = new ReleaseBatchRequest();

        var result = await _batchesService.ReleaseBatchAsync(_tenantId, _batchId, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("empty batch");
    }

    [Fact]
    public async Task ReleaseBatchAsync_WithNonCreatedStatus_ReturnsFailure()
    {
        var releasedBatch = await _context.FulfillmentBatches
            .FirstAsync(b => b.Status == FulfillmentStatus.Released);

        var request = new ReleaseBatchRequest();
        var result = await _batchesService.ReleaseBatchAsync(_tenantId, releasedBatch.Id, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot release");
    }

    #endregion

    #region CancelBatch Tests

    [Fact]
    public async Task CancelBatchAsync_WithValidBatch_CancelsBatch()
    {
        var result = await _batchesService.CancelBatchAsync(_tenantId, _batchId, "Test cancellation");

        result.Success.Should().BeTrue();
        result.Data!.Status.Should().Be(FulfillmentStatus.Cancelled);
        result.Data.Notes.Should().Contain("Cancelled");
    }

    [Fact]
    public async Task CancelBatchAsync_WithCompletedBatch_ReturnsFailure()
    {
        // Create a completed batch
        var completedBatch = new FulfillmentBatch
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            BatchNumber = "FB-COMPLETED",
            WarehouseId = _warehouseId,
            Status = FulfillmentStatus.Complete,
            CreatedAt = DateTime.UtcNow
        };
        _context.FulfillmentBatches.Add(completedBatch);
        await _context.SaveChangesAsync();

        var result = await _batchesService.CancelBatchAsync(_tenantId, completedBatch.Id, "Try cancel");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot cancel");
    }

    #endregion
}
