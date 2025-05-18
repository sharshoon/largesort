using System.Text;

namespace FileGenerationUtil.Operations;

/// <summary>
/// Operation to initialize a source file for text generation by extracting random segments from an input file.
/// </summary>
public class SourceInitOperation : IOperation
{
    private readonly string _inputPath;
    private readonly string _outputPath;
    private readonly int _minTextLength;
    private readonly int _maxTextLength;
    
    /// <summary>
    /// Initializes a new instance of the SourceInitOperation class.
    /// </summary>
    /// <param name="inputPath">The path to the input file containing the source text.</param>
    /// <param name="outputPath">The path where the processed source file will be saved.</param>
    /// <param name="minTextLength">The minimum length of each text segment.</param>
    /// <param name="maxTextLength">The maximum length of each text segment.</param>
    public SourceInitOperation(string inputPath, string outputPath, int minTextLength, int maxTextLength)
    {
        _inputPath = inputPath;
        _outputPath = outputPath;
        _minTextLength = minTextLength;
        _maxTextLength = maxTextLength;
        
        // Ensure the directory for the output file exists
        var outputDirectory = Path.GetDirectoryName(_outputPath);
        if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }
    }
    
    /// <summary>
    /// Executes the operation to create a source file with random text segments.
    /// </summary>
    public async Task ExecuteAsync()
    {
        if (!File.Exists(_inputPath))
        {
            throw new FileNotFoundException($"Input file not found: {_inputPath}");
        }
        
        var rand = new Random();
        await using var input = File.OpenRead(_inputPath);
        await using var output = File.Create(_outputPath);
        
        // Buffer to hold the generated segments
        var segmentsBuffer = new List<string>();
        var totalSegments = 1000; // Target number of segments to generate
        
        while (segmentsBuffer.Count < totalSegments)
        {
            // Determine a random length for this segment
            var segmentLength = rand.Next(_minTextLength, _maxTextLength);
            var buffer = new byte[segmentLength];
            
            // Read a random segment from the input file
            var readCount = await input.ReadAsync(buffer, 0, segmentLength);
            if (readCount == 0)
            {
                // If we've reached the end of the file, reset to the beginning
                input.Position = 0;
                continue;
            }
            
            // Convert the bytes to a string and clean it up
            string result = Encoding.UTF8.GetString(buffer, 0, readCount);
            var cleaned = result.Replace("\r", " ")
                                .Replace("\n", " ")
                                .Replace("\t", " ")
                                .Trim();
            
            // Ensure the segment meets our minimum length requirement
            if (cleaned.Length < _minTextLength)
                continue;
            
            // Add a newline for the file format
            cleaned += "\n";
            segmentsBuffer.Add(cleaned);
        }
        
        // Write all segments to the output file
        foreach (var segment in segmentsBuffer)
        {
            var segmentBytes = Encoding.UTF8.GetBytes(segment);
            await output.WriteAsync(segmentBytes, 0, segmentBytes.Length);
        }
    }
}
