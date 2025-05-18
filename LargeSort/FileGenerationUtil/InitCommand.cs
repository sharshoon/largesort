using System.CommandLine;
using System.Text;
using FileGenerationUtil.Operations;

namespace FileGenerationUtil;

public class InitCommand : Command
{
    const int minTextLength = 15;
    const int maxTextLength = 50;
    
    public InitCommand(string defaultInputForSourceGeneration, string sourceFilePath)
        : base("init", "Init the source file to be used during the generation")
    {
        var inputTextPathOption = new Option<string>(
            name: "--input",
            getDefaultValue: () => defaultInputForSourceGeneration,
            description: "Text file path to be used for source generation."
        );

        this.AddOption(inputTextPathOption);
        this.SetHandler(async (string input) =>
        {
            var performanceMonitor = new PerformanceMonitor();
            
            try
            {
                performanceMonitor.StartMonitoring("Source Initialization");
                
                var operation = new SourceInitOperation(input, sourceFilePath, minTextLength, maxTextLength);
                await operation.ExecuteAsync();
                
                ConsoleLogger.Write(() =>
                {
                    Console.WriteLine("Source file:");
                    Console.WriteLine(sourceFilePath);
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
                var elapsed = performanceMonitor.StopMonitoring("Source Initialization");
                ConsoleLogger.Write(() =>
                {
                    Console.WriteLine($"Initialization time: {elapsed.Seconds}.{elapsed.Milliseconds:000}s");
                }, ConsoleColor.Cyan);
            }
        }, inputTextPathOption);
    }
}
