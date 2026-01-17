using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Automation.Services;
using SmartWMS.API.Modules.Companies.Models;
using SmartWMS.API.Modules.Inventory.Models;
using SmartWMS.API.Modules.Orders.DTOs;
using SmartWMS.API.Modules.Orders.Models;
using SmartWMS.API.Modules.Orders.Services;

namespace SmartWMS.API.Tests.Unit.Orders;

public class SalesOrdersServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly SalesOrdersService _salesOrdersService;
    private readonly Mock<IAutomationEventPublisher> _automationEventsMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _warehouseId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();

    public SalesOrdersServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _salesOrdersService = new SalesOrdersService(_context, _automationEventsMock.Object);

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

        _context.Warehouses.Add(new Modules.Warehouse.Models.Warehouse
        {
            Id = _warehouseId,
            TenantId = _tenantId,
            Code = "WH-001",
            Name = "Test Warehouse",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        _context.Products.Add(new Product
        {
            Id = _productId,
            TenantId = _tenantId,
            Sku = "SKU-001",
            Name = "Test Product",
            UnitOfMeasure = "EA",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Add test order
        var order = new SalesOrder
        {
            Id = _orderId,
            TenantId = _tenantId,
            OrderNumber = "SO-TEST-0001",
            CustomerId = _customerId,
            WarehouseId = _warehouseId,
            Status = SalesOrderStatus.Draft,
            Priority = OrderPriority.Normal,
            OrderDate = DateTime.UtcNow,
            TotalLines = 1,
            TotalQuantity = 10,
            CreatedAt = DateTime.UtcNow
        };

        order.Lines.Add(new SalesOrderLine
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            OrderId = _orderId,
            LineNumber = 1,
            ProductId = _productId,
            Sku = "SKU-001",
            QuantityOrdered = 10,
            CreatedAt = DateTime.UtcNow
        });

        _context.SalesOrders.Add(order);

        // Add confirmed order for status tests
        _context.SalesOrders.Add(new SalesOrder
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            OrderNumber = "SO-TEST-0002",
            CustomerId = _customerId,
            WarehouseId = _warehouseId,
            Status = SalesOrderStatus.Confirmed,
            Priority = OrderPriority.High,
            OrderDate = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetOrders Tests

    [Fact]
    public async Task GetOrdersAsync_ReturnsAllOrders()
    {
        var result = await _salesOrdersService.GetOrdersAsync(_tenantId);

        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOrdersAsync_WithStatusFilter_ReturnsFilteredResults()
    {
        var filters = new SalesOrderFilters { Status = SalesOrderStatus.Draft };
        var result = await _salesOrdersService.GetOrdersAsync(_tenantId, filters);

        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().Status.Should().Be(SalesOrderStatus.Draft);
    }

    [Fact]
    public async Task GetOrdersAsync_WithSearchFilter_ReturnsMatchingResults()
    {
        var filters = new SalesOrderFilters { Search = "0001" };
        var result = await _salesOrdersService.GetOrdersAsync(_tenantId, filters);

        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().OrderNumber.Should().Be("SO-TEST-0001");
    }

    [Fact]
    public async Task GetOrdersAsync_WithCustomerFilter_ReturnsFilteredResults()
    {
        var filters = new SalesOrderFilters { CustomerId = _customerId };
        var result = await _salesOrdersService.GetOrdersAsync(_tenantId, filters);

        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
    }

    #endregion

    #region GetOrderById Tests

    [Fact]
    public async Task GetOrderByIdAsync_WithValidId_ReturnsOrder()
    {
        var result = await _salesOrdersService.GetOrderByIdAsync(_tenantId, _orderId);

        result.Success.Should().BeTrue();
        result.Data!.OrderNumber.Should().Be("SO-TEST-0001");
        result.Data.Lines.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithInvalidId_ReturnsFailure()
    {
        var result = await _salesOrdersService.GetOrderByIdAsync(_tenantId, Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    #endregion

    #region CreateOrder Tests

    [Fact]
    public async Task CreateOrderAsync_WithValidData_CreatesOrder()
    {
        var request = new CreateSalesOrderRequest
        {
            CustomerId = _customerId,
            WarehouseId = _warehouseId,
            Priority = OrderPriority.High,
            Lines = new List<CreateSalesOrderLineRequest>
            {
                new() { ProductId = _productId, QuantityOrdered = 5 }
            }
        };

        var result = await _salesOrdersService.CreateOrderAsync(_tenantId, request);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.OrderNumber.Should().StartWith("SO-");
        result.Data.Priority.Should().Be(OrderPriority.High);
        result.Data.TotalLines.Should().Be(1);
        result.Data.TotalQuantity.Should().Be(5);
    }

    [Fact]
    public async Task CreateOrderAsync_WithInvalidCustomer_ReturnsFailure()
    {
        var request = new CreateSalesOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            WarehouseId = _warehouseId
        };

        var result = await _salesOrdersService.CreateOrderAsync(_tenantId, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Customer not found");
    }

    [Fact]
    public async Task CreateOrderAsync_WithInvalidWarehouse_ReturnsFailure()
    {
        var request = new CreateSalesOrderRequest
        {
            CustomerId = _customerId,
            WarehouseId = Guid.NewGuid()
        };

        var result = await _salesOrdersService.CreateOrderAsync(_tenantId, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Warehouse not found");
    }

    [Fact]
    public async Task CreateOrderAsync_WithDuplicateOrderNumber_ReturnsFailure()
    {
        var request = new CreateSalesOrderRequest
        {
            OrderNumber = "SO-TEST-0001",
            CustomerId = _customerId,
            WarehouseId = _warehouseId
        };

        var result = await _salesOrdersService.CreateOrderAsync(_tenantId, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    #endregion

    #region UpdateOrderStatus Tests

    [Fact]
    public async Task UpdateOrderStatusAsync_WithValidTransition_UpdatesStatus()
    {
        var request = new UpdateSalesOrderStatusRequest
        {
            Status = SalesOrderStatus.Pending,
            Notes = "Submitted for processing"
        };

        var result = await _salesOrdersService.UpdateOrderStatusAsync(_tenantId, _orderId, request);

        result.Success.Should().BeTrue();
        result.Data!.Status.Should().Be(SalesOrderStatus.Pending);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithInvalidTransition_ReturnsFailure()
    {
        var request = new UpdateSalesOrderStatusRequest
        {
            Status = SalesOrderStatus.Shipped
        };

        var result = await _salesOrdersService.UpdateOrderStatusAsync(_tenantId, _orderId, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot transition");
    }

    #endregion

    #region DeleteOrder Tests

    [Fact]
    public async Task DeleteOrderAsync_WithDraftOrder_DeletesOrder()
    {
        var result = await _salesOrdersService.DeleteOrderAsync(_tenantId, _orderId);

        result.Success.Should().BeTrue();

        var dbOrder = await _context.SalesOrders.FindAsync(_orderId);
        dbOrder.Should().BeNull();
    }

    [Fact]
    public async Task DeleteOrderAsync_WithConfirmedOrder_ReturnsFailure()
    {
        var confirmedOrder = await _context.SalesOrders.FirstAsync(o => o.Status == SalesOrderStatus.Confirmed);
        var result = await _salesOrdersService.DeleteOrderAsync(_tenantId, confirmedOrder.Id);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Draft");
    }

    #endregion

    #region AddLine Tests

    [Fact]
    public async Task AddLineAsync_WithInvalidOrder_ReturnsFailure()
    {
        var request = new AddSalesOrderLineRequest
        {
            ProductId = _productId,
            QuantityOrdered = 5
        };

        var result = await _salesOrdersService.AddLineAsync(_tenantId, Guid.NewGuid(), request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task AddLineAsync_WithInvalidProduct_ReturnsFailure()
    {
        var request = new AddSalesOrderLineRequest
        {
            ProductId = Guid.NewGuid(),
            QuantityOrdered = 5
        };

        var result = await _salesOrdersService.AddLineAsync(_tenantId, _orderId, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Product not found");
    }

    #endregion

    #region RemoveLine Tests

    [Fact]
    public async Task RemoveLineAsync_WithValidLine_RemovesLine()
    {
        var order = await _context.SalesOrders.Include(o => o.Lines).FirstAsync(o => o.Id == _orderId);
        var lineId = order.Lines.First().Id;

        var result = await _salesOrdersService.RemoveLineAsync(_tenantId, _orderId, lineId);

        result.Success.Should().BeTrue();

        await _context.Entry(order).ReloadAsync();
        order.TotalLines.Should().Be(0);
    }

    #endregion
}
