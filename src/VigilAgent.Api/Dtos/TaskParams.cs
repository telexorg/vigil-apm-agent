using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Api.Dtos
{
    public class TaskParams
    {
        public TaskMessage Message { get; set; }
        public TaskConfiguration? Configuration { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class TaskMessage
    {
        public string? TaskId { get; set; }
        public string MessageId { get; set; }
        public string ContextId { get; set; }
        public string Role { get; set; }
        public string? Kind { get; set; }
        public List<MessagePart> Parts { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }

    }

    public class MessagePart
    {
        public string Type { get; set; }   // e.g., "text"
        public string Text { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
