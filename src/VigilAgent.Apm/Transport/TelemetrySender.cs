using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VigilAgent.Apm.Config;
using VigilAgent.Apm.Telemetry;
using VigilAgent.Apm.Utils;

namespace VigilAgent.Apm.Transport
{
    public class TelemetrySender
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        // Could be loaded from config/env
        private static readonly string _endpoint = "https://localhost:7116/api/v1/Telemetry";

        public static async Task ExportAsync(List<object> events)
        {
            if (string.IsNullOrWhiteSpace(_endpoint)) return;

            try
            {
                var json = JsonSerializer.Serialize(events, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_endpoint, content);

                response.EnsureSuccessStatusCode();

                Console.WriteLine($"[Exporter] Exported {events.Count} items to {_endpoint}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Exporter] Failed: {ex.Message}");
            }
        }
    
    }

    public class TelemetryClient
    {
        private readonly static HttpClient _httpClient = new();
        private readonly static string _endpoint = "https://localhost:7116/api/v1/Telemetry";
        private readonly static string _apiKey = "tF3H5L0EcDT//Yx+ccYpkDILPmkFsHd0zfkSc3w0Y0xs/zE4uY8Nb6tTyJEdQx3Z.8692c7a6";

        //public TelemetryClient(string apiKey)
        //{
        //    _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        //}

        public static async Task SendBatchAsync(List<object> events)
        {
            if (events is null || events.Count == 0) return;

            var json = JsonSerializer.Serialize(events, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var signature = ApiSignatureUtility.CreateSignature(json, _apiKey);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.Add("X-Api-Key", _apiKey);
            content.Headers.Add("X-Signature", signature);

            var response = await _httpClient.PostAsync(_endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[TelemetryClient] Failed: {response.StatusCode}");
            }
            else
            {
                Console.WriteLine($"[TelemetryClient] Sent {events.Count} telemetry events");
            }
        }
    }

}
