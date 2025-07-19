namespace VigilAgent.Apm.Contracts
{
    public interface ITelemetryFlusher : IAsyncDisposable
    {
        ValueTask DisposeAsync();
        Task FlushAsync();
        void Start();
    }
}