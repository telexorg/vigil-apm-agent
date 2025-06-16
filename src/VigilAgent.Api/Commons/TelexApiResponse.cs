using System.Text.Json;
using System.Text.Json.Serialization;

namespace VigilAgent.Api.Commons
{
    public class TelexApiResponse<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }

        public static TelexApiResponse<T> ExtractResponse(string jsonResponse)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            return JsonSerializer.Deserialize<TelexApiResponse<T>>(jsonResponse, options);
        }

        public static TelexApiResponse<T> ErrorResponse(string jsonResponse)
        {
            var errorResponse = new TelexApiResponse<T>();

            using (var document = JsonDocument.Parse(jsonResponse))
            {
                var root = document.RootElement;

                if (root.TryGetProperty("error", out var errorProp))
                    errorResponse.Error = errorProp.GetString();

                if (root.TryGetProperty("message", out var messageProp))
                    errorResponse.Message = messageProp.GetString();

                if (root.TryGetProperty("status", out var statusProp))
                    errorResponse.Status = statusProp.GetString();

                if (root.TryGetProperty("status_code", out var codeProp))
                    errorResponse.StatusCode = codeProp.GetInt32();
            }

            return errorResponse;
        }
    }
}
