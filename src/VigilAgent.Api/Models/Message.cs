

using VigilAgent.Api.IRepositories;

namespace VigilAgent.Api.Models
{
    public class Message : IEntity
    {
        
        public string Id { get; set; } 
        public string ContextId { get; set; }    
        public string TaskId { get; set; } 
        public string Role { get; set; } = null; 
        public string Content { get; set; } = null;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
