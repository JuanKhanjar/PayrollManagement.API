using Microsoft.EntityFrameworkCore.Storage;
using PayrollManagement.API.Core.Entities;
using PayrollManagement.API.Core.Interfaces;
using PayrollManagement.API.Infrastructure.Data;

namespace PayrollManagement.API.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    
    // Generic repositories
    private IGenericRepository<Department>? _departments;
    private IGenericRepository<Employee>? _employees;
    private IGenericRepository<Payroll>? _payrolls;
    private IGenericRepository<PayrollItem>? _payrollItems;
    private IGenericRepository<PayrollItemType>? _payrollItemTypes;
    
    // Specialized repositories
    private IEmployeeRepository? _employeeRepository;
    private IPayrollRepository? _payrollRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // Generic repository properties
    public IGenericRepository<Department> Departments => 
        _departments ??= new GenericRepository<Department>(_context);

    public IGenericRepository<Employee> Employees => 
        _employees ??= new GenericRepository<Employee>(_context);

    public IGenericRepository<Payroll> Payrolls => 
        _payrolls ??= new GenericRepository<Payroll>(_context);

    public IGenericRepository<PayrollItem> PayrollItems => 
        _payrollItems ??= new GenericRepository<PayrollItem>(_context);

    public IGenericRepository<PayrollItemType> PayrollItemTypes => 
        _payrollItemTypes ??= new GenericRepository<PayrollItemType>(_context);

    // Specialized repository properties
    public IEmployeeRepository EmployeeRepository => 
        _employeeRepository ??= new EmployeeRepository(_context);

    public IPayrollRepository PayrollRepository => 
        _payrollRepository ??= new PayrollRepository(_context);

    // Transaction operations
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

