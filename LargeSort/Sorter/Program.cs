using System.CommandLine;
using System.Diagnostics;
using Sorter;

const int chunkSize = 1_000_000;

// Entry point for the application
return await RunApplication(args);

// Main application execution function
async Task<int> RunApplication(string[] args)
{
    // Get application settings
    var (currentDirectory, defaultOutputPath, tempPath, chunkSize) = GetApplicationSettings();
    
    // Configure and build command line interface
    var rootCommand = BuildCommandLineInterface(defaultOutputPath);
    
    // Execute the command
    return await rootCommand.InvokeAsync(args);
}

// Get application settings and paths

(string currentDirectory, string defaultOutputPath, string tempPath, int chunkSize) GetApplicationSettings()
{
    var currentDirectory = Directory.GetCurrentDirectory();
    var defaultOutputPath = Path.Combine(currentDirectory, "sorted.txt");
    var tempPath = Path.Combine(".", "temp", "3226675");

    return (currentDirectory, defaultOutputPath, tempPath, chunkSize);
}

// Build the command line interface
RootCommand BuildCommandLineInterface(string defaultOutputPath)
{
    var rootCommand = new RootCommand("The Program to sort big file");
    
    // Configure options
    var options = CreateCommandOptions(defaultOutputPath);
    foreach (var option in options)
    {
        rootCommand.AddOption(option.Value);
    }
    
    // Set up the main command handler
    rootCommand.SetHandler(
        HandleCommandExecution,
        (Option<string>)options["output"],
        (Option<string>)options["input"]
    );
    
    return rootCommand;
}

// Create command line options
Dictionary<string, Option> CreateCommandOptions(string defaultOutputPath)
{
    return new Dictionary<string, Option>
    {
        ["output"] = new Option<string>(
            name: "--output",
            getDefaultValue: () => defaultOutputPath,
            description: "The file to save generated content to."
        ),
            
        ["input"] = new Option<string>(
            name: "--input",
            description: "The Source file which should be sorted."
        )
        {
            IsRequired = true
        }
    };
}

// Handle the main command execution
async Task HandleCommandExecution(string outputPath, string sourceFilePath)
{
    // Get application settings for use in handler
    var (_, _, tempPath, chunkSize) = GetApplicationSettings();
    
    if (!File.Exists(sourceFilePath))
    {
        ConsoleLogger.Write(() =>
        {
            Console.WriteLine("Source file doesn't exist, specify one with '--input' flag");
        }, ConsoleColor.Red);
        return;
    }

    Stopwatch stopwatch = new Stopwatch();
    try
    {
        stopwatch.Start();
        await SortFile(sourceFilePath, outputPath, chunkSize, tempPath);
        ConsoleLogger.Write(() =>
        {
            Console.WriteLine("Sorting result:");
            Console.WriteLine(outputPath);
        }, ConsoleColor.Green);
    }
    catch (Exception ex)
    {
        ConsoleLogger.Write(() =>
        {
            Console.WriteLine("Something went wrong: {0}", ex.Message);
        }, ConsoleColor.Red);
    }
    finally
    {
        LogElapsedTime(stopwatch);
    }
}// Log elapsed time for profiling

// Log elapsed time for profiling
void LogElapsedTime(Stopwatch stopwatch)
{
    stopwatch.Stop();
    var ts = stopwatch.Elapsed;
    var elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
    ConsoleLogger.Write(() =>
    {
        Console.WriteLine("RunTime " + elapsedTime);
    }, ConsoleColor.Blue);
}

// Main file sorting logic
async Task SortFile(string inputPath, string outputPath, int linesPerChunk, string tempDirectory)
{
    // Create temp directory
    Directory.CreateDirectory(tempDirectory);

    // Split the file into smaller sorted chunks
    var fileIndex = await SplitInputFileIntoChunks(inputPath, tempDirectory, linesPerChunk);
    
    // Sort each individual chunk in parallel
    await SortChunksInParallel(fileIndex, tempDirectory);
    
    // Merge all sorted chunks
    await MergeSortedChunks(fileIndex, tempDirectory, outputPath);
    
    // Clean up temporary files
    Directory.Delete(tempDirectory, true);
}

// Split input file into manageable chunks
async Task<int> SplitInputFileIntoChunks(string inputPath, string tempDirectory, int linesPerChunk)
{
    var fileIndex = 0;
    using var reader = new StreamReader(inputPath);
    while (!reader.EndOfStream)
    {
        var outputFile = Path.Combine(tempDirectory, $"input_chunk_{fileIndex}.txt");
        await using (var writer = new StreamWriter(outputFile))
        {
            for (var i = 0; i < linesPerChunk && !reader.EndOfStream; i++)
            {
                var line = await reader.ReadLineAsync();
                if (line == null)
                    continue;
                await writer.WriteLineAsync(line);
            }
        }
        fileIndex++;
    }

    return fileIndex;
}

// Sort each chunk in parallel
async Task SortChunksInParallel(int fileCount, string tempDirectory)
{
    var comp = new ContentComparator();
    var chunkIndexes = Enumerable.Range(0, fileCount);
    
    await Parallel.ForEachAsync(
        chunkIndexes,
        async (index, cancellation) => {
            await SortSingleChunk(index, tempDirectory, comp);
        }
    );
}

// Sort a single chunk file
async Task SortSingleChunk(int index, string tempDirectory, ContentComparator comp)
{
    var path = Path.Combine(tempDirectory, $"input_chunk_{index}.txt");
    var output = Path.Combine(tempDirectory, $"sorted_chunk_{index}.txt");

    List<Content> contents = [];
    using var reader = new StreamReader(path);
    while (!reader.EndOfStream)
    {
        var line = await reader.ReadLineAsync();
        if (line == null)
            continue;

        contents.Add(Content.Parse(line));
    }
    
    contents.Sort(comp);

    await using var writer = new StreamWriter(output);
    foreach (var content in contents)
    {
        await writer.WriteLineAsync(content.ToString());
    }
}

// Merge all sorted chunks together
async Task MergeSortedChunks(int fileCount, string tempDirectory, string outputPath)
{
    // Initialize readers for all sorted chunks
    List<IAsyncEnumerator<Content>> sources = new();
    for (var i = 0; i < fileCount; i++)
    {
        var input = Path.Combine(tempDirectory, $"sorted_chunk_{i}.txt");
        var reader = ContentReader.InitRead(input);
        var source = reader.GetAsyncEnumerator();
        if (await source.MoveNextAsync())
            sources.Add(source);
    }
    
    // Ensure output file doesn't exist
    if (File.Exists(outputPath))
    {
        File.Delete(outputPath);
    }

    // Merge sort and write output
    var comp = new ContentComparator();
    var mergeSorter = new MergeSorter<Content>(sources, comp);
    await using var writer = new StreamWriter(outputPath);
    await foreach (var next in mergeSorter.MergeSort())
    {
        await writer.WriteLineAsync(next.ToString());
    }
}
