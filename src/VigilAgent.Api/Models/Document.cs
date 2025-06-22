using System.Text.Json.Serialization;

namespace VigilAgent.Api.Models
{
    public class Document<T>
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("agent_id")]
        public string AgentId { get; set; }

        [JsonPropertyName("organisation_id")]
        public string OrganizationId { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
