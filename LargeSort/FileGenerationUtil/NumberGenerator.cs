using FileGenerationUtil.Interfaces;

namespace FileGenerationUtil;

public class NumberGenerator : IContentGenerator<int>
{
    private readonly Random _rand = new();
    private readonly int _minValue;
    private readonly int _maxValue;

    public NumberGenerator(int minValue = 0, int maxValue = int.MaxValue)
    {
        _minValue = minValue;
        _maxValue = maxValue;
    }

    public int GetNext()
    {
        return _rand.Next(_minValue, _maxValue);
    }
}
