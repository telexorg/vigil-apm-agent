
namespace VigilAgent.Apm.Processing
{
    public interface ITelemetryClient
    {
        Task<bool> SendBatchAsync(List<object> events);
    }
}