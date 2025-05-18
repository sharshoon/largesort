namespace Sorter;

public class ContentComparator : IComparer<Content>
{
    public static ContentComparator Default = new ContentComparator();
    public int Compare(Content? x, Content? y)
    {
        switch (x)
        {
            case null when y == null:
                return 0;
            case null:
                return -1;
        }

        if (y == null)
            return 1;

        var stringComparisonResult = string.Compare(x.String, y.String, StringComparison.Ordinal);

        return stringComparisonResult == 0
            ? x.Number.CompareTo(y.Number)
            : stringComparisonResult;
    }
}