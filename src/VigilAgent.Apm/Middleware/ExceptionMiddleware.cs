using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VigilAgent.Apm.Context;
using VigilAgent.Apm.Telemetry;

namespace VigilAgent.Apm.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                int status = DetermineStatusCode(ex);


                var traceId = TraceContext.TraceId ?? Guid.NewGuid().ToString(); // fallback if not set

                var error = new ErrorDetail
                {
                    TraceId = traceId,
                    ExceptionType = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    InnerExceptionMessage = ex.InnerException?.Message,
                    HttpMethod = context.Request.Method,
                    Url = context.Request.Path.ToString(),
                    StatusCode = StatusCodes.Status500InternalServerError
                };

                TelemetryBuffer.Add(error);

                context.Response.StatusCode = status;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("An unexpected error occurred.");

                Console.WriteLine($"[Vigil] [{TraceContext.TraceId}] {context.Request.Method} {context.Request.Path} -> {status}");


                // Re-throw so ASP.NET shows correct error
                throw;
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
