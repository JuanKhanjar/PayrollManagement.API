using System.ComponentModel.DataAnnotations;
using PayrollManagement.API.Core.Enums;

namespace PayrollManagement.API.Core.DTOs;

public class EmployeeDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public decimal BaseSalary { get; set; }
    public EmployeeStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateEmployeeDto
{
    [Required(ErrorMessage = "Employee code is required")]
    [StringLength(50, ErrorMessage = "Employee code cannot exceed 50 characters")]
    public string EmployeeCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? PhoneNumber { get; set; }
    
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }
    
    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }
    
    [Required(ErrorMessage = "Hire date is required")]
    public DateTime HireDate { get; set; }
    
    [Required(ErrorMessage = "Base salary is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Base salary must be a positive number")]
    public decimal BaseSalary { get; set; }
    
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    
    [Required(ErrorMessage = "Department is required")]
    public int DepartmentId { get; set; }
}

public class UpdateEmployeeDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? PhoneNumber { get; set; }
    
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }
    
    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }
    
    [Required(ErrorMessage = "Hire date is required")]
    public DateTime HireDate { get; set; }
    
    [Required(ErrorMessage = "Base salary is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Base salary must be a positive number")]
    public decimal BaseSalary { get; set; }
    
    public EmployeeStatus Status { get; set; }
    
    [Required(ErrorMessage = "Department is required")]
    public int DepartmentId { get; set; }
}

public class EmployeeSearchDto
{
    public string? SearchTerm { get; set; }
    public int? DepartmentId { get; set; }
    public EmployeeStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

