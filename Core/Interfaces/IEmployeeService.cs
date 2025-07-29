using PayrollManagement.API.Core.DTOs;

namespace PayrollManagement.API.Core.Interfaces;

public interface IEmployeeService
{
    Task<ApiResponse<PagedResult<EmployeeDto>>> GetEmployeesAsync(EmployeeSearchDto searchDto);
    Task<ApiResponse<EmployeeDto>> GetEmployeeByIdAsync(int id);
    Task<ApiResponse<EmployeeDto>> GetEmployeeByCodeAsync(string employeeCode);
    Task<ApiResponse<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto createDto);
    Task<ApiResponse<EmployeeDto>> UpdateEmployeeAsync(int id, UpdateEmployeeDto updateDto);
    Task<ApiResponse<bool>> DeleteEmployeeAsync(int id);
    Task<ApiResponse<IEnumerable<EmployeeDto>>> GetEmployeesByDepartmentAsync(int departmentId);
    Task<ApiResponse<IEnumerable<EmployeeDto>>> GetActiveEmployeesAsync();
    Task<ApiResponse<IEnumerable<EmployeeDto>>> SearchEmployeesAsync(string searchTerm);
}

