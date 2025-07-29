using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PayrollManagement.API.Application.Services;
using PayrollManagement.API.Application.Validators;
using PayrollManagement.API.Core.Entities;
using PayrollManagement.API.Core.Interfaces;
using PayrollManagement.API.Infrastructure.Data;
using PayrollManagement.API.Infrastructure.Repositories;
using PayrollManagement.API.Infrastructure.Services;
using Serilog;
using System.Reflection;

namespace PayrollManagement.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<IAuthService, AuthService>();

        // Register validators
        services.AddScoped<DepartmentValidator>();
        services.AddScoped<EmployeeValidator>();
        services.AddScoped<PayrollValidator>();

        // Register caching service
        services.AddScoped<ICacheService, CacheService>();

        // Register token service
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register repositories
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IPayrollRepository, PayrollRepository>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        return services;
    }

    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlite(connectionString);
            
            // Enable sensitive data logging in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
            }
        });

        // Add Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;

            // Sign in settings
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("RedisConnection");
        
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "PayrollManagement";
            });
        }
        else
        {
            // Add in-memory cache as fallback
            services.AddMemoryCache();
            services.AddSingleton<Microsoft.Extensions.Caching.Distributed.IDistributedCache>(provider =>
                new Microsoft.Extensions.Caching.Distributed.MemoryDistributedCache(
                    Microsoft.Extensions.Options.Options.Create(new Microsoft.Extensions.Caching.Memory.MemoryDistributedCacheOptions())));
        }

        return services;
    }

    public static IServiceCollection AddLoggingServices(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        services.AddSerilog();

        return services;
    }

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        // API documentation disabled - Swagger removed
        return services;
    }

    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // Add database health check
        healthChecksBuilder.AddDbContextCheck<ApplicationDbContext>("database");

        // Add custom API health check
        healthChecksBuilder.AddCheck("api", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"));

        // Add Redis health check if configured
        var redisConnection = configuration.GetConnectionString("RedisConnection");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            healthChecksBuilder.AddRedis(redisConnection, "redis");
        }

        return services;
    }

    public static IServiceCollection AddCorsServices(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });

            options.AddPolicy("Development", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "https://localhost:3000", "https://localhost:5173")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }
}

