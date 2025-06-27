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
        public bool IsConversation { get; set; } = true;
        public string MessageId { get; set; }
        public string UserId { get; set; }
        public string OrgId { get; set; }
        public List<Setting>? Settings { get; set; } = new();
        
    }
    public class TaskContext
    {       
        public string Message { get; set; }        
        public string ContextId { get; set; }         
        public string? TaskId { get; set; }
        public string MessageId { get; set; }
        public string UserId { get; set; }
        public string OrgId { get; set; }
        public List<Setting>? Settings { get; set; } = new();
        
    }
}
