using VigilAgent.Api.Dtos;
using VigilAgent.Api.Helpers;

namespace VigilAgent.Api.IServices
{
    public interface IAgentCommandHandler
    {
        Task<string> HandleAsync(string message);


        //Task<MessageResponse> HandleAsync(TaskRequest taskRequest);
        //Task<bool> SendResponseAsync(string blogPost, TelemetryTask blogDto);
    }
}
