using VigilAgent.Api.Commons;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.IServices
{
    public interface IRequestProcessingService
    {
        Task<Request> ProcessUserInputAsync(TelemetryTask blogDto);
        string GetBlogIntervalOption(TelemetryTask blogDto);
    }
}