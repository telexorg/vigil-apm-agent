namespace VigilAgent.Apm.Contracts
{
    public interface ITelemetryClient
    {
        Task<bool> SendBatchAsync(List<object> events);
    }
}