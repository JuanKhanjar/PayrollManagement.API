using System.ComponentModel.DataAnnotations;

namespace PayrollManagement.API.Core.DTOs;

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int EmployeeCount { get; set; }
}

public class CreateDepartmentDto
{
    [Required(ErrorMessage = "Department name is required")]
    [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Department code is required")]
    [StringLength(50, ErrorMessage = "Department code cannot exceed 50 characters")]
    public string Code { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
}

public class UpdateDepartmentDto
{
    [Required(ErrorMessage = "Department name is required")]
    [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Department code is required")]
    [StringLength(50, ErrorMessage = "Department code cannot exceed 50 characters")]
    public string Code { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
}

