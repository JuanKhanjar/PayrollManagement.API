using PayrollManagement.API.Core.DTOs;
using System.Net;
using System.Text.Json;

namespace PayrollManagement.API.Presentation.Middleware;

public class SimpleExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SimpleExceptionMiddleware> _logger;

    public SimpleExceptionMiddleware(RequestDelegate next, ILogger<SimpleExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An error occurred while processing your request";

        if (exception is ArgumentException or ArgumentNullException)
        {
            statusCode = HttpStatusCode.BadRequest;
            message = "Invalid request parameters";
        }
        else if (exception is UnauthorizedAccessException)
        {
            statusCode = HttpStatusCode.Unauthorized;
            message = "Access denied";
        }
        else if (exception is KeyNotFoundException)
        {
            statusCode = HttpStatusCode.NotFound;
            message = "Resource not found";
        }

        var response = ApiResponse<object>.ErrorResponse(message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

