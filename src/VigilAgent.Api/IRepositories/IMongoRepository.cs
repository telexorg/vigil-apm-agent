using System.Linq.Expressions;

namespace VigilAgent.Api.IRepositories
{
    public interface IMongoRepository<T>
    {
        Task<long> CountAsync(Expression<Func<T, bool>>? filter = null);
        Task<T> Create(T entity);
        Task<long> DeleteAsync(Expression<Func<T, bool>> filter);
        Task<List<T>> FindAllAsync();
        Task<List<T>> FindManyAsync(Expression<Func<T, bool>> filter);
        Task<T?> FindOneAsync(Expression<Func<T, bool>> filter);
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter);
        Task<bool> UpdateAsync(Expression<Func<T, bool>> filter, T entity);
        Task<List<T>> GetLastNAsync(Expression<Func<T, bool>> filter, int limit, string sortField = "_id");
    }
}