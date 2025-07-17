using Microsoft.SemanticKernel;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text.Json;
using VigilAgent.Api.Commons;
using VigilAgent.Api.Helpers;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.IServices;
using VigilAgent.Api.Models;
using VigilAgent.Api.Services;
using VigilAgent.Apm.Telemetry;

namespace VigilAgent.Api.Services
{
    public class TelemetryService : ITelemetryService
    {
        private const int MaxCacheSize = 200;

        private readonly ITelemetryRepository<Trace> _traceRepo;
        private readonly ITelemetryRepository<Error> _errorRepo;
        private readonly ITelemetryRepository<Metric> _metricRepo;
        private readonly ITaskContextProvider _contextProvider;

        private readonly static ConcurrentDictionary<string, CappedConcurrentCache<Trace>> _traceBuckets = new();
        private readonly static ConcurrentDictionary<string, CappedConcurrentCache<Error>> _errorBuckets = new();
        private readonly static ConcurrentDictionary<string, CappedConcurrentCache<Metric>> _metricBuckets = new();
        private readonly ILogger<TelemetryService> _logger;

        public TelemetryService(
            ITelemetryRepository<Trace> traceRepo,
            ITelemetryRepository<Error> errorRepo,
            ITelemetryRepository<Metric> metricRepo,
            ITaskContextProvider contextProvider,
            ILogger<TelemetryService> logger)
        {
            _traceRepo = traceRepo;
            _errorRepo = errorRepo;
            _metricRepo = metricRepo;
            _contextProvider = contextProvider;
            _logger = logger;
        }

        public async Task AddTraceAsync(Trace trace)
        {
            var key = CacheKey.For(trace.OrgId, trace.ProjectName);
            var bucket = _traceBuckets.GetOrAdd(key, _ => new(MaxCacheSize));
            bucket.Add(trace);
            _logger.LogInformation("Trace added to cache for OrgId: {OrgId}, Project: {Project} with Key: {key}", trace.OrgId, trace.ProjectName, key);

            bool success = await _traceRepo.AddAsync(trace);
            if (success)
            {
                _logger.LogInformation("Trace saved to DB for OrgId: {OrgId}, Project: {Project}", trace.OrgId, trace.ProjectName);
            }
            else
            {
                _logger.LogWarning("Failed to save trace to DB for OrgId: {OrgId}, Project: {Project}", trace.OrgId, trace.ProjectName);
            }
        }

        public async Task AddErrorAsync(Error error)
        {
            var key = CacheKey.For(error.OrgId, error.ProjectName);
            var bucket = _errorBuckets.GetOrAdd(key, _ => new(MaxCacheSize));
            bucket.Add(error);
            _logger.LogInformation("Error added to cache for OrgId: {OrgId}, Project: {Project} with Key: {key}", error.OrgId, error.ProjectName, key);

            bool success = await _errorRepo.AddAsync(error);
            if (success)
            {
                _logger.LogInformation("Error saved to DB for OrgId: {OrgId}, Project: {Project}", error.OrgId, error.ProjectName);
            }
            else
            {
                _logger.LogWarning("Failed to save error to DB for OrgId: {OrgId}, Project: {Project}", error.OrgId, error.ProjectName);
            }
        }

        public async Task AddMetricAsync(Metric metric)
        {
            var key = CacheKey.For(metric.OrgId, metric.ProjectName);
            var bucket = _metricBuckets.GetOrAdd(key, _ => new(MaxCacheSize));
            bucket.Add(metric);
            _logger.LogInformation("Metric added to cache for OrgId: {OrgId}, Project: {Project} with {key}", metric.OrgId, metric.ProjectName, key);

            bool success = await _metricRepo.AddAsync(metric);
            if (success)
            {
                _logger.LogInformation("Metric saved to DB for OrgId: {OrgId}, Project: {Project}", metric.OrgId, metric.ProjectName);
            }
            else
            {
                _logger.LogWarning("Failed to save metric to DB for OrgId: {OrgId}, Project: {Project}", metric.OrgId, metric.ProjectName);
            }
        }

