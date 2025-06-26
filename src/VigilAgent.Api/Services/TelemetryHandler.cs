using System.Text.Json;
using System.Threading.Tasks;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.IServices;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.Services
{
    public class TelemetryHandler : ITelemetryHandler
    {
        public readonly ITelemetryService _telemetryHandler;
        public readonly IMongoRepository<Project> _projectRepository;
        public TelemetryHandler(ITelemetryService telemetryHandler, IMongoRepository<Project> projectRepository)
        {
            _telemetryHandler = telemetryHandler;
            _projectRepository = projectRepository;
        }


        public async Task HandleBatchItemsAsync(JsonElement batch, Project project)
        {

            var options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };

            foreach (var item in batch.EnumerateArray())
            {
                if (!item.TryGetProperty("type", out var typeProperty))
                {
                    Console.WriteLine("[Batch] Skipping item with no type");
                    continue;
                }

                var type = typeProperty.GetString();

                switch (type)
                {
                    case "trace":
                        var trace = item.Deserialize<Trace>(options);
                        if (trace != null)
                        {
                            trace.ProjectName = project.ProjectName;
                            trace.ProjectId = project.Id;
                            trace.OrgId = project.OrgId;
                            await _telemetryHandler.AddTraceAsync(trace);
                            Console.WriteLine($"[Batch] {trace.Id} trace - {trace.Path} = {trace.StatusCode} in {trace.DurationMs}ms");
                        }
                        break;

                    case "error":
                        var error = item.Deserialize<Error>(options);
                        if (error != null)
                        {
                            error.ProjectName = project.ProjectName;
                            error.ProjectId = project.Id;
                            error.OrgId = project.OrgId;
                            await _telemetryHandler.AddErrorAsync(error);
                            Console.WriteLine($"[Batch {error.Timestamp.GetDateTimeFormats('t')}] {error.TraceId} exception - {error.ExceptionType}: {error.Message} - {error.Url} = {error.StatusCode} at {error.Timestamp.GetDateTimeFormats('t')} ✖ ");
                        }
                        break;

                    case "metrics":
                        var metrics = item.Deserialize<Metric>(options);
                        if (metrics != null)
                        {
                            metrics.ProjectName = project.ProjectName;
                            metrics.ProjectId = project.Id;
                            metrics.OrgId = project.OrgId;
                            await _telemetryHandler.AddMetricAsync(metrics);
                            Console.WriteLine($"[Batch] metrics - CPU: {metrics.CpuUsagePercent}%, Mem: {metrics.MemoryUsageBytes / 1024 / 1024}MB, GC0: {metrics.Gen0Collections}");
                        }
                        break;

                    default:
                        Console.WriteLine($"[Batch] Unknown type: {type}");
                        break;
                }
            }
        }
    }
}
