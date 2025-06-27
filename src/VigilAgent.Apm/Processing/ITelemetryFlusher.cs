
namespace VigilAgent.Apm.Processing
{
    public interface ITelemetryFlusher : IAsyncDisposable
    {
        ValueTask DisposeAsync();
        Task FlushAsync();
        void Start();
    }
}