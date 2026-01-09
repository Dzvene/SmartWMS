using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Inventory.DTOs;
using SmartWMS.API.Tests.Infrastructure;

namespace SmartWMS.API.Tests.Integration.Inventory;

/// <summary>
/// Integration tests for ProductsController
/// </summary>
public class ProductsControllerTests : IntegrationTestBase
{
    public ProductsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region GetProducts Tests

    [Fact]
    public async Task GetProducts_WithValidToken_ReturnsProducts()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync(TenantUrl("/products"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResult<ProductDto>>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().NotBeEmpty();
        result.Data.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetProducts_WithoutToken_ReturnsUnauthorized()
    {
        // Act (no authentication)
        var response = await Client.GetAsync(TenantUrl("/products"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProducts_WithWrongTenantId_ReturnsForbidden()
    {
        // Arrange
        await AuthenticateAsync();
        var wrongTenantId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/tenant/{wrongTenantId}/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetProducts_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync(TenantUrl("/products?page=1&pageSize=10"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResult<ProductDto>>>(JsonOptions);
        result!.Data!.Page.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetProducts_WithSearchFilter_ReturnsFilteredResults()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync(TenantUrl($"/products?search={TestDataSeeder.TestProductSku}"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResult<ProductDto>>>(JsonOptions);
        result!.Data!.Items.Should().Contain(p => p.Sku == TestDataSeeder.TestProductSku);
    }

    #endregion

    #region GetProduct Tests

    [Fact]
    public async Task GetProduct_WithValidId_ReturnsProduct()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync(TenantUrl($"/products/{TestDataSeeder.TestProductId}"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(TestDataSeeder.TestProductId);
        result.Data.Sku.Should().Be(TestDataSeeder.TestProductSku);
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync(TenantUrl($"/products/{invalidId}"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GetProductBySku Tests

    [Fact]
    public async Task GetProductBySku_WithValidSku_ReturnsProduct()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync(TenantUrl($"/products/sku/{TestDataSeeder.TestProductSku}"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>(JsonOptions);
        result!.Data!.Sku.Should().Be(TestDataSeeder.TestProductSku);
    }

    [Fact]
    public async Task GetProductBySku_WithInvalidSku_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync(TenantUrl("/products/sku/NONEXISTENT-SKU"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GetProductByBarcode Tests

    [Fact]
    public async Task GetProductByBarcode_WithValidBarcode_ReturnsProduct()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync(TenantUrl("/products/barcode/1234567890123"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>(JsonOptions);
        result!.Data!.Barcode.Should().Be("1234567890123");
    }

    #endregion

    #region CreateProduct Tests

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreatedProduct()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateProductRequest
        {
            Sku = $"NEW-SKU-{Guid.NewGuid():N}".Substring(0, 20),
            Name = "New Test Product",
            Description = "A new product for testing",
            UnitOfMeasure = "EA",
            IsActive = true,
            CategoryId = TestDataSeeder.TestCategoryId
        };

        // Act
        var response = await Client.PostAsJsonAsync(TenantUrl("/products"), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Sku.Should().Be(request.Sku);
        result.Data.Name.Should().Be(request.Name);
        result.Data.CategoryId.Should().Be(TestDataSeeder.TestCategoryId);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSku_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateProductRequest
        {
            Sku = TestDataSeeder.TestProductSku, // Existing SKU
            Name = "Duplicate SKU Product",
            UnitOfMeasure = "EA",
            IsActive = true
        };

        // Act
        var response = await Client.PostAsJsonAsync(TenantUrl("/products"), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>(JsonOptions);
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateProduct_WithInvalidCategory_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateProductRequest
        {
            Sku = $"NEW-SKU-{Guid.NewGuid():N}".Substring(0, 20),
            Name = "Product with Invalid Category",
            UnitOfMeasure = "EA",
            IsActive = true,
            CategoryId = Guid.NewGuid() // Non-existent category
        };

        // Act
        var response = await Client.PostAsJsonAsync(TenantUrl("/products"), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>(JsonOptions);
        result!.Message.Should().Contain("Category not found");
    }

    #endregion

    #region UpdateProduct Tests

    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsUpdatedProduct()
    {
        // Arrange
        await AuthenticateAsync();

        // First create a product to update
        var createRequest = new CreateProductRequest
        {
            Sku = $"UPD-SKU-{Guid.NewGuid():N}".Substring(0, 20),
            Name = "Product to Update",
            UnitOfMeasure = "EA",
            IsActive = true
        };
        var createResponse = await Client.PostAsJsonAsync(TenantUrl("/products"), createRequest);
        var createdProduct = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>(JsonOptions))!.Data!;

        var updateRequest = new UpdateProductRequest
        {
            Name = "Updated Product Name",
            Description = "Updated description"
        };

        // Act
        var response = await Client.PutAsJsonAsync(TenantUrl($"/products/{createdProduct.Id}"), updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>(JsonOptions);
        result!.Data!.Name.Should().Be("Updated Product Name");
        result.Data.Description.Should().Be("Updated description");
        result.Data.Sku.Should().Be(createRequest.Sku); // SKU should not change
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();
        var invalidId = Guid.NewGuid();
        var updateRequest = new UpdateProductRequest
        {
            Name = "Updated Name"
        };

        // Act
        var response = await Client.PutAsJsonAsync(TenantUrl($"/products/{invalidId}"), updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DeleteProduct Tests

    [Fact]
    public async Task DeleteProduct_WithValidId_ReturnsSuccess()
    {
        // Arrange
        await AuthenticateAsync();

        // First create a product to delete
        var createRequest = new CreateProductRequest
        {
            Sku = $"DEL-SKU-{Guid.NewGuid():N}".Substring(0, 20),
            Name = "Product to Delete",
            UnitOfMeasure = "EA",
            IsActive = true
        };
        var createResponse = await Client.PostAsJsonAsync(TenantUrl("/products"), createRequest);
        var createdProduct = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>(JsonOptions))!.Data!;

        // Act
        var response = await Client.DeleteAsync(TenantUrl($"/products/{createdProduct.Id}"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify product is deleted
        var getResponse = await Client.GetAsync(TenantUrl($"/products/{createdProduct.Id}"));
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync(TenantUrl($"/products/{invalidId}"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
