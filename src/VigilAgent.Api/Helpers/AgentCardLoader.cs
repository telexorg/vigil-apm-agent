using System.Text.Json;
using VigilAgent.Api.Commons.AgentCardSpecs;

namespace VigilAgent.Api.Helpers
{
    public class AgentCardLoader
    {
        public static string GetAgentCard()
        {
            var telemetryAgent = new AgentCard()
            {
                Name = "Vigil - Monitoring Agent",
                Description = "Vigil helps you monitor and analyze your C# .Net backend app by receiving logs, errors, and metrics, then interacting via chat to provide insights and recommendations.",
                Url = "https://vigil-apm-agent.onrender.com/api/v1", 
                IconUrl = "https://res.cloudinary.com/demo/image/upload/v1695555555/vigil-icon.png",
                DocumentationUrl = "https://your-agent-docs.com",
                Capabilities = new Capability
                {
                    Streaming = true,
                    PushNotifications = true
                },
                DefaultInputModes = new[] { "application/json" },
                DefaultOutputModes = new[] { "application/json" },
                Provider = new AgentProvider()
                {
                    Organization = "Vigil Observability Team",
                    Url = "https://vigil-apm-agent.onrender.com"
                },
                Skills = new[]
                {
                    new Skill
                    {
                        Id = "explain-errors",
                        Name = "Explain Errors",
                        Description = "Analyzes recent error logs and gives human-readable explanations.",
                        Tags = [ "errors", "analysis", "debugging" ],
                        Examples = new[]
                        {
                            "What caused the last server error?",
                            "Explain the recent null reference exceptions.",
                            "Are there frequent authentication issues?"
                        }
                    },
                    new Skill
                    {
                        Id = "show-logs",
                        Name = "Show Logs",
                        Description = "Displays the most recent trace logs for quick inspection.",
                        Tags = [ "logs", "telemetry", "traces" ],
                        Examples = new[]
                        {
                            "Show me logs from the last 10 minutes.",
                            "What requests failed recently?",
                            "List traces with high latency."
                        }
                    },
                    new Skill
                    {
                        Id = "show-metrics",
                        Name = "Show Runtime Metrics",
                        Description = "Provides system metrics such as CPU, memory, and GC activity.",
                        Tags = [ "metrics", "performance", "runtime" ],
                        Examples = new[]
                        {
                            "What is the CPU usage right now?",
                            "Show me the memory consumption trend.",
                            "How often is GC running?"
                        }
                    },
                    new Skill
                    {
                        Id = "recommend-fixes",
                        Name = "Recommend Fixes",
                        Description = "Suggests solutions based on observed telemetry patterns.",
                        Tags = [ "diagnostics", "recommendations", "AI insights" ],
                        Examples = new[]
                        {
                            "Any recommendations based on recent errors?",
                            "Suggest improvements for system stability.",
                            "What should I fix first?"
                        }
                    }
                }
            };

            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(telemetryAgent, options);
        }
    }
}
