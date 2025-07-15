using System.Net.WebSockets;

namespace VigilAgent.Api.Contracts
{
    public interface ITaskStreamClient : IAsyncDisposable
    {
        Task ConnectAsync(CancellationToken cancellationToken = default);
        Task SendChunkAsync(string chunk, CancellationToken cancellationToken = default);
        WebSocketState State { get; }

    }
}
