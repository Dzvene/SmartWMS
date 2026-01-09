using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Auth.DTOs;

namespace SmartWMS.API.Tests.Infrastructure;

/// <summary>
/// Base class for integration tests with common setup and utilities
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly JsonSerializerOptions JsonOptions;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task InitializeAsync()
    {
        // Seed test data
        await TestDataSeeder.SeedAsync(Factory.Services);
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Authenticate and get JWT token for test user
    /// </summary>
    protected async Task<string> GetAuthTokenAsync()
    {
        var loginRequest = new LoginRequest
        {
            TenantCode = TestDataSeeder.TestTenantCode,
            Email = TestDataSeeder.TestUserEmail,
            Password = TestDataSeeder.TestUserPassword
        };

        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);
        return result?.Data?.Token ?? throw new InvalidOperationException("Failed to get auth token");
    }

    /// <summary>
    /// Set authorization header with JWT token
    /// </summary>
    protected async Task AuthenticateAsync()
    {
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Get scoped service from DI container
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Get database context
    /// </summary>
    protected ApplicationDbContext GetDbContext()
    {
        return GetService<ApplicationDbContext>();
    }

    /// <summary>
    /// Build tenant-scoped URL
    /// </summary>
    protected string TenantUrl(string path)
    {
        return $"/api/v1/tenant/{TestDataSeeder.TestTenantId}{path}";
    }
}
