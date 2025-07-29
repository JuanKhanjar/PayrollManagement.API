using PayrollManagement.API.Core.Entities;

namespace PayrollManagement.API.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Repository properties
    IGenericRepository<Department> Departments { get; }
    IGenericRepository<Employee> Employees { get; }
    IGenericRepository<Payroll> Payrolls { get; }
    IGenericRepository<PayrollItem> PayrollItems { get; }
    IGenericRepository<PayrollItemType> PayrollItemTypes { get; }
    
    // Specialized repositories
    IEmployeeRepository EmployeeRepository { get; }
    IPayrollRepository PayrollRepository { get; }
    
    // Transaction operations
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

