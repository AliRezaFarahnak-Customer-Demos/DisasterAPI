using DisasterAPI.Interfaces;

namespace DisasterAPI.Services;

// This logger is problematic because:
// 1. It's registered as transient, so each request gets a new instance
// 2. It holds a static collection that grows forever
// 3. It has a large buffer that's never cleared
public class MemoryHungryLogger : IAppLogger
{
    // Static collection that will grow indefinitely
    // All instances contribute to this shared collection
    private static readonly List<string> _allLogsEver = new();
    
    // Large buffer allocated for each instance - never released
    private readonly byte[] _largeBuffer = new byte[1024 * 1024]; // 1MB per instance
    
    // Instance log collection that's never cleared
    private readonly List<string> _instanceLogs = new();

    public void LogInfo(string message)
    {
        var logEntry = $"INFO [{DateTime.Now}]: {message}";
        
        // Add to instance collection
        _instanceLogs.Add(logEntry);
        
        // Add to static collection that grows forever
        _allLogsEver.Add(logEntry);
        
        // Randomly modify buffer to prevent optimization
        if (Random.Shared.Next(100) > 95)
        {
            _largeBuffer[Random.Shared.Next(_largeBuffer.Length)] = (byte)Random.Shared.Next(256);
        }
        
        // Additional allocations
        var copy = string.Concat(message, " - processed at ", DateTime.Now.Ticks);
    }

    public void LogError(string message, Exception ex = null)
    {
        var logEntry = $"ERROR [{DateTime.Now}]: {message}";
        if (ex != null)
        {
            // Create even more strings that are kept in memory
            logEntry += $"\nException: {ex.Message}\nStack: {ex.StackTrace}";
            
            if (ex.InnerException != null) 
            {
                // Recursive function that creates more objects
                LogInnerExceptions(ex.InnerException, 1);
            }
        }
        
        _instanceLogs.Add(logEntry);
        _allLogsEver.Add(logEntry);
    }
    
    // Recursive method that creates more objects for nested exceptions
    private void LogInnerExceptions(Exception ex, int level)
    {
        var indent = new string(' ', level * 2);
        var logEntry = $"{indent}Inner: {ex.Message}";
        
        _instanceLogs.Add(logEntry);
        _allLogsEver.Add(logEntry);
        
        if (ex.InnerException != null)
        {
            LogInnerExceptions(ex.InnerException, level + 1);
        }
    }
}
