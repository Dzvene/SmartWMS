using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Infrastructure.Identity;
using SmartWMS.API.Modules.Auth.DTOs;
using SmartWMS.API.Modules.Auth.Services;
using SmartWMS.API.Modules.Companies.Models;

namespace SmartWMS.API.Tests.Unit.Auth;

/// <summary>
/// Unit tests for AuthService
/// </summary>
public class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly JwtSettings _jwtSettings;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        // Setup UserManager mock
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        // Setup HttpContextAccessor mock
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        // Setup JWT settings
        _jwtSettings = new JwtSettings
        {
            SecretKey = "ThisIsAVeryLongSecretKeyForTestingPurposes123456789!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        };

        var jwtOptions = Options.Create(_jwtSettings);

        _authService = new AuthService(
            _userManagerMock.Object,
            _context,
            jwtOptions,
            _httpContextAccessorMock.Object);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var tenantId = Guid.NewGuid();

        _context.Companies.Add(new Company
        {
            Id = tenantId,
            Code = "TEST",
            Name = "Test Company",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Login Tests

    [Fact]
    public async Task LoginAsync_WithInvalidTenantCode_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            TenantCode = "INVALID",
            Email = "test@example.com",
            Password = "Test123!"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid tenant code");
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            TenantCode = "TEST",
            Email = "nonexistent@example.com",
            Password = "Test123!"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ReturnsFailure()
    {
        // Arrange
        var tenant = await _context.Companies.FirstAsync(c => c.Code == "TEST");
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "inactive@example.com",
            UserName = "inactive@example.com",
            FirstName = "Inactive",
            LastName = "User",
            TenantId = tenant.Id,
            IsActive = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            TenantCode = "TEST",
            Email = "inactive@example.com",
            Password = "Test123!"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
    {
        // Arrange
        var tenant = await _context.Companies.FirstAsync(c => c.Code == "TEST");
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "wrongpwd@example.com",
            UserName = "wrongpwd@example.com",
            FirstName = "Wrong",
            LastName = "Password",
            TenantId = tenant.Id,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var request = new LoginRequest
        {
            TenantCode = "TEST",
            Email = "wrongpwd@example.com",
            Password = "WrongPassword!"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var tenant = await _context.Companies.FirstAsync(c => c.Code == "TEST");
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "valid@example.com",
            UserName = "valid@example.com",
            FirstName = "Valid",
            LastName = "User",
            TenantId = tenant.Id,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(It.Is<ApplicationUser>(u => u.Email == "valid@example.com"), "Test123!"))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "Admin" });

        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var request = new LoginRequest
        {
            TenantCode = "TEST",
            Email = "valid@example.com",
            Password = "Test123!"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.User.Should().NotBeNull();
        result.Data.User!.Email.Should().Be("valid@example.com");
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        var request = new RefreshRequest
        {
            RefreshToken = "invalid-refresh-token"
        };

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid or expired refresh token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ReturnsFailure()
    {
        // Arrange
        var tenant = await _context.Companies.FirstAsync(c => c.Code == "TEST");
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "expired@example.com",
            UserName = "expired@example.com",
            FirstName = "Expired",
            LastName = "Token",
            TenantId = tenant.Id,
            IsActive = true,
            RefreshToken = "expired-refresh-token",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(-1) // Expired
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new RefreshRequest
        {
            RefreshToken = "expired-refresh-token"
        };

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid or expired refresh token");
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task LogoutAsync_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(nonExistentUserId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _authService.LogoutAsync(nonExistentUserId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("User not found");
    }

    [Fact]
    public async Task LogoutAsync_WithValidUser_ClearsRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "logout@example.com",
            UserName = "logout@example.com",
            FirstName = "Logout",
            LastName = "User",
            RefreshToken = "some-refresh-token",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.LogoutAsync(userId);

        // Assert
        result.Success.Should().BeTrue();
        user.RefreshToken.Should().BeNull();
        user.RefreshTokenExpiresAt.Should().BeNull();
    }

    #endregion

    #region ChangePassword Tests

    [Fact]
    public async Task ChangePasswordAsync_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(nonExistentUserId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Old123!",
            NewPassword = "New123!"
        };

        // Act
        var result = await _authService.ChangePasswordAsync(nonExistentUserId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("User not found");
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "changepwd@example.com",
            UserName = "changepwd@example.com",
            FirstName = "Change",
            LastName = "Password"
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.ChangePasswordAsync(user, "Old123!", "New123!"))
            .ReturnsAsync(IdentityResult.Success);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Old123!",
            NewPassword = "New123!"
        };

        // Act
        var result = await _authService.ChangePasswordAsync(userId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Password changed successfully");
    }

    [Fact]
    public async Task ChangePasswordAsync_WithWrongCurrentPassword_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "wrongcurrent@example.com",
            UserName = "wrongcurrent@example.com",
            FirstName = "Wrong",
            LastName = "Current"
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.ChangePasswordAsync(user, "Wrong123!", "New123!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Incorrect password" }));

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Wrong123!",
            NewPassword = "New123!"
        };

        // Act
        var result = await _authService.ChangePasswordAsync(userId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Password change failed");
    }

    #endregion
}
