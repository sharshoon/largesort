using System.CommandLine;
using System.Diagnostics;
using Sorter;

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
    var defaultOutputPath = @$"{currentDirectory}\sorted.txt";
    var tempPath = @$".\temp\3226675\";
    int chunkSize = 1_000_000;
    
    return (currentDirectory, defaultOutputPath, tempPath, chunkSize);
}

// Build the command line interface
RootCommand BuildCommandLineInterface(string defaultOutputPath)
{
    var rootCommand = new RootCommand("The Program to sort big file");
    
    // Configure options
    var outputFileOption = new Option<string>(
        name: "--output",
        getDefaultValue: () => defaultOutputPath,
        description: "The file to save generated content to."
    );
    
    var sourceFileOption = new Option<string>(
        name: "--input",
        description: "The Source file which should be sorted."
    ) {
        IsRequired = true
    };
    
    rootCommand.AddOption(outputFileOption);
    rootCommand.AddOption(sourceFileOption);
    
    // Set up the main command handler
    rootCommand.SetHandler(
        async (string outputPath, string sourceFilePath) => {
            await HandleCommandExecution(outputPath, sourceFilePath);
        }, 
        outputFileOption, 
        sourceFileOption
    );
    
    return rootCommand;
}

// Handle the main command execution
async Task HandleCommandExecution(string outputPath, string sourceFilePath)
{
    // Get tempPath and chunkSize for use in handler
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
        stopwatch.Stop();
        ConsoleLogger.PrintElapsedTime(stopwatch.Elapsed, "Run time");
    }
}

// Main file sorting logic
async Task SortFile(string inputPath, string outputPath, int linesPerChunk, string tempDirectory)
{
    Directory.CreateDirectory(tempDirectory);
    var chunksDirectory = tempDirectory + "\\chunks\\";
    var sortedChunksDirectory = tempDirectory + "\\sorted\\";
    var stopwatch = new Stopwatch();

    // Step 1: Split input file into chunks
    stopwatch.Start();
    await SplitByChunks(inputPath, linesPerChunk, chunksDirectory);
    stopwatch.Stop();
    ConsoleLogger.PrintElapsedTime(stopwatch.Elapsed, "Elapsed for chunk creation");

    // Step 2: Sort each chunk individually
    stopwatch.Restart();
    await SortChunks(chunksDirectory, sortedChunksDirectory);
    stopwatch.Stop();
    ConsoleLogger.PrintElapsedTime(stopwatch.Elapsed, "Elapsed for chunk sorting");

    // Step 3: Merge all sorted chunks
    stopwatch.Restart();
    await MergeSortedChunks(sortedChunksDirectory, outputPath);
    stopwatch.Stop();
    ConsoleLogger.PrintElapsedTime(stopwatch.Elapsed, "Elapsed for chunk merging");

    // Clean up temporary files
    Directory.Delete(tempDirectory, true);
}

// Merge all sorted chunks together
async Task MergeSortedChunks(string sortedChunksDirectory, string outputPath)
{
    List<IAsyncEnumerator<Content>> sources = new();
    var files = Directory.EnumerateFiles(sortedChunksDirectory);
    
    // Initialize readers for all sorted chunks
    foreach (var file in files)
    {
        var reader = ContentReader.ReadFile(file);
        var source = reader.GetAsyncEnumerator();
        if (await source.MoveNextAsync())
            sources.Add(source);
    }
    
    // Ensure output file doesn't exist
    if (File.Exists(outputPath))
        File.Delete(outputPath);
    
    // Perform merge sort and write output
    var mergeSorter = new MergeSorter<Content>(sources, ContentComparator.Default);
    using var writer = new StreamWriter(outputPath);
    await foreach (var next in mergeSorter.MergeSort())
    {
        await writer.WriteLineAsync(next.ToString());
    }
}

// Sort each chunk in parallel
async Task SortChunks(string chunksDirectory, string outputDirectory)
{
    Directory.CreateDirectory(outputDirectory);
    var files = Directory.EnumerateFiles(chunksDirectory);
    
    await Parallel.ForEachAsync(
        files,
        async (file, cancellation) => {
            await SortSingleChunk(file, outputDirectory);
        }
    );
}

// Sort a single chunk file
async Task SortSingleChunk(string filePath, string outputDirectory)
{
    var fileName = Path.GetFileName(filePath);
    var output = Path.Combine(outputDirectory, fileName);

    // Read content from file
    List<Content> contents = [];
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

// Split input file into manageable chunks
async Task<int> SplitByChunks(string inputPath, int linesPerChunk, string outputDirectory)
{
    Directory.CreateDirectory(outputDirectory);
    var fileIndex = 0;

    using var reader = new StreamReader(inputPath);
    while (!reader.EndOfStream)
    {
        var outputFile = Path.Combine(outputDirectory, $"chunk_{fileIndex}.txt");
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
