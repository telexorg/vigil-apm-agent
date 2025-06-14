using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VigilAgent.Apm.Config;
using VigilAgent.Apm.Telemetry;

namespace VigilAgent.Apm.Transport
{
    public class TelemetrySender
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        // Could be loaded from config/env
        private static readonly string _endpoint = "https://localhost:7116/api/Telemetry";

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

}
