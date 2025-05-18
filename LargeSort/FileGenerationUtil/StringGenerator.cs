using FileGenerationUtil.Interfaces;

namespace FileGenerationUtil;

public class StringGenerator : IContentGenerator<string>
{
    protected string[] _sourceForGeneration;
    public StringGenerator(IEnumerable<string> sourceForGeneration)
    {
        _sourceForGeneration = sourceForGeneration.ToArray();
    }

    private Random _rand = new();
    public string GetNext()
    {
        int index = _rand.Next(_sourceForGeneration.Length);

        return _sourceForGeneration[index];
    }
}