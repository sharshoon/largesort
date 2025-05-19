using FileGenerationUtil;
using Sorter.Pipeline;

namespace Sorter.Operations;

public class FileSortOperation : IOperation
{
    private readonly string _inputPath;
    private readonly string _outputPath;
    private readonly int _chunkSize;
    private readonly string _tempDirectory;
    private readonly PerformanceMonitor _performanceMonitor;
    
    public string OperationName => "File Sort";
    
    public FileSortOperation(string inputPath, string outputPath, int chunkSize, string tempDirectory)
    {
        _inputPath = inputPath;
        _outputPath = outputPath;
        _chunkSize = chunkSize;
        _tempDirectory = tempDirectory;
        _performanceMonitor = new PerformanceMonitor();
    }
    
    public async Task ExecuteAsync()
    {
        Directory.CreateDirectory(_tempDirectory);
        var chunksDirectory = Path.Combine(_tempDirectory, "chunks");
        var sortedChunksDirectory = Path.Combine(_tempDirectory, "sorted");
        
        try
        {
            // Create a pipeline of operations
            var operations = new List<IOperation>
            {
                new ChunkSplitOperation(_inputPath, _chunkSize, chunksDirectory),
                new ChunkSortOperation(chunksDirectory, sortedChunksDirectory),
                new ChunkMergeOperation(sortedChunksDirectory, _outputPath)
            };
            
            // Execute the pipeline
            foreach (var operation in operations)
            {
                _performanceMonitor.StartMonitoring(operation.OperationName);
                try
                {
                    await operation.ExecuteAsync();
                }
                finally
                {
                    var elapsed = _performanceMonitor.StopMonitoring(operation.OperationName);
                    ConsoleLogger.PrintElapsedTime(elapsed, $"Elapsed for {operation.OperationName}");
                }
            }
        }
        finally
        {
            // Clean up temporary files
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
    }
}
