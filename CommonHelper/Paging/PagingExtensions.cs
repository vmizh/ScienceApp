namespace CommonHelper.Paging;

public static class PagingExtensions
{
    public static PagedResult<T> ToPagedResult<T>(
        this IEnumerable<T> source,
        int pageNumber,
        int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var totalCount = source.Count();
        var items = source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    // Process all pages with an action
    public static void ForEachPage<T>(
        this IEnumerable<T> source,
        int pageSize,
        Action<IReadOnlyList<T>, int> pageAction)
    {
        if (pageSize <= 0) pageSize = 10;

        var list = source.ToList();
        var total = list.Count;
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        for (var page = 1; page <= totalPages; page++)
        {
            var pageItems = list
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            pageAction(pageItems, page);
        }
    }
}
