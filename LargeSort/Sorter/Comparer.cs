namespace Sorter;

public class ContentComparator : IComparer<Content>
{
    public int Compare(Content? x, Content? y)
    {
        if (x == y) return 0;

        var stringComparisonResult = string.Compare(x?.String, y?.String, StringComparison.Ordinal);

        return stringComparisonResult == 0
            ? x.Number.CompareTo(y.Number)
            : stringComparisonResult;
    }
}