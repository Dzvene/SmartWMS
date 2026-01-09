using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Inventory.DTOs;
using SmartWMS.API.Modules.Inventory.Services;

namespace SmartWMS.API.Modules.Inventory.Controllers;

/// <summary>
/// Product management endpoints
/// </summary>
[ApiController]
[Route("api/v1/tenant/{tenantId:guid}/products")]
[Authorize]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductsService _productsService;

    public ProductsController(IProductsService productsService)
    {
        _productsService = productsService;
    }

    /// <summary>
    /// Validates that URL tenantId matches the token tenantId
    /// </summary>
    private bool ValidateTenantAccess(Guid urlTenantId, out Guid tokenTenantId)
    {
        var tenantClaim = User.FindFirst("tenant_id")?.Value;
        tokenTenantId = Guid.TryParse(tenantClaim, out var parsed) ? parsed : Guid.Empty;
        return urlTenantId == tokenTenantId;
    }

    /// <summary>
    /// Get paginated list of products
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProducts(
        Guid tenantId,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _productsService.GetProductsAsync(tenantId, categoryId, page, pageSize, search, isActive);
        return Ok(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(Guid tenantId, Guid id)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _productsService.GetProductByIdAsync(tenantId, id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get product by SKU
    /// </summary>
    [HttpGet("sku/{sku}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductBySku(Guid tenantId, string sku)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _productsService.GetProductBySkuAsync(tenantId, sku);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get product by barcode
    /// </summary>
    [HttpGet("barcode/{barcode}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductByBarcode(Guid tenantId, string barcode)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _productsService.GetProductByBarcodeAsync(tenantId, barcode);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateProduct(Guid tenantId, [FromBody] CreateProductRequest request)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _productsService.CreateProductAsync(tenantId, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetProduct), new { tenantId, id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update product
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(Guid tenantId, Guid id, [FromBody] UpdateProductRequest request)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _productsService.UpdateProductAsync(tenantId, id, request);
        if (!result.Success)
        {
            return result.Message == "Product not found" ? NotFound(result) : BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete product
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid tenantId, Guid id)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _productsService.DeleteProductAsync(tenantId, id);
        if (!result.Success)
        {
            if (result.Message == "Product not found")
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }
        return Ok(result);
    }
}
