namespace ScientificApp.RandomHistService.Helper;

public static class RangeSplitter
{
    public static IEnumerable<(int Start, int End)> SplitRange(int start, int end, int n)
    {
        for (var i = start; i <= end; i += n)
        {
            var rangeEnd = Math.Min(i + n - 1, end);
            yield return (i, rangeEnd);
        }
    }
}
