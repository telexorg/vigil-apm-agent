using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using VigilAgent.Api.Services;

namespace VigilAgent.Api.Configuration
{
    public class KernelProvider
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatCompletionService;

        public KernelProvider(IConfiguration configuration)
        {
            var geminiApiKey = configuration.GetValue<string>("Gemini:ApiKey")
                ?? throw new InvalidOperationException("Gemini API Key not configured!");

            var geminiModel = configuration.GetValue("Gemini:Model", "gemini-1.5-pro");           

            _kernel = BuildKernel(geminiModel, geminiApiKey);

            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        }

        public Kernel Kernel => _kernel;

        public IChatCompletionService ChatCompletionService => _chatCompletionService;

        private static Kernel BuildKernel(string model, string apiKey)
        {
            var builder = Kernel.CreateBuilder()
                .AddGoogleAIGeminiChatCompletion(model, apiKey);

            builder.Services.AddLogging(logs => logs.AddConsole().SetMinimumLevel(LogLevel.Trace));

            return builder.Build();
        }

        
        public void RegisterPlugins(IServiceProvider sp)
        {
            _kernel.Plugins.AddFromType<TelemetryFunctions>("Telemetry", sp);
        }
    }

}
