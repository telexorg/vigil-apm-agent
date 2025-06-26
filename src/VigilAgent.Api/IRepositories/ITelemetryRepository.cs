namespace VigilAgent.Api.IRepositories
{
    public interface ITelemetryRepository<T>
    {
        Task AddAsync(T item);
        Task<List<T>> GetLastNAsync(int n);
    }
}