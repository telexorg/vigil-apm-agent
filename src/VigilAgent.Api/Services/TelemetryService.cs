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

        public TelemetryService(
            ITelemetryRepository<Trace> traceRepo,
            ITelemetryRepository<Error> errorRepo,
            ITelemetryRepository<Metric> metricRepo,
            ITaskContextProvider contextProvider)
        {
            _traceRepo = traceRepo;
            _errorRepo = errorRepo;
            _metricRepo = metricRepo;
            _contextProvider = contextProvider;
        }

        public async Task AddTraceAsync(Trace trace)
        {
            var key = CacheKey.For(trace.OrgId, trace.ProjectName);

            var bucket = _traceBuckets.GetOrAdd(key, _ => new(MaxCacheSize));
            bucket.Add(trace);
            await _traceRepo.AddAsync(trace);
        }

        public async Task AddErrorAsync(Error error)
        {
            var key = CacheKey.For(error.OrgId, error.ProjectName);

            var bucket = _errorBuckets.GetOrAdd(key, _ => new(MaxCacheSize));
            bucket.Add(error);
            await _errorRepo.AddAsync(error);
        }

        public async Task AddMetricAsync(Metric metric)
        {
            var key = CacheKey.For(metric.OrgId, metric.ProjectName);

            var bucket = _metricBuckets.GetOrAdd(key, _ => new(MaxCacheSize));
            bucket.Add(metric);
            await _metricRepo.AddAsync(metric);
        }

        public async Task EnsureWarmCacheAsync()
        {
            var recentTraces = await _traceRepo.GetLastNAsync(MaxCacheSize);
            foreach (var trace in recentTraces)
            {
                var key = CacheKey.For(trace.OrgId, trace.ProjectName);
                var bucket = _traceBuckets.GetOrAdd(key, _ => new(MaxCacheSize));
                bucket.Add(trace);
                Console.WriteLine($"[TraceCache] Added: {key}");

            }

            var recentErrors = await _errorRepo.GetLastNAsync(MaxCacheSize);

            //Console.WriteLine($"recent Trace Cache: {JsonSerializer.Serialize(recentTraces)}");
            foreach (var error in recentErrors)
            {
                var key = CacheKey.For(error.OrgId, error.ProjectName);
                var bucket = _errorBuckets.GetOrAdd(key, _ => new(MaxCacheSize));
                bucket.Add(error);
                Console.WriteLine($"[ErrorCache] Added: {key}");

            }

            var recentMetrics = await _metricRepo.GetLastNAsync(MaxCacheSize);


            foreach (var metric in recentMetrics)
            {
                var key = CacheKey.For(metric.OrgId, metric.ProjectName);
                var bucket = _metricBuckets.GetOrAdd(key, _ => new(MaxCacheSize));
                bucket.Add(metric);
                Console.WriteLine($"[MetricCache] Added: {key}");
            }
        }

        public Task<string> GetErrors(string projectName, string timeRange = null)
        {
            var task = _contextProvider.Get();
            Console.WriteLine($"Org ID from Task context: {task.OrgId}");

            Console.WriteLine($"Project Name to Look up: {projectName}");

            if (task == null || task.OrgId == null)
                return Task.FromResult("Missing task context to get client's data");

            Console.WriteLine($"🔍 Cache Contains: {string.Join(", ", _errorBuckets.Keys)}");

            var key = ProjectMatcher.ResolveBestKey(_errorBuckets.Keys, task.OrgId, projectName);
            if (key == null) return Task.FromResult($"No matching project found for '{projectName}'.");

            //var key = CacheKey.For(task.OrgId, projectName);

            Console.WriteLine($"key to look up: {key}");

            var from = ParseTimeRange(timeRange);

            Console.WriteLine($"Timestamp to filter with: {from}");

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
            Console.WriteLine($"Org ID from Task context: {task.OrgId}");

            Console.WriteLine($"Project Name to Look up: {projectName}");

            if (task == null || task.OrgId == null)
                return Task.FromResult("Missing task context to get client's data");

            Console.WriteLine($"🔍 Cache Contains: {string.Join(", ", _metricBuckets.Keys)}");

            var key = ProjectMatcher.ResolveBestKey(_traceBuckets.Keys, task.OrgId, projectName);
            if (key == null) return Task.FromResult($"No matching project found for '{projectName}'.");

            //var key = CacheKey.For(task.OrgId, projectName);
            Console.WriteLine($"key to look up: {key}");

            var from = ParseTimeRange(timeRange);
            Console.WriteLine($"Timestamp to filter with: {from}");

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
            Console.WriteLine($"Org ID from Task context: {task.OrgId}");

            Console.WriteLine($"Project Name to Look up: {projectName}");

            if (task == null || task.OrgId == null)
                return Task.FromResult("Missing task context to get client's data");

            Console.WriteLine($"🔍 Cache Contains: {string.Join(", ", _metricBuckets.Keys)}");

            var key = ProjectMatcher.ResolveBestKey(_metricBuckets.Keys, task.OrgId, projectName);
            Console.WriteLine($"key to look up: {key}");
            if (key == null) return Task.FromResult($"No matching project found for '{projectName}'.");

            //var key = CacheKey.For(task.OrgId, projectName);


            var from = ParseTimeRange(timeRange);

            Console.WriteLine($"Timestamp to filter with: {from}");

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

        private static DateTime ParseTimeRange(string timeRange)
        {
            if (!string.IsNullOrEmpty(timeRange) && TimeFormatter.TryParseTimeRange(timeRange, out var span))
            {
                var from = DateTime.UtcNow.Subtract(span);
                Console.WriteLine($"✅ Parsed time range: {timeRange} → {span} → from: {from:o}");
                return from;
            }

            Console.WriteLine($"⚠️ Could not parse time range '{timeRange}', falling back to 1 hour");
            return DateTime.UtcNow.AddHours(-1);

        }

        private static string TrimStack(string stackTrace, int lines = 3)
        {
            if (string.IsNullOrWhiteSpace(stackTrace)) return "(no stack trace)";
            return string.Join("\n", stackTrace.Split('\n').Take(lines)).Trim();
        }
    }

}



