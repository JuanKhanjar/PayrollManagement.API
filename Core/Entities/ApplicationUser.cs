using Microsoft.AspNetCore.Identity;

namespace PayrollManagement.API.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Computed property
    public string FullName => $"{FirstName} {LastName}".Trim();
}

