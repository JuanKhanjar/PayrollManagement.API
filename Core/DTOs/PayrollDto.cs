using System.ComponentModel.DataAnnotations;
using PayrollManagement.API.Core.Enums;

namespace PayrollManagement.API.Core.DTOs;

public class PayrollDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int PayPeriodMonth { get; set; }
    public int PayPeriodYear { get; set; }
    public string PayPeriod { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public decimal Overtime { get; set; }
    public decimal Bonus { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal TaxDeduction { get; set; }
    public decimal GrossPay { get; set; }
    public decimal NetPay { get; set; }
    public PayrollStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? ProcessedDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PayrollItemDto> PayrollItems { get; set; } = new();
}

public class CreatePayrollDto
{
    [Required(ErrorMessage = "Employee is required")]
    public int EmployeeId { get; set; }
    
    [Required(ErrorMessage = "Pay period month is required")]
    [Range(1, 12, ErrorMessage = "Pay period month must be between 1 and 12")]
    public int PayPeriodMonth { get; set; }
    
    [Required(ErrorMessage = "Pay period year is required")]
    [Range(2000, 3000, ErrorMessage = "Pay period year must be between 2000 and 3000")]
    public int PayPeriodYear { get; set; }
    
    [Required(ErrorMessage = "Base salary is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Base salary must be a positive number")]
    public decimal BaseSalary { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Overtime must be a positive number")]
    public decimal Overtime { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Bonus must be a positive number")]
    public decimal Bonus { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Allowances must be a positive number")]
    public decimal Allowances { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Deductions must be a positive number")]
    public decimal Deductions { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Tax deduction must be a positive number")]
    public decimal TaxDeduction { get; set; } = 0;
    
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
    
    public List<CreatePayrollItemDto> PayrollItems { get; set; } = new();
}

public class UpdatePayrollDto
{
    [Required(ErrorMessage = "Base salary is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Base salary must be a positive number")]
    public decimal BaseSalary { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Overtime must be a positive number")]
    public decimal Overtime { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Bonus must be a positive number")]
    public decimal Bonus { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Allowances must be a positive number")]
    public decimal Allowances { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Deductions must be a positive number")]
    public decimal Deductions { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Tax deduction must be a positive number")]
    public decimal TaxDeduction { get; set; }
    
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
    
    public List<CreatePayrollItemDto> PayrollItems { get; set; } = new();
}

public class PayrollSearchDto
{
    public int? EmployeeId { get; set; }
    public int? PayPeriodMonth { get; set; }
    public int? PayPeriodYear { get; set; }
    public PayrollStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PayrollItemDto
{
    public int Id { get; set; }
    public int PayrollItemTypeId { get; set; }
    public string PayrollItemTypeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsDeduction { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePayrollItemDto
{
    [Required(ErrorMessage = "Payroll item type is required")]
    public int PayrollItemTypeId { get; set; }
    
    [Required(ErrorMessage = "Description is required")]
    [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Amount is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive number")]
    public decimal Amount { get; set; }
    
    public bool IsDeduction { get; set; }
}

