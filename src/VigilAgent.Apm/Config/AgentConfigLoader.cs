using System;

namespace VigilAgent.Apm.Config
{
    public static class AgentConfigLoader
    {
        public static TelemetryOptions Load()
        {
            return new TelemetryOptions
            {
                FlushIntervalMs = GetInt("VIGIL_FLUSH_INTERVAL_MS", 5000),
                MaxBatchSize = GetInt("VIGIL_MAX_BATCH_SIZE", 10),
                ApiKey = GetEnv("VIGIL_API_KEY", null),
                ServiceName = GetEnv("VIGIL_SERVICE_NAME", "default-service"),
                Environment = GetEnv("VIGIL_ENVIRONMENT", "development"),
                EnableRuntimeMetrics = GetBool("VIGIL_ENABLE_METRICS", true),
                EnableExceptionLogging = GetBool("VIGIL_ENABLE_EXCEPTIONS", true),
                EnableDebugLogs = GetBool("VIGIL_ENABLE_DEBUG", false)
            };
        }

        private static string GetEnv(string key, string fallback) =>
            Environment.GetEnvironmentVariable(key) ?? fallback;

        private static int GetInt(string key, int fallback) =>
            int.TryParse(Environment.GetEnvironmentVariable(key), out var val) ? val : fallback;

        private static bool GetBool(string key, bool fallback) =>
            bool.TryParse(Environment.GetEnvironmentVariable(key), out var val) ? val : fallback;
    }
}
