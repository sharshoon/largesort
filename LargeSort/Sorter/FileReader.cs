namespace Sorter;

public abstract class ContentReader
{
    public static async IAsyncEnumerable<Content> ReadFile(string filePath, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(filePath);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            yield return Content.Parse(line);
        }
    }
}