using PayrollManagement.API.Application.Mappings;
using PayrollManagement.API.Application.Validators;
using PayrollManagement.API.Core.DTOs;
using PayrollManagement.API.Core.Interfaces;

namespace PayrollManagement.API.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly DepartmentValidator _validator;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(IUnitOfWork unitOfWork, DepartmentValidator validator, ILogger<DepartmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<DepartmentDto>>> GetAllDepartmentsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all departments");
            
            var departments = await _unitOfWork.Departments.GetAllAsync();
            var departmentDtos = departments.Select(d => d.ToDto());
            
            _logger.LogInformation("Retrieved {Count} departments", departments.Count());
            return ApiResponse<IEnumerable<DepartmentDto>>.SuccessResponse(departmentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departments");
            return ApiResponse<IEnumerable<DepartmentDto>>.ErrorResponse("Failed to retrieve departments");
        }
    }

    public async Task<ApiResponse<DepartmentDto>> GetDepartmentByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving department with ID: {Id}", id);
            
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department == null)
            {
                _logger.LogWarning("Department with ID {Id} not found", id);
                return ApiResponse<DepartmentDto>.ErrorResponse("Department not found");
            }

            var departmentDto = department.ToDto();
            _logger.LogInformation("Retrieved department: {Name}", department.Name);
            return ApiResponse<DepartmentDto>.SuccessResponse(departmentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department with ID: {Id}", id);
            return ApiResponse<DepartmentDto>.ErrorResponse("Failed to retrieve department");
        }
    }

    public async Task<ApiResponse<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto createDto)
    {
        try
        {
            _logger.LogInformation("Creating new department: {Name}", createDto.Name);

            // Validate the request
            var validationResult = await _validator.ValidateCreateDepartmentAsync(createDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Department creation validation failed: {Errors}", string.Join(", ", validationResult.Errors));
                return ApiResponse<DepartmentDto>.ErrorResponse("Validation failed", validationResult.Errors);
            }

            // Create the department
            var department = createDto.ToEntity();
            await _unitOfWork.Departments.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            var departmentDto = department.ToDto();
            _logger.LogInformation("Created department with ID: {Id}", department.Id);
            return ApiResponse<DepartmentDto>.SuccessResponse(departmentDto, "Department created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department: {Name}", createDto.Name);
            return ApiResponse<DepartmentDto>.ErrorResponse("Failed to create department");
        }
    }

    public async Task<ApiResponse<DepartmentDto>> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating department with ID: {Id}", id);

            // Validate the request
            var validationResult = await _validator.ValidateUpdateDepartmentAsync(id, updateDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Department update validation failed: {Errors}", string.Join(", ", validationResult.Errors));
                return ApiResponse<DepartmentDto>.ErrorResponse("Validation failed", validationResult.Errors);
            }

            // Get and update the department
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department == null)
            {
                _logger.LogWarning("Department with ID {Id} not found for update", id);
                return ApiResponse<DepartmentDto>.ErrorResponse("Department not found");
            }

            updateDto.UpdateEntity(department);
            _unitOfWork.Departments.Update(department);
            await _unitOfWork.SaveChangesAsync();

            var departmentDto = department.ToDto();
            _logger.LogInformation("Updated department with ID: {Id}", id);
            return ApiResponse<DepartmentDto>.SuccessResponse(departmentDto, "Department updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department with ID: {Id}", id);
            return ApiResponse<DepartmentDto>.ErrorResponse("Failed to update department");
        }
    }

    public async Task<ApiResponse<bool>> DeleteDepartmentAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting department with ID: {Id}", id);

            // Validate the request
            var validationResult = await _validator.ValidateDeleteDepartmentAsync(id);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Department deletion validation failed: {Errors}", string.Join(", ", validationResult.Errors));
                return ApiResponse<bool>.ErrorResponse("Validation failed", validationResult.Errors);
            }

            // Delete the department
            await _unitOfWork.Departments.DeleteByIdAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted department with ID: {Id}", id);
            return ApiResponse<bool>.SuccessResponse(true, "Department deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department with ID: {Id}", id);
            return ApiResponse<bool>.ErrorResponse("Failed to delete department");
        }
    }

    public async Task<ApiResponse<bool>> DepartmentExistsAsync(int id)
    {
        try
        {
            var exists = await _unitOfWork.Departments.ExistsAsync(d => d.Id == id);
            return ApiResponse<bool>.SuccessResponse(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if department exists with ID: {Id}", id);
            return ApiResponse<bool>.ErrorResponse("Failed to check department existence");
        }
    }

    public async Task<ApiResponse<IEnumerable<DepartmentDto>>> GetActiveDepartmentsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving active departments");
            
            var departments = await _unitOfWork.Departments.FindAsync(d => d.IsActive);
            var departmentDtos = departments.Select(d => d.ToDto());
            
            _logger.LogInformation("Retrieved {Count} active departments", departments.Count());
            return ApiResponse<IEnumerable<DepartmentDto>>.SuccessResponse(departmentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active departments");
            return ApiResponse<IEnumerable<DepartmentDto>>.ErrorResponse("Failed to retrieve active departments");
        }
    }
}

