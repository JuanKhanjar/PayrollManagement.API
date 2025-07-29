using PayrollManagement.API.Core.Entities;
using PayrollManagement.API.Core.Enums;

namespace PayrollManagement.API.Core.Interfaces;

public interface IEmployeeRepository : IGenericRepository<Employee>
{
    Task<Employee?> GetByEmployeeCodeAsync(string employeeCode);
    Task<Employee?> GetByEmailAsync(string email);
    Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<Employee>> GetByStatusAsync(EmployeeStatus status);
    Task<IEnumerable<Employee>> GetActiveEmployeesAsync();
    Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm);
    Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, int? excludeId = null);
    Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
    Task<Employee?> GetEmployeeWithPayrollsAsync(int employeeId);
    Task<Employee?> GetEmployeeWithDepartmentAsync(int employeeId);
}

