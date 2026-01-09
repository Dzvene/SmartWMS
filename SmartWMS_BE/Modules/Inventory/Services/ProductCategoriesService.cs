using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Inventory.DTOs;
using SmartWMS.API.Modules.Inventory.Models;

namespace SmartWMS.API.Modules.Inventory.Services;

/// <summary>
/// Product category management service implementation
/// </summary>
public class ProductCategoriesService : IProductCategoriesService
{
    private readonly ApplicationDbContext _context;

    public ProductCategoriesService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ApiResponse<PaginatedResult<ProductCategoryDto>>> GetCategoriesAsync(
        Guid tenantId,
        Guid? parentCategoryId = null,
        int page = 1,
        int pageSize = 25,
        string? search = null,
        bool? isActive = null)
    {
        var query = _context.ProductCategories
            .Include(c => c.ParentCategory)
            .Where(c => c.TenantId == tenantId)
            .AsQueryable();

        // Filter by parent category
        if (parentCategoryId.HasValue)
        {
            query = query.Where(c => c.ParentCategoryId == parentCategoryId.Value);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(c =>
                c.Code.ToLower().Contains(searchLower) ||
                c.Name.ToLower().Contains(searchLower) ||
                (c.Description != null && c.Description.ToLower().Contains(searchLower)));
        }

        // Filter by active status
        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination and get items
        var items = await query
            .OrderBy(c => c.Level)
            .ThenBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ProductCategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                Level = c.Level,
                Path = c.Path,
                IsActive = c.IsActive,
                // Product Defaults
                DefaultUnitOfMeasure = c.DefaultUnitOfMeasure,
                DefaultStorageZoneType = c.DefaultStorageZoneType,
                // Tracking Requirements
                RequiresBatchTracking = c.RequiresBatchTracking,
                RequiresSerialTracking = c.RequiresSerialTracking,
                RequiresExpiryDate = c.RequiresExpiryDate,
                // Handling & Storage
                HandlingInstructions = c.HandlingInstructions,
                MinTemperature = c.MinTemperature,
                MaxTemperature = c.MaxTemperature,
                IsHazardous = c.IsHazardous,
                IsFragile = c.IsFragile,
                // Counts
                ProductCount = c.Products.Count,
                ChildCategoryCount = c.ChildCategories.Count,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        var result = new PaginatedResult<ProductCategoryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return ApiResponse<PaginatedResult<ProductCategoryDto>>.Ok(result);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<List<ProductCategoryTreeDto>>> GetCategoryTreeAsync(Guid tenantId)
    {
        var categories = await _context.ProductCategories
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderBy(c => c.Level)
            .ThenBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Code,
                c.Name,
                c.ParentCategoryId,
                c.Level,
                c.IsActive,
                c.RequiresBatchTracking,
                c.RequiresSerialTracking,
                c.RequiresExpiryDate,
                ProductCount = c.Products.Count
            })
            .ToListAsync();

