using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace PayrollManagement.API.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Set to true in production
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero, // Remove delay of token when expire
                RequireExpirationTime = true
            };

            // Add custom events for better logging and error handling
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("JWT Authentication failed: {Exception}", context.Exception.Message);
                    
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("JWT Challenge triggered: {Error} - {ErrorDescription}", 
                        context.Error, context.ErrorDescription);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    logger.LogDebug("JWT Token validated for user: {UserId}", userId);
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Role-based policies
            options.AddPolicy("AdminOnly", policy => 
                policy.RequireRole("Administrator"));
            
            options.AddPolicy("HRManagerOnly", policy => 
                policy.RequireRole("Administrator", "HR Manager"));
            
            options.AddPolicy("PayrollOfficerOnly", policy => 
                policy.RequireRole("Administrator", "HR Manager", "Payroll Officer"));
            
            options.AddPolicy("EmployeeAccess", policy => 
                policy.RequireRole("Administrator", "HR Manager", "Payroll Officer", "Employee"));

            // Claim-based policies
            options.AddPolicy("ActiveUser", policy =>
                policy.RequireClaim("IsActive", "True"));

            // Custom policies combining multiple requirements
            options.AddPolicy("ManageEmployees", policy =>
                policy.RequireRole("Administrator", "HR Manager")
                      .RequireClaim("IsActive", "True"));

            options.AddPolicy("ManagePayroll", policy =>
                policy.RequireRole("Administrator", "Payroll Officer")
                      .RequireClaim("IsActive", "True"));

            options.AddPolicy("ViewReports", policy =>
                policy.RequireRole("Administrator", "HR Manager", "Payroll Officer")
                      .RequireClaim("IsActive", "True"));
        });

        return services;
    }
}

