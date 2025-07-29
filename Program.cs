using PayrollManagement.API.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

try
{
    // Configure Serilog early
    builder.Services.AddLoggingServices(builder.Configuration);
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers();
    
    // Add custom services
    builder.Services.AddDatabaseServices(builder.Configuration);
    builder.Services.AddRepositories();
    builder.Services.AddApplicationServices();
    builder.Services.AddCachingServices(builder.Configuration);
    
    // Add authentication and authorization
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddAuthorizationPolicies();
    
    // Add API documentation
    builder.Services.AddApiDocumentation();
    
    // Add health checks
    builder.Services.AddHealthChecks(builder.Configuration);
    
    // Add CORS
    builder.Services.AddCorsServices();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    await app.ConfigureApplicationAsync();

    Log.Information("Payroll Management API started successfully");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

