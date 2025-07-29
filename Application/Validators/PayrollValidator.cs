using PayrollManagement.API.Core.DTOs;
using PayrollManagement.API.Core.Enums;
using PayrollManagement.API.Core.Interfaces;

namespace PayrollManagement.API.Application.Validators;

public class PayrollValidator
{
    private readonly IUnitOfWork _unitOfWork;

    public PayrollValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ValidationResult> ValidateCreatePayrollAsync(CreatePayrollDto dto)
    {
        var errors = new List<string>();

        // Basic validation
        ValidateBasicPayrollFields(dto.EmployeeId, dto.PayPeriodMonth, dto.PayPeriodYear, 
            dto.BaseSalary, dto.Overtime, dto.Bonus, dto.Allowances, dto.Deductions, 
            dto.TaxDeduction, dto.Notes, errors);

        // Business rules validation
        if (dto.EmployeeId > 0)
        {
            var employee = await _unitOfWork.EmployeeRepository.GetEmployeeWithDepartmentAsync(dto.EmployeeId);
            if (employee == null)
                errors.Add("Selected employee does not exist");
            else if (employee.Status != EmployeeStatus.Active)
                errors.Add("Cannot create payroll for inactive employee");

            // Check for duplicate payroll
            var existingPayroll = await _unitOfWork.PayrollRepository.GetByEmployeeAndPeriodAsync(
                dto.EmployeeId, dto.PayPeriodMonth, dto.PayPeriodYear);
            if (existingPayroll != null)
                errors.Add($"Payroll for this employee already exists for {GetMonthName(dto.PayPeriodMonth)} {dto.PayPeriodYear}");
        }

        // Validate payroll items
        if (dto.PayrollItems?.Any() == true)
        {
            var itemValidation = await ValidatePayrollItemsAsync(dto.PayrollItems);
            if (!itemValidation.IsValid)
                errors.AddRange(itemValidation.Errors);
        }

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateUpdatePayrollAsync(int id, UpdatePayrollDto dto)
    {
        var errors = new List<string>();

        // Check if payroll exists
        var existingPayroll = await _unitOfWork.PayrollRepository.GetPayrollWithEmployeeAsync(id);
        if (existingPayroll == null)
        {
            errors.Add("Payroll not found");
            return ValidationResult.Failure(errors);
        }

        // Check if payroll can be updated
        if (existingPayroll.Status == PayrollStatus.Paid)
        {
            errors.Add("Cannot update paid payroll");
            return ValidationResult.Failure(errors);
        }

        // Basic validation (excluding employee and period as they're not updatable)
        ValidateBasicPayrollFields(0, 0, 0, dto.BaseSalary, dto.Overtime, dto.Bonus, 
            dto.Allowances, dto.Deductions, dto.TaxDeduction, dto.Notes, errors, false);

        // Validate payroll items
        if (dto.PayrollItems?.Any() == true)
        {
            var itemValidation = await ValidatePayrollItemsAsync(dto.PayrollItems);
            if (!itemValidation.IsValid)
                errors.AddRange(itemValidation.Errors);
        }

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateDeletePayrollAsync(int id)
    {
        var errors = new List<string>();

        var payroll = await _unitOfWork.PayrollRepository.GetByIdAsync(id);
        if (payroll == null)
        {
            errors.Add("Payroll not found");
            return ValidationResult.Failure(errors);
        }

        // Check if payroll can be deleted
        if (payroll.Status == PayrollStatus.Processed || payroll.Status == PayrollStatus.Paid)
            errors.Add("Cannot delete processed or paid payroll");

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateProcessPayrollAsync(int id)
    {
        var errors = new List<string>();

        var payroll = await _unitOfWork.PayrollRepository.GetPayrollWithEmployeeAsync(id);
        if (payroll == null)
        {
            errors.Add("Payroll not found");
            return ValidationResult.Failure(errors);
        }

        if (payroll.Status != PayrollStatus.Draft)
            errors.Add("Only draft payrolls can be processed");

        if (payroll.Employee?.Status != EmployeeStatus.Active)
            errors.Add("Cannot process payroll for inactive employee");

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateMarkPayrollAsPaidAsync(int id)
    {
        var errors = new List<string>();

        var payroll = await _unitOfWork.PayrollRepository.GetByIdAsync(id);
        if (payroll == null)
        {
            errors.Add("Payroll not found");
            return ValidationResult.Failure(errors);
        }

        if (payroll.Status != PayrollStatus.Processed)
            errors.Add("Only processed payrolls can be marked as paid");

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    private static void ValidateBasicPayrollFields(int employeeId, int payPeriodMonth, int payPeriodYear,
        decimal baseSalary, decimal overtime, decimal bonus, decimal allowances, decimal deductions,
        decimal taxDeduction, string? notes, List<string> errors, bool validateEmployeeAndPeriod = true)
    {
        if (validateEmployeeAndPeriod)
        {
            if (employeeId <= 0)
                errors.Add("Employee is required");

            if (payPeriodMonth < 1 || payPeriodMonth > 12)
                errors.Add("Pay period month must be between 1 and 12");

            if (payPeriodYear < 2000 || payPeriodYear > 3000)
                errors.Add("Pay period year must be between 2000 and 3000");

            // Don't allow future payrolls beyond next month
            var currentDate = DateTime.Now;
            var payrollDate = new DateTime(payPeriodYear, payPeriodMonth, 1);
            var maxAllowedDate = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(1);
            
            if (payrollDate > maxAllowedDate)
                errors.Add("Cannot create payroll more than one month in advance");
        }

        // Financial validation
        if (baseSalary < 0)
            errors.Add("Base salary cannot be negative");
        else if (baseSalary > 10000000)
            errors.Add("Base salary cannot exceed 10,000,000");

        if (overtime < 0)
            errors.Add("Overtime cannot be negative");
        else if (overtime > 1000000)
            errors.Add("Overtime amount is unreasonably high");

        if (bonus < 0)
            errors.Add("Bonus cannot be negative");
        else if (bonus > 10000000)
            errors.Add("Bonus amount is unreasonably high");

        if (allowances < 0)
            errors.Add("Allowances cannot be negative");
        else if (allowances > 1000000)
            errors.Add("Allowances amount is unreasonably high");

        if (deductions < 0)
            errors.Add("Deductions cannot be negative");

        if (taxDeduction < 0)
            errors.Add("Tax deduction cannot be negative");

        // Calculate totals for validation
        var grossPay = baseSalary + overtime + bonus + allowances;
        var totalDeductions = deductions + taxDeduction;

        if (totalDeductions > grossPay)
            errors.Add("Total deductions cannot exceed gross pay");

        if (!string.IsNullOrEmpty(notes) && notes.Length > 1000)
            errors.Add("Notes cannot exceed 1000 characters");
    }

    private async Task<ValidationResult> ValidatePayrollItemsAsync(List<CreatePayrollItemDto> payrollItems)
    {
        var errors = new List<string>();

        for (int i = 0; i < payrollItems.Count; i++)
        {
            var item = payrollItems[i];
            var prefix = $"Payroll item {i + 1}: ";

            if (item.PayrollItemTypeId <= 0)
                errors.Add($"{prefix}Payroll item type is required");
            else
            {
                var itemType = await _unitOfWork.PayrollItemTypes.GetByIdAsync(item.PayrollItemTypeId);
                if (itemType == null)
                    errors.Add($"{prefix}Invalid payroll item type");
                else if (!itemType.IsActive)
                    errors.Add($"{prefix}Selected payroll item type is inactive");
            }

            if (string.IsNullOrWhiteSpace(item.Description))
                errors.Add($"{prefix}Description is required");
            else if (item.Description.Length > 255)
                errors.Add($"{prefix}Description cannot exceed 255 characters");

            if (item.Amount < 0)
                errors.Add($"{prefix}Amount cannot be negative");
            else if (item.Amount > 1000000)
                errors.Add($"{prefix}Amount is unreasonably high");
        }

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    private static string GetMonthName(int month)
    {
        return month switch
        {
            1 => "January", 2 => "February", 3 => "March", 4 => "April",
            5 => "May", 6 => "June", 7 => "July", 8 => "August",
            9 => "September", 10 => "October", 11 => "November", 12 => "December",
            _ => "Unknown"
        };
    }
}

