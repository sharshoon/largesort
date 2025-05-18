using System.Text;
using FileGenerationUtil.Interfaces;

namespace FileGenerationUtil.Operations;

public class FileGenerationOperation : IOperation
{
    private readonly string _outputPath;
    private readonly long _contentSize;
    private readonly string _sourceFilePath;
    
    public FileGenerationOperation(string outputPath, long contentSize, string sourceFilePath)
    {
        _outputPath = outputPath;
        _contentSize = contentSize;
        _sourceFilePath = sourceFilePath;
    }
    
    public async Task ExecuteAsync()
    {
        long currentSize = 0;
        var source = await File.ReadAllLinesAsync(_sourceFilePath);
        
        // Create generators
        IContentGenerator<string> textPartGenerator = new StringGenerator(source);
        IContentGenerator<int> numberPartGenerator = new NumberGenerator();
        
        await using var fs = File.Create(_outputPath);
        
        // Generate content until required size is reached
        var progressTracker = new ProgressTracker(_contentSize);
        
        while (currentSize < _contentSize)
        {
            var textPart = textPartGenerator.GetNext();
            var numberPart = numberPartGenerator.GetNext().ToString();
            
            var line = $"{numberPart}.{textPart}\n";
            var buffer = Encoding.UTF8.GetBytes(line);
            
            await fs.WriteAsync(buffer, 0, buffer.Length);
            currentSize += buffer.Length;
            
            progressTracker.UpdateProgress(currentSize);
        }
    }
}
