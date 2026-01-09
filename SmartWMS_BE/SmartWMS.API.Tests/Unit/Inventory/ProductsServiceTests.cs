using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Companies.Models;
using SmartWMS.API.Modules.Inventory.DTOs;
using SmartWMS.API.Modules.Inventory.Models;
using SmartWMS.API.Modules.Inventory.Services;

namespace SmartWMS.API.Tests.Unit.Inventory;

/// <summary>
/// Unit tests for ProductsService
/// </summary>
public class ProductsServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProductsService _productsService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();

    public ProductsServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _productsService = new ProductsService(_context);

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

        // Seed category
        _context.ProductCategories.Add(new ProductCategory
        {
            Id = _categoryId,
            TenantId = _tenantId,
            Code = "CAT-001",
            Name = "Test Category",
            Path = "Test Category",
            Level = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Seed some products
        _context.Products.AddRange(
            new Product
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                Sku = "SKU-001",
                Name = "Product One",
                Barcode = "1111111111111",
                UnitOfMeasure = "EA",
                CategoryId = _categoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                Sku = "SKU-002",
                Name = "Product Two",
                Barcode = "2222222222222",
                UnitOfMeasure = "EA",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                Sku = "SKU-003",
                Name = "Inactive Product",
                UnitOfMeasure = "EA",
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

    #region GetProducts Tests

    [Fact]
    public async Task GetProductsAsync_ReturnsAllProducts()
    {
        // Act
        var result = await _productsService.GetProductsAsync(_tenantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(3);
        result.Data.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetProductsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Act
        var result = await _productsService.GetProductsAsync(_tenantId, page: 1, pageSize: 2);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Page.Should().Be(1);
        result.Data.PageSize.Should().Be(2);
        result.Data.TotalCount.Should().Be(3);
        result.Data.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetProductsAsync_WithSearchFilter_ReturnsMatchingProducts()
    {
        // Act
        var result = await _productsService.GetProductsAsync(_tenantId, search: "Product One");

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().Name.Should().Be("Product One");
    }

    [Fact]
    public async Task GetProductsAsync_WithSkuSearch_ReturnsMatchingProduct()
    {
        // Act
        var result = await _productsService.GetProductsAsync(_tenantId, search: "SKU-001");

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().Sku.Should().Be("SKU-001");
    }

    [Fact]
    public async Task GetProductsAsync_WithActiveFilter_ReturnsOnlyActiveProducts()
    {
        // Act
        var result = await _productsService.GetProductsAsync(_tenantId, isActive: true);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Items.Should().OnlyContain(p => p.IsActive);
    }

    [Fact]
    public async Task GetProductsAsync_WithCategoryFilter_ReturnsProductsInCategory()
    {
        // Act
        var result = await _productsService.GetProductsAsync(_tenantId, categoryId: _categoryId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().CategoryId.Should().Be(_categoryId);
    }

    [Fact]
    public async Task GetProductsAsync_WithDifferentTenant_ReturnsEmpty()
    {
        // Arrange
        var differentTenantId = Guid.NewGuid();

        // Act
        var result = await _productsService.GetProductsAsync(differentTenantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().BeEmpty();
    }

    #endregion

    #region GetProductById Tests

    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var product = await _context.Products.FirstAsync(p => p.Sku == "SKU-001");

        // Act
        var result = await _productsService.GetProductByIdAsync(_tenantId, product.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Sku.Should().Be("SKU-001");
    }

    [Fact]
    public async Task GetProductByIdAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _productsService.GetProductByIdAsync(_tenantId, invalidId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Product not found");
    }

    [Fact]
    public async Task GetProductByIdAsync_WithWrongTenant_ReturnsFailure()
    {
        // Arrange
        var product = await _context.Products.FirstAsync(p => p.Sku == "SKU-001");
        var wrongTenantId = Guid.NewGuid();

        // Act
        var result = await _productsService.GetProductByIdAsync(wrongTenantId, product.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Product not found");
    }

    #endregion

    #region GetProductBySku Tests

    [Fact]
    public async Task GetProductBySkuAsync_WithValidSku_ReturnsProduct()
    {
        // Act
        var result = await _productsService.GetProductBySkuAsync(_tenantId, "SKU-001");

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Sku.Should().Be("SKU-001");
    }

    [Fact]
    public async Task GetProductBySkuAsync_WithInvalidSku_ReturnsFailure()
    {
        // Act
        var result = await _productsService.GetProductBySkuAsync(_tenantId, "NONEXISTENT");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Product not found");
    }

    #endregion

    #region GetProductByBarcode Tests

    [Fact]
    public async Task GetProductByBarcodeAsync_WithValidBarcode_ReturnsProduct()
    {
        // Act
        var result = await _productsService.GetProductByBarcodeAsync(_tenantId, "1111111111111");

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Barcode.Should().Be("1111111111111");
    }

    [Fact]
    public async Task GetProductByBarcodeAsync_WithInvalidBarcode_ReturnsFailure()
    {
        // Act
        var result = await _productsService.GetProductByBarcodeAsync(_tenantId, "9999999999999");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Product not found");
    }

    #endregion

    #region CreateProduct Tests

    [Fact]
    public async Task CreateProductAsync_WithValidData_CreatesProduct()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "NEW-SKU",
            Name = "New Product",
            Description = "New product description",
            UnitOfMeasure = "EA",
            IsActive = true
        };

        // Act
        var result = await _productsService.CreateProductAsync(_tenantId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Sku.Should().Be("NEW-SKU");
        result.Data.Name.Should().Be("New Product");

        // Verify in database
        var dbProduct = await _context.Products.FirstOrDefaultAsync(p => p.Sku == "NEW-SKU");
        dbProduct.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProductAsync_WithDuplicateSku_ReturnsFailure()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "SKU-001", // Already exists
            Name = "Duplicate SKU Product",
            UnitOfMeasure = "EA",
            IsActive = true
        };

        // Act
        var result = await _productsService.CreateProductAsync(_tenantId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateProductAsync_WithDuplicateBarcode_ReturnsFailure()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "UNIQUE-SKU",
            Name = "Duplicate Barcode Product",
            Barcode = "1111111111111", // Already exists
            UnitOfMeasure = "EA",
            IsActive = true
        };

        // Act
        var result = await _productsService.CreateProductAsync(_tenantId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("barcode");
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateProductAsync_WithInvalidCategory_ReturnsFailure()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "CAT-SKU",
            Name = "Invalid Category Product",
            UnitOfMeasure = "EA",
            IsActive = true,
            CategoryId = Guid.NewGuid() // Non-existent category
        };

        // Act
        var result = await _productsService.CreateProductAsync(_tenantId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Category not found");
    }

    [Fact]
    public async Task CreateProductAsync_WithValidCategory_SetsCategory()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "CAT-SKU-VALID",
            Name = "Categorized Product",
            UnitOfMeasure = "EA",
            IsActive = true,
            CategoryId = _categoryId
        };

        // Act
        var result = await _productsService.CreateProductAsync(_tenantId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.CategoryId.Should().Be(_categoryId);
        result.Data.CategoryName.Should().Be("Test Category");
    }

    #endregion

    #region UpdateProduct Tests

    [Fact]
    public async Task UpdateProductAsync_WithValidData_UpdatesProduct()
    {
        // Arrange
        var product = await _context.Products.FirstAsync(p => p.Sku == "SKU-001");
        var request = new UpdateProductRequest
        {
            Name = "Updated Product Name",
            Description = "Updated description"
        };

        // Act
        var result = await _productsService.UpdateProductAsync(_tenantId, product.Id, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Updated Product Name");
        result.Data.Description.Should().Be("Updated description");
        result.Data.Sku.Should().Be("SKU-001"); // SKU unchanged
    }

    [Fact]
    public async Task UpdateProductAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var request = new UpdateProductRequest { Name = "Updated Name" };

        // Act
        var result = await _productsService.UpdateProductAsync(_tenantId, invalidId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Product not found");
    }

    [Fact]
    public async Task UpdateProductAsync_WithDuplicateBarcode_ReturnsFailure()
    {
        // Arrange
        var product = await _context.Products.FirstAsync(p => p.Sku == "SKU-002");
        var request = new UpdateProductRequest
        {
            Barcode = "1111111111111" // Belongs to SKU-001
        };

        // Act
        var result = await _productsService.UpdateProductAsync(_tenantId, product.Id, request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("barcode");
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task UpdateProductAsync_CanDeactivateProduct()
    {
        // Arrange
        var product = await _context.Products.FirstAsync(p => p.Sku == "SKU-001");
        var request = new UpdateProductRequest { IsActive = false };

        // Act
        var result = await _productsService.UpdateProductAsync(_tenantId, product.Id, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.IsActive.Should().BeFalse();
    }

    #endregion

    #region DeleteProduct Tests

    [Fact]
    public async Task DeleteProductAsync_WithValidId_DeletesProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            Sku = "DELETE-ME",
            Name = "Product to Delete",
            UnitOfMeasure = "EA",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _productsService.DeleteProductAsync(_tenantId, product.Id);

        // Assert
        result.Success.Should().BeTrue();

        // Verify deleted from database
        var dbProduct = await _context.Products.FindAsync(product.Id);
        dbProduct.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProductAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _productsService.DeleteProductAsync(_tenantId, invalidId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Product not found");
    }

    [Fact]
    public async Task DeleteProductAsync_WithStockLevels_ReturnsFailure()
    {
        // Arrange
        var product = await _context.Products.FirstAsync(p => p.Sku == "SKU-001");

        // Add stock level to product
        _context.StockLevels.Add(new StockLevel
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            ProductId = product.Id,
            Sku = product.Sku,
            LocationId = Guid.NewGuid(),
            QuantityOnHand = 10,
            QuantityReserved = 0,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _productsService.DeleteProductAsync(_tenantId, product.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot delete product with");
        result.Message.Should().Contain("stock level");
    }

    #endregion
}
