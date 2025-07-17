using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using VigilAgent.Api.Commons;
using VigilAgent.Api.Dtos;
using VigilAgent.Api.Helpers;

namespace VigilAgent.Api.Middleware
{
    public class RequestHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestHandler> _logger;
        public RequestHandler(RequestDelegate next, ILogger<RequestHandler> logger)
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

                var headers = JsonSerializer.Serialize(request.Headers.ToDictionary(
                    headers => headers.Key,
                    headers => headers.Value.ToString()), 
                    new JsonSerializerOptions { WriteIndented = true }
                    );

                var queryParams = JsonSerializer.Serialize(request.Query.ToDictionary(
                    query => query.Key, 
                    query => query.Value.ToString()), 
                    new JsonSerializerOptions { WriteIndented = true }
                    );
            
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

                             _logger.LogInformation($"======================================================================================" +
                           $"\n🔍 Incoming Request: {request.Method} {request.Path}\n" +
                           $"📌 Headers: {headers}\n" +
                           $"📌 Query Parameters: {queryParams}\n" +
                           $"📌 Body: {formattedBody}\n" +
                           $"=======================================================================================");

                        if (context.Request.Path == "/api/v1" && context.Request.Method == "POST")
                        {
                            var requestBody = JsonSerializer.Deserialize<TaskRequest>(body, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            var telemetryTask = DataExtract.ExtractTaskData(requestBody); // your helper method

                           if (telemetryTask != null) 
                             context.Items["TaskContext"] = telemetryTask;

                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON Parsing Error - Malformed request body. Logging raw body.");
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in RequestLoggingMiddleware");
                throw;
            }           
        }
    }
}
