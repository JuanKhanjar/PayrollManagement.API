using PayrollManagement.API.Core.DTOs;

namespace PayrollManagement.API.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<ApiResponse<bool>> LogoutAsync(string userId);
    Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
    Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<ApiResponse<UserInfoDto>> GetUserProfileAsync(string userId);
    Task<ApiResponse<UserInfoDto>> UpdateUserProfileAsync(string userId, UpdateProfileDto updateProfileDto);
    Task<ApiResponse<bool>> ConfirmEmailAsync(string userId, string token);
    Task<ApiResponse<bool>> ResendEmailConfirmationAsync(string email);
}

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(string userId);
    Task<string> GenerateRefreshTokenAsync(string userId);
    Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken);
    Task RevokeRefreshTokenAsync(string userId, string refreshToken);
    Task RevokeAllRefreshTokensAsync(string userId);
    string? GetUserIdFromToken(string token);
    bool IsTokenExpired(string token);
}

