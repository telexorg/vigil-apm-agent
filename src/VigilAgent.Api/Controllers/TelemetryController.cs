using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using VigilAgent.Api.Models;
using VigilAgent.Api.Services;
using VigilAgent.Apm.Instrumentation;
using VigilAgent.Apm.Telemetry;

namespace VigilAgent.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TelemetryController : ControllerBase
    {
        private static int n = 1;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //var junk = new List<byte[]>();
            //for (int i = 0; i < 10000; i++)
            //{
            //    junk.Add(new byte[1024 * 50]); // 50 KB
            //}

            if (n % 2 == 0)
            {
                n++;
                throw new Exception();
            }
                n++;
            return Ok("Controller is working");
        }

        [HttpPost]
        public async Task<IActionResult> Batch([FromBody] JsonElement batch)
        {
            if (batch.ValueKind != JsonValueKind.Array || batch.GetArrayLength() == 0)
                return BadRequest("Invalid or empty batch");

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
                            TelemetryService.Traces[trace.TraceId] = trace;
                            Console.WriteLine($"[Batch] {trace.TraceId} trace - {trace.Path} = {trace.StatusCode} in {trace.DurationMs}ms");
                        }
                        break;

                    case "error":
                        var error = item.Deserialize<ErrorDetail>(options);
                        if (error != null)
                        {
                            TelemetryService.Errors[error.TraceId] = error;
                            Console.WriteLine($"[Batch] {error.TraceId} exception - {error.Url} = {error.StatusCode} at {error.Timestamp}");
                            Console.WriteLine($"        ✖ {error.ExceptionType}: {error.Message}");
                        }
                        break;

                    case "metrics":
                        var metrics = item.Deserialize<Metric>(options);
                        if (metrics != null)
                        {
                            var id = Guid.NewGuid().ToString();
                            TelemetryService.Metrics[id] = metrics;
                            Console.WriteLine($"[Batch] metrics - CPU: {metrics.CpuUsagePercent}%, Mem: {metrics.MemoryUsageBytes / 1024 / 1024}MB, GC0: {metrics.Gen0Collections}");
                        }
                        break;

                    default:
                        Console.WriteLine($"[Batch] Unknown type: {type}");
                        break;
                }
            }

            return Ok();
        }

    }
}
