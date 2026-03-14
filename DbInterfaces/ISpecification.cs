using System.Linq.Expressions;

namespace Interfaces;

public partial interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();

    // optional paging
    int? Skip { get; }
    int? Take { get; }

    // optional sorting expression and direction
    Expression<Func<T, object>>? OrderBy { get; }
    bool OrderByDescending { get; }

    // optional include expressions for EF eager-loading
    IEnumerable<Expression<Func<T, object>>>? Includes { get; }

    // Combine specifications
    ISpecification<T> And(ISpecification<T> other);
    ISpecification<T> Or(ISpecification<T> other);
    ISpecification<T> Not();
}
