using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Inventory.DTOs;
using SmartWMS.API.Modules.Inventory.Models;

namespace SmartWMS.API.Modules.Inventory.Services;

/// <summary>
/// Product management service implementation
/// </summary>
public class ProductsService : IProductsService
{
    private readonly ApplicationDbContext _context;

    public ProductsService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ApiResponse<PaginatedResult<ProductDto>>> GetProductsAsync(
        Guid tenantId,
        Guid? categoryId = null,
        int page = 1,
        int pageSize = 25,
        string? search = null,
        bool? isActive = null)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.DefaultSupplier)
            .Where(p => p.TenantId == tenantId)
            .AsQueryable();

        // Filter by category
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p =>
                p.Sku.ToLower().Contains(searchLower) ||
                p.Name.ToLower().Contains(searchLower) ||
                (p.Barcode != null && p.Barcode.ToLower().Contains(searchLower)) ||
                (p.Description != null && p.Description.ToLower().Contains(searchLower)));
        }

        // Filter by active status
        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination and get items
        var items = await query
            .OrderBy(p => p.Sku)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                CategoryPath = p.Category != null ? p.Category.Path : null,
                Barcode = p.Barcode,
                AlternativeBarcodes = p.AlternativeBarcodes,
                WidthMm = p.WidthMm,
                HeightMm = p.HeightMm,
                DepthMm = p.DepthMm,
                GrossWeightKg = p.GrossWeightKg,
                NetWeightKg = p.NetWeightKg,
                UnitOfMeasure = p.UnitOfMeasure,
                UnitsPerCase = p.UnitsPerCase,
                CasesPerPallet = p.CasesPerPallet,
                IsActive = p.IsActive,
                IsBatchTracked = p.IsBatchTracked,
                IsSerialTracked = p.IsSerialTracked,
                HasExpiryDate = p.HasExpiryDate,
                MinStockLevel = p.MinStockLevel,
                MaxStockLevel = p.MaxStockLevel,
                ReorderPoint = p.ReorderPoint,
                DefaultSupplierId = p.DefaultSupplierId,
                DefaultSupplierName = p.DefaultSupplier != null ? p.DefaultSupplier.Name : null,
                ImageUrl = p.ImageUrl,
                StockLevelCount = p.StockLevels.Count,
                TotalOnHand = p.StockLevels.Sum(s => s.QuantityOnHand),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        var result = new PaginatedResult<ProductDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return ApiResponse<PaginatedResult<ProductDto>>.Ok(result);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid tenantId, Guid productId)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.DefaultSupplier)
            .Include(p => p.StockLevels)
            .Where(p => p.TenantId == tenantId && p.Id == productId)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                CategoryPath = p.Category != null ? p.Category.Path : null,
                Barcode = p.Barcode,
                AlternativeBarcodes = p.AlternativeBarcodes,
                WidthMm = p.WidthMm,
                HeightMm = p.HeightMm,
                DepthMm = p.DepthMm,
                GrossWeightKg = p.GrossWeightKg,
                NetWeightKg = p.NetWeightKg,
                UnitOfMeasure = p.UnitOfMeasure,
                UnitsPerCase = p.UnitsPerCase,
                CasesPerPallet = p.CasesPerPallet,
                IsActive = p.IsActive,
                IsBatchTracked = p.IsBatchTracked,
                IsSerialTracked = p.IsSerialTracked,
                HasExpiryDate = p.HasExpiryDate,
                MinStockLevel = p.MinStockLevel,
                MaxStockLevel = p.MaxStockLevel,
                ReorderPoint = p.ReorderPoint,
                DefaultSupplierId = p.DefaultSupplierId,
                DefaultSupplierName = p.DefaultSupplier != null ? p.DefaultSupplier.Name : null,
                ImageUrl = p.ImageUrl,
                StockLevelCount = p.StockLevels.Count,
                TotalOnHand = p.StockLevels.Sum(s => s.QuantityOnHand),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return ApiResponse<ProductDto>.Fail("Product not found");
        }

        return ApiResponse<ProductDto>.Ok(product);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<ProductDto>> GetProductBySkuAsync(Guid tenantId, string sku)
    {
        var product = await _context.Products
            .Where(p => p.TenantId == tenantId && p.Sku == sku)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();

        if (product == Guid.Empty)
        {
            return ApiResponse<ProductDto>.Fail("Product not found");
        }

        return await GetProductByIdAsync(tenantId, product);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<ProductDto>> GetProductByBarcodeAsync(Guid tenantId, string barcode)
    {
        var product = await _context.Products
            .Where(p => p.TenantId == tenantId &&
                       (p.Barcode == barcode ||
                        (p.AlternativeBarcodes != null && p.AlternativeBarcodes.Contains(barcode))))
            .Select(p => p.Id)
            .FirstOrDefaultAsync();

        if (product == Guid.Empty)
        {
            return ApiResponse<ProductDto>.Fail("Product not found");
        }

        return await GetProductByIdAsync(tenantId, product);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<ProductDto>> CreateProductAsync(Guid tenantId, CreateProductRequest request)
    {
        // Check if SKU already exists
        var skuExists = await _context.Products
            .AnyAsync(p => p.TenantId == tenantId && p.Sku == request.Sku);

        if (skuExists)
        {
            return ApiResponse<ProductDto>.Fail($"Product with SKU '{request.Sku}' already exists");
        }

        // Validate category if provided
        if (request.CategoryId.HasValue)
        {
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == request.CategoryId.Value);

            if (category == null)
            {
                return ApiResponse<ProductDto>.Fail("Category not found");
            }
        }

        // Check if barcode already exists (if provided)
        if (!string.IsNullOrWhiteSpace(request.Barcode))
        {
            var barcodeExists = await _context.Products
                .AnyAsync(p => p.TenantId == tenantId && p.Barcode == request.Barcode);

            if (barcodeExists)
            {
                return ApiResponse<ProductDto>.Fail($"Product with barcode '{request.Barcode}' already exists");
            }
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Barcode = request.Barcode,
            AlternativeBarcodes = request.AlternativeBarcodes,
            WidthMm = request.WidthMm,
            HeightMm = request.HeightMm,
            DepthMm = request.DepthMm,
            GrossWeightKg = request.GrossWeightKg,
            NetWeightKg = request.NetWeightKg,
            UnitOfMeasure = request.UnitOfMeasure,
            UnitsPerCase = request.UnitsPerCase,
            CasesPerPallet = request.CasesPerPallet,
            IsActive = request.IsActive,
            IsBatchTracked = request.IsBatchTracked,
            IsSerialTracked = request.IsSerialTracked,
            HasExpiryDate = request.HasExpiryDate,
            MinStockLevel = request.MinStockLevel,
            MaxStockLevel = request.MaxStockLevel,
            ReorderPoint = request.ReorderPoint,
            DefaultSupplierId = request.DefaultSupplierId,
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return await GetProductByIdAsync(tenantId, product.Id);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<ProductDto>> UpdateProductAsync(Guid tenantId, Guid productId, UpdateProductRequest request)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == productId);

        if (product == null)
        {
            return ApiResponse<ProductDto>.Fail("Product not found");
        }

        // Validate category if provided
        if (request.CategoryId.HasValue)
        {
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == request.CategoryId.Value);

            if (category == null)
            {
                return ApiResponse<ProductDto>.Fail("Category not found");
            }
        }

        // Check if barcode already exists (if provided and changed)
        if (!string.IsNullOrWhiteSpace(request.Barcode) && request.Barcode != product.Barcode)
        {
            var barcodeExists = await _context.Products
                .AnyAsync(p => p.TenantId == tenantId && p.Id != productId && p.Barcode == request.Barcode);

            if (barcodeExists)
            {
                return ApiResponse<ProductDto>.Fail($"Product with barcode '{request.Barcode}' already exists");
            }
        }

        // Update fields if provided
        if (request.Name != null) product.Name = request.Name;
        if (request.Description != null) product.Description = request.Description;
        if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId;
        if (request.Barcode != null) product.Barcode = request.Barcode;
        if (request.AlternativeBarcodes != null) product.AlternativeBarcodes = request.AlternativeBarcodes;
        if (request.WidthMm.HasValue) product.WidthMm = request.WidthMm.Value;
        if (request.HeightMm.HasValue) product.HeightMm = request.HeightMm.Value;
        if (request.DepthMm.HasValue) product.DepthMm = request.DepthMm.Value;
        if (request.GrossWeightKg.HasValue) product.GrossWeightKg = request.GrossWeightKg.Value;
        if (request.NetWeightKg.HasValue) product.NetWeightKg = request.NetWeightKg.Value;
        if (request.UnitOfMeasure != null) product.UnitOfMeasure = request.UnitOfMeasure;
        if (request.UnitsPerCase.HasValue) product.UnitsPerCase = request.UnitsPerCase.Value;
        if (request.CasesPerPallet.HasValue) product.CasesPerPallet = request.CasesPerPallet.Value;
        if (request.IsActive.HasValue) product.IsActive = request.IsActive.Value;
        if (request.IsBatchTracked.HasValue) product.IsBatchTracked = request.IsBatchTracked.Value;
        if (request.IsSerialTracked.HasValue) product.IsSerialTracked = request.IsSerialTracked.Value;
        if (request.HasExpiryDate.HasValue) product.HasExpiryDate = request.HasExpiryDate.Value;
        if (request.MinStockLevel.HasValue) product.MinStockLevel = request.MinStockLevel.Value;
        if (request.MaxStockLevel.HasValue) product.MaxStockLevel = request.MaxStockLevel.Value;
        if (request.ReorderPoint.HasValue) product.ReorderPoint = request.ReorderPoint.Value;
        if (request.DefaultSupplierId.HasValue) product.DefaultSupplierId = request.DefaultSupplierId;
        if (request.ImageUrl != null) product.ImageUrl = request.ImageUrl;

        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetProductByIdAsync(tenantId, productId);
    }

    /// <inheritdoc />
    public async Task<ApiResponse> DeleteProductAsync(Guid tenantId, Guid productId)
    {
        var product = await _context.Products
            .Include(p => p.StockLevels)
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == productId);

        if (product == null)
        {
            return ApiResponse.Fail("Product not found");
        }

        // Cannot delete product with stock
        if (product.StockLevels.Any())
        {
            return ApiResponse.Fail($"Cannot delete product with {product.StockLevels.Count} stock level(s). Remove stock first.");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return ApiResponse.Ok("Product deleted successfully");
    }
}
