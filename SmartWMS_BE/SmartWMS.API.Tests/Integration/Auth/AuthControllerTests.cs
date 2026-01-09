using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Auth.DTOs;
using SmartWMS.API.Tests.Infrastructure;

namespace SmartWMS.API.Tests.Integration.Auth;

/// <summary>
/// Integration tests for AuthController
/// </summary>
public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var request = new LoginRequest
        {
            TenantCode = TestDataSeeder.TestTenantCode,
            Email = TestDataSeeder.TestUserEmail,
            Password = TestDataSeeder.TestUserPassword
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.User.Should().NotBeNull();
        result.Data.User!.Email.Should().Be(TestDataSeeder.TestUserEmail);
        result.Data.User.TenantId.Should().Be(TestDataSeeder.TestTenantId);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            TenantCode = TestDataSeeder.TestTenantCode,
            Email = TestDataSeeder.TestUserEmail,
            Password = "WrongPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            TenantCode = TestDataSeeder.TestTenantCode,
            Email = "nonexistent@example.com",
            Password = TestDataSeeder.TestUserPassword
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidTenantCode_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            TenantCode = "INVALID",
            Email = TestDataSeeder.TestUserEmail,
            Password = TestDataSeeder.TestUserPassword
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);
        result!.Message.Should().Contain("Invalid tenant code");
    }

    #endregion

    #region Validate Tests

    [Fact]
    public async Task Validate_WithValidToken_ReturnsUserInfo()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/v1/auth/validate");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserInfo>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(TestDataSeeder.TestUserEmail);
        result.Data.TenantId.Should().Be(TestDataSeeder.TestTenantId);
    }

    [Fact]
    public async Task Validate_WithoutToken_ReturnsUnauthorized()
    {
        // Act (no authentication)
        var response = await Client.GetAsync("/api/v1/auth/validate");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_ReturnsNewTokens()
    {
        // Arrange - first login to get refresh token
        var loginRequest = new LoginRequest
        {
            TenantCode = TestDataSeeder.TestTenantCode,
            Email = TestDataSeeder.TestUserEmail,
            Password = TestDataSeeder.TestUserPassword
        };

        var loginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);
        var originalRefreshToken = loginResult!.Data!.RefreshToken;

        var refreshRequest = new RefreshRequest
        {
            RefreshToken = originalRefreshToken
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        // Refresh token should be different (rotated)
        result.Data.RefreshToken.Should().NotBe(originalRefreshToken);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidRefreshToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshRequest
        {
            RefreshToken = "invalid-refresh-token"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidToken_InvalidatesRefreshToken()
    {
        // Arrange - login first
        var loginRequest = new LoginRequest
        {
            TenantCode = TestDataSeeder.TestTenantCode,
            Email = TestDataSeeder.TestUserEmail,
            Password = TestDataSeeder.TestUserPassword
        };

        var loginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);
        var token = loginResult!.Data!.Token;
        var refreshToken = loginResult.Data.RefreshToken;

        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - logout
        var logoutResponse = await Client.PostAsync("/api/v1/auth/logout", null);

        // Assert - logout should succeed
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Try to use the old refresh token - should fail
        var refreshRequest = new RefreshRequest { RefreshToken = refreshToken };
        var refreshResponse = await Client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Change Password Tests

    [Fact]
    public async Task ChangePassword_WithValidCurrentPassword_Succeeds()
    {
        // Arrange
        await AuthenticateAsync();

        var request = new ChangePasswordRequest
        {
            CurrentPassword = TestDataSeeder.TestUserPassword,
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/v1/auth/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse>(JsonOptions);
        result!.Success.Should().BeTrue();

        // Verify old password no longer works
        Client.DefaultRequestHeaders.Authorization = null;
        var loginWithOldPassword = new LoginRequest
        {
            TenantCode = TestDataSeeder.TestTenantCode,
            Email = TestDataSeeder.TestUserEmail,
            Password = TestDataSeeder.TestUserPassword
        };
        var oldPasswordResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", loginWithOldPassword);
        oldPasswordResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Restore original password for other tests
        var loginWithNewPassword = new LoginRequest
        {
            TenantCode = TestDataSeeder.TestTenantCode,
            Email = TestDataSeeder.TestUserEmail,
            Password = "NewPassword123!"
        };
        var newLoginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", loginWithNewPassword);
        var newLoginResult = await newLoginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newLoginResult!.Data!.Token);

        var restoreRequest = new ChangePasswordRequest
        {
            CurrentPassword = "NewPassword123!",
            NewPassword = TestDataSeeder.TestUserPassword
        };
        await Client.PutAsJsonAsync("/api/v1/auth/change-password", restoreRequest);
    }

    [Fact]
    public async Task ChangePassword_WithInvalidCurrentPassword_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "WrongPassword123!",
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/v1/auth/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
