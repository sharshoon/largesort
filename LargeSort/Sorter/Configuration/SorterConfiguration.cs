namespace Sorter.Configuration;

public class SorterConfiguration
{
    public string CurrentDirectory { get; }
    public string DefaultOutputPath { get; }
    public string TempPath { get; }
    public int DefaultChunkSize { get; }
    
    public SorterConfiguration()
    {
        CurrentDirectory = Directory.GetCurrentDirectory();
        DefaultOutputPath = Path.Combine(CurrentDirectory, "sorted.txt");
        TempPath = Path.Combine(".", "temp", Guid.NewGuid().ToString("N"));
        DefaultChunkSize = 1_000_000;
    }
    
    public string GetChunksDirectory() => Path.Combine(TempPath, "chunks");
    
    public string GetSortedChunksDirectory() => Path.Combine(TempPath, "sorted");
}
