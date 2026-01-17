using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Infrastructure.Identity;
using SmartWMS.API.Modules.Companies.Models;
using SmartWMS.API.Modules.Inventory.Models;
using SmartWMS.API.Modules.Orders.Models;
using SmartWMS.API.Modules.Sites.Models;

namespace SmartWMS.API.Infrastructure.Data;

/// <summary>
/// Seeds initial data for the database including test tenant, site, warehouse, and admin user.
/// </summary>
public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public DatabaseSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();

        await SeedRolesAsync();
        await SeedTestTenantAsync();
        await SeedAdminUserAsync();
        await SeedProductCategoriesAndProductsAsync();
        await SeedCustomersAsync();
        await SeedSuppliersAsync();
        await SeedSalesOrdersAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            new ApplicationRole("Admin") { IsSystemRole = true, Description = "System administrator with full access" },
            new ApplicationRole("Manager") { IsSystemRole = true, Description = "Warehouse manager" },
            new ApplicationRole("Operator") { IsSystemRole = true, Description = "Warehouse operator" },
            new ApplicationRole("Viewer") { IsSystemRole = true, Description = "Read-only access" }
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role.Name!))
            {
                await _roleManager.CreateAsync(role);
            }
        }
    }

    private async Task SeedTestTenantAsync()
    {
        var tenantId = Guid.Parse("e9006ab8-257f-4021-b60a-cbba785bad46");
        var siteId = Guid.Parse("f69314bd-025a-4107-9b84-a09948827f8b");
        var warehouseId = Guid.Parse("ae5e6339-6212-4435-89a9-f5940815fe20");

        // Create Company (Tenant) - Company extends BaseEntity, not TenantEntity
        if (!await _context.Companies.AnyAsync(c => c.Id == tenantId))
        {
            var company = new Company
            {
                Id = tenantId,
                Code = "TEST001",
                Name = "Test Company",
                IsActive = true
            };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
        }

        // Create Site
        if (!await _context.Sites.AnyAsync(s => s.Id == siteId))
        {
            var site = new Site
            {
                Id = siteId,
                TenantId = tenantId,
                CompanyId = tenantId,
                Code = "SITE001",
                Name = "Main Site",
                City = "Stockholm",
                CountryCode = "SE",
                Timezone = "Europe/Stockholm",
                IsPrimary = true,
                IsActive = true
            };
            _context.Sites.Add(site);
            await _context.SaveChangesAsync();
        }

        // Create Warehouse
        if (!await _context.Warehouses.AnyAsync(w => w.Id == warehouseId))
        {
            var warehouse = new Modules.Warehouse.Models.Warehouse
            {
                Id = warehouseId,
                TenantId = tenantId,
                SiteId = siteId,
                Code = "WH001",
                Name = "Main Warehouse",
                IsActive = true
            };
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedAdminUserAsync()
    {
        var tenantId = Guid.Parse("e9006ab8-257f-4021-b60a-cbba785bad46");
        var siteId = Guid.Parse("f69314bd-025a-4107-9b84-a09948827f8b");
        var warehouseId = Guid.Parse("ae5e6339-6212-4435-89a9-f5940815fe20");

        const string adminEmail = "admin@smartwms.one";
        const string adminPassword = "Admin@123";

        if (await _userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                TenantId = tenantId,
                DefaultSiteId = siteId,
                DefaultWarehouseId = warehouseId,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }

    private async Task SeedProductCategoriesAndProductsAsync()
    {
        var tenantId = Guid.Parse("e9006ab8-257f-4021-b60a-cbba785bad46");

        // Skip if categories already exist
        if (await _context.ProductCategories.AnyAsync(c => c.TenantId == tenantId))
        {
            return;
        }

        // ===================================================================
        // Create Product Categories with hierarchy
        // ===================================================================

        // Level 0: Root Categories
        var electronicsId = Guid.NewGuid();
        var foodBeverageId = Guid.NewGuid();
        var appliancesId = Guid.NewGuid();
        var clothingId = Guid.NewGuid();
        var chemicalsId = Guid.NewGuid();

        // Level 1: Sub-categories
        var computersId = Guid.NewGuid();
        var mobilesId = Guid.NewGuid();
        var audioId = Guid.NewGuid();
        var dairyId = Guid.NewGuid();
        var frozenId = Guid.NewGuid();
        var beveragesId = Guid.NewGuid();
        var cleaningId = Guid.NewGuid();

        var categories = new List<ProductCategory>
        {
            // === ROOT CATEGORIES ===
            new()
            {
                Id = electronicsId,
                TenantId = tenantId,
                Code = "ELEC",
                Name = "Electronics",
                Description = "Electronic devices and accessories",
                Level = 0,
                IsActive = true,
                DefaultUnitOfMeasure = "EA",
                DefaultStorageZoneType = "AMBIENT",
                RequiresBatchTracking = false,
                RequiresSerialTracking = true,
                RequiresExpiryDate = false,
                IsHazardous = false,
                IsFragile = true,
                HandlingInstructions = "Handle with care. Keep away from moisture.",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = foodBeverageId,
                TenantId = tenantId,
                Code = "FOOD",
                Name = "Food & Beverage",
                Description = "Food products and drinks",
                Level = 0,
                IsActive = true,
                DefaultUnitOfMeasure = "EA",
                DefaultStorageZoneType = "COLD",
                RequiresBatchTracking = true,
                RequiresSerialTracking = false,
                RequiresExpiryDate = true,
                IsHazardous = false,
                IsFragile = false,
                HandlingInstructions = "Follow FIFO. Check expiry dates regularly.",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = appliancesId,
                TenantId = tenantId,
                Code = "APPL",
                Name = "Home Appliances",
                Description = "Household appliances and equipment",
                Level = 0,
                IsActive = true,
                DefaultUnitOfMeasure = "EA",
                DefaultStorageZoneType = "AMBIENT",
                RequiresBatchTracking = false,
                RequiresSerialTracking = true,
                RequiresExpiryDate = false,
                IsHazardous = false,
                IsFragile = true,
                HandlingInstructions = "Keep upright. Do not stack heavy items on top.",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = clothingId,
                TenantId = tenantId,
                Code = "CLTH",
                Name = "Clothing & Apparel",
                Description = "Clothes, footwear and accessories",
                Level = 0,
                IsActive = true,
                DefaultUnitOfMeasure = "EA",
                DefaultStorageZoneType = "AMBIENT",
                RequiresBatchTracking = false,
                RequiresSerialTracking = false,
                RequiresExpiryDate = false,
                IsHazardous = false,
                IsFragile = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = chemicalsId,
                TenantId = tenantId,
                Code = "CHEM",
                Name = "Chemicals & Hazmat",
                Description = "Chemical products requiring special handling",
                Level = 0,
                IsActive = true,
                DefaultUnitOfMeasure = "LTR",
                DefaultStorageZoneType = "HAZMAT",
                RequiresBatchTracking = true,
                RequiresSerialTracking = false,
                RequiresExpiryDate = true,
                IsHazardous = true,
                IsFragile = false,
                HandlingInstructions = "HAZARDOUS MATERIAL. Use protective equipment. Store in ventilated area.",
                MinTemperature = 5,
                MaxTemperature = 25,
                CreatedAt = DateTime.UtcNow
            },

            // === ELECTRONICS SUB-CATEGORIES ===
            new()
            {
                Id = computersId,
                TenantId = tenantId,
                Code = "COMP",
                Name = "Computers & Laptops",
                Description = "Desktop computers, laptops and accessories",
                ParentCategoryId = electronicsId,
                Level = 1,
                Path = "ELEC",
                IsActive = true,
                DefaultUnitOfMeasure = "EA",
                RequiresBatchTracking = false,
                RequiresSerialTracking = true,
                RequiresExpiryDate = false,
                IsHazardous = false,
                IsFragile = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = mobilesId,
                TenantId = tenantId,
                Code = "MOBL",
                Name = "Mobile Phones & Tablets",
                Description = "Smartphones, tablets and accessories",
                ParentCategoryId = electronicsId,
                Level = 1,
                Path = "ELEC",
                IsActive = true,
                DefaultUnitOfMeasure = "EA",
                RequiresBatchTracking = false,
                RequiresSerialTracking = true,
                RequiresExpiryDate = false,
                IsHazardous = false,
                IsFragile = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = audioId,
                TenantId = tenantId,
                Code = "AUDI",
                Name = "Audio & Headphones",
                Description = "Speakers, headphones and audio equipment",
                ParentCategoryId = electronicsId,
                Level = 1,
                Path = "ELEC",
                IsActive = true,
                DefaultUnitOfMeasure = "EA",
                RequiresBatchTracking = false,
                RequiresSerialTracking = true,
                RequiresExpiryDate = false,
                IsHazardous = false,
                IsFragile = true,
                CreatedAt = DateTime.UtcNow
            },

            // === FOOD & BEVERAGE SUB-CATEGORIES ===
            new()
            {
                Id = dairyId,
                TenantId = tenantId,
                Code = "DAIR",
                Name = "Dairy Products",
                Description = "Milk, cheese, yogurt and dairy items",
                ParentCategoryId = foodBeverageId,
                Level = 1,
                Path = "FOOD",
                IsActive = true,
                DefaultUnitOfMeasure = "EA",
                DefaultStorageZoneType = "COLD",
                RequiresBatchTracking = true,
                RequiresSerialTracking = false,
                RequiresExpiryDate = true,
                IsHazardous = false,
                IsFragile = false,
                MinTemperature = 2,
                MaxTemperature = 6,
                HandlingInstructions = "Keep refrigerated at 2-6°C. Check expiry dates.",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = frozenId,
                TenantId = tenantId,
                Code = "FRZN",
                Name = "Frozen Foods",
                Description = "Frozen meals, ice cream and frozen items",
                ParentCategoryId = foodBeverageId,
                Level = 1,
                Path = "FOOD",
                IsActive = true,
                DefaultUnitOfMeasure = "EA",
                DefaultStorageZoneType = "FROZEN",
                RequiresBatchTracking = true,
                RequiresSerialTracking = false,
                RequiresExpiryDate = true,
                IsHazardous = false,
                IsFragile = false,
                MinTemperature = -25,
                MaxTemperature = -18,
                HandlingInstructions = "Keep frozen at -18°C or below. Do not refreeze.",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = beveragesId,
                TenantId = tenantId,
                Code = "BEVR",
                Name = "Beverages",
                Description = "Soft drinks, juices and beverages",
                ParentCategoryId = foodBeverageId,
                Level = 1,
                Path = "FOOD",
                IsActive = true,
                DefaultUnitOfMeasure = "EA",
                DefaultStorageZoneType = "AMBIENT",
                RequiresBatchTracking = true,
                RequiresSerialTracking = false,
                RequiresExpiryDate = true,
                IsHazardous = false,
                IsFragile = true,
                HandlingInstructions = "Handle glass bottles with care.",
                CreatedAt = DateTime.UtcNow
            },

            // === CHEMICALS SUB-CATEGORY ===
            new()
            {
                Id = cleaningId,
                TenantId = tenantId,
                Code = "CLNG",
                Name = "Cleaning Products",
                Description = "Industrial and household cleaning chemicals",
                ParentCategoryId = chemicalsId,
                Level = 1,
                Path = "CHEM",
                IsActive = true,
                DefaultUnitOfMeasure = "LTR",
                DefaultStorageZoneType = "HAZMAT",
                RequiresBatchTracking = true,
                RequiresSerialTracking = false,
                RequiresExpiryDate = true,
                IsHazardous = true,
                IsFragile = false,
                HandlingInstructions = "Keep away from food products. Use protective gloves.",
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.ProductCategories.AddRange(categories);
        await _context.SaveChangesAsync();

        // ===================================================================
        // Create Products
        // ===================================================================

        var products = new List<Product>
        {
            // === COMPUTERS ===
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "LAPTOP-DELL-XPS15",
                Name = "Dell XPS 15 Laptop",
                Description = "15.6 inch laptop with Intel i7, 16GB RAM, 512GB SSD",
                CategoryId = computersId,
                Barcode = "5901234123457",
                UnitOfMeasure = "EA",
                WidthMm = 344,
                HeightMm = 18,
                DepthMm = 230,
                GrossWeightKg = 2.1m,
                NetWeightKg = 1.8m,
                IsActive = true,
                IsBatchTracked = false,
                IsSerialTracked = true,
                HasExpiryDate = false,
                MinStockLevel = 5,
                MaxStockLevel = 50,
                ReorderPoint = 10,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "LAPTOP-MAC-AIR-M2",
                Name = "MacBook Air M2",
                Description = "Apple MacBook Air with M2 chip, 8GB RAM, 256GB SSD",
                CategoryId = computersId,
                Barcode = "5901234123458",
                UnitOfMeasure = "EA",
                WidthMm = 304,
                HeightMm = 11,
                DepthMm = 215,
                GrossWeightKg = 1.5m,
                NetWeightKg = 1.24m,
                IsActive = true,
                IsBatchTracked = false,
                IsSerialTracked = true,
                HasExpiryDate = false,
                MinStockLevel = 10,
                MaxStockLevel = 100,
                ReorderPoint = 20,
                CreatedAt = DateTime.UtcNow
            },

            // === MOBILES ===
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "PHONE-IPHONE-15PRO",
                Name = "iPhone 15 Pro",
                Description = "Apple iPhone 15 Pro, 256GB, Titanium",
                CategoryId = mobilesId,
                Barcode = "5901234123459",
                UnitOfMeasure = "EA",
                WidthMm = 70,
                HeightMm = 146,
                DepthMm = 8,
                GrossWeightKg = 0.22m,
                NetWeightKg = 0.187m,
                IsActive = true,
                IsBatchTracked = false,
                IsSerialTracked = true,
                HasExpiryDate = false,
                MinStockLevel = 20,
                MaxStockLevel = 200,
                ReorderPoint = 50,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "PHONE-SAMSUNG-S24",
                Name = "Samsung Galaxy S24 Ultra",
                Description = "Samsung Galaxy S24 Ultra, 512GB, Titanium Gray",
                CategoryId = mobilesId,
                Barcode = "5901234123460",
                UnitOfMeasure = "EA",
                WidthMm = 79,
                HeightMm = 162,
                DepthMm = 8,
                GrossWeightKg = 0.28m,
                NetWeightKg = 0.232m,
                IsActive = true,
                IsBatchTracked = false,
                IsSerialTracked = true,
                HasExpiryDate = false,
                MinStockLevel = 15,
                MaxStockLevel = 150,
                ReorderPoint = 30,
                CreatedAt = DateTime.UtcNow
            },

            // === AUDIO ===
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "AUDIO-AIRPODS-PRO2",
                Name = "AirPods Pro 2",
                Description = "Apple AirPods Pro 2nd generation with USB-C",
                CategoryId = audioId,
                Barcode = "5901234123461",
                UnitOfMeasure = "EA",
                WidthMm = 45,
                HeightMm = 60,
                DepthMm = 21,
                GrossWeightKg = 0.1m,
                NetWeightKg = 0.05m,
                IsActive = true,
                IsBatchTracked = false,
                IsSerialTracked = true,
                HasExpiryDate = false,
                MinStockLevel = 30,
                MaxStockLevel = 300,
                ReorderPoint = 60,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "AUDIO-SONY-WH1000",
                Name = "Sony WH-1000XM5",
                Description = "Sony wireless noise canceling headphones",
                CategoryId = audioId,
                Barcode = "5901234123462",
                UnitOfMeasure = "EA",
                WidthMm = 190,
                HeightMm = 230,
                DepthMm = 50,
                GrossWeightKg = 0.35m,
                NetWeightKg = 0.25m,
                IsActive = true,
                IsBatchTracked = false,
                IsSerialTracked = true,
                HasExpiryDate = false,
                MinStockLevel = 10,
                MaxStockLevel = 100,
                ReorderPoint = 20,
                CreatedAt = DateTime.UtcNow
            },

            // === DAIRY ===
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "DAIRY-MILK-1L",
                Name = "Whole Milk 1L",
                Description = "Fresh whole milk, 1 liter carton",
                CategoryId = dairyId,
                Barcode = "5901234123463",
                UnitOfMeasure = "EA",
                UnitsPerCase = 12,
                CasesPerPallet = 80,
                WidthMm = 70,
                HeightMm = 200,
                DepthMm = 70,
                GrossWeightKg = 1.05m,
                NetWeightKg = 1.0m,
                IsActive = true,
                IsBatchTracked = true,
                IsSerialTracked = false,
                HasExpiryDate = true,
                MinStockLevel = 100,
                MaxStockLevel = 1000,
                ReorderPoint = 200,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "DAIRY-CHEESE-GOUDA",
                Name = "Gouda Cheese 500g",
                Description = "Aged Gouda cheese, 500 gram block",
                CategoryId = dairyId,
                Barcode = "5901234123464",
                UnitOfMeasure = "EA",
                UnitsPerCase = 20,
                CasesPerPallet = 40,
                WidthMm = 100,
                HeightMm = 60,
                DepthMm = 100,
                GrossWeightKg = 0.52m,
                NetWeightKg = 0.5m,
                IsActive = true,
                IsBatchTracked = true,
                IsSerialTracked = false,
                HasExpiryDate = true,
                MinStockLevel = 50,
                MaxStockLevel = 500,
                ReorderPoint = 100,
                CreatedAt = DateTime.UtcNow
            },

            // === FROZEN ===
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "FROZEN-PIZZA-MARG",
                Name = "Frozen Margherita Pizza",
                Description = "Stone-baked frozen margherita pizza, 350g",
                CategoryId = frozenId,
                Barcode = "5901234123465",
                UnitOfMeasure = "EA",
                UnitsPerCase = 10,
                CasesPerPallet = 50,
                WidthMm = 280,
                HeightMm = 30,
                DepthMm = 280,
                GrossWeightKg = 0.38m,
                NetWeightKg = 0.35m,
                IsActive = true,
                IsBatchTracked = true,
                IsSerialTracked = false,
                HasExpiryDate = true,
                MinStockLevel = 100,
                MaxStockLevel = 1000,
                ReorderPoint = 200,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "FROZEN-ICECREAM-VAN",
                Name = "Vanilla Ice Cream 1L",
                Description = "Premium vanilla ice cream, 1 liter tub",
                CategoryId = frozenId,
                Barcode = "5901234123466",
                UnitOfMeasure = "EA",
                UnitsPerCase = 6,
                CasesPerPallet = 60,
                WidthMm = 120,
                HeightMm = 100,
                DepthMm = 120,
                GrossWeightKg = 0.65m,
                NetWeightKg = 0.55m,
                IsActive = true,
                IsBatchTracked = true,
                IsSerialTracked = false,
                HasExpiryDate = true,
                MinStockLevel = 80,
                MaxStockLevel = 800,
                ReorderPoint = 150,
                CreatedAt = DateTime.UtcNow
            },

            // === BEVERAGES ===
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "BEVR-COLA-330ML",
                Name = "Cola 330ml Can",
                Description = "Classic cola soft drink, 330ml aluminum can",
                CategoryId = beveragesId,
                Barcode = "5901234123467",
                UnitOfMeasure = "EA",
                UnitsPerCase = 24,
                CasesPerPallet = 100,
                WidthMm = 66,
                HeightMm = 115,
                DepthMm = 66,
                GrossWeightKg = 0.35m,
                NetWeightKg = 0.33m,
                IsActive = true,
                IsBatchTracked = true,
                IsSerialTracked = false,
                HasExpiryDate = true,
                MinStockLevel = 500,
                MaxStockLevel = 5000,
                ReorderPoint = 1000,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "BEVR-WATER-500ML",
                Name = "Spring Water 500ml",
                Description = "Natural spring water, 500ml PET bottle",
                CategoryId = beveragesId,
                Barcode = "5901234123468",
                UnitOfMeasure = "EA",
                UnitsPerCase = 24,
                CasesPerPallet = 120,
                WidthMm = 65,
                HeightMm = 210,
                DepthMm = 65,
                GrossWeightKg = 0.52m,
                NetWeightKg = 0.5m,
                IsActive = true,
                IsBatchTracked = true,
                IsSerialTracked = false,
                HasExpiryDate = true,
                MinStockLevel = 1000,
                MaxStockLevel = 10000,
                ReorderPoint = 2000,
                CreatedAt = DateTime.UtcNow
            },

            // === CLOTHING ===
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "CLTH-TSHIRT-BLK-M",
                Name = "Black T-Shirt Size M",
                Description = "100% cotton black t-shirt, medium size",
                CategoryId = clothingId,
                Barcode = "5901234123469",
                UnitOfMeasure = "EA",
                UnitsPerCase = 50,
                CasesPerPallet = 40,
                WidthMm = 300,
                HeightMm = 20,
                DepthMm = 200,
                GrossWeightKg = 0.18m,
                NetWeightKg = 0.15m,
                IsActive = true,
                IsBatchTracked = false,
                IsSerialTracked = false,
                HasExpiryDate = false,
                MinStockLevel = 100,
                MaxStockLevel = 1000,
                ReorderPoint = 200,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "CLTH-JEANS-BLU-32",
                Name = "Blue Jeans Size 32",
                Description = "Classic fit blue denim jeans, waist 32",
                CategoryId = clothingId,
                Barcode = "5901234123470",
                UnitOfMeasure = "EA",
                UnitsPerCase = 30,
                CasesPerPallet = 30,
                WidthMm = 350,
                HeightMm = 30,
                DepthMm = 250,
                GrossWeightKg = 0.55m,
                NetWeightKg = 0.5m,
                IsActive = true,
                IsBatchTracked = false,
                IsSerialTracked = false,
                HasExpiryDate = false,
                MinStockLevel = 50,
                MaxStockLevel = 500,
                ReorderPoint = 100,
                CreatedAt = DateTime.UtcNow
            },

            // === APPLIANCES ===
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "APPL-MICROWAVE-800W",
                Name = "Microwave Oven 800W",
                Description = "Countertop microwave oven, 800W, 20L capacity",
                CategoryId = appliancesId,
                Barcode = "5901234123471",
                UnitOfMeasure = "EA",
                UnitsPerCase = 1,
                CasesPerPallet = 24,
                WidthMm = 440,
                HeightMm = 260,
                DepthMm = 360,
                GrossWeightKg = 12.5m,
                NetWeightKg = 11.0m,
                IsActive = true,
                IsBatchTracked = false,
                IsSerialTracked = true,
                HasExpiryDate = false,
                MinStockLevel = 10,
                MaxStockLevel = 100,
                ReorderPoint = 20,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "APPL-KETTLE-1.7L",
                Name = "Electric Kettle 1.7L",
                Description = "Stainless steel electric kettle, 1.7 liter",
                CategoryId = appliancesId,
                Barcode = "5901234123472",
                UnitOfMeasure = "EA",
                UnitsPerCase = 6,
                CasesPerPallet = 60,
                WidthMm = 220,
                HeightMm = 250,
                DepthMm = 160,
                GrossWeightKg = 1.2m,
                NetWeightKg = 1.0m,
                IsActive = true,
                IsBatchTracked = false,
                IsSerialTracked = true,
                HasExpiryDate = false,
                MinStockLevel = 20,
                MaxStockLevel = 200,
                ReorderPoint = 40,
                CreatedAt = DateTime.UtcNow
            },

            // === CLEANING / CHEMICALS ===
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "CHEM-CLEANER-ALL-1L",
                Name = "All-Purpose Cleaner 1L",
                Description = "Industrial strength all-purpose cleaning solution",
                CategoryId = cleaningId,
                Barcode = "5901234123473",
                UnitOfMeasure = "LTR",
                UnitsPerCase = 12,
                CasesPerPallet = 60,
                WidthMm = 80,
                HeightMm = 250,
                DepthMm = 80,
                GrossWeightKg = 1.1m,
                NetWeightKg = 1.0m,
                IsActive = true,
                IsBatchTracked = true,
                IsSerialTracked = false,
                HasExpiryDate = true,
                MinStockLevel = 50,
                MaxStockLevel = 500,
                ReorderPoint = 100,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Sku = "CHEM-BLEACH-5L",
                Name = "Industrial Bleach 5L",
                Description = "Concentrated chlorine bleach for industrial use",
                CategoryId = cleaningId,
                Barcode = "5901234123474",
                UnitOfMeasure = "LTR",
                UnitsPerCase = 4,
                CasesPerPallet = 40,
                WidthMm = 150,
                HeightMm = 300,
                DepthMm = 150,
                GrossWeightKg = 5.5m,
                NetWeightKg = 5.0m,
                IsActive = true,
                IsBatchTracked = true,
                IsSerialTracked = false,
                HasExpiryDate = true,
                MinStockLevel = 20,
                MaxStockLevel = 200,
                ReorderPoint = 40,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();
    }

    private async Task SeedCustomersAsync()
    {
        var tenantId = Guid.Parse("e9006ab8-257f-4021-b60a-cbba785bad46");

        // Skip if customers already exist
        if (await _context.Customers.AnyAsync(c => c.TenantId == tenantId))
        {
            return;
        }

        var customers = new List<Customer>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "CUST001",
                Name = "Acme Corporation",
                ContactName = "John Smith",
                Email = "john.smith@acme.com",
                Phone = "+1 555 123 4567",
                AddressLine1 = "123 Business Park",
                AddressLine2 = "Suite 100",
                City = "New York",
                Region = "NY",
                PostalCode = "10001",
                CountryCode = "US",
                TaxId = "12-3456789",
                PaymentTerms = "Net 30",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "CUST002",
                Name = "TechStart AB",
                ContactName = "Erik Johansson",
                Email = "erik@techstart.se",
                Phone = "+46 8 123 45 67",
                AddressLine1 = "Drottninggatan 50",
                City = "Stockholm",
                Region = "Stockholm",
                PostalCode = "111 21",
                CountryCode = "SE",
                TaxId = "SE556677889901",
                PaymentTerms = "Net 15",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "CUST003",
                Name = "Global Logistics GmbH",
                ContactName = "Hans Mueller",
                Email = "h.mueller@globallogistics.de",
                Phone = "+49 30 987654",
                AddressLine1 = "Industriestrasse 42",
                City = "Berlin",
                Region = "Berlin",
                PostalCode = "10115",
                CountryCode = "DE",
                TaxId = "DE123456789",
                PaymentTerms = "Net 45",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "CUST004",
                Name = "Nordic Supplies",
                ContactName = "Anna Lindgren",
                Email = "anna@nordicsupplies.no",
                Phone = "+47 22 33 44 55",
                AddressLine1 = "Storgata 15",
                City = "Oslo",
                Region = "Oslo",
                PostalCode = "0154",
                CountryCode = "NO",
                TaxId = "NO987654321MVA",
                PaymentTerms = "Net 30",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "CUST005",
                Name = "Pacific Trading Co",
                ContactName = "Sarah Chen",
                Email = "s.chen@pacifictrading.com",
                Phone = "+1 415 555 0199",
                AddressLine1 = "888 Market Street",
                AddressLine2 = "Floor 12",
                City = "San Francisco",
                Region = "CA",
                PostalCode = "94102",
                CountryCode = "US",
                TaxId = "94-7654321",
                PaymentTerms = "Net 60",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "CUST006",
                Name = "EuroMart Distribution",
                ContactName = "Pierre Dubois",
                Email = "pdubois@euromart.fr",
                Phone = "+33 1 42 68 53 00",
                AddressLine1 = "25 Rue de la Paix",
                City = "Paris",
                Region = "Île-de-France",
                PostalCode = "75002",
                CountryCode = "FR",
                TaxId = "FR12345678901",
                PaymentTerms = "Net 30",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "CUST007",
                Name = "British Retail Ltd",
                ContactName = "James Wilson",
                Email = "j.wilson@britishretail.co.uk",
                Phone = "+44 20 7946 0958",
                AddressLine1 = "100 Oxford Street",
                City = "London",
                Region = "Greater London",
                PostalCode = "W1D 1LL",
                CountryCode = "GB",
                TaxId = "GB123456789",
                PaymentTerms = "Net 30",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "CUST008",
                Name = "Helsinki Electronics Oy",
                ContactName = "Mika Virtanen",
                Email = "mika@helsinkielectronics.fi",
                Phone = "+358 9 123 4567",
                AddressLine1 = "Mannerheimintie 10",
                City = "Helsinki",
                Region = "Uusimaa",
                PostalCode = "00100",
                CountryCode = "FI",
                TaxId = "FI12345678",
                PaymentTerms = "Net 14",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "CUST009",
                Name = "Mediterranean Foods",
                ContactName = "Marco Rossi",
                Email = "m.rossi@medfoods.it",
                Phone = "+39 06 1234567",
                AddressLine1 = "Via Roma 25",
                City = "Rome",
                Region = "Lazio",
                PostalCode = "00184",
                CountryCode = "IT",
                TaxId = "IT12345678901",
                PaymentTerms = "Net 45",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "CUST010",
                Name = "Danish Design ApS",
                ContactName = "Lars Nielsen",
                Email = "lars@danishdesign.dk",
                Phone = "+45 33 12 34 56",
                AddressLine1 = "Strøget 55",
                City = "Copenhagen",
                Region = "Capital Region",
                PostalCode = "1160",
                CountryCode = "DK",
                TaxId = "DK12345678",
                PaymentTerms = "Net 30",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "CUST011",
                Name = "Inactive Corp",
                ContactName = "Old Contact",
                Email = "old@inactive.com",
                Phone = "+1 555 000 0000",
                AddressLine1 = "Closed Business Park",
                City = "Nowhere",
                Region = "NA",
                PostalCode = "00000",
                CountryCode = "US",
                PaymentTerms = "Net 30",
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "CUST012",
                Name = "Warsaw Wholesale Sp. z o.o.",
                ContactName = "Piotr Kowalski",
                Email = "p.kowalski@warsawwholesale.pl",
                Phone = "+48 22 123 45 67",
                AddressLine1 = "ul. Marszałkowska 100",
                City = "Warsaw",
                Region = "Mazovia",
                PostalCode = "00-026",
                CountryCode = "PL",
                TaxId = "PL1234567890",
                PaymentTerms = "Net 30",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();
    }

    private async Task SeedSuppliersAsync()
    {
        var tenantId = Guid.Parse("e9006ab8-257f-4021-b60a-cbba785bad46");

        // Skip if suppliers already exist
        if (await _context.Suppliers.AnyAsync(s => s.TenantId == tenantId))
        {
            return;
        }

        var suppliers = new List<Supplier>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "SUPP001",
                Name = "TechSupply Inc",
                ContactName = "Michael Johnson",
                Email = "m.johnson@techsupply.com",
                Phone = "+1 555 987 6543",
                AddressLine1 = "500 Industrial Blvd",
                City = "Chicago",
                Region = "IL",
                PostalCode = "60601",
                CountryCode = "US",
                TaxId = "36-1234567",
                PaymentTerms = "Net 30",
                LeadTimeDays = 7,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "SUPP002",
                Name = "Nordic Components AB",
                ContactName = "Anna Svensson",
                Email = "anna@nordiccomp.se",
                Phone = "+46 8 555 1234",
                AddressLine1 = "Industrivägen 25",
                City = "Gothenburg",
                Region = "Västra Götaland",
                PostalCode = "411 04",
                CountryCode = "SE",
                TaxId = "SE556677889902",
                PaymentTerms = "Net 45",
                LeadTimeDays = 14,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "SUPP003",
                Name = "Deutsche Elektronik GmbH",
                ContactName = "Klaus Weber",
                Email = "k.weber@deutschelektronik.de",
                Phone = "+49 89 12345678",
                AddressLine1 = "Münchner Straße 100",
                City = "Munich",
                Region = "Bavaria",
                PostalCode = "80331",
                CountryCode = "DE",
                TaxId = "DE987654321",
                PaymentTerms = "Net 30",
                LeadTimeDays = 10,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "SUPP004",
                Name = "Asian Manufacturing Ltd",
                ContactName = "Li Wei",
                Email = "l.wei@asianmfg.cn",
                Phone = "+86 21 5555 8888",
                AddressLine1 = "888 Pudong Avenue",
                City = "Shanghai",
                Region = "Shanghai",
                PostalCode = "200120",
                CountryCode = "CN",
                TaxId = "CN912345678901234567",
                PaymentTerms = "Net 60",
                LeadTimeDays = 30,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "SUPP005",
                Name = "UK Industrial Supplies",
                ContactName = "David Brown",
                Email = "d.brown@ukindustrial.co.uk",
                Phone = "+44 121 555 7890",
                AddressLine1 = "50 Birmingham Road",
                City = "Birmingham",
                Region = "West Midlands",
                PostalCode = "B1 1AA",
                CountryCode = "GB",
                TaxId = "GB987654321",
                PaymentTerms = "Net 30",
                LeadTimeDays = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "SUPP006",
                Name = "French Parts SARL",
                ContactName = "Marie Dupont",
                Email = "m.dupont@frenchparts.fr",
                Phone = "+33 4 91 55 66 77",
                AddressLine1 = "15 Avenue de Marseille",
                City = "Lyon",
                Region = "Auvergne-Rhône-Alpes",
                PostalCode = "69001",
                CountryCode = "FR",
                TaxId = "FR98765432101",
                PaymentTerms = "Net 45",
                LeadTimeDays = 12,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "SUPP007",
                Name = "Italian Quality SpA",
                ContactName = "Giuseppe Romano",
                Email = "g.romano@italianquality.it",
                Phone = "+39 02 555 1234",
                AddressLine1 = "Via Milano 50",
                City = "Milan",
                Region = "Lombardy",
                PostalCode = "20121",
                CountryCode = "IT",
                TaxId = "IT12345678901",
                PaymentTerms = "Net 30",
                LeadTimeDays = 8,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "SUPP008",
                Name = "Polish Manufacturing Sp. z o.o.",
                ContactName = "Jan Nowak",
                Email = "j.nowak@polishmfg.pl",
                Phone = "+48 22 555 6789",
                AddressLine1 = "ul. Przemysłowa 200",
                City = "Lodz",
                Region = "Lodzkie",
                PostalCode = "90-001",
                CountryCode = "PL",
                TaxId = "PL9876543210",
                PaymentTerms = "Net 30",
                LeadTimeDays = 7,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "SUPP009",
                Name = "Inactive Supplier Co",
                ContactName = "Old Contact",
                Email = "old@inactive-supplier.com",
                Phone = "+1 555 000 0001",
                AddressLine1 = "Closed Factory",
                City = "Nowhere",
                Region = "NA",
                PostalCode = "00001",
                CountryCode = "US",
                PaymentTerms = "Net 30",
                LeadTimeDays = 0,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "SUPP010",
                Name = "Japan Electronics Co Ltd",
                ContactName = "Takeshi Yamamoto",
                Email = "t.yamamoto@japanelec.jp",
                Phone = "+81 3 5555 6666",
                AddressLine1 = "1-2-3 Akihabara",
                City = "Tokyo",
                Region = "Tokyo",
                PostalCode = "101-0021",
                CountryCode = "JP",
                TaxId = "JP1234567890123",
                PaymentTerms = "Net 60",
                LeadTimeDays = 21,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Suppliers.AddRange(suppliers);
        await _context.SaveChangesAsync();
    }

    private async Task SeedSalesOrdersAsync()
    {
        var tenantId = Guid.Parse("e9006ab8-257f-4021-b60a-cbba785bad46");
        var warehouseId = Guid.Parse("ae5e6339-6212-4435-89a9-f5940815fe20");

        // Skip if orders already exist
        if (await _context.SalesOrders.AnyAsync(o => o.TenantId == tenantId))
        {
            return;
        }

        // Get customers and products from database
        var customers = await _context.Customers
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderBy(c => c.Code)
            .Take(8)
            .ToListAsync();

        var products = await _context.Products
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .OrderBy(p => p.Sku)
            .Take(10)
            .ToListAsync();

        if (customers.Count == 0 || products.Count == 0)
        {
            return; // Need customers and products first
        }

        var orders = new List<SalesOrder>();
        var orderLines = new List<SalesOrderLine>();
        var random = new Random(42); // Fixed seed for reproducibility

        // Order 1: Pending order
        var order1Id = Guid.NewGuid();
        orders.Add(new SalesOrder
        {
            Id = order1Id,
            TenantId = tenantId,
            OrderNumber = "SO-2024-0001",
            CustomerId = customers[0].Id,
            WarehouseId = warehouseId,
            Status = SalesOrderStatus.Pending,
            Priority = OrderPriority.Normal,
            OrderDate = DateTime.UtcNow.AddDays(-5),
            RequiredDate = DateTime.UtcNow.AddDays(7),
            ShipToName = customers[0].Name,
            ShipToAddressLine1 = customers[0].AddressLine1,
            ShipToCity = customers[0].City,
            ShipToRegion = customers[0].Region,
            ShipToPostalCode = customers[0].PostalCode,
            ShipToCountryCode = customers[0].CountryCode,
            TotalLines = 3,
            TotalQuantity = 25,
            Notes = "Standard delivery",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order1Id, LineNumber = 1, ProductId = products[0].Id, Sku = products[0].Sku, QuantityOrdered = 10, CreatedAt = DateTime.UtcNow.AddDays(-5) });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order1Id, LineNumber = 2, ProductId = products[1].Id, Sku = products[1].Sku, QuantityOrdered = 5, CreatedAt = DateTime.UtcNow.AddDays(-5) });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order1Id, LineNumber = 3, ProductId = products[2].Id, Sku = products[2].Sku, QuantityOrdered = 10, CreatedAt = DateTime.UtcNow.AddDays(-5) });

        // Order 2: Confirmed and allocated
        var order2Id = Guid.NewGuid();
        orders.Add(new SalesOrder
        {
            Id = order2Id,
            TenantId = tenantId,
            OrderNumber = "SO-2024-0002",
            CustomerId = customers[1].Id,
            WarehouseId = warehouseId,
            Status = SalesOrderStatus.Allocated,
            Priority = OrderPriority.High,
            OrderDate = DateTime.UtcNow.AddDays(-3),
            RequiredDate = DateTime.UtcNow.AddDays(2),
            ShipToName = customers[1].Name,
            ShipToAddressLine1 = customers[1].AddressLine1,
            ShipToCity = customers[1].City,
            ShipToRegion = customers[1].Region,
            ShipToPostalCode = customers[1].PostalCode,
            ShipToCountryCode = customers[1].CountryCode,
            TotalLines = 2,
            TotalQuantity = 15,
            AllocatedQuantity = 15,
            Notes = "Priority customer - expedite",
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order2Id, LineNumber = 1, ProductId = products[3].Id, Sku = products[3].Sku, QuantityOrdered = 10, QuantityAllocated = 10, CreatedAt = DateTime.UtcNow.AddDays(-3) });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order2Id, LineNumber = 2, ProductId = products[4].Id, Sku = products[4].Sku, QuantityOrdered = 5, QuantityAllocated = 5, CreatedAt = DateTime.UtcNow.AddDays(-3) });

        // Order 3: Picked and ready for packing
        var order3Id = Guid.NewGuid();
        orders.Add(new SalesOrder
        {
            Id = order3Id,
            TenantId = tenantId,
            OrderNumber = "SO-2024-0003",
            CustomerId = customers[2].Id,
            WarehouseId = warehouseId,
            Status = SalesOrderStatus.Picked,
            Priority = OrderPriority.Normal,
            OrderDate = DateTime.UtcNow.AddDays(-4),
            RequiredDate = DateTime.UtcNow.AddDays(1),
            ShipToName = customers[2].Name,
            ShipToAddressLine1 = customers[2].AddressLine1,
            ShipToCity = customers[2].City,
            ShipToRegion = customers[2].Region,
            ShipToPostalCode = customers[2].PostalCode,
            ShipToCountryCode = customers[2].CountryCode,
            TotalLines = 1,
            TotalQuantity = 20,
            AllocatedQuantity = 20,
            PickedQuantity = 20,
            CreatedAt = DateTime.UtcNow.AddDays(-4)
        });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order3Id, LineNumber = 1, ProductId = products[5].Id, Sku = products[5].Sku, QuantityOrdered = 20, QuantityAllocated = 20, QuantityPicked = 20, CreatedAt = DateTime.UtcNow.AddDays(-4) });

        // Order 4: Shipped
        var order4Id = Guid.NewGuid();
        orders.Add(new SalesOrder
        {
            Id = order4Id,
            TenantId = tenantId,
            OrderNumber = "SO-2024-0004",
            CustomerId = customers[3].Id,
            WarehouseId = warehouseId,
            Status = SalesOrderStatus.Shipped,
            Priority = OrderPriority.Normal,
            OrderDate = DateTime.UtcNow.AddDays(-10),
            RequiredDate = DateTime.UtcNow.AddDays(-3),
            ShippedDate = DateTime.UtcNow.AddDays(-4),
            ShipToName = customers[3].Name,
            ShipToAddressLine1 = customers[3].AddressLine1,
            ShipToCity = customers[3].City,
            ShipToRegion = customers[3].Region,
            ShipToPostalCode = customers[3].PostalCode,
            ShipToCountryCode = customers[3].CountryCode,
            CarrierCode = "DHL",
            ServiceLevel = "Express",
            TotalLines = 2,
            TotalQuantity = 30,
            AllocatedQuantity = 30,
            PickedQuantity = 30,
            ShippedQuantity = 30,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order4Id, LineNumber = 1, ProductId = products[6].Id, Sku = products[6].Sku, QuantityOrdered = 15, QuantityAllocated = 15, QuantityPicked = 15, QuantityShipped = 15, CreatedAt = DateTime.UtcNow.AddDays(-10) });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order4Id, LineNumber = 2, ProductId = products[7].Id, Sku = products[7].Sku, QuantityOrdered = 15, QuantityAllocated = 15, QuantityPicked = 15, QuantityShipped = 15, CreatedAt = DateTime.UtcNow.AddDays(-10) });

        // Order 5: Delivered
        var order5Id = Guid.NewGuid();
        orders.Add(new SalesOrder
        {
            Id = order5Id,
            TenantId = tenantId,
            OrderNumber = "SO-2024-0005",
            CustomerId = customers[4].Id,
            WarehouseId = warehouseId,
            Status = SalesOrderStatus.Delivered,
            Priority = OrderPriority.Low,
            OrderDate = DateTime.UtcNow.AddDays(-15),
            RequiredDate = DateTime.UtcNow.AddDays(-8),
            ShippedDate = DateTime.UtcNow.AddDays(-9),
            ShipToName = customers[4].Name,
            ShipToAddressLine1 = customers[4].AddressLine1,
            ShipToCity = customers[4].City,
            ShipToRegion = customers[4].Region,
            ShipToPostalCode = customers[4].PostalCode,
            ShipToCountryCode = customers[4].CountryCode,
            CarrierCode = "FEDEX",
            ServiceLevel = "Ground",
            TotalLines = 3,
            TotalQuantity = 50,
            AllocatedQuantity = 50,
            PickedQuantity = 50,
            ShippedQuantity = 50,
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order5Id, LineNumber = 1, ProductId = products[0].Id, Sku = products[0].Sku, QuantityOrdered = 20, QuantityAllocated = 20, QuantityPicked = 20, QuantityShipped = 20, CreatedAt = DateTime.UtcNow.AddDays(-15) });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order5Id, LineNumber = 2, ProductId = products[1].Id, Sku = products[1].Sku, QuantityOrdered = 15, QuantityAllocated = 15, QuantityPicked = 15, QuantityShipped = 15, CreatedAt = DateTime.UtcNow.AddDays(-15) });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order5Id, LineNumber = 3, ProductId = products[2].Id, Sku = products[2].Sku, QuantityOrdered = 15, QuantityAllocated = 15, QuantityPicked = 15, QuantityShipped = 15, CreatedAt = DateTime.UtcNow.AddDays(-15) });

        // Order 6: Urgent order
        var order6Id = Guid.NewGuid();
        orders.Add(new SalesOrder
        {
            Id = order6Id,
            TenantId = tenantId,
            OrderNumber = "SO-2024-0006",
            CustomerId = customers[5].Id,
            WarehouseId = warehouseId,
            Status = SalesOrderStatus.Confirmed,
            Priority = OrderPriority.Urgent,
            OrderDate = DateTime.UtcNow.AddDays(-1),
            RequiredDate = DateTime.UtcNow,
            ShipToName = customers[5].Name,
            ShipToAddressLine1 = customers[5].AddressLine1,
            ShipToCity = customers[5].City,
            ShipToRegion = customers[5].Region,
            ShipToPostalCode = customers[5].PostalCode,
            ShipToCountryCode = customers[5].CountryCode,
            CarrierCode = "UPS",
            ServiceLevel = "Next Day Air",
            TotalLines = 1,
            TotalQuantity = 5,
            Notes = "URGENT - Customer escalation",
            InternalNotes = "CEO personal order - handle with care",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order6Id, LineNumber = 1, ProductId = products[3].Id, Sku = products[3].Sku, QuantityOrdered = 5, CreatedAt = DateTime.UtcNow.AddDays(-1) });

        // Order 7: On Hold
        var order7Id = Guid.NewGuid();
        orders.Add(new SalesOrder
        {
            Id = order7Id,
            TenantId = tenantId,
            OrderNumber = "SO-2024-0007",
            CustomerId = customers[6].Id,
            WarehouseId = warehouseId,
            Status = SalesOrderStatus.OnHold,
            Priority = OrderPriority.Normal,
            OrderDate = DateTime.UtcNow.AddDays(-7),
            RequiredDate = DateTime.UtcNow.AddDays(5),
            ShipToName = customers[6].Name,
            ShipToAddressLine1 = customers[6].AddressLine1,
            ShipToCity = customers[6].City,
            ShipToRegion = customers[6].Region,
            ShipToPostalCode = customers[6].PostalCode,
            ShipToCountryCode = customers[6].CountryCode,
            TotalLines = 2,
            TotalQuantity = 100,
            Notes = "Awaiting payment confirmation",
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order7Id, LineNumber = 1, ProductId = products[8].Id, Sku = products[8].Sku, QuantityOrdered = 50, CreatedAt = DateTime.UtcNow.AddDays(-7) });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order7Id, LineNumber = 2, ProductId = products[9].Id, Sku = products[9].Sku, QuantityOrdered = 50, CreatedAt = DateTime.UtcNow.AddDays(-7) });

        // Order 8: Cancelled
        var order8Id = Guid.NewGuid();
        orders.Add(new SalesOrder
        {
            Id = order8Id,
            TenantId = tenantId,
            OrderNumber = "SO-2024-0008",
            CustomerId = customers[7].Id,
            WarehouseId = warehouseId,
            Status = SalesOrderStatus.Cancelled,
            Priority = OrderPriority.Normal,
            OrderDate = DateTime.UtcNow.AddDays(-12),
            RequiredDate = DateTime.UtcNow.AddDays(-5),
            ShipToName = customers[7].Name,
            ShipToAddressLine1 = customers[7].AddressLine1,
            ShipToCity = customers[7].City,
            ShipToRegion = customers[7].Region,
            ShipToPostalCode = customers[7].PostalCode,
            ShipToCountryCode = customers[7].CountryCode,
            TotalLines = 1,
            TotalQuantity = 10,
            Notes = "Cancelled by customer request",
            CreatedAt = DateTime.UtcNow.AddDays(-12)
        });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order8Id, LineNumber = 1, ProductId = products[4].Id, Sku = products[4].Sku, QuantityOrdered = 10, QuantityCancelled = 10, CreatedAt = DateTime.UtcNow.AddDays(-12) });

        // Order 9: Draft order
        var order9Id = Guid.NewGuid();
        orders.Add(new SalesOrder
        {
            Id = order9Id,
            TenantId = tenantId,
            OrderNumber = "SO-2024-0009",
            CustomerId = customers[0].Id,
            WarehouseId = warehouseId,
            Status = SalesOrderStatus.Draft,
            Priority = OrderPriority.Normal,
            OrderDate = DateTime.UtcNow,
            RequiredDate = DateTime.UtcNow.AddDays(14),
            ShipToName = customers[0].Name,
            ShipToAddressLine1 = customers[0].AddressLine1,
            ShipToCity = customers[0].City,
            ShipToRegion = customers[0].Region,
            ShipToPostalCode = customers[0].PostalCode,
            ShipToCountryCode = customers[0].CountryCode,
            TotalLines = 4,
            TotalQuantity = 40,
            Notes = "Draft - pending review",
            CreatedAt = DateTime.UtcNow
        });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order9Id, LineNumber = 1, ProductId = products[0].Id, Sku = products[0].Sku, QuantityOrdered = 10, CreatedAt = DateTime.UtcNow });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order9Id, LineNumber = 2, ProductId = products[1].Id, Sku = products[1].Sku, QuantityOrdered = 10, CreatedAt = DateTime.UtcNow });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order9Id, LineNumber = 3, ProductId = products[2].Id, Sku = products[2].Sku, QuantityOrdered = 10, CreatedAt = DateTime.UtcNow });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order9Id, LineNumber = 4, ProductId = products[3].Id, Sku = products[3].Sku, QuantityOrdered = 10, CreatedAt = DateTime.UtcNow });

        // Order 10: Packed and ready for shipping
        var order10Id = Guid.NewGuid();
        orders.Add(new SalesOrder
        {
            Id = order10Id,
            TenantId = tenantId,
            OrderNumber = "SO-2024-0010",
            CustomerId = customers[1].Id,
            WarehouseId = warehouseId,
            Status = SalesOrderStatus.Packed,
            Priority = OrderPriority.High,
            OrderDate = DateTime.UtcNow.AddDays(-2),
            RequiredDate = DateTime.UtcNow.AddDays(1),
            ShipToName = customers[1].Name,
            ShipToAddressLine1 = customers[1].AddressLine1,
            ShipToCity = customers[1].City,
            ShipToRegion = customers[1].Region,
            ShipToPostalCode = customers[1].PostalCode,
            ShipToCountryCode = customers[1].CountryCode,
            CarrierCode = "DHL",
            ServiceLevel = "Express",
            TotalLines = 2,
            TotalQuantity = 12,
            AllocatedQuantity = 12,
            PickedQuantity = 12,
            Notes = "Ready for dispatch",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order10Id, LineNumber = 1, ProductId = products[5].Id, Sku = products[5].Sku, QuantityOrdered = 6, QuantityAllocated = 6, QuantityPicked = 6, CreatedAt = DateTime.UtcNow.AddDays(-2) });
        orderLines.Add(new SalesOrderLine { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = order10Id, LineNumber = 2, ProductId = products[6].Id, Sku = products[6].Sku, QuantityOrdered = 6, QuantityAllocated = 6, QuantityPicked = 6, CreatedAt = DateTime.UtcNow.AddDays(-2) });

        _context.SalesOrders.AddRange(orders);
        _context.SalesOrderLines.AddRange(orderLines);
        await _context.SaveChangesAsync();
    }
}
