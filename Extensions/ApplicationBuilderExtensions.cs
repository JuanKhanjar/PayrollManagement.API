using Microsoft.AspNetCore.Identity;
using PayrollManagement.API.Core.Entities;
using PayrollManagement.API.Infrastructure.Data;
using PayrollManagement.API.Presentation.Middleware;
using Serilog;

namespace PayrollManagement.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task<WebApplication> ConfigureApplicationAsync(this WebApplication app)
    {
        // Configure Serilog request logging
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, elapsed, ex) => ex != null
                ? Serilog.Events.LogEventLevel.Error
                : elapsed > 1000
                    ? Serilog.Events.LogEventLevel.Warning
                    : Serilog.Events.LogEventLevel.Information;

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "");
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme ?? "");
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "");
                diagnosticContext.Set("RequestId", httpContext.Items["RequestId"] ?? "");
            };
        });
        
        // Add exception handling middleware
        app.UseMiddleware<SimpleExceptionMiddleware>();
        
        // Add request logging middleware
        app.UseMiddleware<RequestLoggingMiddleware>();

        app.UseHttpsRedirection();

        // Configure CORS
        if (app.Environment.IsDevelopment())
        {
            app.UseCors("Development");
        }
        else
        {
            app.UseCors("AllowAll");
        }

        // Configure Swagger - temporarily disabled
        /*
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payroll Management API v1");
                c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
            });
        }
        */

        // Authentication and Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Health checks
        app.MapHealthChecks("/health");

        // Map controllers
        app.MapControllers();

        // Initialize database
        await app.InitializeDatabaseAsync();

        return app;
    }

    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database ensured created");

            // Seed data
            await SeedDataAsync(context, userManager, roleManager, logger);
            logger.LogInformation("Database seeding completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }

        return app;
    }

    private static async Task SeedDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager, Microsoft.Extensions.Logging.ILogger logger)
    {
        try
        {
            // Seed roles
            var roles = new[] { "Administrator", "HR Manager", "Payroll Officer", "Employee" };
            
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    logger.LogInformation("Created role: {RoleName}", roleName);
                }
            }

            // Seed admin user
            var adminEmail = "admin@payroll.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                    logger.LogInformation("Created admin user: {Email}", adminEmail);
                }
                else
                {
                    logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // Seed HR Manager user
            var hrEmail = "hr@payroll.com";
            var hrUser = await userManager.FindByEmailAsync(hrEmail);
            
            if (hrUser == null)
            {
                hrUser = new ApplicationUser
                {
                    UserName = hrEmail,
                    Email = hrEmail,
                    FirstName = "HR",
                    LastName = "Manager",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(hrUser, "HR@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(hrUser, "HR Manager");
                    logger.LogInformation("Created HR user: {Email}", hrEmail);
                }
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while seeding data");
            throw;
        }
    }
}

