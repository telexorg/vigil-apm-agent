using VigilAgent.Api.Helpers;

namespace VigilAgent.Api.IRepositories
{
    public interface ITelemetryRepository<T>
    {
        Task<bool> AddAsync(T item);
        Task<List<T>> GetLastNAsync(int n);
        Task<Dictionary<string, List<T>>> GetLastNPerProjectAsync(int countPerProject);
    }
}