using System.Linq.Expressions;
using Interfaces;

namespace MongoDbBaseRepository.Specification;

public class BaseSpecification<T> : ISpecification<T>
{
    private readonly Expression<Func<T, bool>> _expression;

    public BaseSpecification(Expression<Func<T, bool>> expression)
    {
        _expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public Expression<Func<T, bool>> ToExpression()
    {
        return _expression;
    }

    public int? Skip { get; set; }
    public int? Take { get; set; }
    public Expression<Func<T, object>>? OrderBy { get; set; }
    public bool OrderByDescending { get; set; }

    public IEnumerable<Expression<Func<T, object>>>? Includes => throw new NotImplementedException();

    public ISpecification<T> And(ISpecification<T> other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        var invoked = Expression.Invoke(other.ToExpression(), _expression.Parameters);
        var body = Expression.AndAlso(_expression.Body, invoked);
        var lambda = Expression.Lambda<Func<T, bool>>(body, _expression.Parameters);
        return new BaseSpecification<T>(lambda);
    }

    public ISpecification<T> Or(ISpecification<T> other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        var invoked = Expression.Invoke(other.ToExpression(), _expression.Parameters);
        var body = Expression.OrElse(_expression.Body, invoked);
        var lambda = Expression.Lambda<Func<T, bool>>(body, _expression.Parameters);
        return new BaseSpecification<T>(lambda);
    }

    public ISpecification<T> Not()
    {
        var body = Expression.Not(_expression.Body);
        var lambda = Expression.Lambda<Func<T, bool>>(body, _expression.Parameters);
        return new BaseSpecification<T>(lambda);
    }
}
