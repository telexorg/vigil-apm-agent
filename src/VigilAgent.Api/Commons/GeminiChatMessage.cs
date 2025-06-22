using System.Text.Json.Serialization;

namespace VigilAgent.Api.Commons
{
    public class GeminiChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; } = new();
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
