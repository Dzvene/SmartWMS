using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Auth.DTOs;

namespace SmartWMS.API.Modules.Auth.Services;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse> LogoutAsync(Guid userId);
    Task<ApiResponse<LoginResponse>> RefreshTokenAsync(RefreshRequest request);
    Task<ApiResponse<UserInfo>> ValidateTokenAsync();
    Task<ApiResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}
