using System.Diagnostics;
using System.Text;

namespace PayrollManagement.API.Presentation.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];
        
        // Add request ID to context for correlation
        context.Items["RequestId"] = requestId;

        // Log request
        await LogRequestAsync(context, requestId);

        // Capture response
        var originalResponseBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Log response
            await LogResponseAsync(context, requestId, stopwatch.ElapsedMilliseconds);

            // Copy response back to original stream
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBodyStream);
            context.Response.Body = originalResponseBodyStream;
        }
    }

    private async Task LogRequestAsync(HttpContext context, string requestId)
    {
        try
        {
            var request = context.Request;
            var logBuilder = new StringBuilder();
            
            logBuilder.AppendLine($"[{requestId}] HTTP Request:");
            logBuilder.AppendLine($"  Method: {request.Method}");
            logBuilder.AppendLine($"  Path: {request.Path}");
            logBuilder.AppendLine($"  Query: {request.QueryString}");
            logBuilder.AppendLine($"  User-Agent: {request.Headers.UserAgent}");
            logBuilder.AppendLine($"  Content-Type: {request.ContentType}");
            logBuilder.AppendLine($"  Content-Length: {request.ContentLength}");

            // Log request body for POST/PUT requests (be careful with sensitive data)
            if ((request.Method == "POST" || request.Method == "PUT") && 
                request.ContentLength > 0 && 
                request.ContentLength < 10000) // Limit body logging size
            {
                request.EnableBuffering();
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadAsync(buffer, 0, buffer.Length);
                var bodyText = Encoding.UTF8.GetString(buffer);
                
                // Mask sensitive data
                bodyText = MaskSensitiveData(bodyText);
                logBuilder.AppendLine($"  Body: {bodyText}");
                
                request.Body.Position = 0;
            }

            _logger.LogInformation(logBuilder.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{RequestId}] Error logging request", requestId);
        }
    }

    private async Task LogResponseAsync(HttpContext context, string requestId, long elapsedMilliseconds)
    {
        try
        {
            var response = context.Response;
            var logBuilder = new StringBuilder();
            
            logBuilder.AppendLine($"[{requestId}] HTTP Response:");
            logBuilder.AppendLine($"  Status: {response.StatusCode}");
            logBuilder.AppendLine($"  Content-Type: {response.ContentType}");
            logBuilder.AppendLine($"  Content-Length: {response.ContentLength}");
            logBuilder.AppendLine($"  Duration: {elapsedMilliseconds}ms");

            // Log response body for errors (be careful with sensitive data)
            if (response.StatusCode >= 400 && response.Body.CanRead)
            {
                response.Body.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(response.Body).ReadToEndAsync();
                response.Body.Seek(0, SeekOrigin.Begin);
                
                if (!string.IsNullOrEmpty(responseBody) && responseBody.Length < 5000)
                {
                    logBuilder.AppendLine($"  Body: {responseBody}");
                }
            }

            var logLevel = response.StatusCode >= 500 ? LogLevel.Error :
                          response.StatusCode >= 400 ? LogLevel.Warning :
                          elapsedMilliseconds > 5000 ? LogLevel.Warning :
                          LogLevel.Information;

            _logger.Log(logLevel, logBuilder.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{RequestId}] Error logging response", requestId);
        }
    }

    private static string MaskSensitiveData(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Mask common sensitive fields
        var sensitiveFields = new[] { "password", "token", "secret", "key", "authorization" };
        
        foreach (var field in sensitiveFields)
        {
            // Simple regex to mask JSON field values
            var pattern = $@"(""{field}"":\s*"")[^""]*("")";
            input = System.Text.RegularExpressions.Regex.Replace(
                input, pattern, $"$1***MASKED***$2", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return input;
    }
}

