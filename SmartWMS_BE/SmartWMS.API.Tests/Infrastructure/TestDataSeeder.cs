using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Infrastructure.Identity;
using SmartWMS.API.Modules.Companies.Models;
using SmartWMS.API.Modules.Inventory.Models;
using SmartWMS.API.Modules.Sites.Models;
using SmartWMS.API.Modules.Warehouse.Models;

namespace SmartWMS.API.Tests.Infrastructure;

/// <summary>
/// Test data seeder for integration tests
/// </summary>
public static class TestDataSeeder
{
    // Known test IDs
    public static readonly Guid TestTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TestSiteId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid TestWarehouseId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid TestUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid TestAdminRoleId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid TestCategoryId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid TestProductId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static readonly Guid TestZoneId = Guid.Parse("88888888-8888-8888-8888-888888888888");

    public const string TestTenantCode = "TEST";
    public const string TestUserEmail = "test@example.com";
    public const string TestUserPassword = "Test123!";
    public const string TestProductSku = "TEST-SKU-001";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        // Seed Company (Tenant)
        if (!context.Companies.Any(c => c.Id == TestTenantId))
        {
            context.Companies.Add(new Company
            {
                Id = TestTenantId,
                Code = TestTenantCode,
                Name = "Test Company",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // Seed Site
        if (!context.Sites.Any(s => s.Id == TestSiteId))
        {
            context.Sites.Add(new Site
            {
                Id = TestSiteId,
                TenantId = TestTenantId,
                Code = "TEST-SITE",
                Name = "Test Site",
                CompanyId = TestTenantId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // Seed Warehouse
        if (!context.Warehouses.Any(w => w.Id == TestWarehouseId))
        {
            context.Warehouses.Add(new Warehouse
            {
                Id = TestWarehouseId,
                TenantId = TestTenantId,
                SiteId = TestSiteId,
                Code = "TEST-WH",
                Name = "Test Warehouse",
                IsActive = true,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // Seed Zone
        if (!context.Zones.Any(z => z.Id == TestZoneId))
        {
            context.Zones.Add(new Zone
            {
                Id = TestZoneId,
                TenantId = TestTenantId,
                WarehouseId = TestWarehouseId,
                Code = "TEST-ZONE",
                Name = "Test Zone",
                ZoneType = Common.Enums.ZoneType.Storage,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // Seed Admin Role
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var adminRole = new ApplicationRole
            {
                Id = TestAdminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN",
                TenantId = TestTenantId,
                Description = "Administrator role"
            };
            await roleManager.CreateAsync(adminRole);
        }

        // Seed Test User
        var existingUser = await userManager.FindByEmailAsync(TestUserEmail);
        if (existingUser == null)
        {
            var user = new ApplicationUser
            {
                Id = TestUserId,
                UserName = TestUserEmail,
                Email = TestUserEmail,
                NormalizedEmail = TestUserEmail.ToUpper(),
                NormalizedUserName = TestUserEmail.ToUpper(),
                FirstName = "Test",
                LastName = "User",
                TenantId = TestTenantId,
                DefaultSiteId = TestSiteId,
                DefaultWarehouseId = TestWarehouseId,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, TestUserPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }

        // Seed Product Category
        if (!context.ProductCategories.Any(c => c.Id == TestCategoryId))
        {
            context.ProductCategories.Add(new ProductCategory
            {
                Id = TestCategoryId,
                TenantId = TestTenantId,
                Code = "TEST-CAT",
                Name = "Test Category",
                Path = "Test Category",
                Level = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // Seed Test Product
        if (!context.Products.Any(p => p.Id == TestProductId))
        {
            context.Products.Add(new Product
            {
                Id = TestProductId,
                TenantId = TestTenantId,
                Sku = TestProductSku,
                Name = "Test Product",
                Description = "Test product for integration tests",
                CategoryId = TestCategoryId,
                Barcode = "1234567890123",
                UnitOfMeasure = "EA",
                IsActive = true,
                IsBatchTracked = false,
                IsSerialTracked = false,
                HasExpiryDate = false,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
    }
}
