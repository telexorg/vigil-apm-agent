using System.Text.Json;
using System.Text.Json.Serialization;
using VigilAgent.Api.Commons;
using VigilAgent.Api.Dtos;

namespace VigilAgent.Api.Helpers
{
    public class TelemetryTask
    {       
        public string Message { get; set; }
        
        public string ContextId { get; set; } 
        
        public string? TaskId { get; set; }

        public string MessageId { get; set; }

        public List<Setting>? Settings { get; set; } = new();

        public static TelemetryTask ExtractTaskData(TaskRequest request)
        {
            var message = request?.Params?.Message;

            if (message == null || message.Parts == null || !message.Parts.Any())
                throw new ArgumentException("Invalid message structure");

            return new TelemetryTask
            {
                Message = message.Parts.First().Text,
                ContextId = message.ContextId,
                TaskId = message.TaskId,
                MessageId = message.MessageId,
                //OrganizationId = request.Params.Metadata != null &&
                //                 request.Params.Metadata.TryGetValue("org_id", out var org)
                //                 ? org?.ToString()
                //                 : null,
                Settings = request.Params.Metadata != null &&
                           request.Params.Metadata.TryGetValue("settings", out var settingsObj)
                           ? JsonSerializer.Deserialize<List<Setting>>(settingsObj.ToString())
                           : new List<Setting>()
            };
        }
    }
}
