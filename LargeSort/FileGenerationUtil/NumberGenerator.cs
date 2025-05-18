using FileGenerationUtil.Interfaces;

namespace FileGenerationUtil;

public class NumberGenerator : IContentGenerator<int>
{
    private Random _rand = new();

    public int GetNext()
    {
        return _rand.Next();
    }
}