using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PayrollManagement.API.Core.Entities;
using PayrollManagement.API.Core.Interfaces;
using PayrollManagement.API.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PayrollManagement.API.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager, 
        ApplicationDbContext context, ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public async Task<string> GenerateAccessTokenAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found", nameof(userId));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new("FirstName", user.FirstName),
                new("LastName", user.LastName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogDebug("Generated access token for user: {UserId}", userId);
            
            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating access token for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<string> GenerateRefreshTokenAsync(string userId)
    {
        try
        {
            var refreshToken = GenerateRandomToken();
            var expirationDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "30");
            
            var userRefreshToken = new Core.Entities.UserRefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(expirationDays),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false,
                IsRevoked = false
            };

            _context.UserRefreshTokens.Add(userRefreshToken);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Generated refresh token for user: {UserId}", userId);
            return refreshToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating refresh token for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
    {
        try
        {
            var userRefreshToken = _context.UserRefreshTokens
                .FirstOrDefault(rt => rt.UserId == userId && 
                                     rt.Token == refreshToken && 
                                     rt.IsActive && 
                                     rt.ExpiryDate > DateTime.UtcNow);

            var isValid = userRefreshToken != null;
            _logger.LogDebug("Refresh token validation for user {UserId}: {IsValid}", userId, isValid);
            
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating refresh token for user: {UserId}", userId);
            return false;
        }
    }

    public async Task RevokeRefreshTokenAsync(string userId, string refreshToken)
    {
        try
        {
            var userRefreshToken = _context.UserRefreshTokens
                .FirstOrDefault(rt => rt.UserId == userId && rt.Token == refreshToken);

            if (userRefreshToken != null)
            {
                userRefreshToken.IsRevoked = true;
                userRefreshToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogDebug("Revoked refresh token for user: {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token for user: {UserId}", userId);
            throw;
        }
    }

    public async Task RevokeAllRefreshTokensAsync(string userId)
    {
        try
        {
            var userRefreshTokens = _context.UserRefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToList();

            foreach (var token in userRefreshTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            _logger.LogDebug("Revoked all refresh tokens for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all refresh tokens for user: {UserId}", userId);
            throw;
        }
    }

    public string? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = false, // We're just extracting the user ID, not validating expiration
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting user ID from token");
            return null;
        }
    }

    public bool IsTokenExpired(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking token expiration");
            return true; // Assume expired if we can't parse it
        }
    }

    private static string GenerateRandomToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

// Entity for storing refresh tokens
public class UserRefreshToken
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}

