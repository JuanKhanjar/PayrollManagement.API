using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PayrollManagement.API.Core.Entities;
using PayrollManagement.API.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace PayrollManagement.API.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Payroll> Payrolls { get; set; }
    public DbSet<PayrollItem> PayrollItems { get; set; }
    public DbSet<PayrollItemType> PayrollItemTypes { get; set; }
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Department configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EmployeeNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Position).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BaseSalary).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).HasConversion<string>();
            
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.EmployeeNumber).IsUnique();
            
            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Employees)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Payroll configuration
        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PayPeriodStart).IsRequired();
            entity.Property(e => e.PayPeriodEnd).IsRequired();
            entity.Property(e => e.GrossPay).HasColumnType("decimal(18,2)");
            entity.Property(e => e.NetPay).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalDeductions).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).HasConversion<string>();
            
            entity.HasOne(p => p.Employee)
                  .WithMany(e => e.Payrolls)
                  .HasForeignKey(p => p.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // PayrollItem configuration
        modelBuilder.Entity<PayrollItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Description).HasMaxLength(200);
            
            entity.HasOne(pi => pi.Payroll)
                  .WithMany(p => p.PayrollItems)
                  .HasForeignKey(pi => pi.PayrollId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(pi => pi.PayrollItemType)
                  .WithMany()
                  .HasForeignKey(pi => pi.PayrollItemTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // PayrollItemType configuration
        modelBuilder.Entity<PayrollItemType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // UserRefreshToken configuration
        modelBuilder.Entity<UserRefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.Property(e => e.JwtId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UserId).IsRequired();
            
            entity.HasOne<ApplicationUser>()
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Departments
        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "Human Resources", Code = "HR", Description = "Human Resources Department", CreatedAt = DateTime.UtcNow },
            new Department { Id = 2, Name = "Information Technology", Code = "IT", Description = "IT Department", CreatedAt = DateTime.UtcNow },
            new Department { Id = 3, Name = "Finance", Code = "FIN", Description = "Finance Department", CreatedAt = DateTime.UtcNow },
            new Department { Id = 4, Name = "Operations", Code = "OPS", Description = "Operations Department", CreatedAt = DateTime.UtcNow }
        );

        // Seed PayrollItemTypes
        modelBuilder.Entity<PayrollItemType>().HasData(
            new PayrollItemType { Id = 1, Name = "Basic Salary", Code = "BASIC", Description = "Basic monthly salary", IsEarning = true, CreatedAt = DateTime.UtcNow },
            new PayrollItemType { Id = 2, Name = "Overtime", Code = "OT", Description = "Overtime payment", IsEarning = true, CreatedAt = DateTime.UtcNow },
            new PayrollItemType { Id = 3, Name = "Bonus", Code = "BONUS", Description = "Performance bonus", IsEarning = true, CreatedAt = DateTime.UtcNow },
            new PayrollItemType { Id = 4, Name = "Health Insurance", Code = "HEALTH", Description = "Health insurance deduction", IsEarning = false, CreatedAt = DateTime.UtcNow },
            new PayrollItemType { Id = 5, Name = "Tax", Code = "TAX", Description = "Income tax deduction", IsEarning = false, CreatedAt = DateTime.UtcNow },
            new PayrollItemType { Id = 6, Name = "Retirement", Code = "401K", Description = "401K retirement contribution", IsEarning = false, CreatedAt = DateTime.UtcNow }
        );
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            // Only update timestamps for our custom entities, not Identity entities
            if (entry.Entity is Department || entry.Entity is Employee || 
                entry.Entity is Payroll || entry.Entity is PayrollItem || 
                entry.Entity is PayrollItemType || entry.Entity is ApplicationUser)
            {
                try
                {
                    if (entry.State == EntityState.Added)
                    {
                        var createdAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                        if (createdAtProperty != null)
                            createdAtProperty.CurrentValue = DateTime.UtcNow;
                    }

                    if (entry.State == EntityState.Modified)
                    {
                        var updatedAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                        if (updatedAtProperty != null)
                            updatedAtProperty.CurrentValue = DateTime.UtcNow;
                    }
                }
                catch
                {
                    // Ignore if property doesn't exist
                }
            }
        }
    }
}

