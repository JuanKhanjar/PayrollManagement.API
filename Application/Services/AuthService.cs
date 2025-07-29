using Microsoft.AspNetCore.Identity;
using PayrollManagement.API.Core.DTOs;
using PayrollManagement.API.Core.Entities;
using PayrollManagement.API.Core.Interfaces;

namespace PayrollManagement.API.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService, ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Email}", loginDto.Email);

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed - user not found: {Email}", loginDto.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Errors = new List<string> { "Invalid credentials" }
                };
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed - user account is inactive: {Email}", loginDto.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Account is inactive",
                    Errors = new List<string> { "Your account has been deactivated" }
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);
            
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Login failed - account locked out: {Email}", loginDto.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Account is locked out",
                    Errors = new List<string> { "Account is temporarily locked due to multiple failed login attempts" }
                };
            }

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed - invalid password: {Email}", loginDto.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Errors = new List<string> { "Invalid credentials" }
                };
            }

            // Generate tokens
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user.Id);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);
            var roles = await _userManager.GetRolesAsync(user);

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Login successful for user: {Email}", loginDto.Email);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = accessToken,
                RefreshToken = refreshToken,
                TokenExpiration = DateTime.UtcNow.AddMinutes(60), // Should match JWT expiration
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList(),
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Email}", loginDto.Email);
            return new AuthResponseDto
            {
                Success = false,
                Message = "An error occurred during login",
                Errors = new List<string> { "Please try again later" }
            };
        }
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            _logger.LogInformation("Registration attempt for user: {Email}", registerDto.Email);

            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed - email already exists: {Email}", registerDto.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email already registered",
                    Errors = new List<string> { "An account with this email already exists" }
                };
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            
            if (!result.Succeeded)
            {
                _logger.LogWarning("Registration failed for user {Email}: {Errors}", 
                    registerDto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Registration failed",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, "Employee");

            // Generate email confirmation token
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            // In a real application, you would send an email here
            _logger.LogInformation("Registration successful for user: {Email}. Email confirmation token: {Token}", 
                registerDto.Email, emailToken);

            // Generate tokens for immediate login
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user.Id);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful",
                Token = accessToken,
                RefreshToken = refreshToken,
                TokenExpiration = DateTime.UtcNow.AddMinutes(60),
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList(),
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {Email}", registerDto.Email);
            return new AuthResponseDto
            {
                Success = false,
                Message = "An error occurred during registration",
                Errors = new List<string> { "Please try again later" }
            };
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        try
        {
            var userId = _tokenService.GetUserIdFromToken(refreshTokenDto.Token);
            if (string.IsNullOrEmpty(userId))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid token",
                    Errors = new List<string> { "Unable to extract user information from token" }
                };
            }

            var isValidRefreshToken = await _tokenService.ValidateRefreshTokenAsync(userId, refreshTokenDto.RefreshToken);
            if (!isValidRefreshToken)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid refresh token",
                    Errors = new List<string> { "Refresh token is invalid or expired" }
                };
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "User not found or inactive",
                    Errors = new List<string> { "User account is not available" }
                };
            }

            // Revoke the old refresh token
            await _tokenService.RevokeRefreshTokenAsync(userId, refreshTokenDto.RefreshToken);

            // Generate new tokens
            var newAccessToken = await _tokenService.GenerateAccessTokenAsync(userId);
            var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(userId);
            var roles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation("Token refresh successful for user: {UserId}", userId);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Token refreshed successfully",
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                TokenExpiration = DateTime.UtcNow.AddMinutes(60),
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList(),
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return new AuthResponseDto
            {
                Success = false,
                Message = "An error occurred during token refresh",
                Errors = new List<string> { "Please login again" }
            };
        }
    }

    public async Task<ApiResponse<bool>> LogoutAsync(string userId)
    {
        try
        {
            await _tokenService.RevokeAllRefreshTokensAsync(userId);
            _logger.LogInformation("Logout successful for user: {UserId}", userId);
            
            return ApiResponse<bool>.SuccessResponse(true, "Logout successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
            return ApiResponse<bool>.ErrorResponse("An error occurred during logout");
        }
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.ErrorResponse("User not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            
            if (!result.Succeeded)
            {
                return ApiResponse<bool>.ErrorResponse("Password change failed", result.Errors.Select(e => e.Description).ToList());
            }

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
            return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return ApiResponse<bool>.ErrorResponse("An error occurred while changing password");
        }
    }

    public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return ApiResponse<bool>.SuccessResponse(true, "If the email exists, a password reset link has been sent");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // In a real application, you would send an email here
            _logger.LogInformation("Password reset token generated for user: {Email}. Token: {Token}", 
                forgotPasswordDto.Email, token);

            return ApiResponse<bool>.SuccessResponse(true, "If the email exists, a password reset link has been sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password for email: {Email}", forgotPasswordDto.Email);
            return ApiResponse<bool>.ErrorResponse("An error occurred while processing your request");
        }
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                return ApiResponse<bool>.ErrorResponse("Invalid reset request");
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            
            if (!result.Succeeded)
            {
                return ApiResponse<bool>.ErrorResponse("Password reset failed", result.Errors.Select(e => e.Description).ToList());
            }

            _logger.LogInformation("Password reset successful for user: {Email}", resetPasswordDto.Email);
            return ApiResponse<bool>.SuccessResponse(true, "Password reset successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for email: {Email}", resetPasswordDto.Email);
            return ApiResponse<bool>.ErrorResponse("An error occurred while resetting password");
        }
    }

    public async Task<ApiResponse<UserInfoDto>> GetUserProfileAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserInfoDto>.ErrorResponse("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            var userInfo = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return ApiResponse<UserInfoDto>.SuccessResponse(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile for user: {UserId}", userId);
            return ApiResponse<UserInfoDto>.ErrorResponse("An error occurred while retrieving user profile");
        }
    }

    public async Task<ApiResponse<UserInfoDto>> UpdateUserProfileAsync(string userId, UpdateProfileDto updateProfileDto)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserInfoDto>.ErrorResponse("User not found");
            }

            user.FirstName = updateProfileDto.FirstName;
            user.LastName = updateProfileDto.LastName;
            user.PhoneNumber = updateProfileDto.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            
            if (!result.Succeeded)
            {
                return ApiResponse<UserInfoDto>.ErrorResponse("Profile update failed", result.Errors.Select(e => e.Description).ToList());
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            var userInfo = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            _logger.LogInformation("Profile updated successfully for user: {UserId}", userId);
            return ApiResponse<UserInfoDto>.SuccessResponse(userInfo, "Profile updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile for user: {UserId}", userId);
            return ApiResponse<UserInfoDto>.ErrorResponse("An error occurred while updating profile");
        }
    }

    public async Task<ApiResponse<bool>> ConfirmEmailAsync(string userId, string token)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.ErrorResponse("User not found");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            
            if (!result.Succeeded)
            {
                return ApiResponse<bool>.ErrorResponse("Email confirmation failed", result.Errors.Select(e => e.Description).ToList());
            }

            _logger.LogInformation("Email confirmed successfully for user: {UserId}", userId);
            return ApiResponse<bool>.SuccessResponse(true, "Email confirmed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for user: {UserId}", userId);
            return ApiResponse<bool>.ErrorResponse("An error occurred while confirming email");
        }
    }

    public async Task<ApiResponse<bool>> ResendEmailConfirmationAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return ApiResponse<bool>.SuccessResponse(true, "If the email exists, a confirmation link has been sent");
            }

            if (user.EmailConfirmed)
            {
                return ApiResponse<bool>.ErrorResponse("Email is already confirmed");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            // In a real application, you would send an email here
            _logger.LogInformation("Email confirmation token generated for user: {Email}. Token: {Token}", 
                email, token);

            return ApiResponse<bool>.SuccessResponse(true, "If the email exists, a confirmation link has been sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email confirmation for email: {Email}", email);
            return ApiResponse<bool>.ErrorResponse("An error occurred while processing your request");
        }
    }
}

