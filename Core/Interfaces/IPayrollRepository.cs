using PayrollManagement.API.Core.Entities;
using PayrollManagement.API.Core.Enums;

namespace PayrollManagement.API.Core.Interfaces;

public interface IPayrollRepository : IGenericRepository<Payroll>
{
    Task<Payroll?> GetByEmployeeAndPeriodAsync(int employeeId, int month, int year);
    Task<IEnumerable<Payroll>> GetByEmployeeAsync(int employeeId);
    Task<IEnumerable<Payroll>> GetByPeriodAsync(int month, int year);
    Task<IEnumerable<Payroll>> GetByStatusAsync(PayrollStatus status);
    Task<IEnumerable<Payroll>> GetPendingPayrollsAsync();
    Task<IEnumerable<Payroll>> GetProcessedPayrollsAsync();
    Task<Payroll?> GetPayrollWithItemsAsync(int payrollId);
    Task<Payroll?> GetPayrollWithEmployeeAsync(int payrollId);
    Task<IEnumerable<Payroll>> GetPayrollsForProcessingAsync(int month, int year);
    Task<decimal> GetTotalPayrollAmountAsync(int month, int year);
    Task<bool> HasPayrollForPeriodAsync(int employeeId, int month, int year);
}

