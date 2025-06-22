using System.Text.Json;
using System.Text.Json.Serialization;
using VigilAgent.Api.Dtos;

namespace VigilAgent.Api.Commons
{
    public class TelemetryTask
    {       
        public string Message { get; set; }
        
        public string ContextId { get; set; } 
        
        public string? TaskId { get; set; }

        public string MessageId { get; set; }

        public List<Setting>? Settings { get; set; } = new();
        
    }
}
