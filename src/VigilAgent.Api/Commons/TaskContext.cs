using System.Text.Json;
using System.Text.Json.Serialization;
using VigilAgent.Api.Dtos;

namespace VigilAgent.Api.Commons
{
    public class TaskContext
    {
        public string Message { get; set; } = string.Empty;
        public string ContextId { get; set; } = string.Empty;
        public string? TaskId { get; set; }
        public string MessageId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string OrgId { get; set; } = string.Empty;

        public List<Setting> Settings { get; set; } = new();

        // Newly added fields
        public List<string> AcceptedOutputModes { get; set; } = new();
        public string CallbackUrl { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public int HistoryLength { get; set; } = 0;
        public bool IsBlocking { get; set; } = false;
        public bool IsConversation { get; set; } = true;


        // Optional: Telex Channel ID (for routing or broadcasting)
        public string ChannelId { get; set; } = string.Empty;

    }
    
  
}
