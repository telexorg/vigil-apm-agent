using System.Text.Json;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.IServices
{
    public interface ITelemetryHandler
    {
        Task HandleBatchItemsAsync(JsonElement batch, Project project);
    }
}