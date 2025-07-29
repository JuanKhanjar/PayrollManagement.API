using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace PayrollManagement.API.Infrastructure.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task RemovePatternAsync(string pattern);
}

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CacheService(IDistributedCache distributedCache, ILogger<CacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var cachedValue = await _distributedCache.GetStringAsync(key);
            
            if (string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }

            var result = JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached value for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            
            var options = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.SetAbsoluteExpiration(expiration.Value);
            }
            else
            {
                // Default expiration of 30 minutes
                options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
            }

            await _distributedCache.SetStringAsync(key, serializedValue, options);
            _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", 
                key, expiration ?? TimeSpan.FromMinutes(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching value for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _distributedCache.RemoveAsync(key);
            _logger.LogDebug("Removed cached value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached value for key: {Key}", key);
        }
    }

    public async Task RemovePatternAsync(string pattern)
    {
        try
        {
            // Note: This is a simplified implementation
            // For production, you might want to use Redis-specific commands
            // or implement a more sophisticated pattern matching
            _logger.LogWarning("RemovePatternAsync is not fully implemented for distributed cache. Pattern: {Pattern}", pattern);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached values for pattern: {Pattern}", pattern);
        }
    }
}

// Cache key constants
public static class CacheKeys
{
    public const string DepartmentPrefix = "department:";
    public const string EmployeePrefix = "employee:";
    public const string PayrollPrefix = "payroll:";
    public const string ActiveDepartments = "departments:active";
    public const string ActiveEmployees = "employees:active";
    
    public static string DepartmentById(int id) => $"{DepartmentPrefix}{id}";
    public static string EmployeeById(int id) => $"{EmployeePrefix}{id}";
    public static string EmployeeByCode(string code) => $"{EmployeePrefix}code:{code}";
    public static string PayrollById(int id) => $"{PayrollPrefix}{id}";
    public static string PayrollsByEmployee(int employeeId) => $"{PayrollPrefix}employee:{employeeId}";
    public static string PayrollsByPeriod(int month, int year) => $"{PayrollPrefix}period:{year}:{month}";
}

