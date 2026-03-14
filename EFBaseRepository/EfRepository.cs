using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Interfaces;

namespace EFBaseRepository;

public class EfRepository<T> : IBaseRepository<T> where T : class, IEntity
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;
    private readonly ILogger<EfRepository<T>> _logger;

    public EfRepository(DbContext context, ILogger<EfRepository<T>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        try
        {
            await _dbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Inserted entity {Id} into {Set}", entity.Id, typeof(T).Name);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting entity into {Set}", typeof(T).Name);
            throw;
        }
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity {Id} from {Set}", id, typeof(T).Name);
            throw;
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all entities from {Set}", typeof(T).Name);
            throw;
        }
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        try
        {
            return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding entities in {Set}", typeof(T).Name);
            throw;
        }
    }

    public async Task<IEnumerable<T>> ListAsync(ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        if (specification == null) throw new ArgumentNullException(nameof(specification));
        try
        {
            var query = _dbSet.AsNoTracking().Where(specification.ToExpression());
            // apply includes for eager loading
            if (specification.Includes != null)
                foreach (var include in specification.Includes)
                    query = query.Include(include);

            if (specification.OrderBy != null)
                query = specification.OrderByDescending
                    ? query.OrderByDescending(specification.OrderBy)
                    : query.OrderBy(specification.OrderBy);

            if (specification.Skip.HasValue)
                query = query.Skip(specification.Skip.Value);
            if (specification.Take.HasValue)
                query = query.Take(specification.Take.Value);

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing entities in {Set}", typeof(T).Name);
            throw;
        }
    }

    public async Task<long> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        if (specification == null) throw new ArgumentNullException(nameof(specification));
        try
        {
            return await _dbSet.CountAsync(specification.ToExpression(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting entities in {Set}", typeof(T).Name);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        try
        {
            _dbSet.Update(entity);
            var changes = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var success = changes > 0;
            _logger.LogInformation("Update {Id} in {Set} success={Success}", entity.Id, typeof(T).Name, success);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity {Id} in {Set}", entity.Id, typeof(T).Name);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
            if (entity == null) return false;
            _dbSet.Remove(entity);
            var changes = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var success = changes > 0;
            _logger.LogInformation("Delete {Id} from {Set} success={Success}", id, typeof(T).Name, success);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity {Id} from {Set}", id, typeof(T).Name);
            throw;
        }
    }
}
