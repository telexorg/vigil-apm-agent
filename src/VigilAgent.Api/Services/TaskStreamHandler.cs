using VigilAgent.Api.Contracts;
using VigilAgent.Api.IServices;

namespace VigilAgent.Api.Services
{
    public class TaskStreamHandler : ITaskStreamHandler
    {
        private readonly ITaskStreamClient _streamClient;

        public TaskStreamHandler(ITaskStreamClient streamClient)
        {
            _streamClient = streamClient;
        }

        public async Task StreamResponseAsync(IEnumerable<string> tokens)
        {
            await _streamClient.ConnectAsync();

            foreach (var token in tokens)
            {
                await _streamClient.SendChunkAsync(token);
                await Task.Delay(40);
            }
        }
    }
}
