namespace PayrollManagement.API.Core.Entities;

public class UserRefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    
    // Computed property
    public bool IsActive => !IsUsed && !IsRevoked && DateTime.UtcNow < ExpiryDate;
}

