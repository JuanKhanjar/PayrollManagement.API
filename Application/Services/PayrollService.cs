using PayrollManagement.API.Application.Mappings;
using PayrollManagement.API.Application.Validators;
using PayrollManagement.API.Core.DTOs;
using PayrollManagement.API.Core.Enums;
using PayrollManagement.API.Core.Interfaces;

namespace PayrollManagement.API.Application.Services;

public class PayrollService : IPayrollService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayrollValidator _validator;
    private readonly ILogger<PayrollService> _logger;

    public PayrollService(IUnitOfWork unitOfWork, PayrollValidator validator, ILogger<PayrollService> logger)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<PayrollDto>>> GetPayrollsAsync(PayrollSearchDto searchDto)
    {
        try
        {
            _logger.LogInformation("Retrieving payrolls with search criteria");

            IEnumerable<Core.Entities.Payroll> payrolls;

            // Apply filters
            if (searchDto.EmployeeId.HasValue)
            {
                payrolls = await _unitOfWork.PayrollRepository.GetByEmployeeAsync(searchDto.EmployeeId.Value);
            }
            else if (searchDto.PayPeriodMonth.HasValue && searchDto.PayPeriodYear.HasValue)
            {
                payrolls = await _unitOfWork.PayrollRepository.GetByPeriodAsync(searchDto.PayPeriodMonth.Value, searchDto.PayPeriodYear.Value);
            }
            else if (searchDto.Status.HasValue)
            {
                payrolls = await _unitOfWork.PayrollRepository.GetByStatusAsync(searchDto.Status.Value);
            }
            else
            {
                payrolls = await _unitOfWork.PayrollRepository.GetAllAsync();
            }

            // Apply additional filters
            if (searchDto.Status.HasValue && !searchDto.EmployeeId.HasValue && (!searchDto.PayPeriodMonth.HasValue || !searchDto.PayPeriodYear.HasValue))
            {
                payrolls = payrolls.Where(p => p.Status == searchDto.Status.Value);
            }

            // Convert to list for paging
            var payrollList = payrolls.ToList();
            var totalCount = payrollList.Count;

            // Apply paging
            var pagedPayrolls = payrollList
                .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(p => p.ToDto())
                .ToList();

            var result = new PagedResult<PayrollDto>
            {
                Items = pagedPayrolls,
                TotalCount = totalCount,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize
            };

            _logger.LogInformation("Retrieved {Count} payrolls (page {Page} of {TotalPages})", 
                pagedPayrolls.Count, searchDto.PageNumber, result.TotalPages);

            return ApiResponse<PagedResult<PayrollDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payrolls");
            return ApiResponse<PagedResult<PayrollDto>>.ErrorResponse("Failed to retrieve payrolls");
        }
    }

    public async Task<ApiResponse<PayrollDto>> GetPayrollByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving payroll with ID: {Id}", id);

            var payroll = await _unitOfWork.PayrollRepository.GetByIdAsync(id);
            if (payroll == null)
            {
                _logger.LogWarning("Payroll with ID {Id} not found", id);
                return ApiResponse<PayrollDto>.ErrorResponse("Payroll not found");
            }

            var payrollDto = payroll.ToDto();
            _logger.LogInformation("Retrieved payroll for employee: {Employee} ({Period})", 
                payroll.Employee?.FullName, $"{payroll.PayPeriodMonth}/{payroll.PayPeriodYear}");
            return ApiResponse<PayrollDto>.SuccessResponse(payrollDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payroll with ID: {Id}", id);
            return ApiResponse<PayrollDto>.ErrorResponse("Failed to retrieve payroll");
        }
    }

    public async Task<ApiResponse<PayrollDto>> CreatePayrollAsync(CreatePayrollDto createDto)
    {
        try
        {
            _logger.LogInformation("Creating new payroll for employee {EmployeeId} for period {Month}/{Year}", 
                createDto.EmployeeId, createDto.PayPeriodMonth, createDto.PayPeriodYear);

            // Validate the request
            var validationResult = await _validator.ValidateCreatePayrollAsync(createDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Payroll creation validation failed: {Errors}", string.Join(", ", validationResult.Errors));
                return ApiResponse<PayrollDto>.ErrorResponse("Validation failed", validationResult.Errors);
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Create the payroll
                var payroll = createDto.ToEntity();
                await _unitOfWork.PayrollRepository.AddAsync(payroll);
                await _unitOfWork.SaveChangesAsync();

                // Add payroll items if any
                if (createDto.PayrollItems?.Any() == true)
                {
                    var payrollItems = createDto.PayrollItems.Select(item => item.ToEntity(payroll.Id));
                    await _unitOfWork.PayrollItems.AddRangeAsync(payrollItems);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();

                // Reload with all related data
                var createdPayroll = await _unitOfWork.PayrollRepository.GetByIdAsync(payroll.Id);
                var payrollDto = createdPayroll!.ToDto();

                _logger.LogInformation("Created payroll with ID: {Id}", payroll.Id);
                return ApiResponse<PayrollDto>.SuccessResponse(payrollDto, "Payroll created successfully");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payroll for employee {EmployeeId}", createDto.EmployeeId);
            return ApiResponse<PayrollDto>.ErrorResponse("Failed to create payroll");
        }
    }

    public async Task<ApiResponse<PayrollDto>> UpdatePayrollAsync(int id, UpdatePayrollDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating payroll with ID: {Id}", id);

            // Validate the request
            var validationResult = await _validator.ValidateUpdatePayrollAsync(id, updateDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Payroll update validation failed: {Errors}", string.Join(", ", validationResult.Errors));
                return ApiResponse<PayrollDto>.ErrorResponse("Validation failed", validationResult.Errors);
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get and update the payroll
                var payroll = await _unitOfWork.PayrollRepository.GetPayrollWithItemsAsync(id);
                if (payroll == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogWarning("Payroll with ID {Id} not found for update", id);
                    return ApiResponse<PayrollDto>.ErrorResponse("Payroll not found");
                }

                updateDto.UpdateEntity(payroll);
                _unitOfWork.PayrollRepository.Update(payroll);

                // Update payroll items
                if (updateDto.PayrollItems?.Any() == true)
                {
                    // Remove existing items
                    if (payroll.PayrollItems.Any())
                    {
                        _unitOfWork.PayrollItems.DeleteRange(payroll.PayrollItems);
                    }

                    // Add new items
                    var newPayrollItems = updateDto.PayrollItems.Select(item => item.ToEntity(payroll.Id));
                    await _unitOfWork.PayrollItems.AddRangeAsync(newPayrollItems);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Reload with updated data
                var updatedPayroll = await _unitOfWork.PayrollRepository.GetByIdAsync(id);
                var payrollDto = updatedPayroll!.ToDto();

                _logger.LogInformation("Updated payroll with ID: {Id}", id);
                return ApiResponse<PayrollDto>.SuccessResponse(payrollDto, "Payroll updated successfully");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payroll with ID: {Id}", id);
            return ApiResponse<PayrollDto>.ErrorResponse("Failed to update payroll");
        }
    }

    public async Task<ApiResponse<bool>> DeletePayrollAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting payroll with ID: {Id}", id);

            // Validate the request
            var validationResult = await _validator.ValidateDeletePayrollAsync(id);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Payroll deletion validation failed: {Errors}", string.Join(", ", validationResult.Errors));
                return ApiResponse<bool>.ErrorResponse("Validation failed", validationResult.Errors);
            }

            // Delete the payroll (cascade will handle payroll items)
            await _unitOfWork.PayrollRepository.DeleteByIdAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted payroll with ID: {Id}", id);
            return ApiResponse<bool>.SuccessResponse(true, "Payroll deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payroll with ID: {Id}", id);
            return ApiResponse<bool>.ErrorResponse("Failed to delete payroll");
        }
    }

    public async Task<ApiResponse<IEnumerable<PayrollDto>>> GetPayrollsByEmployeeAsync(int employeeId)
    {
        try
        {
            _logger.LogInformation("Retrieving payrolls for employee: {EmployeeId}", employeeId);

            var payrolls = await _unitOfWork.PayrollRepository.GetByEmployeeAsync(employeeId);
            var payrollDtos = payrolls.Select(p => p.ToDto());

            _logger.LogInformation("Retrieved {Count} payrolls for employee {EmployeeId}", 
                payrolls.Count(), employeeId);
            return ApiResponse<IEnumerable<PayrollDto>>.SuccessResponse(payrollDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payrolls for employee: {EmployeeId}", employeeId);
            return ApiResponse<IEnumerable<PayrollDto>>.ErrorResponse("Failed to retrieve payrolls");
        }
    }

    public async Task<ApiResponse<IEnumerable<PayrollDto>>> GetPayrollsByPeriodAsync(int month, int year)
    {
        try
        {
            _logger.LogInformation("Retrieving payrolls for period: {Month}/{Year}", month, year);

            var payrolls = await _unitOfWork.PayrollRepository.GetByPeriodAsync(month, year);
            var payrollDtos = payrolls.Select(p => p.ToDto());

            _logger.LogInformation("Retrieved {Count} payrolls for period {Month}/{Year}", 
                payrolls.Count(), month, year);
            return ApiResponse<IEnumerable<PayrollDto>>.SuccessResponse(payrollDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payrolls for period: {Month}/{Year}", month, year);
            return ApiResponse<IEnumerable<PayrollDto>>.ErrorResponse("Failed to retrieve payrolls");
        }
    }

    public async Task<ApiResponse<PayrollDto>> ProcessPayrollAsync(int id)
    {
        try
        {
            _logger.LogInformation("Processing payroll with ID: {Id}", id);

            // Validate the request
            var validationResult = await _validator.ValidateProcessPayrollAsync(id);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Payroll processing validation failed: {Errors}", string.Join(", ", validationResult.Errors));
                return ApiResponse<PayrollDto>.ErrorResponse("Validation failed", validationResult.Errors);
            }

            var payroll = await _unitOfWork.PayrollRepository.GetByIdAsync(id);
            if (payroll == null)
            {
                return ApiResponse<PayrollDto>.ErrorResponse("Payroll not found");
            }

            // Process the payroll (business logic in entity)
            payroll.ProcessPayroll();
            _unitOfWork.PayrollRepository.Update(payroll);
            await _unitOfWork.SaveChangesAsync();

            var payrollDto = payroll.ToDto();
            _logger.LogInformation("Processed payroll with ID: {Id}", id);
            return ApiResponse<PayrollDto>.SuccessResponse(payrollDto, "Payroll processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payroll with ID: {Id}", id);
            return ApiResponse<PayrollDto>.ErrorResponse("Failed to process payroll");
        }
    }

    public async Task<ApiResponse<PayrollDto>> MarkPayrollAsPaidAsync(int id)
    {
        try
        {
            _logger.LogInformation("Marking payroll as paid with ID: {Id}", id);

            // Validate the request
            var validationResult = await _validator.ValidateMarkPayrollAsPaidAsync(id);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Mark payroll as paid validation failed: {Errors}", string.Join(", ", validationResult.Errors));
                return ApiResponse<PayrollDto>.ErrorResponse("Validation failed", validationResult.Errors);
            }

            var payroll = await _unitOfWork.PayrollRepository.GetByIdAsync(id);
            if (payroll == null)
            {
                return ApiResponse<PayrollDto>.ErrorResponse("Payroll not found");
            }

            // Mark as paid
            payroll.Status = PayrollStatus.Paid;
            payroll.PaidDate = DateTime.UtcNow;
            payroll.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.PayrollRepository.Update(payroll);
            await _unitOfWork.SaveChangesAsync();

            var payrollDto = payroll.ToDto();
            _logger.LogInformation("Marked payroll as paid with ID: {Id}", id);
            return ApiResponse<PayrollDto>.SuccessResponse(payrollDto, "Payroll marked as paid successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payroll as paid with ID: {Id}", id);
            return ApiResponse<PayrollDto>.ErrorResponse("Failed to mark payroll as paid");
        }
    }

    public async Task<ApiResponse<bool>> GeneratePayrollForEmployeeAsync(int employeeId, int month, int year)
    {
        try
        {
            _logger.LogInformation("Generating payroll for employee {EmployeeId} for period {Month}/{Year}", 
                employeeId, month, year);

            // Check if payroll already exists
            var existingPayroll = await _unitOfWork.PayrollRepository.GetByEmployeeAndPeriodAsync(employeeId, month, year);
            if (existingPayroll != null)
            {
                return ApiResponse<bool>.ErrorResponse("Payroll already exists for this employee and period");
            }

            // Get employee information
            var employee = await _unitOfWork.EmployeeRepository.GetEmployeeWithDepartmentAsync(employeeId);
            if (employee == null || employee.Status != EmployeeStatus.Active)
            {
                return ApiResponse<bool>.ErrorResponse("Employee not found or inactive");
            }

            // Create payroll with employee's base salary
            var createDto = new CreatePayrollDto
            {
                EmployeeId = employeeId,
                PayPeriodMonth = month,
                PayPeriodYear = year,
                BaseSalary = employee.BaseSalary
            };

            var result = await CreatePayrollAsync(createDto);
            if (result.Success)
            {
                _logger.LogInformation("Generated payroll for employee {EmployeeId}", employeeId);
                return ApiResponse<bool>.SuccessResponse(true, "Payroll generated successfully");
            }

            return ApiResponse<bool>.ErrorResponse("Failed to generate payroll", result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payroll for employee {EmployeeId}", employeeId);
            return ApiResponse<bool>.ErrorResponse("Failed to generate payroll");
        }
    }

    public async Task<ApiResponse<bool>> GeneratePayrollsForAllEmployeesAsync(int month, int year)
    {
        try
        {
            _logger.LogInformation("Generating payrolls for all active employees for period {Month}/{Year}", month, year);

            var activeEmployees = await _unitOfWork.EmployeeRepository.GetActiveEmployeesAsync();
            var successCount = 0;
            var errorCount = 0;
            var errors = new List<string>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var employee in activeEmployees)
                {
                    // Check if payroll already exists
                    var existingPayroll = await _unitOfWork.PayrollRepository.GetByEmployeeAndPeriodAsync(employee.Id, month, year);
                    if (existingPayroll != null)
                    {
                        _logger.LogWarning("Payroll already exists for employee {EmployeeId}", employee.Id);
                        continue;
                    }

                    try
                    {
                        var createDto = new CreatePayrollDto
                        {
                            EmployeeId = employee.Id,
                            PayPeriodMonth = month,
                            PayPeriodYear = year,
                            BaseSalary = employee.BaseSalary
                        };

                        var payroll = createDto.ToEntity();
                        await _unitOfWork.PayrollRepository.AddAsync(payroll);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error generating payroll for employee {EmployeeId}", employee.Id);
                        errors.Add($"Failed to generate payroll for employee {employee.EmployeeCode}");
                        errorCount++;
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Generated {SuccessCount} payrolls, {ErrorCount} errors", successCount, errorCount);

                if (errorCount > 0)
                {
                    return ApiResponse<bool>.ErrorResponse($"Generated {successCount} payrolls with {errorCount} errors", errors);
                }

                return ApiResponse<bool>.SuccessResponse(true, $"Generated {successCount} payrolls successfully");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payrolls for all employees");
            return ApiResponse<bool>.ErrorResponse("Failed to generate payrolls");
        }
    }

    public async Task<ApiResponse<decimal>> GetTotalPayrollAmountAsync(int month, int year)
    {
        try
        {
            _logger.LogInformation("Calculating total payroll amount for period {Month}/{Year}", month, year);

            var totalAmount = await _unitOfWork.PayrollRepository.GetTotalPayrollAmountAsync(month, year);

            _logger.LogInformation("Total payroll amount for {Month}/{Year}: {Amount:C}", month, year, totalAmount);
            return ApiResponse<decimal>.SuccessResponse(totalAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total payroll amount for period {Month}/{Year}", month, year);
            return ApiResponse<decimal>.ErrorResponse("Failed to calculate total payroll amount");
        }
    }
}

