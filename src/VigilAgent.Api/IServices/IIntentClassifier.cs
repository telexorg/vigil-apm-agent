using VigilAgent.Api.Commons;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.IServices
{
    public interface IIntentClassifier
    {
        Task<Request> ProcessUserInputAsync(TelemetryTask blogDto);
    }
}