using Microsoft.EntityFrameworkCore;
using PayrollManagement.API.Core.Entities;
using PayrollManagement.API.Core.Enums;
using PayrollManagement.API.Core.Interfaces;
using PayrollManagement.API.Infrastructure.Data;

namespace PayrollManagement.API.Infrastructure.Repositories;

public class PayrollRepository : GenericRepository<Payroll>, IPayrollRepository
{
    public PayrollRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Payroll?> GetByEmployeeAndPeriodAsync(int employeeId, int month, int year)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .ThenInclude(e => e.Department)
            .Include(p => p.PayrollItems)
            .ThenInclude(pi => pi.PayrollItemType)
            .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && 
                                    p.PayPeriodMonth == month && 
                                    p.PayPeriodYear == year);
    }

    public async Task<IEnumerable<Payroll>> GetByEmployeeAsync(int employeeId)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .ThenInclude(e => e.Department)
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.PayPeriodYear)
            .ThenByDescending(p => p.PayPeriodMonth)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payroll>> GetByPeriodAsync(int month, int year)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .ThenInclude(e => e.Department)
            .Where(p => p.PayPeriodMonth == month && p.PayPeriodYear == year)
            .OrderBy(p => p.Employee.FirstName)
            .ThenBy(p => p.Employee.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payroll>> GetByStatusAsync(PayrollStatus status)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .ThenInclude(e => e.Department)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.PayPeriodYear)
            .ThenByDescending(p => p.PayPeriodMonth)
            .ThenBy(p => p.Employee.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payroll>> GetPendingPayrollsAsync()
    {
        return await GetByStatusAsync(PayrollStatus.Draft);
    }

    public async Task<IEnumerable<Payroll>> GetProcessedPayrollsAsync()
    {
        return await _dbSet
            .Include(p => p.Employee)
            .ThenInclude(e => e.Department)
            .Where(p => p.Status == PayrollStatus.Processed || p.Status == PayrollStatus.Paid)
            .OrderByDescending(p => p.ProcessedDate)
            .ToListAsync();
    }

    public async Task<Payroll?> GetPayrollWithItemsAsync(int payrollId)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .ThenInclude(e => e.Department)
            .Include(p => p.PayrollItems)
            .ThenInclude(pi => pi.PayrollItemType)
            .FirstOrDefaultAsync(p => p.Id == payrollId);
    }

    public async Task<Payroll?> GetPayrollWithEmployeeAsync(int payrollId)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .ThenInclude(e => e.Department)
            .FirstOrDefaultAsync(p => p.Id == payrollId);
    }

    public async Task<IEnumerable<Payroll>> GetPayrollsForProcessingAsync(int month, int year)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .ThenInclude(e => e.Department)
            .Where(p => p.PayPeriodMonth == month && 
                       p.PayPeriodYear == year && 
                       p.Status == PayrollStatus.Draft)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalPayrollAmountAsync(int month, int year)
    {
        return await _dbSet
            .Where(p => p.PayPeriodMonth == month && 
                       p.PayPeriodYear == year && 
                       (p.Status == PayrollStatus.Processed || p.Status == PayrollStatus.Paid))
            .SumAsync(p => p.NetPay);
    }

    public async Task<bool> HasPayrollForPeriodAsync(int employeeId, int month, int year)
    {
        return await _dbSet.AnyAsync(p => p.EmployeeId == employeeId && 
                                         p.PayPeriodMonth == month && 
                                         p.PayPeriodYear == year);
    }

    public override async Task<IEnumerable<Payroll>> GetAllAsync()
    {
        return await _dbSet
            .Include(p => p.Employee)
            .ThenInclude(e => e.Department)
            .OrderByDescending(p => p.PayPeriodYear)
            .ThenByDescending(p => p.PayPeriodMonth)
            .ThenBy(p => p.Employee.FirstName)
            .ToListAsync();
    }

    public override async Task<Payroll?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .ThenInclude(e => e.Department)
            .Include(p => p.PayrollItems)
            .ThenInclude(pi => pi.PayrollItemType)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}

