using System.Linq.Expressions;
using MongoDB.Driver;
using Play.Common.Interfaces;

namespace Play.Common.MongoDb;

public class MongoRepository<T> : IRepository<T> where T : IEntity
{
    private readonly IMongoCollection<T> _dbCollection;
    private readonly FilterDefinitionBuilder<T> _filterDefinitionBuilder = Builders<T>.Filter;

    public MongoRepository(IMongoDatabase database, string collectionName)
    {
        _dbCollection = database.GetCollection<T>(collectionName);
    }

    public async Task<IReadOnlyCollection<T>> GetAllAsync()
        => await _dbCollection.Find(_filterDefinitionBuilder.Empty).ToListAsync();

    public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        => await _dbCollection.Find(filter).ToListAsync();


    public async Task<T> GetItemAsync(Guid id)
    {
        FilterDefinition<T> filterDefinition = _filterDefinitionBuilder.Eq(entity => entity.Id, id);
        return await _dbCollection.Find(filterDefinition).FirstOrDefaultAsync();
    }

    public async Task<T> GetItemAsync(Expression<Func<T, bool>> filter)
        => await _dbCollection.Find(filter).FirstOrDefaultAsync();
    public async Task CreateAsync(T entity)
    {
        EntityNullChecker(entity);
        await _dbCollection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        EntityNullChecker(entity);
        FilterDefinition<T> filterDefinition
            = _filterDefinitionBuilder.Eq(exsistingEntity => exsistingEntity.Id, entity.Id);
        await _dbCollection.ReplaceOneAsync(filterDefinition, entity);
    }

    public async Task RemoveAsync(Guid id)
    {
        if (id.Equals(Guid.Empty)) throw new ArgumentException(null, nameof(id));
        FilterDefinition<T> filterDefinition = _filterDefinitionBuilder.Eq(entity => entity.Id, id);
        await _dbCollection.DeleteOneAsync(filterDefinition);
    }

    private void EntityNullChecker(T entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
    }
}
