using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Inventory.DTOs;
using SmartWMS.API.Modules.Inventory.Services;

namespace SmartWMS.API.Modules.Inventory.Controllers;

/// <summary>
/// Product category management endpoints
/// </summary>
[ApiController]
[Route("api/v1/tenant/{tenantId:guid}/product-categories")]
[Authorize]
[Produces("application/json")]
public class ProductCategoriesController : ControllerBase
{
    private readonly IProductCategoriesService _categoriesService;

    public ProductCategoriesController(IProductCategoriesService categoriesService)
    {
        _categoriesService = categoriesService;
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
    /// Get paginated list of categories
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ProductCategoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCategories(
        Guid tenantId,
        [FromQuery] Guid? parentCategoryId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _categoriesService.GetCategoriesAsync(tenantId, parentCategoryId, page, pageSize, search, isActive);
        return Ok(result);
    }

    /// <summary>
    /// Get category tree (hierarchical structure)
    /// </summary>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(ApiResponse<List<ProductCategoryTreeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCategoryTree(Guid tenantId)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _categoriesService.GetCategoryTreeAsync(tenantId);
        return Ok(result);
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(Guid tenantId, Guid id)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _categoriesService.GetCategoryByIdAsync(tenantId, id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create new category
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductCategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCategory(Guid tenantId, [FromBody] CreateProductCategoryRequest request)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _categoriesService.CreateCategoryAsync(tenantId, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetCategory), new { tenantId, id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update category
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(Guid tenantId, Guid id, [FromBody] UpdateProductCategoryRequest request)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _categoriesService.UpdateCategoryAsync(tenantId, id, request);
        if (!result.Success)
        {
            return result.Message == "Category not found" ? NotFound(result) : BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete category
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid tenantId, Guid id)
    {
        if (!ValidateTenantAccess(tenantId, out _))
        {
            return Forbid();
        }

        var result = await _categoriesService.DeleteCategoryAsync(tenantId, id);
        if (!result.Success)
        {
            if (result.Message == "Category not found")
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }
        return Ok(result);
    }
}
