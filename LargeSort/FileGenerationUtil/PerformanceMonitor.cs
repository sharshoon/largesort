using System.Collections.Concurrent;
using System.Diagnostics;

namespace FileGenerationUtil;

public class PerformanceMonitor
{
    private readonly ConcurrentDictionary<string, Stopwatch> _stopwatches = new();
    
    /// <summary>
    /// Starts monitoring an operation with the specified name.
    /// </summary>
    /// <param name="operationName">The name of the operation to monitor.</param>
    public void StartMonitoring(string operationName)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _stopwatches[operationName] = stopwatch;
    }
    
    /// <summary>
    /// Stops monitoring an operation and returns the elapsed time.
    /// </summary>
    /// <param name="operationName">The name of the operation to stop monitoring.</param>
    /// <returns>The elapsed time for the operation, or TimeSpan.Zero if the operation was not found.</returns>
    public TimeSpan StopMonitoring(string operationName)
    {
        if (_stopwatches.TryGetValue(operationName, out var stopwatch))
        {
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
        
        return TimeSpan.Zero;
    }
    
    /// <summary>
    /// Gets the current elapsed time for a running operation without stopping it.
    /// </summary>
    /// <param name="operationName">The name of the operation.</param>
    /// <returns>The current elapsed time, or TimeSpan.Zero if the operation was not found.</returns>
    public TimeSpan GetCurrentElapsed(string operationName)
    {
        if (_stopwatches.TryGetValue(operationName, out var stopwatch))
        {
            return stopwatch.Elapsed;
        }
        
        return TimeSpan.Zero;
    }
    
    /// <summary>
    /// Resets all monitoring stopwatches.
    /// </summary>
    public void Reset()
    {
        foreach (var stopwatch in _stopwatches.Values)
        {
            stopwatch.Reset();
        }
    }
}
