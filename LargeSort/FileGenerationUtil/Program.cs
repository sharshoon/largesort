using System.CommandLine;
using System.Reflection;
using System.Text;
using FileGenerationUtil;
using FileGenerationUtil.Configuration;
using FileGenerationUtil.Interfaces;
using FileGenerationUtil.Operations;

// Entry point for the application
return await RunApplication(args);

// Main application execution function
async Task<int> RunApplication(string[] args)
{
    // Initialize application configuration
    var config = new GeneratorConfiguration();
    
    // Configure and build command line interface
    var rootCommand = BuildCommandLineInterface(config);
    
    // Execute the command
    return await rootCommand.InvokeAsync(args);
}

// Build the command line interface
RootCommand BuildCommandLineInterface(GeneratorConfiguration config)
{
    var rootCommand = new RootCommand("Utility to generate file with random data in format 'Number.String'");
    
    // Add init command
    var initSourceCommand = new InitCommand(config.DefaultInputForSourceGeneration, config.GeneratedSourceFilePath);
    rootCommand.AddCommand(initSourceCommand);
    
    // Configure options
    var options = CreateCommandOptions(config);
    foreach (var option in options)
    {
        rootCommand.AddOption(option.Value);
    }
    
    // Set up the main command handler
    rootCommand.SetHandler(
        (string outputPath, long contentSize, string sourceFilePath) => 
            HandleCommandExecution(outputPath, contentSize, sourceFilePath, config),
        (Option<string>)options["output"],
        (Option<long>)options["size"],
        (Option<string>)options["source"]
    );
    
    return rootCommand;
}

// Create command line options
Dictionary<string, Option> CreateCommandOptions(GeneratorConfiguration config)
{
    return new Dictionary<string, Option>
    {
        ["output"] = new Option<string>(
            name: "--output", 
            getDefaultValue: () => config.DefaultFilepath, 
            description: "The file to save generated content to"),
            
        ["size"] = new Option<long>(
            name: "--size", 
            getDefaultValue: () => 1024 * 1024 * 1024, 
            description: "The generated content size in bytes"),
            
        ["source"] = new Option<string>(
            name: "--source", 
            getDefaultValue: () => config.GeneratedSourceFilePath, 
            description: "The source file for string generation")
    };
}

// Handle the main command execution
async Task HandleCommandExecution(string outputPath, long contentSize, string sourceFilePath, GeneratorConfiguration config)
{
    if (!File.Exists(sourceFilePath))
    {
        ConsoleLogger.Write(() =>
        {
            Console.WriteLine("Source file doesn't exist, specify one with '--source' flag, or create default running 'init' command");
        }, ConsoleColor.Red);
        return;
    }
    
    var performanceMonitor = new PerformanceMonitor();
    
    try
    {
        performanceMonitor.StartMonitoring("File Generation");
        
        // Use operation pattern
        var operation = new FileGenerationOperation(outputPath, contentSize, sourceFilePath);
        await operation.ExecuteAsync();
        
        ConsoleLogger.Write(() =>
        {
            Console.WriteLine("Content was generated and written to:");
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
        var elapsed = performanceMonitor.StopMonitoring("File Generation");
        ConsoleLogger.Write(() =>
        {
            Console.WriteLine($"Total time: {elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds:000}");
        }, ConsoleColor.Cyan);
        
        ConsoleLogger.Write(() =>
        {
            Console.WriteLine("Press any key to exit...");
        }, ConsoleColor.Yellow);
        Console.ReadKey();
    }
}
