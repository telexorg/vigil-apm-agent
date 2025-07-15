using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Security.Authentication;
using VigilAgent.Apm.Context;
using VigilAgent.Apm.Instrumentation;
using VigilAgent.Apm.Processing;
using VigilAgent.Apm.Telemetry;

namespace VigilAgent.Apm.Middleware
{
    public class VigilMiddleware 
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<VigilMiddleware> _logger;
        private readonly TelemetryBuffer _telemetryBuffer;
        private readonly MetricsCollector _metricsCollector;

        public VigilMiddleware(RequestDelegate next, ILogger<VigilMiddleware> logger, TelemetryBuffer telemetryBuffer, MetricsCollector metricsCollector)
        {
            _next = next;
            _logger = logger;
            _telemetryBuffer = telemetryBuffer;
            _metricsCollector = metricsCollector;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api/v1/Telemetry") &&
                context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context); // Avoid recursive telemetry tracking
                return;
            }

            var traceId = Guid.NewGuid().ToString();
            TraceContext.TraceId = traceId;

            var method = context.Request.Method;
            var path = context.Request.Path;
            _logger.LogDebug("[Vigil] [{TraceId}] {Method} {Path} -> started", traceId, method, path);

            var stopwatch = Stopwatch.StartNew();

            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Trace-ID"] = traceId;
                return Task.CompletedTask;
            });

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var status = DetermineStatusCode(ex);
                context.Response.StatusCode = status;
                context.Response.ContentType = "application/json";

                var error = new ErrorEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "error",
                    TraceId = traceId,
                    ExceptionType = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    InnerExceptionMessage = ex.InnerException?.Message,
                    HttpMethod = method,
                    Url = path.ToString(),
                    StatusCode = status
                };

                await _telemetryBuffer.AddAsync(error);
                _logger.LogWarning(ex, "[Vigil] error - [{TraceId}]  {Method} {Path} -> {StatusCode}", traceId, method, path, status);

                throw; // Let ASP.NET handle the actual error rendering
            }
            finally
            {
                stopwatch.Stop();

                var duration = stopwatch.ElapsedMilliseconds;
                var statusCode = context.Response.StatusCode;

                var traceEvent = new TraceEvent
                {
                    Id = traceId,
                    Type = "trace",
                    Method = method,
                    Path = path,
                    StatusCode = statusCode,
                    DurationMs = duration,
                    isError = statusCode >= 400
                };

                await _telemetryBuffer.AddAsync(traceEvent);

                _logger.LogInformation("[Vigil] trace - [{TraceId}] {Method} {Path} -> {StatusCode} in {Duration}ms",
                    traceId, method, path, statusCode, duration);

                TraceContext.Clear();
                _metricsCollector.Collect();
            }
        }

        private static int DetermineStatusCode(Exception exception) => exception switch
        {
            ArgumentException or ArgumentNullException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            NotImplementedException => (int)HttpStatusCode.NotImplemented,
            AuthenticationException => (int)HttpStatusCode.Forbidden,
            FileNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }
}