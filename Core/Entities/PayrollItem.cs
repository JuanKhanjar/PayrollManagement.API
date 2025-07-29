using System.ComponentModel.DataAnnotations;

namespace PayrollManagement.API.Core.Entities;

public class PayrollItem
{
    public int Id { get; set; }
    
    public int PayrollId { get; set; }
    
    public int PayrollItemTypeId { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Description { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }
    
    public bool IsDeduction { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Payroll Payroll { get; set; } = null!;
    public virtual PayrollItemType PayrollItemType { get; set; } = null!;
}

