using Interfaces;
using System.Linq.Expressions;

namespace EFBaseRepository.Specification;

public class EFBaseSpecification<T> : ISpecification<T>
{
    private readonly Expression<Func<T, bool>> _expression;

    public EFBaseSpecification(Expression<Func<T, bool>> expression)
    {
        _expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public Expression<Func<T, bool>> ToExpression()
    {
        return _expression;
    }

    public int? Skip { get; init; }
    public int? Take { get; init; }
    public Expression<Func<T, object>>? OrderBy { get; init; }
    public bool OrderByDescending { get; init; }
    public IEnumerable<Expression<Func<T, object>>>? Includes { get; init; }

    public ISpecification<T> And(ISpecification<T> other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        var invoked = Expression.Invoke(other.ToExpression(), _expression.Parameters);
        var body = Expression.AndAlso(_expression.Body, invoked);
        var lambda = Expression.Lambda<Func<T, bool>>(body, _expression.Parameters);
        return new EFBaseSpecification<T>(lambda)
        {
            Skip = Skip,
            Take = Take,
            OrderBy = OrderBy,
            OrderByDescending = OrderByDescending,
            Includes = Includes
        };
    }

    public ISpecification<T> Or(ISpecification<T> other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        var invoked = Expression.Invoke(other.ToExpression(), _expression.Parameters);
        var body = Expression.OrElse(_expression.Body, invoked);
        var lambda = Expression.Lambda<Func<T, bool>>(body, _expression.Parameters);
        return new EFBaseSpecification<T>(lambda)
        {
            Skip = Skip,
            Take = Take,
            OrderBy = OrderBy,
            OrderByDescending = OrderByDescending,
            Includes = Includes
        };
    }

    public ISpecification<T> Not()
    {
        var body = Expression.Not(_expression.Body);
        var lambda = Expression.Lambda<Func<T, bool>>(body, _expression.Parameters);
        return new EFBaseSpecification<T>(lambda)
        {
            Skip = Skip,
            Take = Take,
            OrderBy = OrderBy,
            OrderByDescending = OrderByDescending,
            Includes = Includes
        };
    }
}
