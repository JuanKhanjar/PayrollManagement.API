using PayrollManagement.API.Application.Mappings;
using PayrollManagement.API.Application.Validators;
using PayrollManagement.API.Core.DTOs;
using PayrollManagement.API.Core.Enums;
using PayrollManagement.API.Core.Interfaces;

namespace PayrollManagement.API.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly EmployeeValidator _validator;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IUnitOfWork unitOfWork, EmployeeValidator validator, ILogger<EmployeeService> logger)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<EmployeeDto>>> GetEmployeesAsync(EmployeeSearchDto searchDto)
    {
        try
        {
            _logger.LogInformation("Retrieving employees with search criteria");

            IEnumerable<Core.Entities.Employee> employees;

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
            {
                employees = await _unitOfWork.EmployeeRepository.SearchEmployeesAsync(searchDto.SearchTerm);
            }
            else if (searchDto.DepartmentId.HasValue)
            {
                employees = await _unitOfWork.EmployeeRepository.GetByDepartmentAsync(searchDto.DepartmentId.Value);
            }
            else if (searchDto.Status.HasValue)
            {
                employees = await _unitOfWork.EmployeeRepository.GetByStatusAsync(searchDto.Status.Value);
            }
            else
            {
                employees = await _unitOfWork.EmployeeRepository.GetAllAsync();
            }

            // Apply additional filters
            if (searchDto.Status.HasValue && string.IsNullOrWhiteSpace(searchDto.SearchTerm) && !searchDto.DepartmentId.HasValue)
            {
                employees = employees.Where(e => e.Status == searchDto.Status.Value);
            }

            if (searchDto.DepartmentId.HasValue && string.IsNullOrWhiteSpace(searchDto.SearchTerm))
            {
                employees = employees.Where(e => e.DepartmentId == searchDto.DepartmentId.Value);
            }

            // Convert to list for paging
            var employeeList = employees.ToList();
            var totalCount = employeeList.Count;

            // Apply paging
            var pagedEmployees = employeeList
                .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(e => e.ToDto())
                .ToList();

            var result = new PagedResult<EmployeeDto>
            {
                Items = pagedEmployees,
                TotalCount = totalCount,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize
            };

            _logger.LogInformation("Retrieved {Count} employees (page {Page} of {TotalPages})", 
                pagedEmployees.Count, searchDto.PageNumber, result.TotalPages);

            return ApiResponse<PagedResult<EmployeeDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees");
            return ApiResponse<PagedResult<EmployeeDto>>.ErrorResponse("Failed to retrieve employees");
        }
    }

    public async Task<ApiResponse<EmployeeDto>> GetEmployeeByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving employee with ID: {Id}", id);

            var employee = await _unitOfWork.EmployeeRepository.GetEmployeeWithDepartmentAsync(id);
            if (employee == null)
            {
                _logger.LogWarning("Employee with ID {Id} not found", id);
                return ApiResponse<EmployeeDto>.ErrorResponse("Employee not found");
            }

            var employeeDto = employee.ToDto();
            _logger.LogInformation("Retrieved employee: {Name} ({Code})", employee.FullName, employee.EmployeeCode);
            return ApiResponse<EmployeeDto>.SuccessResponse(employeeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee with ID: {Id}", id);
            return ApiResponse<EmployeeDto>.ErrorResponse("Failed to retrieve employee");
        }
    }

    public async Task<ApiResponse<EmployeeDto>> GetEmployeeByCodeAsync(string employeeCode)
    {
        try
        {
            _logger.LogInformation("Retrieving employee with code: {Code}", employeeCode);

            var employee = await _unitOfWork.EmployeeRepository.GetByEmployeeCodeAsync(employeeCode);
            if (employee == null)
            {
                _logger.LogWarning("Employee with code {Code} not found", employeeCode);
                return ApiResponse<EmployeeDto>.ErrorResponse("Employee not found");
            }

            var employeeDto = employee.ToDto();
            _logger.LogInformation("Retrieved employee: {Name}", employee.FullName);
            return ApiResponse<EmployeeDto>.SuccessResponse(employeeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee with code: {Code}", employeeCode);
            return ApiResponse<EmployeeDto>.ErrorResponse("Failed to retrieve employee");
        }
    }

    public async Task<ApiResponse<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto createDto)
    {
        try
        {
            _logger.LogInformation("Creating new employee: {Name} ({Code})", 
                $"{createDto.FirstName} {createDto.LastName}", createDto.EmployeeCode);

            // Validate the request
            var validationResult = await _validator.ValidateCreateEmployeeAsync(createDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Employee creation validation failed: {Errors}", string.Join(", ", validationResult.Errors));
                return ApiResponse<EmployeeDto>.ErrorResponse("Validation failed", validationResult.Errors);
            }

            // Create the employee
            var employee = createDto.ToEntity();
            await _unitOfWork.EmployeeRepository.AddAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            // Reload with department information
            var createdEmployee = await _unitOfWork.EmployeeRepository.GetEmployeeWithDepartmentAsync(employee.Id);
            var employeeDto = createdEmployee!.ToDto();

            _logger.LogInformation("Created employee with ID: {Id}", employee.Id);
            return ApiResponse<EmployeeDto>.SuccessResponse(employeeDto, "Employee created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee: {Name}", $"{createDto.FirstName} {createDto.LastName}");
            return ApiResponse<EmployeeDto>.ErrorResponse("Failed to create employee");
        }
    }

    public async Task<ApiResponse<EmployeeDto>> UpdateEmployeeAsync(int id, UpdateEmployeeDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating employee with ID: {Id}", id);

            // Validate the request
            var validationResult = await _validator.ValidateUpdateEmployeeAsync(id, updateDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Employee update validation failed: {Errors}", string.Join(", ", validationResult.Errors));
                return ApiResponse<EmployeeDto>.ErrorResponse("Validation failed", validationResult.Errors);
            }

            // Get and update the employee
            var employee = await _unitOfWork.EmployeeRepository.GetEmployeeWithDepartmentAsync(id);
            if (employee == null)
            {
                _logger.LogWarning("Employee with ID {Id} not found for update", id);
                return ApiResponse<EmployeeDto>.ErrorResponse("Employee not found");
            }

            updateDto.UpdateEntity(employee);
            _unitOfWork.EmployeeRepository.Update(employee);
            await _unitOfWork.SaveChangesAsync();

            // Reload with updated department information
            var updatedEmployee = await _unitOfWork.EmployeeRepository.GetEmployeeWithDepartmentAsync(id);
            var employeeDto = updatedEmployee!.ToDto();

            _logger.LogInformation("Updated employee with ID: {Id}", id);
            return ApiResponse<EmployeeDto>.SuccessResponse(employeeDto, "Employee updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee with ID: {Id}", id);
            return ApiResponse<EmployeeDto>.ErrorResponse("Failed to update employee");
        }
    }

    public async Task<ApiResponse<bool>> DeleteEmployeeAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting employee with ID: {Id}", id);

            // Validate the request
            var validationResult = await _validator.ValidateDeleteEmployeeAsync(id);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Employee deletion validation failed: {Errors}", string.Join(", ", validationResult.Errors));
                return ApiResponse<bool>.ErrorResponse("Validation failed", validationResult.Errors);
            }

            // Delete the employee
            await _unitOfWork.EmployeeRepository.DeleteByIdAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted employee with ID: {Id}", id);
            return ApiResponse<bool>.SuccessResponse(true, "Employee deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee with ID: {Id}", id);
            return ApiResponse<bool>.ErrorResponse("Failed to delete employee");
        }
    }

    public async Task<ApiResponse<IEnumerable<EmployeeDto>>> GetEmployeesByDepartmentAsync(int departmentId)
    {
        try
        {
            _logger.LogInformation("Retrieving employees for department: {DepartmentId}", departmentId);

            var employees = await _unitOfWork.EmployeeRepository.GetByDepartmentAsync(departmentId);
            var employeeDtos = employees.Select(e => e.ToDto());

            _logger.LogInformation("Retrieved {Count} employees for department {DepartmentId}", 
                employees.Count(), departmentId);
            return ApiResponse<IEnumerable<EmployeeDto>>.SuccessResponse(employeeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees for department: {DepartmentId}", departmentId);
            return ApiResponse<IEnumerable<EmployeeDto>>.ErrorResponse("Failed to retrieve employees");
        }
    }

    public async Task<ApiResponse<IEnumerable<EmployeeDto>>> GetActiveEmployeesAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving active employees");

            var employees = await _unitOfWork.EmployeeRepository.GetActiveEmployeesAsync();
            var employeeDtos = employees.Select(e => e.ToDto());

            _logger.LogInformation("Retrieved {Count} active employees", employees.Count());
            return ApiResponse<IEnumerable<EmployeeDto>>.SuccessResponse(employeeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active employees");
            return ApiResponse<IEnumerable<EmployeeDto>>.ErrorResponse("Failed to retrieve active employees");
        }
    }

    public async Task<ApiResponse<IEnumerable<EmployeeDto>>> SearchEmployeesAsync(string searchTerm)
    {
        try
        {
            _logger.LogInformation("Searching employees with term: {SearchTerm}", searchTerm);

            var employees = await _unitOfWork.EmployeeRepository.SearchEmployeesAsync(searchTerm);
            var employeeDtos = employees.Select(e => e.ToDto());

            _logger.LogInformation("Found {Count} employees matching search term", employees.Count());
            return ApiResponse<IEnumerable<EmployeeDto>>.SuccessResponse(employeeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching employees with term: {SearchTerm}", searchTerm);
            return ApiResponse<IEnumerable<EmployeeDto>>.ErrorResponse("Failed to search employees");
        }
    }
}

