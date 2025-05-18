using FileGenerationUtil.Interfaces;

namespace FileGenerationUtil;

public class StringGenerator : IContentGenerator<string>
{
    protected string[] _sourceForGeneration;
    private readonly Random _rand = new();
    
    public StringGenerator(IEnumerable<string> sourceForGeneration)
    {
        _sourceForGeneration = sourceForGeneration.ToArray();
        
        if (_sourceForGeneration.Length == 0)
        {
            throw new ArgumentException("Source for string generation cannot be empty");
        }
    }

    public string GetNext()
    {
        int index = _rand.Next(_sourceForGeneration.Length);
        return _sourceForGeneration[index];
    }
}
