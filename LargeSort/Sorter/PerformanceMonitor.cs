using System.Collections.Concurrent;
using System.Diagnostics;

namespace FileGenerationUtil;

public class PerformanceMonitor
{
    private readonly ConcurrentDictionary<string, Stopwatch> _stopwatches = new();
    
    public void StartMonitoring(string operationName)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _stopwatches[operationName] = stopwatch;
    }
    
    public TimeSpan StopMonitoring(string operationName)
    {
        if (_stopwatches.TryGetValue(operationName, out var stopwatch))
        {
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
        
        return TimeSpan.Zero;
    }
}