        public async Task EnsureWarmCacheAsync()
        {
            var traceGroups = await _traceRepo.GetLastNPerProjectAsync(50);
            foreach (var (key, items) in traceGroups)
            {
                var bucket = _traceBuckets.GetOrAdd(key, _ => new(MaxCacheSize));
                foreach (var item in items) bucket.Add(item);
                _logger.LogInformation("[TraceCache] Warmed {Count} items for {Key}", items.Count, key);
            }

            var errorGroups = await _errorRepo.GetLastNPerProjectAsync(50);
            foreach (var (key, items) in errorGroups)
            {
                var bucket = _errorBuckets.GetOrAdd(key, _ => new(MaxCacheSize));
                foreach (var item in items) bucket.Add(item);
                _logger.LogInformation("[ErrorCache] Warmed {Count} items for {Key}", items.Count, key);
            }

            var metricGroups = await _metricRepo.GetLastNPerProjectAsync(50);
            foreach (var (key, items) in metricGroups)
            {
                var bucket = _metricBuckets.GetOrAdd(key, _ => new(MaxCacheSize));
                foreach (var item in items) bucket.Add(item);
                _logger.LogInformation("[MetricCache] Warmed {Count} items for {Key}", items.Count, key);
            }
        }

       
            public Task<string> GetErrors(string projectName, string timeRange = null)
            {
                var task = _contextProvider.Get();
                _logger.LogInformation("Org ID from Task context: {OrgId}", task?.OrgId);
                _logger.LogInformation("Project Name to Look up: {ProjectName}", projectName);

                if (task == null || task.OrgId == null)
                    return Task.FromResult("Missing task context to get client's data");

                _logger.LogInformation("🔍 Error Cache Contains: {Keys}", string.Join(", ", _errorBuckets.Keys));

                var key = ProjectMatcher.ResolveBestKey(_errorBuckets.Keys, task.OrgId, projectName);
                if (key == null)
                {
                    _logger.LogWarning("No matching project found for '{ProjectName}'", projectName);
                    return Task.FromResult($"No matching project found for '{projectName}'.");
                }

                _logger.LogInformation("Key to look up: {Key}", key);

                var from = ParseTimeRange(timeRange);

                _logger.LogInformation("Timestamp to filter with: {From}", from);

                var errors = _errorBuckets.TryGetValue(key, out var bucket)
                    ? bucket.GetAll(e => e.Timestamp >= from)
                    : Enumerable.Empty<Error>();

                var result = errors
                    .OrderByDescending(e => e.Timestamp)
                    .Take(10)
                    .Select(e =>
                        $"❌ **{e.ExceptionType}** - {e.Message}\n" +
                        $"    ↳ Path: {e.HttpMethod} {e.Url}\n" +
                        $"    ↳ Trace ID: {e.TraceId}\n" +
                        $"    ↳ Time: {e.Timestamp:yyyy-MM-dd HH:mm:ss}\n" +
                        $"    ↳ Stack: {TrimStack(e.StackTrace, 2)}")
                    .ToList();

                var response = result.Any()
                    ? "🚨 **Recent Errors**\n\n" + string.Join("\n\n", result)
                    : "No errors recorded.";

                return Task.FromResult(JsonSerializer.Serialize(response));
            }

