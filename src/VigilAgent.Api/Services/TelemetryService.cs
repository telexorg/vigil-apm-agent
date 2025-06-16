using System.Collections.Concurrent;
using VigilAgent.Api.IServices;
using VigilAgent.Api.Models;
using VigilAgent.Api.Services;
using VigilAgent.Apm.Telemetry;

namespace VigilAgent.Api.Services
{
    public class TelemetryService
    {
        public readonly static ConcurrentDictionary<string, ErrorDetail> Errors;
        public readonly static ConcurrentDictionary<string, Trace> Traces;
        public readonly static ConcurrentDictionary<string, Metric> Metrics;

        static TelemetryService()
        {
            Errors = new();
            Traces = new();
            Metrics = new();
        }
    }


    // ExplainErrorsHandler.cs
    public class ExplainErrorsHandler : IAgentCommandHandler
    {
        public Task<string> HandleAsync(string message)
        {
            var errors = TelemetryService.Errors.Values
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

            return Task.FromResult(response);
        }

        private static string TrimStack(string stackTrace, int lines = 3)
        {
            if (string.IsNullOrWhiteSpace(stackTrace)) return "(no stack trace)";
            var trimmed = string.Join("\n", stackTrace.Split('\n').Take(lines));
            return trimmed.Trim();
        }
    }



    // ShowLogsHandler.cs
    public class ShowLogsHandler : IAgentCommandHandler
    {
        public Task<string> HandleAsync(string message)
        {
            var logs = TelemetryService.Traces.Values
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
    }


    public class ShowRuntimeMetrics : IAgentCommandHandler
    {
        public Task<string> HandleAsync(string message)
        {
            var metrics = TelemetryService.Metrics.Values
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
    }

    // RecommendFixHandler.cs
    public class RecommendFixHandler : IAgentCommandHandler
    {
        public Task<string> HandleAsync(string message)
        {
            // Dummy rule: recommend based on error volume
            int errorCount = TelemetryService.Errors.Count;

            string recommendation = errorCount > 5
                ? "High error rate. Consider checking authentication or service availability."
                : "System seems stable. No immediate action required.";

            return Task.FromResult("🛠 Recommendation: " + recommendation);
        }
    }

}



