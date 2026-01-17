using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartWMS.API.Infrastructure.Data;

namespace SmartWMS.API.Tests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration tests with in-memory database
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Use a fixed database name per factory instance so all scopes share the same database
    private readonly string _databaseName = "TestDb_" + Guid.NewGuid();

    public CustomWebApplicationFactory()
    {
        // Set JWT secret key for tests before WebApplicationFactory builds the host
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "TestSecretKeyForIntegrationTests_AtLeast32Characters!");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Also remove the DbContext registration if present
            var dbContextServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ApplicationDbContext));

            if (dbContextServiceDescriptor != null)
            {
                services.Remove(dbContextServiceDescriptor);
            }

            // Add in-memory database for testing with fixed name
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Build service provider
            var sp = services.BuildServiceProvider();

            // Create scope and initialize database
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}
