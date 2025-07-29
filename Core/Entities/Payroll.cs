using System.ComponentModel.DataAnnotations;
using PayrollManagement.API.Core.Enums;

namespace PayrollManagement.API.Core.Entities;

public class Payroll
{
    public int Id { get; set; }
    
    public int EmployeeId { get; set; }
    
    public DateTime PayPeriodStart { get; set; }
    
    public DateTime PayPeriodEnd { get; set; }
    
    [Range(1, 12)]
    public int PayPeriodMonth { get; set; }
    
    [Range(2000, 3000)]
    public int PayPeriodYear { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal BaseSalary { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal Overtime { get; set; } = 0;
    
    [Range(0, double.MaxValue)]
    public decimal Bonus { get; set; } = 0;
    
    [Range(0, double.MaxValue)]
    public decimal Allowances { get; set; } = 0;
    
    [Range(0, double.MaxValue)]
    public decimal Deductions { get; set; } = 0;
    
    [Range(0, double.MaxValue)]
    public decimal TaxDeduction { get; set; } = 0;
    
    [Range(0, double.MaxValue)]
    public decimal TotalDeductions { get; set; } = 0;
    
    [Range(0, double.MaxValue)]
    public decimal GrossPay { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal NetPay { get; set; }
    
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    
    public DateTime? ProcessedDate { get; set; }
    
    public DateTime? PaidDate { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual ICollection<PayrollItem> PayrollItems { get; set; } = new List<PayrollItem>();
    
    // Business methods
    public void CalculateGrossPay()
    {
        GrossPay = BaseSalary + Overtime + Bonus + Allowances;
    }
    
    public void CalculateNetPay()
    {
        NetPay = GrossPay - Deductions - TaxDeduction;
    }
    
    public void ProcessPayroll()
    {
        if (Status != PayrollStatus.Draft)
            throw new InvalidOperationException("Only draft payrolls can be processed");
            
        CalculateGrossPay();
        CalculateNetPay();
        Status = PayrollStatus.Processed;
        ProcessedDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

