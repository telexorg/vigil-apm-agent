using VigilAgent.Api.Commons;
using VigilAgent.Api.Enums;

namespace VigilAgent.Api.Helpers
{
    public static class PromptBuilder
    {
        public static string BuildSystemSummaryPrompt(string systemContextJson)
        {
            // Prompt to instruct AI to summarize telemetry context
            return $"""
                You are Vigil, a system observability assistant.

                Below is telemetry data (logs, metrics, errors) in JSON format. Read through it and respond to the users question on what happened recently:
                - Highlight unusual activity, failures, and spikes
                - Avoid quoting raw data unless necessary
                - Summarize like a monitoring expert writing a daily report

                DATA:
                {systemContextJson}
                """;

        }

        public static string BuildUserResponsePrompt(string userMessage, string systemSummary)
        {
            return $"""
                You are Vigil, a system assistant that speaks plainly to people about backend status and performance.

                Here is a summary of recent telemetry:
                {systemSummary}

                A user has now asked this question:
                "{userMessage}"

                Use the summary to answer helpfully and clearly. Suggest next steps if needed. Speak like an intelligent, reassuring assistant—not a log file.
                """;

        }

        public static string GenerateSystemMessage(RequestType requestType, List<Setting> settings)
        {
            string systemMessage = "Your name is Vigil. You are an APM agent who specializes in helping developers monitor there application providing insights for debugging and improving performance.";


            return systemMessage;
        }

        public static string BuildCustomPrompt(string userMessage, string context, string tone = "professional")
        {
            // Optional: More open-ended or stylized prompts based on tone or usage
            return tone switch
            {
                "observer" => $"You are a calm narrator analyzing backend behavior.\n\n{context}\n\nUSER: {userMessage}",
                "casual" => $"You're Vigil, a helpful but laid-back assistant.\nRecent status: {context}\nQuestion: {userMessage}",
                _ => BuildUserResponsePrompt(userMessage, context)
            };

        }
    }
}
