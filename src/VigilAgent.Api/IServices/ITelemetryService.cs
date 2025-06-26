using VigilAgent.Api.Dtos;
using VigilAgent.Api.Helpers;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.IServices
{
    public interface ITelemetryService
    {
        Task AddErrorAsync(Error error);
        Task AddMetricAsync(Metric metric);
        Task AddTraceAsync(Trace trace);
        Task EnsureWarmCacheAsync();
        Task<string> GetErrors(string projectName, string timeRange = null);
        Task<string> GetLogs(string projectName, string timeRange = null);
        Task<string> GetMetrics(string projectName, string timeRange = null);
    }
}
