using BloggerAgent.Domain.Commons;
using VigilAgent.Api.Data;

namespace VigilAgent.Api.IRepositories
{
    public interface ITelexRepository<T> where T : IEntity
    {
        Task<bool> CreateAsync(T document);
        Task<bool> DeleteAsync(string id);
        Task<List<Document<T?>>> GetAllAsync(object filter);
        Task<Document<T?>> GetByIdAsync(string id);
        Task<bool> UpdateAsync(string id, T document);
        Task<List<Document<T?>>> FilterAsync(object filter);
    }
}