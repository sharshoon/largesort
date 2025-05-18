using System.CommandLine;
using System.Reflection;
using System.Text;
using FileGenerationUtil;
using FileGenerationUtil.Interfaces;

// Entry point for the application
return await RunApplication(args);

// Main application execution function
async Task<int> RunApplication(string[] args)
{
    // Get paths for files 
    var (currentDirectory, defaultFilepath, exeDirectory, defaultInputForSourceGeneration, generatedSourceFilePath) = GetFilePaths();
    
    // Configure and build command line interface
    var rootCommand = BuildCommandLineInterface(defaultFilepath, defaultInputForSourceGeneration, generatedSourceFilePath);
    
    // Execute the command
    return await rootCommand.InvokeAsync(args);
}

// Get all necessary file paths
(string currentDirectory, string defaultFilepath, string exeDirectory, string defaultInputForSourceGeneration, string generatedSourceFilePath) GetFilePaths()
{
    var currentDirectory = Directory.GetCurrentDirectory();
    var defaultFilepath = Path.Combine(currentDirectory, "default.txt");
    var exePath = Assembly.GetExecutingAssembly().Location;
    var exeDirectory = Path.GetDirectoryName(exePath);
    var defaultInputForSourceGeneration = Path.Combine(exeDirectory, "loremIpsum.txt");
    var generatedSourceFilePath = Path.Combine(exeDirectory, "source.txt");
    
    return (currentDirectory, defaultFilepath, exeDirectory, defaultInputForSourceGeneration, generatedSourceFilePath);
}

// Build the command line interface
RootCommand BuildCommandLineInterface(string defaultFilepath, string defaultInputForSourceGeneration, string generatedSourceFilePath)
{
    var rootCommand = new RootCommand("Utility to generate file with random data in format 'Number.String'");
    
    // Add init command
    var initSourceCommand = new InitCommand(defaultInputForSourceGeneration, generatedSourceFilePath);
    rootCommand.AddCommand(initSourceCommand);
    
    // Configure options
    var options = CreateCommandOptions(defaultFilepath, generatedSourceFilePath);
    foreach (var option in options)
    {
        rootCommand.AddOption(option.Value);
    }
    
    // Set up the main command handler
    rootCommand.SetHandler(
        HandleCommandExecution,
        (Option<string>)options["output"],
        (Option<long>)options["size"],
        (Option<string>)options["source"]
    );
    
    return rootCommand;
}

// Create command line options
Dictionary<string, Option> CreateCommandOptions(string defaultFilepath, string generatedSourceFilePath)
{
    return new Dictionary<string, Option>
    {
        ["output"] = new Option<string>(
            name: "--output", 
            getDefaultValue: () => defaultFilepath, 
            description: "The file to save generated content to"),
            
        ["size"] = new Option<long>(
            name: "--size", 
            getDefaultValue: () => 1024 * 1024 * 1024, 
            description: "The generated content size in bytes"),
            
        ["source"] = new Option<string>(
            name: "--source", 
            getDefaultValue: () => generatedSourceFilePath, 
            description: "The source file for string generation")
    };
}

// Handle the main command execution
async Task HandleCommandExecution(string outputPath, long contentSize, string sourceFilePath)
{
    if (!File.Exists(sourceFilePath))
    {
        ConsoleLogger.Write(() =>
        {
            Console.WriteLine("Source file doesn't exist, specify one with '--source' flag, or create default running 'init' command");
        }, ConsoleColor.Red);
        return;
    }
    
    try
    {
        await GenerateFile(outputPath, contentSize, sourceFilePath);
        
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
        ConsoleLogger.Write(() =>
        {
            Console.WriteLine("Press any key to exit...");
        }, ConsoleColor.Yellow);
        Console.ReadKey();
    }
}

// Generate file with random content
async Task GenerateFile(string path, long contentSize, string sourceFilePath)
{   
    var currentSize = 0;
    var source = await File.ReadAllLinesAsync(sourceFilePath);
    
    // Create generators
    IContentGenerator<string> textPartGenerator = new StringGenerator(source);
    IContentGenerator<int> numberPartGenerator = new NumberGenerator();
    
    await using var fs = File.Create(path);
    
    // Generate content until required size is reached
    int percentage = 10;
    while (currentSize < contentSize)
    {
        var textPart = textPartGenerator.GetNext();
        var numberPart = numberPartGenerator.GetNext().ToString();
        
        var line = $"{numberPart}.{textPart}\n";
        var buffer = Encoding.UTF8.GetBytes(line);
        
        await fs.WriteAsync(buffer, 0, buffer.Length);
        currentSize += buffer.Length;
        
        if ((double)currentSize / contentSize * 100.0 > percentage)
        {
            ConsoleLogger.Write(
                () =>
                {
                    Console.WriteLine($"{percentage}%");
                },
                ConsoleColor.Blue);
            percentage += 10;
        }
    }
}
