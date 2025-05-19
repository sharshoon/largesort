using Sorter.Operations;

namespace Sorter.Pipeline;

public class ChunkSplitOperation : IOperation
{
    private readonly string _inputPath;
    private readonly int _linesPerChunk;
    private readonly string _outputDirectory;
    
    public string OperationName => "Chunk Split";
    
    public ChunkSplitOperation(string inputPath, int linesPerChunk, string outputDirectory)
    {
        _inputPath = inputPath;
        _linesPerChunk = linesPerChunk;
        _outputDirectory = outputDirectory;
    }
    
    public async Task ExecuteAsync()
    {
        Directory.CreateDirectory(_outputDirectory);
        var fileIndex = 0;

        using var reader = new StreamReader(_inputPath);
        while (!reader.EndOfStream)
        {
            var outputFile = Path.Combine(_outputDirectory, $"chunk_{fileIndex}.txt");
            await using (var writer = new StreamWriter(outputFile))
            {
                for (var i = 0; i < _linesPerChunk && !reader.EndOfStream; i++)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null)
                        continue;
                    await writer.WriteLineAsync(line);
                }
            }
            fileIndex++;
        }
    }
}
