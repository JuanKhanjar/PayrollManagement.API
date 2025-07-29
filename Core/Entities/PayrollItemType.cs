using System.ComponentModel.DataAnnotations;

namespace PayrollManagement.API.Core.Entities;

public class PayrollItemType
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsEarning { get; set; } = true;
    
    public bool IsDeduction { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<PayrollItem> PayrollItems { get; set; } = new List<PayrollItem>();
}

