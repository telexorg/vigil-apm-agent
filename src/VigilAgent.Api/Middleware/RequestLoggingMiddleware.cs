using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace VigilAgent.Api.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {

                context.Request.EnableBuffering();

                var request = context.Request;
                var headers = JsonSerializer.Serialize(request.Headers.ToDictionary(headers => headers.Key, headers => headers.Value.ToString()), new JsonSerializerOptions { WriteIndented = true });
                var queryParams = JsonSerializer.Serialize(request.Query.ToDictionary(query => query.Key, query => query.Value.ToString()), new JsonSerializerOptions { WriteIndented = true });
            
                string body = "";
                if (request.Body.CanRead)
                {
                    using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
                    {
                        body = await reader.ReadToEndAsync();
                        context.Request.Body.Position = 0; // Reset the stream for next middleware
                    }
                }

                string formattedBody = body;
                try
                {
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        using var jsonDoc = JsonDocument.Parse(body);
                        formattedBody = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "❌ JSON Parsing Error - Malformed request body. Logging raw body.");
                }

                _logger.LogInformation($"🔍 Incoming Request: {request.Method} {request.Path}");
                _logger.LogInformation($"📌 Headers: {headers}");
                _logger.LogInformation($"📌 Query Parameters: {queryParams}");
                _logger.LogInformation($"📌 Body: {formattedBody}");

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception in RequestLoggingMiddleware");
                throw;
            }           
        }
    }
}
