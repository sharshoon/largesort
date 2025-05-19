using System.CommandLine;
using FileGenerationUtil;
using Sorter;
using Sorter.Configuration;
using Sorter.Operations;

// Entry point for the application
return await RunApplication(args);

// Main application execution function
async Task<int> RunApplication(string[] args)
{
    // Initialize application configuration
    var config = new SorterConfiguration();
    
    // Configure and build command line interface
    var rootCommand = BuildCommandLineInterface(config);
    
    // Execute the command
    return await rootCommand.InvokeAsync(args);
}

// Build the command line interface
RootCommand BuildCommandLineInterface(SorterConfiguration config)
{
    var rootCommand = new RootCommand("The Program to sort big file");
    
    // Configure options
    var outputFileOption = new Option<string>(
        name: "--output",
        getDefaultValue: () => config.DefaultOutputPath,
        description: "The file to save sorted content to."
    );
    
    var sourceFileOption = new Option<string>(
        name: "--input",
        description: "The Source file which should be sorted."
    ) {
        IsRequired = true
    };
    
    var chunkSizeOption = new Option<int>(
        name: "--chunk-size",
        getDefaultValue: () => config.DefaultChunkSize,
        description: "Number of lines per chunk (default: 1,000,000)"
    );
    
    rootCommand.AddOption(outputFileOption);
    rootCommand.AddOption(sourceFileOption);
    rootCommand.AddOption(chunkSizeOption);
    
    // Set up the main command handler
    rootCommand.SetHandler(
        async (string outputPath, string sourceFilePath, int chunkSize) => {
            await HandleCommandExecution(outputPath, sourceFilePath, chunkSize, config);
        }, 
        outputFileOption, 
        sourceFileOption,
        chunkSizeOption
    );
    
    return rootCommand;
}

// Handle the main command execution
async Task HandleCommandExecution(string outputPath, string sourceFilePath, int chunkSize, SorterConfiguration config)
{
    if (!File.Exists(sourceFilePath))
    {
        ConsoleLogger.Write(() =>
        {
            Console.WriteLine("Source file doesn't exist, specify one with '--input' flag");
        }, ConsoleColor.Red);
        return;
    }

    var performanceMonitor = new PerformanceMonitor();
    
    try
    {
        performanceMonitor.StartMonitoring("Total Operation");
        var sortOperation = new FileSortOperation(sourceFilePath, outputPath, chunkSize, config.TempPath);
        await sortOperation.ExecuteAsync();
        
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
            Console.WriteLine($"Something went wrong: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }, ConsoleColor.Red);
    }
    finally
    {
        var elapsed = performanceMonitor.StopMonitoring("Total Operation");
        ConsoleLogger.PrintElapsedTime(elapsed, "Run time");
    }
}
