using System.Collections.Concurrent;
using System.Text.Json;
using VigilAgent.Api.Helpers;
using VigilAgent.Api.IServices;
using VigilAgent.Api.Models;
using VigilAgent.Api.Services;
using VigilAgent.Apm.Telemetry;

namespace VigilAgent.Api.Services
{
    public class TelemetryHandler : ITelemetryHandler
    {
        public readonly static ConcurrentDictionary<string, ErrorDetail> Errors;
        public readonly static ConcurrentDictionary<string, Trace> Traces;
        public readonly static ConcurrentDictionary<string, Metric> Metrics;

        static TelemetryHandler()
        {
            Errors = new();
            Traces = new();
            Metrics = new();
        }
        public async Task<string> HandleErrorAsync(string message)
        {
            var errors = TelemetryHandler.Errors.Values
                .OrderByDescending(e => e.Timestamp)
                .Take(5)
                .Select(e =>
                    $"❌ **{e.ExceptionType}** - {e.Message}\n" +
                    $"    ↳ Path: {e.HttpMethod} {e.Url}\n" +
                    $"    ↳ Trace ID: {e.TraceId}\n" +
                    $"    ↳ Time: {e.Timestamp:yyyy-MM-dd HH:mm:ss}\n" +
                    $"    ↳ Stack: {TrimStack(e.StackTrace, 2)}")
                .ToList();

            var response = errors.Any()
                ? "🚨 **Recent Errors**\n\n" + string.Join("\n\n", errors)
                : "No errors recorded.";

            //if (TelemetryService.Errors.Count > 1000)
            //{
            //    var oldErrors = TelemetryService.Errors.Values.ToList();

            //    // Persist to file (append mode)
            //    await TelemetryFileStore.AppendBatchAsync(oldErrors);

            //    // Trim or clear after persistence
            //    TelemetryService.Errors.Clear();
            //}

            return await Task.FromResult(JsonSerializer.Serialize(errors));
        }

        private static string TrimStack(string stackTrace, int lines = 3)
        {
            if (string.IsNullOrWhiteSpace(stackTrace)) return "(no stack trace)";
            var trimmed = string.Join("\n", stackTrace.Split('\n').Take(lines));
            return trimmed.Trim();
        }

        public Task<string> HandleTraceAsync(string message)
        {
            var logs = TelemetryHandler.Traces.Values
                .OrderByDescending(t => t.Timestamp)
                .Take(5)
                .Select(t =>
                    $"[{(t.isError ? "❌ Error" : "✅ Success")}] {t.Method} {t.Path}\n" +
                    $"    ↳ Duration: {t.DurationMs}ms | Status: {t.StatusCode}\n" +
                    $"    ↳ Trace ID: {t.TraceId}\n" +
                    $"    ↳ Timestamp: {t.Timestamp:yyyy-MM-dd HH:mm:ss}")
                .ToList();

            var response = logs.Any()
                ? "📄 **Recent Trace Logs**\n\n" + string.Join("\n\n", logs)
                : "No trace logs available.";

            return Task.FromResult(response);
        }

        public Task<string> HandleMetricAsync(string message)
        {
            var metrics = TelemetryHandler.Metrics.Values
                .OrderByDescending(m => m.Timestamp)
                .Take(5)
                .Select(m =>
                    $"🧠 **Runtime Snapshot** @ {m.Timestamp:yyyy-MM-dd HH:mm:ss}\n" +
                    $"    ↳ CPU Usage: {m.CpuUsagePercent}%\n" +
                    $"    ↳ Memory: {m.MemoryUsageBytes / 1024 / 1024} MB\n" +
                    $"    ↳ GC Gen0 Collections: {m.Gen0Collections}\n" +
                    $"    ↳ Worker Threads Available: {m.AvailableWorkerThreads}")
                .ToList();

            var response = metrics.Any()
                ? "📊 **Runtime Metrics (Recent)**\n\n" + string.Join("\n\n", metrics)
                : "No runtime metrics recorded.";

            return Task.FromResult(response);
        }

        public Task<string> HandleAsync(string message)
        {
            // Dummy rule: recommend based on error volume
            int errorCount = TelemetryHandler.Errors.Count;

            string recommendation = errorCount > 5
                ? "High error rate. Consider checking authentication or service availability."
                : "System seems stable. No immediate action required.";

            return Task.FromResult("🛠 Recommendation: " + recommendation);
        }
    }
    
}



