using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Companies.Models;
using SmartWMS.API.Modules.Orders.DTOs;
using SmartWMS.API.Modules.Orders.Models;
using SmartWMS.API.Modules.Orders.Services;

namespace SmartWMS.API.Tests.Unit.Orders;

public class CustomersServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CustomersService _customersService;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CustomersServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _customersService = new CustomersService(_context);

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

        _context.Customers.AddRange(
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                Code = "CUST-001",
                Name = "Acme Corporation",
                ContactName = "John Smith",
                Email = "john@acme.com",
                Phone = "+1234567890",
                City = "New York",
                CountryCode = "US",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                Code = "CUST-002",
                Name = "Beta Industries",
                ContactName = "Jane Doe",
                Email = "jane@beta.com",
                City = "London",
                CountryCode = "GB",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                Code = "CUST-003",
                Name = "Inactive Customer",
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        );

        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetCustomers Tests

    [Fact]
    public async Task GetCustomersAsync_ReturnsAllCustomers()
    {
        var result = await _customersService.GetCustomersAsync(_tenantId);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetCustomersAsync_WithSearchFilter_ReturnsMatchingResults()
    {
        var filters = new CustomerFilters { Search = "acme" };
        var result = await _customersService.GetCustomersAsync(_tenantId, filters);

        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().Name.Should().Be("Acme Corporation");
    }

    [Fact]
    public async Task GetCustomersAsync_WithIsActiveFilter_ReturnsActiveCustomers()
    {
        var filters = new CustomerFilters { IsActive = true };
        var result = await _customersService.GetCustomersAsync(_tenantId, filters);

        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Items.Should().OnlyContain(c => c.IsActive);
    }

    [Fact]
    public async Task GetCustomersAsync_WithCountryFilter_ReturnsFilteredResults()
    {
        var filters = new CustomerFilters { CountryCode = "US" };
        var result = await _customersService.GetCustomersAsync(_tenantId, filters);

        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().Code.Should().Be("CUST-001");
    }

    #endregion

    #region GetCustomerById Tests

    [Fact]
    public async Task GetCustomerByIdAsync_WithValidId_ReturnsCustomer()
    {
        var customer = await _context.Customers.FirstAsync(c => c.Code == "CUST-001");
        var result = await _customersService.GetCustomerByIdAsync(_tenantId, customer.Id);

        result.Success.Should().BeTrue();
        result.Data!.Code.Should().Be("CUST-001");
        result.Data.Name.Should().Be("Acme Corporation");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithInvalidId_ReturnsFailure()
    {
        var result = await _customersService.GetCustomerByIdAsync(_tenantId, Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    #endregion

    #region GetCustomerByCode Tests

    [Fact]
    public async Task GetCustomerByCodeAsync_WithValidCode_ReturnsCustomer()
    {
        var result = await _customersService.GetCustomerByCodeAsync(_tenantId, "CUST-002");

        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Beta Industries");
    }

    [Fact]
    public async Task GetCustomerByCodeAsync_WithInvalidCode_ReturnsFailure()
    {
        var result = await _customersService.GetCustomerByCodeAsync(_tenantId, "NONEXISTENT");

        result.Success.Should().BeFalse();
    }

    #endregion

    #region CreateCustomer Tests

    [Fact]
    public async Task CreateCustomerAsync_WithValidData_CreatesCustomer()
    {
        var request = new CreateCustomerRequest
        {
            Code = "CUST-NEW",
            Name = "New Customer",
            Email = "new@customer.com",
            City = "Berlin",
            CountryCode = "DE",
            IsActive = true
        };

        var result = await _customersService.CreateCustomerAsync(_tenantId, request);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be("CUST-NEW");
        result.Data.Name.Should().Be("New Customer");

        var dbCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Code == "CUST-NEW");
        dbCustomer.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCustomerAsync_WithDuplicateCode_ReturnsFailure()
    {
        var request = new CreateCustomerRequest
        {
            Code = "CUST-001",
            Name = "Duplicate Customer"
        };

        var result = await _customersService.CreateCustomerAsync(_tenantId, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    #endregion

    #region UpdateCustomer Tests

    [Fact]
    public async Task UpdateCustomerAsync_WithValidData_UpdatesCustomer()
    {
        var customer = await _context.Customers.FirstAsync(c => c.Code == "CUST-001");
        var request = new UpdateCustomerRequest
        {
            Name = "Acme Corp Updated",
            Email = "updated@acme.com"
        };

        var result = await _customersService.UpdateCustomerAsync(_tenantId, customer.Id, request);

        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Acme Corp Updated");
        result.Data.Email.Should().Be("updated@acme.com");
    }

    [Fact]
    public async Task UpdateCustomerAsync_WithInvalidId_ReturnsFailure()
    {
        var request = new UpdateCustomerRequest { Name = "Updated" };
        var result = await _customersService.UpdateCustomerAsync(_tenantId, Guid.NewGuid(), request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    #endregion

    #region DeleteCustomer Tests

    [Fact]
    public async Task DeleteCustomerAsync_WithValidId_DeletesCustomer()
    {
        var customer = await _context.Customers.FirstAsync(c => c.Code == "CUST-003");
        var result = await _customersService.DeleteCustomerAsync(_tenantId, customer.Id);

        result.Success.Should().BeTrue();

        var dbCustomer = await _context.Customers.FindAsync(customer.Id);
        dbCustomer.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCustomerAsync_WithInvalidId_ReturnsFailure()
    {
        var result = await _customersService.DeleteCustomerAsync(_tenantId, Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    #endregion
}
