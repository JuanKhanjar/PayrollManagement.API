using Microsoft.EntityFrameworkCore;
using PayrollManagement.API.Core.Entities;
using PayrollManagement.API.Core.Enums;
using PayrollManagement.API.Core.Interfaces;
using PayrollManagement.API.Infrastructure.Data;

namespace PayrollManagement.API.Infrastructure.Repositories;

public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Employee?> GetByEmployeeCodeAsync(string employeeCode)
    {
        return await _dbSet
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
    }

    public async Task<Employee?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId)
    {
        return await _dbSet
            .Include(e => e.Department)
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetByStatusAsync(EmployeeStatus status)
    {
        return await _dbSet
            .Include(e => e.Department)
            .Where(e => e.Status == status)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
    {
        return await GetByStatusAsync(EmployeeStatus.Active);
    }

    public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        return await _dbSet
            .Include(e => e.Department)
            .Where(e => 
                e.FirstName.ToLower().Contains(lowerSearchTerm) ||
                e.LastName.ToLower().Contains(lowerSearchTerm) ||
                e.Email.ToLower().Contains(lowerSearchTerm) ||
                e.EmployeeCode.ToLower().Contains(lowerSearchTerm) ||
                e.Department.Name.ToLower().Contains(lowerSearchTerm))
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, int? excludeId = null)
    {
        var query = _dbSet.Where(e => e.EmployeeCode == employeeCode);
        
        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }
        
        return !await query.AnyAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
    {
        var query = _dbSet.Where(e => e.Email == email);
        
        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }
        
        return !await query.AnyAsync();
    }

    public async Task<Employee?> GetEmployeeWithPayrollsAsync(int employeeId)
    {
        return await _dbSet
            .Include(e => e.Department)
            .Include(e => e.Payrolls.OrderByDescending(p => p.PayPeriodYear).ThenByDescending(p => p.PayPeriodMonth))
            .FirstOrDefaultAsync(e => e.Id == employeeId);
    }

    public async Task<Employee?> GetEmployeeWithDepartmentAsync(int employeeId)
    {
        return await _dbSet
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == employeeId);
    }

    public override async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _dbSet
            .Include(e => e.Department)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();
    }

    public override async Task<Employee?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}

