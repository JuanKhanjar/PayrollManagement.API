using PayrollManagement.API.Core.DTOs;

namespace PayrollManagement.API.Core.Interfaces;

public interface IDepartmentService
{
    Task<ApiResponse<IEnumerable<DepartmentDto>>> GetAllDepartmentsAsync();
    Task<ApiResponse<DepartmentDto>> GetDepartmentByIdAsync(int id);
    Task<ApiResponse<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto createDto);
    Task<ApiResponse<DepartmentDto>> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDto);
    Task<ApiResponse<bool>> DeleteDepartmentAsync(int id);
    Task<ApiResponse<bool>> DepartmentExistsAsync(int id);
    Task<ApiResponse<IEnumerable<DepartmentDto>>> GetActiveDepartmentsAsync();
}

