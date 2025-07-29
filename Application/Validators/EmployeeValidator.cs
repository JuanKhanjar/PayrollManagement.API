using PayrollManagement.API.Core.DTOs;
using PayrollManagement.API.Core.Interfaces;
using System.Text.RegularExpressions;

namespace PayrollManagement.API.Application.Validators;

public class EmployeeValidator
{
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ValidationResult> ValidateCreateEmployeeAsync(CreateEmployeeDto dto)
    {
        var errors = new List<string>();

        // Basic validation
        ValidateBasicEmployeeFields(dto.EmployeeCode, dto.FirstName, dto.LastName, dto.Email, 
            dto.PhoneNumber, dto.Address, dto.DateOfBirth, dto.HireDate, dto.BaseSalary, errors);

        // Business rules validation
        if (!string.IsNullOrWhiteSpace(dto.EmployeeCode))
        {
            var existingByCode = await _unitOfWork.EmployeeRepository.GetByEmployeeCodeAsync(dto.EmployeeCode);
            if (existingByCode != null)
                errors.Add("An employee with this employee code already exists");
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var existingByEmail = await _unitOfWork.EmployeeRepository.GetByEmailAsync(dto.Email);
            if (existingByEmail != null)
                errors.Add("An employee with this email already exists");
        }

        // Department validation
        if (dto.DepartmentId > 0)
        {
            var departmentExists = await _unitOfWork.Departments.ExistsAsync(d => d.Id == dto.DepartmentId && d.IsActive);
            if (!departmentExists)
                errors.Add("Selected department does not exist or is inactive");
        }
        else
        {
            errors.Add("Department is required");
        }

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateUpdateEmployeeAsync(int id, UpdateEmployeeDto dto)
    {
        var errors = new List<string>();

        // Check if employee exists
        var existingEmployee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (existingEmployee == null)
        {
            errors.Add("Employee not found");
            return ValidationResult.Failure(errors);
        }

        // Basic validation (excluding employee code as it's not updatable)
        ValidateBasicEmployeeFields(null, dto.FirstName, dto.LastName, dto.Email, 
            dto.PhoneNumber, dto.Address, dto.DateOfBirth, dto.HireDate, dto.BaseSalary, errors);

        // Business rules validation
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var existingByEmail = await _unitOfWork.EmployeeRepository.GetByEmailAsync(dto.Email);
            if (existingByEmail != null && existingByEmail.Id != id)
                errors.Add("An employee with this email already exists");
        }

        // Department validation
        if (dto.DepartmentId > 0)
        {
            var departmentExists = await _unitOfWork.Departments.ExistsAsync(d => d.Id == dto.DepartmentId && d.IsActive);
            if (!departmentExists)
                errors.Add("Selected department does not exist or is inactive");
        }
        else
        {
            errors.Add("Department is required");
        }

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateDeleteEmployeeAsync(int id)
    {
        var errors = new List<string>();

        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee == null)
        {
            errors.Add("Employee not found");
            return ValidationResult.Failure(errors);
        }

        // Check if employee has processed payrolls
        var hasProcessedPayrolls = await _unitOfWork.PayrollRepository.ExistsAsync(p => 
            p.EmployeeId == id && 
            (p.Status == Core.Enums.PayrollStatus.Processed || p.Status == Core.Enums.PayrollStatus.Paid));
        
        if (hasProcessedPayrolls)
            errors.Add("Cannot delete employee with processed payrolls. Consider marking as inactive instead.");

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    private static void ValidateBasicEmployeeFields(string? employeeCode, string firstName, string lastName, 
        string email, string? phoneNumber, string? address, DateTime dateOfBirth, DateTime hireDate, 
        decimal baseSalary, List<string> errors)
    {
        // Employee code validation (only for create)
        if (employeeCode != null)
        {
            if (string.IsNullOrWhiteSpace(employeeCode))
                errors.Add("Employee code is required");
            else if (employeeCode.Length > 50)
                errors.Add("Employee code cannot exceed 50 characters");
        }

        // Name validation
        if (string.IsNullOrWhiteSpace(firstName))
            errors.Add("First name is required");
        else if (firstName.Length > 100)
            errors.Add("First name cannot exceed 100 characters");

        if (string.IsNullOrWhiteSpace(lastName))
            errors.Add("Last name is required");
        else if (lastName.Length > 100)
            errors.Add("Last name cannot exceed 100 characters");

        // Email validation
        if (string.IsNullOrWhiteSpace(email))
            errors.Add("Email is required");
        else if (email.Length > 255)
            errors.Add("Email cannot exceed 255 characters");
        else if (!IsValidEmail(email))
            errors.Add("Invalid email format");

        // Phone number validation
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            if (phoneNumber.Length > 20)
                errors.Add("Phone number cannot exceed 20 characters");
            else if (!IsValidPhoneNumber(phoneNumber))
                errors.Add("Invalid phone number format");
        }

        // Address validation
        if (!string.IsNullOrEmpty(address) && address.Length > 500)
            errors.Add("Address cannot exceed 500 characters");

        // Date validation
        if (dateOfBirth == default)
            errors.Add("Date of birth is required");
        else if (dateOfBirth > DateTime.Today.AddYears(-16))
            errors.Add("Employee must be at least 16 years old");
        else if (dateOfBirth < DateTime.Today.AddYears(-100))
            errors.Add("Invalid date of birth");

        if (hireDate == default)
            errors.Add("Hire date is required");
        else if (hireDate > DateTime.Today)
            errors.Add("Hire date cannot be in the future");
        else if (hireDate < dateOfBirth)
            errors.Add("Hire date cannot be before date of birth");

        // Salary validation
        if (baseSalary <= 0)
            errors.Add("Base salary must be greater than zero");
        else if (baseSalary > 10000000) // 10 million limit
            errors.Add("Base salary cannot exceed 10,000,000");
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return emailRegex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        try
        {
            var phoneRegex = new Regex(@"^[\+]?[1-9][\d]{0,15}$");
            var cleanPhone = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            return phoneRegex.IsMatch(cleanPhone);
        }
        catch
        {
            return false;
        }
    }
}

