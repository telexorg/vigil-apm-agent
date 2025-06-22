using VigilAgent.Api.Dtos;
using VigilAgent.Api.Helpers;

namespace VigilAgent.Api.IServices
{
    public interface ITelemetryHandler
    {
        Task<string> HandleAsync(string message);
        Task<string> HandleErrorAsync(string message);
        Task<string> HandleMetricAsync(string message);
        Task<string> HandleTraceAsync(string message);
        
        //Task<MessageResponse> HandleAsync(TaskRequest taskRequest);
        //Task<bool> SendResponseAsync(string blogPost, TelemetryTask blogDto);
    }
}
