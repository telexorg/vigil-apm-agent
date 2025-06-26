

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using VigilAgent.Api.IRepositories;

namespace VigilAgent.Api.Models
{
    public class Message : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } 
        public string ContextId { get; set; }    
        public string TaskId { get; set; } 
        public string Role { get; set; } = null; 
        public string Content { get; set; } = null;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public Message()
        {
            Id = ObjectId.GenerateNewId().ToString();
        }
    }

}
