using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using VigilAgent.Apm.Context;
using VigilAgent.Apm.Instrumentation;
using VigilAgent.Apm.Telemetry;

namespace VigilAgent.Apm.Middleware
{
    public class VigilMiddleware
    {
        private readonly RequestDelegate _next;

        public VigilMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api/Telemetry") && context.Request.Method.Equals(HttpMethod.Post))
            {
                await _next(context); // Don't trace export calls
                return;
            }

            var traceId = Guid.NewGuid().ToString();
            TraceContext.TraceId = traceId;
            Console.WriteLine($"[Vigil] [{traceId}] trace {context.Request.Method} {context.Request.Path} -> in progress...");

            var stopwatch = Stopwatch.StartNew();
            MetricsCollector.Start();
            TelemetryFlusher.Start();

            try
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers["X-Trace-ID"] = traceId;
                    return Task.CompletedTask;
                });

                await _next(context);
            }
            catch (Exception ex)
            {

                int status = DetermineStatusCode(ex);

                var duration = stopwatch.ElapsedMilliseconds;

                var error = new ErrorDetail
                {
                    TraceId = traceId,
                    ExceptionType = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    InnerExceptionMessage = ex.InnerException?.Message,
                    HttpMethod = context.Request.Method,
                    Url = context.Request.Path.ToString(),
                    OccurredAt = duration,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    //Source = context.Request.
                };

                TelemetryBuffer.Add(error);

                context.Response.StatusCode = status;
                context.Response.ContentType = "application/json";

                Console.WriteLine($"[Vigil EX] [{error.TraceId}] error {context.Request.Method} {context.Request.Path} -> {status} in {error.OccurredAt}ms");


                // Re-throw so ASP.NET shows correct error
                throw;
            }
            finally
            {
                stopwatch.Stop();
                
                var method = context.Request.Method;
                var path = context.Request.Path;
                var statusCode = context.Response.StatusCode;
                var duration = stopwatch.ElapsedMilliseconds;
                var evt = new TelemetryEvent()
                {
                    TraceId = traceId,
                    Method = method,
                    Path = path,
                    StatusCode = statusCode,
                    DurationMs = duration,
                    isError = -statusCode != StatusCodes.Status200OK
                };
                TelemetryBuffer.Add(evt);

                Console.WriteLine($"[Vigil] [{traceId}] {evt.Type} {method} {path} -> {statusCode} in {duration}ms");

                TraceContext.Clear();

            }
        }

        private int DetermineStatusCode(Exception exception)
        {
            // Map certain exception types to specifc status codes
            return exception switch
            {
                ArgumentException or ArgumentNullException _ => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException _ => (int)HttpStatusCode.Unauthorized,
                NotImplementedException _ => (int)HttpStatusCode.NotImplemented,
                AuthenticationException _ => (int)HttpStatusCode.Forbidden,
                FileNotFoundException _ => (int)HttpStatusCode.NotFound,

                _ => (int)HttpStatusCode.InternalServerError,

            };
        }
    }
}
