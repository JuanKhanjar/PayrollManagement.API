using PayrollManagement.API.Core.DTOs;

namespace PayrollManagement.API.Core.Interfaces;

public interface IPayrollService
{
    Task<ApiResponse<PagedResult<PayrollDto>>> GetPayrollsAsync(PayrollSearchDto searchDto);
    Task<ApiResponse<PayrollDto>> GetPayrollByIdAsync(int id);
    Task<ApiResponse<PayrollDto>> CreatePayrollAsync(CreatePayrollDto createDto);
    Task<ApiResponse<PayrollDto>> UpdatePayrollAsync(int id, UpdatePayrollDto updateDto);
    Task<ApiResponse<bool>> DeletePayrollAsync(int id);
    Task<ApiResponse<IEnumerable<PayrollDto>>> GetPayrollsByEmployeeAsync(int employeeId);
    Task<ApiResponse<IEnumerable<PayrollDto>>> GetPayrollsByPeriodAsync(int month, int year);
    Task<ApiResponse<PayrollDto>> ProcessPayrollAsync(int id);
    Task<ApiResponse<PayrollDto>> MarkPayrollAsPaidAsync(int id);
    Task<ApiResponse<bool>> GeneratePayrollForEmployeeAsync(int employeeId, int month, int year);
    Task<ApiResponse<bool>> GeneratePayrollsForAllEmployeesAsync(int month, int year);
    Task<ApiResponse<decimal>> GetTotalPayrollAmountAsync(int month, int year);
}

