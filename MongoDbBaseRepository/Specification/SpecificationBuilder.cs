using System.Linq.Expressions;
using Interfaces;

namespace MongoDbBaseRepository.Specification;

public static class SpecificationBuilder
{
    public static ISpecification<T> Create<T>(Expression<Func<T, bool>> expression, int? skip = null, int? take = null,
        Expression<Func<T, object>>? orderBy = null, bool orderByDescending = false) where T : class
    {
        var spec = new BaseSpecification<T>(expression)
        {
            Skip = skip,
            Take = take,
            OrderBy = orderBy,
            OrderByDescending = orderByDescending
        };
        return spec;
    }

    public static ISpecification<T> And<T>(ISpecification<T> a, ISpecification<T> b) where T : class
    {
        if (a == null) return b ?? throw new ArgumentNullException(nameof(b));
        if (b == null) return a;
        var combined = a.And(b);
        // preserve paging / ordering from first spec if present
        if (a.Skip.HasValue) ((BaseSpecification<T>)combined).Skip = a.Skip;
        if (a.Take.HasValue) ((BaseSpecification<T>)combined).Take = a.Take;
        if (a.OrderBy != null) ((BaseSpecification<T>)combined).OrderBy = a.OrderBy;
        ((BaseSpecification<T>)combined).OrderByDescending = a.OrderByDescending;
        return combined;
    }

    public static ISpecification<T> Or<T>(ISpecification<T> a, ISpecification<T> b) where T : class
    {
        if (a == null) return b ?? throw new ArgumentNullException(nameof(b));
        if (b == null) return a;
        var combined = a.Or(b);
        if (a.Skip.HasValue) ((BaseSpecification<T>)combined).Skip = a.Skip;
        if (a.Take.HasValue) ((BaseSpecification<T>)combined).Take = a.Take;
        if (a.OrderBy != null) ((BaseSpecification<T>)combined).OrderBy = a.OrderBy;
        ((BaseSpecification<T>)combined).OrderByDescending = a.OrderByDescending;
        return combined;
    }

    public static ISpecification<T> Not<T>(ISpecification<T> spec) where T : class
    {
        if (spec == null) throw new ArgumentNullException(nameof(spec));
        return spec.Not();
    }

    public static ISpecification<T> CombineAnd<T>(IEnumerable<ISpecification<T>> specs) where T : class
    {
        if (specs == null) throw new ArgumentNullException(nameof(specs));
        var list = specs.Where(s => s != null).ToList();
        if (list.Count == 0) return Create<T>(x => true);
        var result = list[0];
        for (var i = 1; i < list.Count; i++) result = result.And(list[i]);
        // preserve paging/ordering from first
        if (list[0].Skip.HasValue) ((BaseSpecification<T>)result).Skip = list[0].Skip;
        if (list[0].Take.HasValue) ((BaseSpecification<T>)result).Take = list[0].Take;
        if (list[0].OrderBy != null) ((BaseSpecification<T>)result).OrderBy = list[0].OrderBy;
        ((BaseSpecification<T>)result).OrderByDescending = list[0].OrderByDescending;
        return result;
    }

    public static ISpecification<T> CombineOr<T>(IEnumerable<ISpecification<T>> specs) where T : class
    {
        if (specs == null) throw new ArgumentNullException(nameof(specs));
        var list = specs.Where(s => s != null).ToList();
        if (list.Count == 0) return Create<T>(x => false);
        var result = list[0];
        for (var i = 1; i < list.Count; i++) result = result.Or(list[i]);
        if (list[0].Skip.HasValue) ((BaseSpecification<T>)result).Skip = list[0].Skip;
        if (list[0].Take.HasValue) ((BaseSpecification<T>)result).Take = list[0].Take;
        if (list[0].OrderBy != null) ((BaseSpecification<T>)result).OrderBy = list[0].OrderBy;
        ((BaseSpecification<T>)result).OrderByDescending = list[0].OrderByDescending;
        return result;
    }
}
