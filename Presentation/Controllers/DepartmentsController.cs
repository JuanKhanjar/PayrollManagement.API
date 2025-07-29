using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollManagement.API.Core.DTOs;
using PayrollManagement.API.Core.Interfaces;

namespace PayrollManagement.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger)
    {
        _departmentService = departmentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all departments
    /// </summary>
    /// <returns>List of all departments</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DepartmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentDto>>>> GetAllDepartments()
    {
        _logger.LogInformation("GET /api/departments - Retrieving all departments");
        
        var result = await _departmentService.GetAllDepartmentsAsync();
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully retrieved {Count} departments", result.Data?.Count() ?? 0);
            return Ok(result);
        }

        _logger.LogWarning("Failed to retrieve departments: {Message}", result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Get active departments only
    /// </summary>
    /// <returns>List of active departments</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DepartmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentDto>>>> GetActiveDepartments()
    {
        _logger.LogInformation("GET /api/departments/active - Retrieving active departments");
        
        var result = await _departmentService.GetActiveDepartmentsAsync();
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully retrieved {Count} active departments", result.Data?.Count() ?? 0);
            return Ok(result);
        }

        _logger.LogWarning("Failed to retrieve active departments: {Message}", result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Get department by ID
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <returns>Department details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetDepartmentById(int id)
    {
        _logger.LogInformation("GET /api/departments/{Id} - Retrieving department", id);
        
        var result = await _departmentService.GetDepartmentByIdAsync(id);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully retrieved department: {Name}", result.Data?.Name);
            return Ok(result);
        }

        if (result.Message.Contains("not found"))
        {
            _logger.LogWarning("Department with ID {Id} not found", id);
            return NotFound(result);
        }

        _logger.LogError("Failed to retrieve department {Id}: {Message}", id, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Create a new department
    /// </summary>
    /// <param name="createDto">Department creation data</param>
    /// <returns>Created department</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> CreateDepartment([FromBody] CreateDepartmentDto createDto)
    {
        _logger.LogInformation("POST /api/departments - Creating department: {Name}", createDto.Name);
        
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Invalid model state for department creation: {Errors}", string.Join(", ", errors));
            return BadRequest(ApiResponse<DepartmentDto>.ErrorResponse("Invalid input data", errors));
        }

        var result = await _departmentService.CreateDepartmentAsync(createDto);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully created department with ID: {Id}", result.Data?.Id);
            return CreatedAtAction(nameof(GetDepartmentById), new { id = result.Data?.Id }, result);
        }

        if (result.Errors.Any())
        {
            _logger.LogWarning("Department creation validation failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogError("Failed to create department: {Message}", result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Update an existing department
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <param name="updateDto">Department update data</param>
    /// <returns>Updated department</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto updateDto)
    {
        _logger.LogInformation("PUT /api/departments/{Id} - Updating department", id);
        
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Invalid model state for department update: {Errors}", string.Join(", ", errors));
            return BadRequest(ApiResponse<DepartmentDto>.ErrorResponse("Invalid input data", errors));
        }

        var result = await _departmentService.UpdateDepartmentAsync(id, updateDto);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully updated department with ID: {Id}", id);
            return Ok(result);
        }

        if (result.Message.Contains("not found"))
        {
            _logger.LogWarning("Department with ID {Id} not found for update", id);
            return NotFound(result);
        }

        if (result.Errors.Any())
        {
            _logger.LogWarning("Department update validation failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogError("Failed to update department {Id}: {Message}", id, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Delete a department
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDepartment(int id)
    {
        _logger.LogInformation("DELETE /api/departments/{Id} - Deleting department", id);
        
        var result = await _departmentService.DeleteDepartmentAsync(id);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully deleted department with ID: {Id}", id);
            return Ok(result);
        }

        if (result.Message.Contains("not found"))
        {
            _logger.LogWarning("Department with ID {Id} not found for deletion", id);
            return NotFound(result);
        }

        if (result.Errors.Any())
        {
            _logger.LogWarning("Department deletion validation failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogError("Failed to delete department {Id}: {Message}", id, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Check if department exists
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <returns>Existence status</returns>
    [HttpHead("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DepartmentExists(int id)
    {
        _logger.LogInformation("HEAD /api/departments/{Id} - Checking department existence", id);
        
        var result = await _departmentService.DepartmentExistsAsync(id);
        
        if (result.Success && result.Data == true)
        {
            return Ok();
        }

        return NotFound();
    }
}

