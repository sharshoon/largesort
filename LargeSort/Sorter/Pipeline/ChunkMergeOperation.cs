using Sorter.Operations;

namespace Sorter.Pipeline;

public class ChunkMergeOperation : IOperation
{
    private readonly string _sortedChunksDirectory;
    private readonly string _outputPath;
    
    public string OperationName => "Chunk Merge";
    
    public ChunkMergeOperation(string sortedChunksDirectory, string outputPath)
    {
        _sortedChunksDirectory = sortedChunksDirectory;
        _outputPath = outputPath;
    }
    
    public async Task ExecuteAsync()
    {
        List<IAsyncEnumerator<Content>> sources = new();
        var files = Directory.EnumerateFiles(_sortedChunksDirectory);
        
        // Initialize readers for all sorted chunks
        foreach (var file in files)
        {
            var reader = ContentReader.ReadFile(file);
            var source = reader.GetAsyncEnumerator();
            if (await source.MoveNextAsync())
                sources.Add(source);
        }
        
        // Ensure output file doesn't exist
        if (File.Exists(_outputPath))
            File.Delete(_outputPath);
        
        // Perform merge sort and write output
        var mergeSorter = new MergeSorter<Content>(sources, ContentComparator.Default);
        using var writer = new StreamWriter(_outputPath);
        await foreach (var next in mergeSorter.MergeSort())
        {
            await writer.WriteLineAsync(next.ToString());
        }
    }
}
