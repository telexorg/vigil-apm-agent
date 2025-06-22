using System.Text.Json;

namespace VigilAgent.Api.Helpers
{
    public static class TelemetryFileStore
    {
        private static readonly string StorePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "telemetry-log.json");

        static TelemetryFileStore()
        {
            if (!Directory.Exists(StorePath))
            {
               var directory = Directory.CreateDirectory(Path.GetDirectoryName(StorePath));
            }
        }

        public static async Task AppendBatchAsync(IEnumerable<object> events)
        {
            using var writer = new StreamWriter(StorePath, append: true);
            foreach (var evt in events)
            {
                string json = JsonSerializer.Serialize(evt);
                await writer.WriteLineAsync(json);
            }
        }
    }
}
