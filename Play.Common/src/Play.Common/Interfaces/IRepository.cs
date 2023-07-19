using System.Linq.Expressions;

namespace Play.Common.Interfaces;

public interface IRepository<T> where T : IEntity
{
    Task CreateAsync(T entity);
    Task<IReadOnlyCollection<T>> GetAllAsync();
    Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter);
    Task<T> GetItemAsync(Guid id);
    Task<T> GetItemAsync(Expression<Func<T, bool>> filter);
    Task RemoveAsync(Guid id);
    Task UpdateAsync(T item);

}
