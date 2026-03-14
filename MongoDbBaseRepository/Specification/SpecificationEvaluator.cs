using MongoDB.Driver;
using Interfaces;

namespace MongoDbBaseRepository.Specification;

public static class SpecificationEvaluator
{
    /// <summary>
    ///     Apply specification to a collection and return a configured IFindFluent for further execution.
    /// </summary>
    public static IFindFluent<T, T> GetQuery<T>(IMongoCollection<T> collection, ISpecification<T> specification)
        where T : class
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        if (specification == null) throw new ArgumentNullException(nameof(specification));

        var filter = Builders<T>.Filter.Where(specification.ToExpression());
        var find = collection.Find(filter);

        if (specification.OrderBy != null)
        {
            find = find.Sort(specification.OrderByDescending
                ? Builders<T>.Sort.Descending(specification.OrderBy)
                : Builders<T>.Sort.Ascending(specification.OrderBy));
        }

        if (specification.Skip.HasValue)
            find = find.Skip(specification.Skip.Value);
        if (specification.Take.HasValue)
            find = find.Limit(specification.Take.Value);

        return find;
    }
}
