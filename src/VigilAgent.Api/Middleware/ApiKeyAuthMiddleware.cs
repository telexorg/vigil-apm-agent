using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using VigilAgent.Api.Helpers;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.IServices;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.Middleware
{
    public class ApiKeyAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        public ApiKeyAuthMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IApiKeyValidator>();

            context.Response.ContentType = "application/json";

            if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing API key");
                return;
            }

            var rawKey = apiKey.ToString();

            // Extract org + project from encrypted API key
            var project = await validator.ValidateAsync(apiKey.ToString());
            if (project is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                var error = JsonSerializer.Serialize(new { error = "Invalid api key" });
                await context.Response.WriteAsync(error);
                return;
            }

           
            // ✅ Store project context for downstream logic
            context.Items["Project"] = project;

            await _next(context);
        }
    }
}