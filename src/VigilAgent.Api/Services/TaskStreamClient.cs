using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text;
using VigilAgent.Api.Commons;
using VigilAgent.Api.Contracts;

namespace VigilAgent.Api.Services
{
    public class TaskStreamClient : IAsyncDisposable, ITaskStreamClient
    {
        private ClientWebSocket _webSocket;
        private readonly ILogger<TaskStreamHandler> _logger;
        private readonly Uri _uri;

        public TaskStreamClient(IOptions<TelexApiSettings> options, ILogger<TaskStreamHandler> logger)
        {
            _uri = new Uri(options.Value.WebSocketUrl);
            _logger = logger;
        }

        public WebSocketState State => _webSocket?.State ?? WebSocketState.None;

        public async Task ConnectAsync(CancellationToken cancellation = default)
        {
            _webSocket = new ClientWebSocket();
            _logger.LogInformation("Connecting to chat stream: {Uri}", _uri);

            await _webSocket.ConnectAsync(_uri, cancellation);

            if (_webSocket.State != WebSocketState.Open)
                throw new InvalidOperationException("WebSocket connection failed.");
        }

        public async Task SendChunkAsync(string chunk, CancellationToken cancellation = default)
        {
            if (_webSocket == null || _webSocket.State != WebSocketState.Open)
                throw new InvalidOperationException("WebSocket is not connected.");

            var bytes = Encoding.UTF8.GetBytes(chunk);
            var buffer = new ArraySegment<byte>(bytes);
            await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, endOfMessage: true, cancellation);
        }

        public async ValueTask DisposeAsync()
        {
            if (_webSocket is { State: WebSocketState.Open })
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }

            _webSocket?.Dispose();
        }
    }
}
