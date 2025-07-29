using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollManagement.API.Core.DTOs;
using PayrollManagement.API.Core.Interfaces;

namespace PayrollManagement.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>
    /// Get employees with optional filtering and pagination
    /// </summary>
    /// <param name="searchDto">Search and pagination parameters</param>
    /// <returns>Paged list of employees</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EmployeeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<EmployeeDto>>>> GetEmployees([FromQuery] EmployeeSearchDto searchDto)
    {
        _logger.LogInformation("GET /api/employees - Retrieving employees with search criteria");
        
        var result = await _employeeService.GetEmployeesAsync(searchDto);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully retrieved {Count} employees (page {Page})", 
                result.Data?.Items.Count ?? 0, searchDto.PageNumber);
            return Ok(result);
        }

        _logger.LogWarning("Failed to retrieve employees: {Message}", result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Get active employees only
    /// </summary>
    /// <returns>List of active employees</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeDto>>>> GetActiveEmployees()
    {
        _logger.LogInformation("GET /api/employees/active - Retrieving active employees");
        
        var result = await _employeeService.GetActiveEmployeesAsync();
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully retrieved {Count} active employees", result.Data?.Count() ?? 0);
            return Ok(result);
        }

        _logger.LogWarning("Failed to retrieve active employees: {Message}", result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <returns>Employee details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetEmployeeById(int id)
    {
        _logger.LogInformation("GET /api/employees/{Id} - Retrieving employee", id);
        
        var result = await _employeeService.GetEmployeeByIdAsync(id);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully retrieved employee: {Name}", result.Data?.FullName);
            return Ok(result);
        }

        if (result.Message.Contains("not found"))
        {
            _logger.LogWarning("Employee with ID {Id} not found", id);
            return NotFound(result);
        }

        _logger.LogError("Failed to retrieve employee {Id}: {Message}", id, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Get employee by employee code
    /// </summary>
    /// <param name="code">Employee code</param>
    /// <returns>Employee details</returns>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetEmployeeByCode(string code)
    {
        _logger.LogInformation("GET /api/employees/by-code/{Code} - Retrieving employee", code);
        
        var result = await _employeeService.GetEmployeeByCodeAsync(code);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully retrieved employee: {Name}", result.Data?.FullName);
            return Ok(result);
        }

        if (result.Message.Contains("not found"))
        {
            _logger.LogWarning("Employee with code {Code} not found", code);
            return NotFound(result);
        }

        _logger.LogError("Failed to retrieve employee {Code}: {Message}", code, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Get employees by department
    /// </summary>
    /// <param name="departmentId">Department ID</param>
    /// <returns>List of employees in the department</returns>
    [HttpGet("by-department/{departmentId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeDto>>>> GetEmployeesByDepartment(int departmentId)
    {
        _logger.LogInformation("GET /api/employees/by-department/{DepartmentId} - Retrieving employees", departmentId);
        
        var result = await _employeeService.GetEmployeesByDepartmentAsync(departmentId);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully retrieved {Count} employees for department {DepartmentId}", 
                result.Data?.Count() ?? 0, departmentId);
            return Ok(result);
        }

        _logger.LogWarning("Failed to retrieve employees for department {DepartmentId}: {Message}", 
            departmentId, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Search employees by term
    /// </summary>
    /// <param name="term">Search term</param>
    /// <returns>List of matching employees</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeDto>>>> SearchEmployees([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            _logger.LogWarning("Empty search term provided");
            return BadRequest(ApiResponse<IEnumerable<EmployeeDto>>.ErrorResponse("Search term is required"));
        }

        _logger.LogInformation("GET /api/employees/search?term={Term} - Searching employees", term);
        
        var result = await _employeeService.SearchEmployeesAsync(term);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully found {Count} employees matching '{Term}'", 
                result.Data?.Count() ?? 0, term);
            return Ok(result);
        }

        _logger.LogWarning("Failed to search employees: {Message}", result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Create a new employee
    /// </summary>
    /// <param name="createDto">Employee creation data</param>
    /// <returns>Created employee</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> CreateEmployee([FromBody] CreateEmployeeDto createDto)
    {
        _logger.LogInformation("POST /api/employees - Creating employee: {Name} ({Code})", 
            $"{createDto.FirstName} {createDto.LastName}", createDto.EmployeeCode);
        
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Invalid model state for employee creation: {Errors}", string.Join(", ", errors));
            return BadRequest(ApiResponse<EmployeeDto>.ErrorResponse("Invalid input data", errors));
        }

        var result = await _employeeService.CreateEmployeeAsync(createDto);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully created employee with ID: {Id}", result.Data?.Id);
            return CreatedAtAction(nameof(GetEmployeeById), new { id = result.Data?.Id }, result);
        }

        if (result.Errors.Any())
        {
            _logger.LogWarning("Employee creation validation failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogError("Failed to create employee: {Message}", result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Update an existing employee
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="updateDto">Employee update data</param>
    /// <returns>Updated employee</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto updateDto)
    {
        _logger.LogInformation("PUT /api/employees/{Id} - Updating employee", id);
        
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Invalid model state for employee update: {Errors}", string.Join(", ", errors));
            return BadRequest(ApiResponse<EmployeeDto>.ErrorResponse("Invalid input data", errors));
        }

        var result = await _employeeService.UpdateEmployeeAsync(id, updateDto);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully updated employee with ID: {Id}", id);
            return Ok(result);
        }

        if (result.Message.Contains("not found"))
        {
            _logger.LogWarning("Employee with ID {Id} not found for update", id);
            return NotFound(result);
        }

        if (result.Errors.Any())
        {
            _logger.LogWarning("Employee update validation failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogError("Failed to update employee {Id}: {Message}", id, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Delete an employee
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteEmployee(int id)
    {
        _logger.LogInformation("DELETE /api/employees/{Id} - Deleting employee", id);
        
        var result = await _employeeService.DeleteEmployeeAsync(id);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully deleted employee with ID: {Id}", id);
            return Ok(result);
        }

        if (result.Message.Contains("not found"))
        {
            _logger.LogWarning("Employee with ID {Id} not found for deletion", id);
            return NotFound(result);
        }

        if (result.Errors.Any())
        {
            _logger.LogWarning("Employee deletion validation failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogError("Failed to delete employee {Id}: {Message}", id, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}

