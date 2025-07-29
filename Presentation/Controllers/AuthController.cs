using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollManagement.API.Core.DTOs;
using PayrollManagement.API.Core.Interfaces;
using System.Security.Claims;

namespace PayrollManagement.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// User login
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        _logger.LogInformation("POST /api/auth/login - Login attempt for: {Email}", loginDto.Email);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Invalid model state for login: {Errors}", string.Join(", ", errors));
            
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Message = "Invalid input data",
                Errors = errors
            });
        }

        var result = await _authService.LoginAsync(loginDto);

        if (result.Success)
        {
            _logger.LogInformation("Login successful for user: {Email}", loginDto.Email);
            return Ok(result);
        }

        _logger.LogWarning("Login failed for user: {Email} - {Message}", loginDto.Email, result.Message);
        return Unauthorized(result);
    }

    /// <summary>
    /// User registration
    /// </summary>
    /// <param name="registerDto">Registration data</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        _logger.LogInformation("POST /api/auth/register - Registration attempt for: {Email}", registerDto.Email);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Invalid model state for registration: {Errors}", string.Join(", ", errors));
            
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Message = "Invalid input data",
                Errors = errors
            });
        }

        var result = await _authService.RegisterAsync(registerDto);

        if (result.Success)
        {
            _logger.LogInformation("Registration successful for user: {Email}", registerDto.Email);
            return CreatedAtAction(nameof(GetProfile), null, result);
        }

        _logger.LogWarning("Registration failed for user: {Email} - {Message}", registerDto.Email, result.Message);
        return BadRequest(result);
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    /// <param name="refreshTokenDto">Token refresh data</param>
    /// <returns>New authentication tokens</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        _logger.LogInformation("POST /api/auth/refresh-token - Token refresh attempt");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Invalid model state for token refresh: {Errors}", string.Join(", ", errors));
            
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Message = "Invalid input data",
                Errors = errors
            });
        }

        var result = await _authService.RefreshTokenAsync(refreshTokenDto);

        if (result.Success)
        {
            _logger.LogInformation("Token refresh successful");
            return Ok(result);
        }

        _logger.LogWarning("Token refresh failed: {Message}", result.Message);
        return Unauthorized(result);
    }

    /// <summary>
    /// User logout
    /// </summary>
    /// <returns>Logout confirmation</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Logout attempt without valid user ID");
            return Unauthorized(ApiResponse<bool>.ErrorResponse("Invalid user session"));
        }

        _logger.LogInformation("POST /api/auth/logout - Logout attempt for user: {UserId}", userId);

        var result = await _authService.LogoutAsync(userId);

        if (result.Success)
        {
            _logger.LogInformation("Logout successful for user: {UserId}", userId);
            return Ok(result);
        }

        _logger.LogWarning("Logout failed for user: {UserId} - {Message}", userId, result.Message);
        return BadRequest(result);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="changePasswordDto">Password change data</param>
    /// <returns>Password change confirmation</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Change password attempt without valid user ID");
            return Unauthorized(ApiResponse<bool>.ErrorResponse("Invalid user session"));
        }

        _logger.LogInformation("POST /api/auth/change-password - Password change attempt for user: {UserId}", userId);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Invalid model state for password change: {Errors}", string.Join(", ", errors));
            return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid input data", errors));
        }

        var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

        if (result.Success)
        {
            _logger.LogInformation("Password change successful for user: {UserId}", userId);
            return Ok(result);
        }

        _logger.LogWarning("Password change failed for user: {UserId} - {Message}", userId, result.Message);
        return BadRequest(result);
    }

    /// <summary>
    /// Forgot password request
    /// </summary>
    /// <param name="forgotPasswordDto">Email for password reset</param>
    /// <returns>Password reset confirmation</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        _logger.LogInformation("POST /api/auth/forgot-password - Password reset request for: {Email}", forgotPasswordDto.Email);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Invalid model state for forgot password: {Errors}", string.Join(", ", errors));
            return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid input data", errors));
        }

        var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);

        _logger.LogInformation("Forgot password request processed for: {Email}", forgotPasswordDto.Email);
        return Ok(result);
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    /// <param name="resetPasswordDto">Password reset data</param>
    /// <returns>Password reset confirmation</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        _logger.LogInformation("POST /api/auth/reset-password - Password reset attempt for: {Email}", resetPasswordDto.Email);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Invalid model state for password reset: {Errors}", string.Join(", ", errors));
            return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid input data", errors));
        }

        var result = await _authService.ResetPasswordAsync(resetPasswordDto);

        if (result.Success)
        {
            _logger.LogInformation("Password reset successful for: {Email}", resetPasswordDto.Email);
            return Ok(result);
        }

        _logger.LogWarning("Password reset failed for: {Email} - {Message}", resetPasswordDto.Email, result.Message);
        return BadRequest(result);
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <returns>User profile information</returns>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserInfoDto>>> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Get profile attempt without valid user ID");
            return Unauthorized(ApiResponse<UserInfoDto>.ErrorResponse("Invalid user session"));
        }

        _logger.LogInformation("GET /api/auth/profile - Profile request for user: {UserId}", userId);

        var result = await _authService.GetUserProfileAsync(userId);

        if (result.Success)
        {
            _logger.LogInformation("Profile retrieved successfully for user: {UserId}", userId);
            return Ok(result);
        }

        _logger.LogWarning("Profile retrieval failed for user: {UserId} - {Message}", userId, result.Message);
        return BadRequest(result);
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    /// <param name="updateProfileDto">Profile update data</param>
    /// <returns>Updated user profile</returns>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserInfoDto>>> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Update profile attempt without valid user ID");
            return Unauthorized(ApiResponse<UserInfoDto>.ErrorResponse("Invalid user session"));
        }

        _logger.LogInformation("PUT /api/auth/profile - Profile update attempt for user: {UserId}", userId);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Invalid model state for profile update: {Errors}", string.Join(", ", errors));
            return BadRequest(ApiResponse<UserInfoDto>.ErrorResponse("Invalid input data", errors));
        }

        var result = await _authService.UpdateUserProfileAsync(userId, updateProfileDto);

        if (result.Success)
        {
            _logger.LogInformation("Profile update successful for user: {UserId}", userId);
            return Ok(result);
        }

        _logger.LogWarning("Profile update failed for user: {UserId} - {Message}", userId, result.Message);
        return BadRequest(result);
    }

    /// <summary>
    /// Confirm email address
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="token">Email confirmation token</param>
    /// <returns>Email confirmation result</returns>
    [HttpGet("confirm-email")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        _logger.LogInformation("GET /api/auth/confirm-email - Email confirmation attempt for user: {UserId}", userId);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Email confirmation attempt with missing parameters");
            return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid confirmation parameters"));
        }

        var result = await _authService.ConfirmEmailAsync(userId, token);

        if (result.Success)
        {
            _logger.LogInformation("Email confirmation successful for user: {UserId}", userId);
            return Ok(result);
        }

        _logger.LogWarning("Email confirmation failed for user: {UserId} - {Message}", userId, result.Message);
        return BadRequest(result);
    }

    /// <summary>
    /// Resend email confirmation
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>Resend confirmation result</returns>
    [HttpPost("resend-email-confirmation")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> ResendEmailConfirmation([FromBody] string email)
    {
        _logger.LogInformation("POST /api/auth/resend-email-confirmation - Resend confirmation for: {Email}", email);

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Resend email confirmation attempt with empty email");
            return BadRequest(ApiResponse<bool>.ErrorResponse("Email is required"));
        }

        var result = await _authService.ResendEmailConfirmationAsync(email);

        _logger.LogInformation("Resend email confirmation processed for: {Email}", email);
        return Ok(result);
    }
}

