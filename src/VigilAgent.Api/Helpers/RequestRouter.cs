using System.Text.RegularExpressions;
using VigilAgent.Api.IServices;

namespace VigilAgent.Api.Helpers;

public class RequestRouter
{

    private readonly ITelemetryService _telemetryHandler;
    private readonly Dictionary<string, Func<string, Task<string>>> _handlers;
    public RequestRouter(ITelemetryService telemetryHandler)
    {
        _telemetryHandler = telemetryHandler;

        _handlers = new Dictionary<string, Func<string, Task<string>>>
            {
                { "show-logs", param => _telemetryHandler.GetLogs(param) },
                { "explain-errors", param => _telemetryHandler.GetErrors(param) },
                { "show-metrics", param => _telemetryHandler.GetMetrics(param)},
            };
    }

    public async Task<string> RouteAsync(string message)
    {       

        message = message.ToLowerInvariant();

        if (Regex.IsMatch(message, @"logs|requests|traces"))
            return await _handlers["show-logs"](message);

        if (Regex.IsMatch(message, @"error|errors|exception|fail"))
            return await _handlers["explain-errors"](message);
        
        if (Regex.IsMatch(message, @"metrics|proccesses|runtime|fail"))
            return await _handlers["show-metrics"](message);

        return "NO_MATCH";
    }
}

