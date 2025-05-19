using Sorter.Operations;

namespace Sorter.Pipeline;

public class ChunkSortOperation : IOperation
{
    private readonly string _chunksDirectory;
    private readonly string _outputDirectory;
    
    public string OperationName => "Chunk Sort";
    
    public ChunkSortOperation(string chunksDirectory, string outputDirectory)
    {
        _chunksDirectory = chunksDirectory;
        _outputDirectory = outputDirectory;
    }
    
    public async Task ExecuteAsync()
    {
        Directory.CreateDirectory(_outputDirectory);
        var files = Directory.EnumerateFiles(_chunksDirectory);
        
        await Parallel.ForEachAsync(
            files,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            async (file, cancellation) => {
                await SortSingleChunk(file, _outputDirectory);
            }
        );
    }
    
    private async Task SortSingleChunk(string filePath, string outputDirectory)
    {
        var fileName = Path.GetFileName(filePath);
        var output = Path.Combine(outputDirectory, fileName);

        // Read content from file
        List<Content> contents = new();
        await foreach (var content in ContentReader.ReadFile(filePath))
        {
            contents.Add(content);
        }
        
        // Remove original file once data is loaded
        File.Delete(filePath);
        
        // Sort contents and write to output
        contents.Sort(ContentComparator.Default);
        using var writer = new StreamWriter(output);
        foreach (var content in contents)
        {
            await writer.WriteLineAsync(content.ToString());
        }
    }
}
