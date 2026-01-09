using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Infrastructure.Identity;
using SmartWMS.API.Modules.Companies.Models;
using SmartWMS.API.Modules.Equipment.DTOs;
using SmartWMS.API.Modules.Equipment.Models;
using SmartWMS.API.Modules.Equipment.Services;

namespace SmartWMS.API.Tests.Unit.Equipment;

public class EquipmentServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly EquipmentService _equipmentService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _warehouseId = Guid.NewGuid();
    private readonly Guid _zoneId = Guid.NewGuid();
    private readonly Guid _testUserId = Guid.NewGuid();

    public EquipmentServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _equipmentService = new EquipmentService(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Seed tenant
        _context.Companies.Add(new Company
        {
            Id = _tenantId,
            Code = "TEST",
            Name = "Test Company",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Seed warehouse
        _context.Warehouses.Add(new Modules.Warehouse.Models.Warehouse
        {
            Id = _warehouseId,
            TenantId = _tenantId,
            Code = "WH-001",
            Name = "Test Warehouse",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Seed zone
        _context.Zones.Add(new Modules.Warehouse.Models.Zone
        {
            Id = _zoneId,
            TenantId = _tenantId,
            WarehouseId = _warehouseId,
            Code = "ZONE-001",
            Name = "Test Zone",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Seed test user
        _context.Users.Add(new ApplicationUser
        {
            Id = _testUserId,
            UserName = "testuser@test.com",
            Email = "testuser@test.com",
            NormalizedUserName = "TESTUSER@TEST.COM",
            NormalizedEmail = "TESTUSER@TEST.COM",
            FirstName = "Test",
            LastName = "User",
            TenantId = _tenantId,
            IsActive = true
        });

        // Seed equipment
        _context.Equipment.AddRange(
            new Modules.Equipment.Models.Equipment
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                Code = "FL-001",
                Name = "Forklift 1",
                Type = EquipmentType.Forklift,
                Status = EquipmentStatus.Available,
                WarehouseId = _warehouseId,
                Specifications = """{"loadCapacityKg": 2000, "liftHeightMm": 5000, "fuelType": "Electric"}""",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Modules.Equipment.Models.Equipment
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                Code = "SC-001",
                Name = "Scanner 1",
                Type = EquipmentType.Scanner,
                Status = EquipmentStatus.InUse,
                WarehouseId = _warehouseId,
                AssignedToUserId = Guid.NewGuid(),
                AssignedToUserName = "John Doe",
                Specifications = """{"model": "Zebra MC9300", "connectionType": "WiFi"}""",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Modules.Equipment.Models.Equipment
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                Code = "PR-001",
                Name = "Printer 1",
                Type = EquipmentType.Printer,
                Status = EquipmentStatus.Maintenance,
                Specifications = """{"model": "Zebra ZT411", "printType": "Thermal", "dpi": 300}""",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Modules.Equipment.Models.Equipment
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                Code = "FL-002",
                Name = "Forklift 2 (Inactive)",
                Type = EquipmentType.Forklift,
                Status = EquipmentStatus.OutOfService,
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

    #region GetEquipment Tests

    [Fact]
    public async Task GetEquipmentAsync_ReturnsAllEquipment()
    {
        // Act
        var result = await _equipmentService.GetEquipmentAsync(_tenantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(4);
        result.Data.TotalCount.Should().Be(4);
    }

    [Fact]
    public async Task GetEquipmentAsync_WithPagination_ReturnsCorrectPage()
    {
        // Act
        var result = await _equipmentService.GetEquipmentAsync(_tenantId, page: 1, pageSize: 2);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Page.Should().Be(1);
        result.Data.PageSize.Should().Be(2);
        result.Data.TotalCount.Should().Be(4);
        result.Data.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetEquipmentAsync_WithTypeFilter_ReturnsFilteredResults()
    {
        // Act
        var result = await _equipmentService.GetEquipmentAsync(_tenantId,
            new EquipmentFilters { Type = EquipmentType.Forklift });

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Items.Should().OnlyContain(e => e.Type == EquipmentType.Forklift);
    }

    [Fact]
    public async Task GetEquipmentAsync_WithStatusFilter_ReturnsFilteredResults()
    {
        // Act
        var result = await _equipmentService.GetEquipmentAsync(_tenantId,
            new EquipmentFilters { Status = EquipmentStatus.Available });

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().Status.Should().Be(EquipmentStatus.Available);
    }

    [Fact]
    public async Task GetEquipmentAsync_WithSearchFilter_ReturnsMatchingResults()
    {
        // Act
        var result = await _equipmentService.GetEquipmentAsync(_tenantId,
            new EquipmentFilters { Search = "Scanner" });

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().Code.Should().Be("SC-001");
    }

    [Fact]
    public async Task GetEquipmentAsync_WithIsAssignedFilter_ReturnsAssignedEquipment()
    {
        // Act
        var result = await _equipmentService.GetEquipmentAsync(_tenantId,
            new EquipmentFilters { IsAssigned = true });

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().AssignedToUserName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetEquipmentAsync_WithIsActiveFilter_ReturnsActiveEquipment()
    {
        // Act
        var result = await _equipmentService.GetEquipmentAsync(_tenantId,
            new EquipmentFilters { IsActive = true });

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(3);
        result.Data.Items.Should().OnlyContain(e => e.IsActive);
    }

    [Fact]
    public async Task GetEquipmentAsync_WithWarehouseFilter_ReturnsFilteredResults()
    {
        // Act
        var result = await _equipmentService.GetEquipmentAsync(_tenantId,
            new EquipmentFilters { WarehouseId = _warehouseId });

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
    }

    #endregion

    #region GetEquipmentById Tests

    [Fact]
    public async Task GetEquipmentByIdAsync_WithValidId_ReturnsEquipment()
    {
        // Arrange
        var equipment = await _context.Equipment.FirstAsync(e => e.Code == "FL-001");

        // Act
        var result = await _equipmentService.GetEquipmentByIdAsync(_tenantId, equipment.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be("FL-001");
        result.Data.Specifications.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEquipmentByIdAsync_WithInvalidId_ReturnsFailure()
    {
        // Act
        var result = await _equipmentService.GetEquipmentByIdAsync(_tenantId, Guid.NewGuid());

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    #endregion

    #region GetEquipmentByCode Tests

    [Fact]
    public async Task GetEquipmentByCodeAsync_WithValidCode_ReturnsEquipment()
    {
        // Act
        var result = await _equipmentService.GetEquipmentByCodeAsync(_tenantId, "SC-001");

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Code.Should().Be("SC-001");
        result.Data.Type.Should().Be(EquipmentType.Scanner);
    }

    [Fact]
    public async Task GetEquipmentByCodeAsync_WithInvalidCode_ReturnsFailure()
    {
        // Act
        var result = await _equipmentService.GetEquipmentByCodeAsync(_tenantId, "NONEXISTENT");

        // Assert
        result.Success.Should().BeFalse();
    }

    #endregion

    #region CreateEquipment Tests

    [Fact]
    public async Task CreateEquipmentAsync_WithValidData_CreatesEquipment()
    {
        // Arrange
        var request = new CreateEquipmentRequest
        {
            Code = "PJ-001",
            Name = "Pallet Jack 1",
            Type = EquipmentType.PalletJack,
            Status = EquipmentStatus.Available,
            WarehouseId = _warehouseId,
            Specifications = """{"loadCapacityKg": 1500, "type": "Electric"}""",
            IsActive = true
        };

        // Act
        var result = await _equipmentService.CreateEquipmentAsync(_tenantId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be("PJ-001");
        result.Data.Type.Should().Be(EquipmentType.PalletJack);

        // Verify in database
        var dbEquipment = await _context.Equipment.FirstOrDefaultAsync(e => e.Code == "PJ-001");
        dbEquipment.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateEquipmentAsync_WithDuplicateCode_ReturnsFailure()
    {
        // Arrange
        var request = new CreateEquipmentRequest
        {
            Code = "FL-001", // Already exists
            Name = "Duplicate Forklift",
            Type = EquipmentType.Forklift,
            IsActive = true
        };

        // Act
        var result = await _equipmentService.CreateEquipmentAsync(_tenantId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateEquipmentAsync_WithInvalidWarehouse_ReturnsFailure()
    {
        // Arrange
        var request = new CreateEquipmentRequest
        {
            Code = "NEW-001",
            Name = "New Equipment",
            Type = EquipmentType.Scanner,
            WarehouseId = Guid.NewGuid(), // Non-existent warehouse
            IsActive = true
        };

        // Act
        var result = await _equipmentService.CreateEquipmentAsync(_tenantId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Warehouse not found");
    }

    [Fact]
    public async Task CreateEquipmentAsync_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        var request = new CreateEquipmentRequest
        {
            Code = "NEW-002",
            Name = "New Equipment",
            Type = EquipmentType.Scanner,
            Specifications = "not valid json",
            IsActive = true
        };

        // Act
        var result = await _equipmentService.CreateEquipmentAsync(_tenantId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid JSON");
    }

    #endregion

    #region UpdateEquipment Tests

    [Fact]
    public async Task UpdateEquipmentAsync_WithValidData_UpdatesEquipment()
    {
        // Arrange
        var equipment = await _context.Equipment.FirstAsync(e => e.Code == "FL-001");
        var request = new UpdateEquipmentRequest
        {
            Name = "Updated Forklift Name",
            Status = EquipmentStatus.Maintenance
        };

        // Act
        var result = await _equipmentService.UpdateEquipmentAsync(_tenantId, equipment.Id, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Updated Forklift Name");
        result.Data.Status.Should().Be(EquipmentStatus.Maintenance);
    }

    [Fact]
    public async Task UpdateEquipmentAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var request = new UpdateEquipmentRequest { Name = "Updated Name" };

        // Act
        var result = await _equipmentService.UpdateEquipmentAsync(_tenantId, Guid.NewGuid(), request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    #endregion

    #region DeleteEquipment Tests

    [Fact]
    public async Task DeleteEquipmentAsync_WithValidId_DeletesEquipment()
    {
        // Arrange
        var equipment = new Modules.Equipment.Models.Equipment
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            Code = "DELETE-ME",
            Name = "Equipment to Delete",
            Type = EquipmentType.Trolley,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Equipment.Add(equipment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _equipmentService.DeleteEquipmentAsync(_tenantId, equipment.Id);

        // Assert
        result.Success.Should().BeTrue();

        var dbEquipment = await _context.Equipment.FindAsync(equipment.Id);
        dbEquipment.Should().BeNull();
    }

    [Fact]
    public async Task DeleteEquipmentAsync_WithInvalidId_ReturnsFailure()
    {
        // Act
        var result = await _equipmentService.DeleteEquipmentAsync(_tenantId, Guid.NewGuid());

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    #endregion

    #region AssignEquipment Tests

    [Fact]
    public async Task AssignEquipmentAsync_WithValidUser_AssignsEquipment()
    {
        // Arrange
        var equipment = await _context.Equipment.FirstAsync(e => e.Code == "FL-001");
        var request = new AssignEquipmentRequest { UserId = _testUserId };

        // Act
        var result = await _equipmentService.AssignEquipmentAsync(_tenantId, equipment.Id, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.AssignedToUserId.Should().Be(_testUserId);
        result.Data.AssignedToUserName.Should().Be("Test User");
        result.Data.Status.Should().Be(EquipmentStatus.InUse);
    }

    [Fact]
    public async Task AssignEquipmentAsync_WithNullUser_UnassignsEquipment()
    {
        // Arrange
        var equipment = await _context.Equipment.FirstAsync(e => e.Code == "SC-001");
        var request = new AssignEquipmentRequest { UserId = null };

        // Act
        var result = await _equipmentService.AssignEquipmentAsync(_tenantId, equipment.Id, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.AssignedToUserId.Should().BeNull();
        result.Data.AssignedToUserName.Should().BeNull();
        result.Data.Status.Should().Be(EquipmentStatus.Available);
    }

    #endregion

    #region UpdateEquipmentStatus Tests

    [Fact]
    public async Task UpdateEquipmentStatusAsync_ToMaintenance_UpdatesStatusAndDate()
    {
        // Arrange
        var equipment = await _context.Equipment.FirstAsync(e => e.Code == "FL-001");
        var request = new UpdateEquipmentStatusRequest
        {
            Status = EquipmentStatus.Maintenance,
            Notes = "Scheduled maintenance"
        };

        // Act
        var result = await _equipmentService.UpdateEquipmentStatusAsync(_tenantId, equipment.Id, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Status.Should().Be(EquipmentStatus.Maintenance);
        result.Data.LastMaintenanceDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Data.MaintenanceNotes.Should().Be("Scheduled maintenance");
    }

    #endregion

    #region GetAvailableEquipment Tests

    [Fact]
    public async Task GetAvailableEquipmentAsync_ReturnsOnlyAvailable()
    {
        // Act
        var result = await _equipmentService.GetAvailableEquipmentAsync(_tenantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        var first = result.Data!.First();
        first.Status.Should().Be(EquipmentStatus.Available);
        first.AssignedToUserId.Should().BeNull();
    }

    [Fact]
    public async Task GetAvailableEquipmentAsync_WithTypeFilter_ReturnsFilteredResults()
    {
        // Arrange - Add another available forklift
        _context.Equipment.Add(new Modules.Equipment.Models.Equipment
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            Code = "FL-003",
            Name = "Forklift 3",
            Type = EquipmentType.Forklift,
            Status = EquipmentStatus.Available,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _equipmentService.GetAvailableEquipmentAsync(_tenantId, EquipmentType.Forklift);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data!.Should().OnlyContain(e => e.Type == EquipmentType.Forklift);
    }

    #endregion
}
