using PayrollManagement.API.Core.DTOs;
using PayrollManagement.API.Core.Entities;

namespace PayrollManagement.API.Application.Mappings;

public static class MappingExtensions
{
    // Department mappings
    public static DepartmentDto ToDto(this Department department)
    {
        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            Code = department.Code,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt,
            UpdatedAt = department.UpdatedAt,
            EmployeeCount = department.Employees?.Count ?? 0
        };
    }

    public static Department ToEntity(this CreateDepartmentDto dto)
    {
        return new Department
        {
            Name = dto.Name,
            Description = dto.Description,
            Code = dto.Code,
            IsActive = dto.IsActive
        };
    }

    public static void UpdateEntity(this UpdateDepartmentDto dto, Department department)
    {
        department.Name = dto.Name;
        department.Description = dto.Description;
        department.Code = dto.Code;
        department.IsActive = dto.IsActive;
        department.UpdatedAt = DateTime.UtcNow;
    }

    // Employee mappings
    public static EmployeeDto ToDto(this Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FullName = employee.FullName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            Address = employee.Address,
            DateOfBirth = employee.DateOfBirth,
            HireDate = employee.HireDate,
            BaseSalary = employee.BaseSalary,
            Status = employee.Status,
            StatusName = employee.Status.ToString(),
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name ?? string.Empty,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt
        };
    }

    public static Employee ToEntity(this CreateEmployeeDto dto)
    {
        return new Employee
        {
            EmployeeCode = dto.EmployeeCode,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            DateOfBirth = dto.DateOfBirth,
            HireDate = dto.HireDate,
            BaseSalary = dto.BaseSalary,
            Status = dto.Status,
            DepartmentId = dto.DepartmentId
        };
    }

    public static void UpdateEntity(this UpdateEmployeeDto dto, Employee employee)
    {
        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.PhoneNumber = dto.PhoneNumber;
        employee.Address = dto.Address;
        employee.DateOfBirth = dto.DateOfBirth;
        employee.HireDate = dto.HireDate;
        employee.BaseSalary = dto.BaseSalary;
        employee.Status = dto.Status;
        employee.DepartmentId = dto.DepartmentId;
        employee.UpdatedAt = DateTime.UtcNow;
    }

    // Payroll mappings
    public static PayrollDto ToDto(this Payroll payroll)
    {
        return new PayrollDto
        {
            Id = payroll.Id,
            EmployeeId = payroll.EmployeeId,
            EmployeeName = payroll.Employee?.FullName ?? string.Empty,
            EmployeeCode = payroll.Employee?.EmployeeCode ?? string.Empty,
            DepartmentName = payroll.Employee?.Department?.Name ?? string.Empty,
            PayPeriodMonth = payroll.PayPeriodMonth,
            PayPeriodYear = payroll.PayPeriodYear,
            PayPeriod = $"{GetMonthName(payroll.PayPeriodMonth)} {payroll.PayPeriodYear}",
            BaseSalary = payroll.BaseSalary,
            Overtime = payroll.Overtime,
            Bonus = payroll.Bonus,
            Allowances = payroll.Allowances,
            Deductions = payroll.Deductions,
            TaxDeduction = payroll.TaxDeduction,
            GrossPay = payroll.GrossPay,
            NetPay = payroll.NetPay,
            Status = payroll.Status,
            StatusName = payroll.Status.ToString(),
            ProcessedDate = payroll.ProcessedDate,
            PaidDate = payroll.PaidDate,
            Notes = payroll.Notes,
            CreatedAt = payroll.CreatedAt,
            UpdatedAt = payroll.UpdatedAt,
            PayrollItems = payroll.PayrollItems?.Select(pi => pi.ToDto()).ToList() ?? new List<PayrollItemDto>()
        };
    }

    public static Payroll ToEntity(this CreatePayrollDto dto)
    {
        var payroll = new Payroll
        {
            EmployeeId = dto.EmployeeId,
            PayPeriodMonth = dto.PayPeriodMonth,
            PayPeriodYear = dto.PayPeriodYear,
            BaseSalary = dto.BaseSalary,
            Overtime = dto.Overtime,
            Bonus = dto.Bonus,
            Allowances = dto.Allowances,
            Deductions = dto.Deductions,
            TaxDeduction = dto.TaxDeduction,
            Notes = dto.Notes
        };

        // Calculate gross and net pay
        payroll.CalculateGrossPay();
        payroll.CalculateNetPay();

        return payroll;
    }

    public static void UpdateEntity(this UpdatePayrollDto dto, Payroll payroll)
    {
        payroll.BaseSalary = dto.BaseSalary;
        payroll.Overtime = dto.Overtime;
        payroll.Bonus = dto.Bonus;
        payroll.Allowances = dto.Allowances;
        payroll.Deductions = dto.Deductions;
        payroll.TaxDeduction = dto.TaxDeduction;
        payroll.Notes = dto.Notes;
        payroll.UpdatedAt = DateTime.UtcNow;

        // Recalculate gross and net pay
        payroll.CalculateGrossPay();
        payroll.CalculateNetPay();
    }

    // PayrollItem mappings
    public static PayrollItemDto ToDto(this PayrollItem payrollItem)
    {
        return new PayrollItemDto
        {
            Id = payrollItem.Id,
            PayrollItemTypeId = payrollItem.PayrollItemTypeId,
            PayrollItemTypeName = payrollItem.PayrollItemType?.Name ?? string.Empty,
            Description = payrollItem.Description,
            Amount = payrollItem.Amount,
            IsDeduction = payrollItem.IsDeduction,
            CreatedAt = payrollItem.CreatedAt
        };
    }

    public static PayrollItem ToEntity(this CreatePayrollItemDto dto, int payrollId)
    {
        return new PayrollItem
        {
            PayrollId = payrollId,
            PayrollItemTypeId = dto.PayrollItemTypeId,
            Description = dto.Description,
            Amount = dto.Amount,
            IsDeduction = dto.IsDeduction
        };
    }

    // Helper methods
    private static string GetMonthName(int month)
    {
        return month switch
        {
            1 => "January",
            2 => "February",
            3 => "March",
            4 => "April",
            5 => "May",
            6 => "June",
            7 => "July",
            8 => "August",
            9 => "September",
            10 => "October",
            11 => "November",
            12 => "December",
            _ => "Unknown"
        };
    }
}

