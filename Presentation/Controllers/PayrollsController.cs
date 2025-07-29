using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollManagement.API.Core.DTOs;
using PayrollManagement.API.Core.Interfaces;

namespace PayrollManagement.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PayrollsController : ControllerBase
{
    private readonly IPayrollService _payrollService;
    private readonly ILogger<PayrollsController> _logger;

    public PayrollsController(IPayrollService payrollService, ILogger<PayrollsController> logger)
    {
        _payrollService = payrollService;
        _logger = logger;
    }

    /// <summary>
    /// Get payrolls with optional filtering and pagination
    /// </summary>
    /// <param name="searchDto">Search and pagination parameters</param>
    /// <returns>Paged list of payrolls</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PayrollDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<PayrollDto>>>> GetPayrolls([FromQuery] PayrollSearchDto searchDto)
    {
        _logger.LogInformation("GET /api/payrolls - Retrieving payrolls with search criteria");
        
        var result = await _payrollService.GetPayrollsAsync(searchDto);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully retrieved {Count} payrolls (page {Page})", 
                result.Data?.Items.Count ?? 0, searchDto.PageNumber);
            return Ok(result);
        }

        _logger.LogWarning("Failed to retrieve payrolls: {Message}", result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Get payroll by ID
    /// </summary>
    /// <param name="id">Payroll ID</param>
    /// <returns>Payroll details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PayrollDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PayrollDto>>> GetPayrollById(int id)
    {
        _logger.LogInformation("GET /api/payrolls/{Id} - Retrieving payroll", id);
        
        var result = await _payrollService.GetPayrollByIdAsync(id);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully retrieved payroll for employee: {Employee}", 
                result.Data?.EmployeeName);
            return Ok(result);
        }

        if (result.Message.Contains("not found"))
        {
            _logger.LogWarning("Payroll with ID {Id} not found", id);
            return NotFound(result);
        }

        _logger.LogError("Failed to retrieve payroll {Id}: {Message}", id, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Get payrolls by employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>List of payrolls for the employee</returns>
    [HttpGet("by-employee/{employeeId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PayrollDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PayrollDto>>>> GetPayrollsByEmployee(int employeeId)
    {
        _logger.LogInformation("GET /api/payrolls/by-employee/{EmployeeId} - Retrieving payrolls", employeeId);
        
        var result = await _payrollService.GetPayrollsByEmployeeAsync(employeeId);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully retrieved {Count} payrolls for employee {EmployeeId}", 
                result.Data?.Count() ?? 0, employeeId);
            return Ok(result);
        }

        _logger.LogWarning("Failed to retrieve payrolls for employee {EmployeeId}: {Message}", 
            employeeId, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Get payrolls by period
    /// </summary>
    /// <param name="month">Month (1-12)</param>
    /// <param name="year">Year</param>
    /// <returns>List of payrolls for the period</returns>
    [HttpGet("by-period/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PayrollDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PayrollDto>>>> GetPayrollsByPeriod(int year, int month)
    {
        if (month < 1 || month > 12)
        {
            _logger.LogWarning("Invalid month provided: {Month}", month);
            return BadRequest(ApiResponse<IEnumerable<PayrollDto>>.ErrorResponse("Month must be between 1 and 12"));
        }

        if (year < 2000 || year > 3000)
        {
            _logger.LogWarning("Invalid year provided: {Year}", year);
            return BadRequest(ApiResponse<IEnumerable<PayrollDto>>.ErrorResponse("Year must be between 2000 and 3000"));
        }

        _logger.LogInformation("GET /api/payrolls/by-period/{Year}/{Month} - Retrieving payrolls", year, month);
        
        var result = await _payrollService.GetPayrollsByPeriodAsync(month, year);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully retrieved {Count} payrolls for period {Month}/{Year}", 
                result.Data?.Count() ?? 0, month, year);
            return Ok(result);
        }

        _logger.LogWarning("Failed to retrieve payrolls for period {Month}/{Year}: {Message}", 
            month, year, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Get total payroll amount for a period
    /// </summary>
    /// <param name="month">Month (1-12)</param>
    /// <param name="year">Year</param>
    /// <returns>Total payroll amount</returns>
    [HttpGet("total-amount/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<decimal>>> GetTotalPayrollAmount(int year, int month)
    {
        if (month < 1 || month > 12)
        {
            _logger.LogWarning("Invalid month provided: {Month}", month);
            return BadRequest(ApiResponse<decimal>.ErrorResponse("Month must be between 1 and 12"));
        }

        if (year < 2000 || year > 3000)
        {
            _logger.LogWarning("Invalid year provided: {Year}", year);
            return BadRequest(ApiResponse<decimal>.ErrorResponse("Year must be between 2000 and 3000"));
        }

        _logger.LogInformation("GET /api/payrolls/total-amount/{Year}/{Month} - Calculating total amount", year, month);
        
        var result = await _payrollService.GetTotalPayrollAmountAsync(month, year);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully calculated total amount for period {Month}/{Year}: {Amount:C}", 
                month, year, result.Data);
            return Ok(result);
        }

        _logger.LogWarning("Failed to calculate total amount for period {Month}/{Year}: {Message}", 
            month, year, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Create a new payroll
    /// </summary>
    /// <param name="createDto">Payroll creation data</param>
    /// <returns>Created payroll</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PayrollDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PayrollDto>>> CreatePayroll([FromBody] CreatePayrollDto createDto)
    {
        _logger.LogInformation("POST /api/payrolls - Creating payroll for employee {EmployeeId} for period {Month}/{Year}", 
            createDto.EmployeeId, createDto.PayPeriodMonth, createDto.PayPeriodYear);
        
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Invalid model state for payroll creation: {Errors}", string.Join(", ", errors));
            return BadRequest(ApiResponse<PayrollDto>.ErrorResponse("Invalid input data", errors));
        }

        var result = await _payrollService.CreatePayrollAsync(createDto);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully created payroll with ID: {Id}", result.Data?.Id);
            return CreatedAtAction(nameof(GetPayrollById), new { id = result.Data?.Id }, result);
        }

        if (result.Errors.Any())
        {
            _logger.LogWarning("Payroll creation validation failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogError("Failed to create payroll: {Message}", result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Update an existing payroll
    /// </summary>
    /// <param name="id">Payroll ID</param>
    /// <param name="updateDto">Payroll update data</param>
    /// <returns>Updated payroll</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PayrollDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PayrollDto>>> UpdatePayroll(int id, [FromBody] UpdatePayrollDto updateDto)
    {
        _logger.LogInformation("PUT /api/payrolls/{Id} - Updating payroll", id);
        
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Invalid model state for payroll update: {Errors}", string.Join(", ", errors));
            return BadRequest(ApiResponse<PayrollDto>.ErrorResponse("Invalid input data", errors));
        }

        var result = await _payrollService.UpdatePayrollAsync(id, updateDto);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully updated payroll with ID: {Id}", id);
            return Ok(result);
        }

        if (result.Message.Contains("not found"))
        {
            _logger.LogWarning("Payroll with ID {Id} not found for update", id);
            return NotFound(result);
        }

        if (result.Errors.Any())
        {
            _logger.LogWarning("Payroll update validation failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogError("Failed to update payroll {Id}: {Message}", id, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Delete a payroll
    /// </summary>
    /// <param name="id">Payroll ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePayroll(int id)
    {
        _logger.LogInformation("DELETE /api/payrolls/{Id} - Deleting payroll", id);
        
        var result = await _payrollService.DeletePayrollAsync(id);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully deleted payroll with ID: {Id}", id);
            return Ok(result);
        }

        if (result.Message.Contains("not found"))
        {
            _logger.LogWarning("Payroll with ID {Id} not found for deletion", id);
            return NotFound(result);
        }

        if (result.Errors.Any())
        {
            _logger.LogWarning("Payroll deletion validation failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogError("Failed to delete payroll {Id}: {Message}", id, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Process a payroll (calculate final amounts and mark as processed)
    /// </summary>
    /// <param name="id">Payroll ID</param>
    /// <returns>Processed payroll</returns>
    [HttpPost("{id:int}/process")]
    [ProducesResponseType(typeof(ApiResponse<PayrollDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PayrollDto>>> ProcessPayroll(int id)
    {
        _logger.LogInformation("POST /api/payrolls/{Id}/process - Processing payroll", id);
        
        var result = await _payrollService.ProcessPayrollAsync(id);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully processed payroll with ID: {Id}", id);
            return Ok(result);
        }

        if (result.Message.Contains("not found"))
        {
            _logger.LogWarning("Payroll with ID {Id} not found for processing", id);
            return NotFound(result);
        }

        if (result.Errors.Any())
        {
            _logger.LogWarning("Payroll processing validation failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogError("Failed to process payroll {Id}: {Message}", id, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Mark a payroll as paid
    /// </summary>
    /// <param name="id">Payroll ID</param>
    /// <returns>Updated payroll</returns>
    [HttpPost("{id:int}/mark-paid")]
    [ProducesResponseType(typeof(ApiResponse<PayrollDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PayrollDto>>> MarkPayrollAsPaid(int id)
    {
        _logger.LogInformation("POST /api/payrolls/{Id}/mark-paid - Marking payroll as paid", id);
        
        var result = await _payrollService.MarkPayrollAsPaidAsync(id);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully marked payroll as paid with ID: {Id}", id);
            return Ok(result);
        }

        if (result.Message.Contains("not found"))
        {
            _logger.LogWarning("Payroll with ID {Id} not found for marking as paid", id);
            return NotFound(result);
        }

        if (result.Errors.Any())
        {
            _logger.LogWarning("Mark payroll as paid validation failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogError("Failed to mark payroll as paid {Id}: {Message}", id, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Generate payroll for a specific employee and period
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="month">Month (1-12)</param>
    /// <param name="year">Year</param>
    /// <returns>Success status</returns>
    [HttpPost("generate/{employeeId:int}/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> GeneratePayrollForEmployee(int employeeId, int year, int month)
    {
        if (month < 1 || month > 12)
        {
            _logger.LogWarning("Invalid month provided: {Month}", month);
            return BadRequest(ApiResponse<bool>.ErrorResponse("Month must be between 1 and 12"));
        }

        if (year < 2000 || year > 3000)
        {
            _logger.LogWarning("Invalid year provided: {Year}", year);
            return BadRequest(ApiResponse<bool>.ErrorResponse("Year must be between 2000 and 3000"));
        }

        _logger.LogInformation("POST /api/payrolls/generate/{EmployeeId}/{Year}/{Month} - Generating payroll", 
            employeeId, year, month);
        
        var result = await _payrollService.GeneratePayrollForEmployeeAsync(employeeId, month, year);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully generated payroll for employee {EmployeeId} for period {Month}/{Year}", 
                employeeId, month, year);
            return Ok(result);
        }

        if (result.Errors.Any())
        {
            _logger.LogWarning("Payroll generation validation failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogError("Failed to generate payroll for employee {EmployeeId}: {Message}", employeeId, result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Generate payrolls for all active employees for a specific period
    /// </summary>
    /// <param name="month">Month (1-12)</param>
    /// <param name="year">Year</param>
    /// <returns>Success status with count</returns>
    [HttpPost("generate-all/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> GeneratePayrollsForAllEmployees(int year, int month)
    {
        if (month < 1 || month > 12)
        {
            _logger.LogWarning("Invalid month provided: {Month}", month);
            return BadRequest(ApiResponse<bool>.ErrorResponse("Month must be between 1 and 12"));
        }

        if (year < 2000 || year > 3000)
        {
            _logger.LogWarning("Invalid year provided: {Year}", year);
            return BadRequest(ApiResponse<bool>.ErrorResponse("Year must be between 2000 and 3000"));
        }

        _logger.LogInformation("POST /api/payrolls/generate-all/{Year}/{Month} - Generating payrolls for all employees", 
            year, month);
        
        var result = await _payrollService.GeneratePayrollsForAllEmployeesAsync(month, year);
        
        if (result.Success)
        {
            _logger.LogInformation("Successfully generated payrolls for all employees for period {Month}/{Year}", 
                month, year);
            return Ok(result);
        }

        if (result.Errors.Any())
        {
            _logger.LogWarning("Bulk payroll generation had errors: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogError("Failed to generate payrolls for all employees: {Message}", result.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}

