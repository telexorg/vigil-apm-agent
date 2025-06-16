using System.Text.RegularExpressions;
using VigilAgent.Api.IServices;

namespace VigilAgent.Api.Services;

public class RequestRouter
{
    private readonly Dictionary<string, IAgentCommandHandler> _handlers;

    public RequestRouter(Dictionary<string, IAgentCommandHandler> handlers)
    {
        _handlers = handlers;
    }

    public async Task<string> RouteAsync(string message)
    {
        message = message.ToLowerInvariant();

        if (Regex.IsMatch(message, @"logs|requests|traces"))
            return await _handlers["show-logs"].HandleAsync(message);

        if (Regex.IsMatch(message, @"error|errors|exception|fail"))
            return await _handlers["explain-errors"].HandleAsync(message);
        
        if (Regex.IsMatch(message, @"metrics|proccesses|runtime|fail"))
            return await _handlers["show-metrics"].HandleAsync(message);

        if (Regex.IsMatch(message, @"recommend|fix|what should we do"))
            return await _handlers["recommend-fix"].HandleAsync(message);

        return "🤖 I didn't understand that yet. Try asking about logs, errors, or recommendations.";
    }
}

