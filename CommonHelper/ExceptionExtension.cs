namespace CommonHelper;

public static class ExceptionExtension
{
    public static string GetAllMessages(this Exception ex, string separator = " | ")
    {
        if (ex == null) return string.Empty;

        var messages = new List<string>();
        CollectMessages(ex, messages);

        return string.Join(separator, messages.Where(m => !string.IsNullOrWhiteSpace(m)));
    }

    private static void CollectMessages(Exception ex, List<string> messages)
    {
        if (ex == null) return;

        messages.Add(ex.Message);

        if (ex is AggregateException agg)
            foreach (var inner in agg.InnerExceptions)
                CollectMessages(inner, messages);
        else if (ex.InnerException != null) CollectMessages(ex.InnerException, messages);
    }
}
