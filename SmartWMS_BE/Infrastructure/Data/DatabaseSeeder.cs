using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Infrastructure.Identity;
using SmartWMS.API.Modules.Companies.Models;
using SmartWMS.API.Modules.Inventory.Models;
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
}
