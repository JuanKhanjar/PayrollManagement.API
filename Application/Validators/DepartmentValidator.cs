using PayrollManagement.API.Core.DTOs;
using PayrollManagement.API.Core.Interfaces;

namespace PayrollManagement.API.Application.Validators;

public class DepartmentValidator
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ValidationResult> ValidateCreateDepartmentAsync(CreateDepartmentDto dto)
    {
        var errors = new List<string>();

        // Basic validation
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Department name is required");
        else if (dto.Name.Length > 100)
            errors.Add("Department name cannot exceed 100 characters");

        if (string.IsNullOrWhiteSpace(dto.Code))
            errors.Add("Department code is required");
        else if (dto.Code.Length > 50)
            errors.Add("Department code cannot exceed 50 characters");

        if (!string.IsNullOrEmpty(dto.Description) && dto.Description.Length > 500)
            errors.Add("Description cannot exceed 500 characters");

        // Business rules validation
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var existingByName = await _unitOfWork.Departments.FirstOrDefaultAsync(d => d.Name.ToLower() == dto.Name.ToLower());
            if (existingByName != null)
                errors.Add("A department with this name already exists");
        }

        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            var existingByCode = await _unitOfWork.Departments.FirstOrDefaultAsync(d => d.Code.ToLower() == dto.Code.ToLower());
            if (existingByCode != null)
                errors.Add("A department with this code already exists");
        }

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateUpdateDepartmentAsync(int id, UpdateDepartmentDto dto)
    {
        var errors = new List<string>();

        // Check if department exists
        var existingDepartment = await _unitOfWork.Departments.GetByIdAsync(id);
        if (existingDepartment == null)
        {
            errors.Add("Department not found");
            return ValidationResult.Failure(errors);
        }

        // Basic validation
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Department name is required");
        else if (dto.Name.Length > 100)
            errors.Add("Department name cannot exceed 100 characters");

        if (string.IsNullOrWhiteSpace(dto.Code))
            errors.Add("Department code is required");
        else if (dto.Code.Length > 50)
            errors.Add("Department code cannot exceed 50 characters");

        if (!string.IsNullOrEmpty(dto.Description) && dto.Description.Length > 500)
            errors.Add("Description cannot exceed 500 characters");

        // Business rules validation
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var existingByName = await _unitOfWork.Departments.FirstOrDefaultAsync(d => d.Name.ToLower() == dto.Name.ToLower() && d.Id != id);
            if (existingByName != null)
                errors.Add("A department with this name already exists");
        }

        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            var existingByCode = await _unitOfWork.Departments.FirstOrDefaultAsync(d => d.Code.ToLower() == dto.Code.ToLower() && d.Id != id);
            if (existingByCode != null)
                errors.Add("A department with this code already exists");
        }

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateDeleteDepartmentAsync(int id)
    {
        var errors = new List<string>();

        var department = await _unitOfWork.Departments.GetByIdAsync(id);
        if (department == null)
        {
            errors.Add("Department not found");
            return ValidationResult.Failure(errors);
        }

        // Check if department has active employees
        var hasActiveEmployees = await _unitOfWork.Employees.ExistsAsync(e => e.DepartmentId == id && e.Status == Core.Enums.EmployeeStatus.Active);
        if (hasActiveEmployees)
            errors.Add("Cannot delete department with active employees");

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
}

