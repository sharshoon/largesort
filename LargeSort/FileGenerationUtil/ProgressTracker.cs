namespace FileGenerationUtil;

/// <summary>
/// Tracks progress of operations and reports percentage completion.
/// </summary>
public class ProgressTracker
{
    private readonly long _totalSize;
    private int _lastReportedPercentage;
    private readonly int _reportingIncrement;
    
    /// <summary>
    /// Initializes a new instance of the ProgressTracker class.
    /// </summary>
    /// <param name="totalSize">The total size or count representing 100% completion.</param>
    /// <param name="reportingIncrement">The percentage increment at which to report progress.</param>
    public ProgressTracker(long totalSize, int reportingIncrement = 10)
    {
        if (totalSize <= 0)
            throw new ArgumentException("Total size must be greater than zero", nameof(totalSize));
            
        if (reportingIncrement <= 0 || reportingIncrement > 100)
            throw new ArgumentException("Reporting increment must be between 1 and 100", nameof(reportingIncrement));
            
        _totalSize = totalSize;
        _reportingIncrement = reportingIncrement;
        _lastReportedPercentage = 0;
    }
    
    /// <summary>
    /// Updates the progress and reports if a threshold has been crossed.
    /// </summary>
    /// <param name="currentSize">The current size or count of completed work.</param>
    public void UpdateProgress(long currentSize)
    {
        int currentPercentage = (int)((double)currentSize / _totalSize * 100.0);
        
        if (currentPercentage >= _lastReportedPercentage + _reportingIncrement)
        {
            _lastReportedPercentage = (currentPercentage / _reportingIncrement) * _reportingIncrement;
            
            ConsoleLogger.Write(
                () => Console.WriteLine($"Progress: {_lastReportedPercentage}%"),
                ConsoleColor.Blue);
        }
    }
    
    /// <summary>
    /// Gets the current percentage of completion.
    /// </summary>
    /// <param name="currentSize">The current size or count of completed work.</param>
    /// <returns>The percentage of completion (0-100).</returns>
    public int GetCurrentPercentage(long currentSize)
    {
        return (int)((double)currentSize / _totalSize * 100.0);
    }
}
