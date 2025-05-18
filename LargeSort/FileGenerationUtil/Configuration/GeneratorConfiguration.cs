using System.Reflection;

namespace FileGenerationUtil.Configuration;

public class GeneratorConfiguration
{
    public string CurrentDirectory { get; }
    public string DefaultFilepath { get; }
    public string ExeDirectory { get; }
    public string DefaultInputForSourceGeneration { get; }
    public string GeneratedSourceFilePath { get; }
    
    public GeneratorConfiguration()
    {
        CurrentDirectory = Directory.GetCurrentDirectory();
        DefaultFilepath = Path.Combine(CurrentDirectory, "default.txt");
        
        var exePath = Assembly.GetExecutingAssembly().Location;
        ExeDirectory = Path.GetDirectoryName(exePath) ?? CurrentDirectory;
        
        DefaultInputForSourceGeneration = Path.Combine(ExeDirectory, "loremIpsum.txt");
        GeneratedSourceFilePath = Path.Combine(ExeDirectory, "source.txt");
    }
}
