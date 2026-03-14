using System.Linq.Expressions;

namespace Interfaces;

public interface IBaseRepository<T> where T : IEntity
{
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    Task<long> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
