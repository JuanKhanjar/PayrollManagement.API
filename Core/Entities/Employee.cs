using System.ComponentModel.DataAnnotations;
using PayrollManagement.API.Core.Enums;

namespace PayrollManagement.API.Core.Entities;

public class Employee
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string EmployeeNumber { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Position { get; set; } = string.Empty;
    
    public DateTime DateOfBirth { get; set; }
    
    public DateTime HireDate { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal BaseSalary { get; set; }
    
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    
    public int DepartmentId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Department Department { get; set; } = null!;
    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
    
    // Computed property
    public string FullName => $"{FirstName} {LastName}";
}