        // Build tree structure
        var categoryDict = categories.ToDictionary(
            c => c.Id,
            c => new ProductCategoryTreeDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Level = c.Level,
                ProductCount = c.ProductCount,
                IsActive = c.IsActive,
                RequiresBatchTracking = c.RequiresBatchTracking,
                RequiresSerialTracking = c.RequiresSerialTracking,
                RequiresExpiryDate = c.RequiresExpiryDate
            });

        var rootCategories = new List<ProductCategoryTreeDto>();

        foreach (var category in categories)
        {
            var treeNode = categoryDict[category.Id];

            if (category.ParentCategoryId.HasValue && categoryDict.ContainsKey(category.ParentCategoryId.Value))
            {
                categoryDict[category.ParentCategoryId.Value].Children.Add(treeNode);
            }
            else
            {
                rootCategories.Add(treeNode);
            }
        }

        return ApiResponse<List<ProductCategoryTreeDto>>.Ok(rootCategories);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<ProductCategoryDto>> GetCategoryByIdAsync(Guid tenantId, Guid categoryId)
    {
        var category = await _context.ProductCategories
            .Include(c => c.ParentCategory)
            .Where(c => c.TenantId == tenantId && c.Id == categoryId)
            .Select(c => new ProductCategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                Level = c.Level,
                Path = c.Path,
                IsActive = c.IsActive,
                // Product Defaults
                DefaultUnitOfMeasure = c.DefaultUnitOfMeasure,
                DefaultStorageZoneType = c.DefaultStorageZoneType,
                // Tracking Requirements
                RequiresBatchTracking = c.RequiresBatchTracking,
                RequiresSerialTracking = c.RequiresSerialTracking,
                RequiresExpiryDate = c.RequiresExpiryDate,
                // Handling & Storage
                HandlingInstructions = c.HandlingInstructions,
                MinTemperature = c.MinTemperature,
                MaxTemperature = c.MaxTemperature,
                IsHazardous = c.IsHazardous,
                IsFragile = c.IsFragile,
                // Counts
                ProductCount = c.Products.Count,
                ChildCategoryCount = c.ChildCategories.Count,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            return ApiResponse<ProductCategoryDto>.Fail("Category not found");
        }

        return ApiResponse<ProductCategoryDto>.Ok(category);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<ProductCategoryDto>> CreateCategoryAsync(Guid tenantId, CreateProductCategoryRequest request)
    {
        // Check if code already exists
        var codeExists = await _context.ProductCategories
            .AnyAsync(c => c.TenantId == tenantId && c.Code == request.Code);

        if (codeExists)
        {
            return ApiResponse<ProductCategoryDto>.Fail($"Category with code '{request.Code}' already exists");
        }

        int level = 0;
        string? path = null;

        // Validate parent category if provided
        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == request.ParentCategoryId.Value);

            if (parentCategory == null)
            {
                return ApiResponse<ProductCategoryDto>.Fail("Parent category not found");
            }

            level = parentCategory.Level + 1;
            path = string.IsNullOrEmpty(parentCategory.Path)
                ? parentCategory.Code
                : $"{parentCategory.Path}/{parentCategory.Code}";
        }

        var category = new ProductCategory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId,
            Level = level,
            Path = path,
            IsActive = request.IsActive,
            // Product Defaults
            DefaultUnitOfMeasure = request.DefaultUnitOfMeasure,
            DefaultStorageZoneType = request.DefaultStorageZoneType,
            // Tracking Requirements
            RequiresBatchTracking = request.RequiresBatchTracking,
            RequiresSerialTracking = request.RequiresSerialTracking,
            RequiresExpiryDate = request.RequiresExpiryDate,
            // Handling & Storage
            HandlingInstructions = request.HandlingInstructions,
            MinTemperature = request.MinTemperature,
            MaxTemperature = request.MaxTemperature,
            IsHazardous = request.IsHazardous,
            IsFragile = request.IsFragile,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync();

        return await GetCategoryByIdAsync(tenantId, category.Id);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<ProductCategoryDto>> UpdateCategoryAsync(Guid tenantId, Guid categoryId, UpdateProductCategoryRequest request)
    {
        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == categoryId);

        if (category == null)
        {
            return ApiResponse<ProductCategoryDto>.Fail("Category not found");
        }

        // Check if code already exists (if changing)
        if (!string.IsNullOrWhiteSpace(request.Code) && request.Code != category.Code)
        {
            var codeExists = await _context.ProductCategories
                .AnyAsync(c => c.TenantId == tenantId && c.Id != categoryId && c.Code == request.Code);

            if (codeExists)
            {
                return ApiResponse<ProductCategoryDto>.Fail($"Category with code '{request.Code}' already exists");
            }
        }

        // Handle parent category change
        if (request.ParentCategoryId.HasValue && request.ParentCategoryId != category.ParentCategoryId)
        {
            // Cannot set parent to itself
            if (request.ParentCategoryId.Value == categoryId)
            {
                return ApiResponse<ProductCategoryDto>.Fail("Category cannot be its own parent");
            }

            // Check for circular reference
            var potentialParent = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == request.ParentCategoryId.Value);

            if (potentialParent == null)
            {
                return ApiResponse<ProductCategoryDto>.Fail("Parent category not found");
            }

            // Check if the new parent is a descendant of the current category
            if (!string.IsNullOrEmpty(potentialParent.Path) && potentialParent.Path.Contains(category.Code))
            {
                return ApiResponse<ProductCategoryDto>.Fail("Cannot set a descendant category as parent");
            }

            category.ParentCategoryId = request.ParentCategoryId;
            category.Level = potentialParent.Level + 1;
            category.Path = string.IsNullOrEmpty(potentialParent.Path)
                ? potentialParent.Code
                : $"{potentialParent.Path}/{potentialParent.Code}";
        }

        // Update basic fields
        if (request.Code != null)
        {
            category.Code = request.Code;
        }

        if (request.Name != null)
        {
            category.Name = request.Name;
        }

        if (request.Description != null)
        {
            category.Description = request.Description;
        }

        if (request.IsActive.HasValue)
        {
            category.IsActive = request.IsActive.Value;
        }

        // Update product defaults
        if (request.DefaultUnitOfMeasure != null)
        {
            category.DefaultUnitOfMeasure = request.DefaultUnitOfMeasure;
        }

        if (request.DefaultStorageZoneType != null)
        {
            category.DefaultStorageZoneType = request.DefaultStorageZoneType;
        }

        // Update tracking requirements
        if (request.RequiresBatchTracking.HasValue)
        {
            category.RequiresBatchTracking = request.RequiresBatchTracking.Value;
        }

        if (request.RequiresSerialTracking.HasValue)
        {
            category.RequiresSerialTracking = request.RequiresSerialTracking.Value;
        }

        if (request.RequiresExpiryDate.HasValue)
        {
            category.RequiresExpiryDate = request.RequiresExpiryDate.Value;
        }

        // Update handling & storage
        if (request.HandlingInstructions != null)
        {
            category.HandlingInstructions = request.HandlingInstructions;
        }

        if (request.MinTemperature.HasValue)
        {
            category.MinTemperature = request.MinTemperature.Value;
        }

        if (request.MaxTemperature.HasValue)
        {
            category.MaxTemperature = request.MaxTemperature.Value;
        }

        if (request.IsHazardous.HasValue)
        {
            category.IsHazardous = request.IsHazardous.Value;
        }

        if (request.IsFragile.HasValue)
        {
            category.IsFragile = request.IsFragile.Value;
        }

        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetCategoryByIdAsync(tenantId, categoryId);
    }

    /// <inheritdoc />
    public async Task<ApiResponse> DeleteCategoryAsync(Guid tenantId, Guid categoryId)
    {
        var category = await _context.ProductCategories
            .Include(c => c.Products)
            .Include(c => c.ChildCategories)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == categoryId);

        if (category == null)
        {
            return ApiResponse.Fail("Category not found");
        }

        // Cannot delete category with products
        if (category.Products.Any())
        {
            return ApiResponse.Fail($"Cannot delete category with {category.Products.Count} product(s). Move products first.");
        }

        // Cannot delete category with child categories
        if (category.ChildCategories.Any())
        {
            return ApiResponse.Fail($"Cannot delete category with {category.ChildCategories.Count} child category(ies). Remove child categories first.");
        }

        _context.ProductCategories.Remove(category);
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Category deleted successfully");
    }
}
