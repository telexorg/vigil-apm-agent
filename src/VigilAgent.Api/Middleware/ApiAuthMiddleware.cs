using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VigilAgent.Api.Contracts;

namespace VigilAgent.Api.Middleware;

public class VigilAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VigilAuthorizationMiddleware> _logger;

    public VigilAuthorizationMiddleware(
        RequestDelegate next,
        IServiceProvider serviceProvider,
        ILogger<VigilAuthorizationMiddleware> logger)
    {
        _next = next;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IApiKeyValidator>();

        context.Response.ContentType = "application/json";

        if (!context.Request.Headers.TryGetValue("X-VIGIL-API-KEY", out var apiKey))
        {
            _logger.LogWarning("[Vigil.Auth] Missing API key from {RemoteIp}", context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing API key");
            return;
        }

        var rawKey = apiKey.ToString();

        _logger.LogDebug("[Vigil.Auth] Validating API key: {ApiKeyPrefix}…", rawKey?.Substring(0, Math.Min(6, rawKey.Length)));

        var project = await validator.ValidateAsync(rawKey);
        if (project is null)
        {
            _logger.LogWarning("[Vigil.Auth] Invalid API key from {RemoteIp}", context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            var error = JsonSerializer.Serialize(new { error = "Invalid api key" });
            await context.Response.WriteAsync(error);
            return;
        }

        _logger.LogInformation("[Vigil.Auth] Authorized request for ProjectId={ProjectId}, Org={Org}", project.Id, project.OrgId);

        context.Items["Project"] = project;

        await _next(context);
    }
}