            public Task<string> GetLogs(string projectName, string timeRange = null)
            {
                var task = _contextProvider.Get();
                _logger.LogInformation("Org ID from Task context: {OrgId}", task?.OrgId);
                _logger.LogInformation("Project Name to Look up: {ProjectName}", projectName);

                if (task == null || task.OrgId == null)
                    return Task.FromResult("Missing task context to get client's data");

                _logger.LogInformation("🔍 Trace Cache Contains: {Keys}", string.Join(", ", _traceBuckets.Keys));

                var key = ProjectMatcher.ResolveBestKey(_traceBuckets.Keys, task.OrgId, projectName);
                if (key == null)
                {
                    _logger.LogWarning("No matching project found for '{ProjectName}'", projectName);
                    return Task.FromResult($"No matching project found for '{projectName}'.");
                }

                _logger.LogInformation("Key to look up: {Key}", key);

                var from = ParseTimeRange(timeRange);

                _logger.LogInformation("Timestamp to filter with: {From}", from);

                var traces = _traceBuckets.TryGetValue(key, out var bucket)
                    ? bucket.GetAll(t => t.Timestamp >= from)
                    : Enumerable.Empty<Trace>();

                var result = traces
                    .OrderByDescending(t => t.Timestamp)
                    .Take(10)
                    .Select(t =>
                        $"[{(t.isError ? "❌ Error" : "✅ Success")}] {t.Method} {t.Path}\n" +
                        $"    ↳ Duration: {t.DurationMs}ms | Status: {t.StatusCode}\n" +
                        $"    ↳ Trace ID: {t.Id}\n" +
                        $"    ↳ Timestamp: {t.Timestamp:yyyy-MM-dd HH:mm:ss}")
                    .ToList();

                var response = result.Any()
                    ? "📄 **Recent Trace Logs**\n\n" + string.Join("\n\n", result)
                    : "No trace logs available.";

                return Task.FromResult(response);
            }

            public Task<string> GetMetrics(string projectName, string timeRange = null)
            {
                var task = _contextProvider.Get();
                _logger.LogInformation("Org ID from Task context: {OrgId}", task?.OrgId);
                _logger.LogInformation("Project Name to Look up: {ProjectName}", projectName);

                if (task == null || task.OrgId == null)
                    return Task.FromResult("Missing task context to get client's data");

                _logger.LogInformation("🔍 Metric Cache Contains: {Keys}", string.Join(", ", _metricBuckets.Keys));

                var key = ProjectMatcher.ResolveBestKey(_metricBuckets.Keys, task.OrgId, projectName);
                if (key == null)
                {
                    _logger.LogWarning("No matching project found for '{ProjectName}'", projectName);
                    return Task.FromResult($"No matching project found for '{projectName}'.");
                }

                _logger.LogInformation("Key to look up: {Key}", key);

                var from = ParseTimeRange(timeRange);
                _logger.LogInformation("Timestamp to filter with: {From}", from);

                var metrics = _metricBuckets.TryGetValue(key, out var bucket)
                    ? bucket.GetAll()
                    : Enumerable.Empty<Metric>();

                var result = metrics
                    .OrderByDescending(m => m.Timestamp)
                    .Take(10)
                    .Select(m =>
                        $"🧠 **Runtime Snapshot** @ {m.Timestamp:yyyy-MM-dd HH:mm:ss}\n" +
                        $"    ↳ CPU Usage: {m.CpuUsagePercent}%\n" +
                        $"    ↳ Memory: {m.MemoryUsageBytes / 1024 / 1024} MB\n" +
                        $"    ↳ GC Collections: Gen0 - {m.Gen0Collections}, Gen1 - {m.Gen1Collections}, Gen2 - {m.Gen2Collections}\n" +
                        $"    ↳ Threads: Worker {m.AvailableWorkerThreads}, IO {m.AvailableIOThreads}")
                    .ToList();

                var response = result.Any()
                    ? "📊 **Runtime Metrics (Recent)**\n\n" + string.Join("\n\n", result)
                    : "No runtime metrics recorded.";

                return Task.FromResult(response);
            }

            private DateTime ParseTimeRange(string timeRange)
            {
                if (!string.IsNullOrEmpty(timeRange) && TimeFormatter.TryParseTimeRange(timeRange, out var span))
                {
                    var from = DateTime.UtcNow.Subtract(span);
                    _logger.LogInformation("✅ Parsed time range: {Input} → {Span} → from: {From}", timeRange, span, from);
                    return from;
                }

                _logger.LogWarning("⚠️ Could not parse time range '{TimeRange}', falling back to 1 hour", timeRange);
                return DateTime.UtcNow.AddHours(-1);
            }

            private static string TrimStack(string stackTrace, int lines)
            {
                if (string.IsNullOrWhiteSpace(stackTrace)) return "No stack trace available.";
                return string.Join("\n", stackTrace.Split('\n').Take(lines)).Trim();
            }
        

    }

}



