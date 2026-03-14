using Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;


namespace MangoDbBaseRepository;

public class MongoBaseRepository<T> : IBaseRepository<T> where T : class, IEntity
{
    private IMongoClient _client;
    protected IMongoCollection<T> Сollection => _db.GetCollection<T>(CollectionName);
    private readonly ILogger<MongoBaseRepository<T>> _logger;
    private  IMongoDatabase _db => _client.GetDatabase(DatabaseName);

    public string CollectionName { set; get; }
    public string DatabaseName { set; get; }

    public MongoBaseRepository(IMongoClient client, ILogger<MongoBaseRepository<T>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        try
        {
            await Сollection.InsertOneAsync(entity, null, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Inserted document {Id} into {Collection}", entity.Id,
                Сollection.CollectionNamespace.CollectionName);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting document into {Collection}",
                Сollection.CollectionNamespace.CollectionName);
            throw;
        }
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, id);
            return await Сollection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document {Id} from {Collection}", id,
                Сollection.CollectionNamespace.CollectionName);
            throw;
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Сollection.Find(Builders<T>.Filter.Empty).ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all documents from {Collection}",
                Сollection.CollectionNamespace.CollectionName);
            throw;
        }
    }

    public async Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        if (filter == null) throw new ArgumentNullException(nameof(filter));
        try
        {
            return await Сollection.Find(filter).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding documents in {Collection}", Сollection.CollectionNamespace.CollectionName);
            throw;
        }
    }

    public async Task<IEnumerable<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        if (specification == null) throw new ArgumentNullException(nameof(specification));
        try
        {
            var filter = specification.ToExpression();
            var find = Сollection.Find(filter);
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

            return await find.ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing documents in {Collection}", Сollection.CollectionNamespace.CollectionName);
            throw;
        }
    }

    public async Task<long> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        if (specification == null) throw new ArgumentNullException(nameof(specification));
        try
        {
            return await Сollection.CountDocumentsAsync(specification.ToExpression(), null, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting documents in {Collection}", Сollection.CollectionNamespace.CollectionName);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        try
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, entity.Id);
            var result = await Сollection
                .ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = false }, cancellationToken)
                .ConfigureAwait(false);
            var success = result.ModifiedCount > 0;
            _logger.LogInformation("Update {Id} in {Collection} success={Success}", entity.Id,
                Сollection.CollectionNamespace.CollectionName, success);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document {Id} in {Collection}", entity.Id,
                Сollection.CollectionNamespace.CollectionName);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, id);
            var result = await Сollection.DeleteOneAsync(filter, cancellationToken).ConfigureAwait(false);
            var success = result.DeletedCount > 0;
            _logger.LogInformation("Delete {Id} from {Collection} success={Success}", id,
                Сollection.CollectionNamespace.CollectionName, success);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {Id} from {Collection}", id,
                Сollection.CollectionNamespace.CollectionName);
            throw;
        }
    }
}
