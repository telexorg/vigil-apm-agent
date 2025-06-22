using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using VigilAgent.Api.Services;

namespace VigilAgent.Api.Middleware
{
    public class KernelProvider
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatCompletionService;
        public KernelProvider(IConfiguration configuration)
        {
            var geminiApiKey = configuration.GetValue<string>("Gemini:ApiKey")
                ?? throw new InvalidOperationException("Gemini API Key not configured!");

            var geminiModel = configuration.GetValue<string>("Gemini:Model", "gemini-1.5-pro");


            var builder = Kernel.CreateBuilder()
                .AddGoogleAIGeminiChatCompletion(geminiModel, geminiApiKey);

            // Add enterprise components
            builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));


            // Build the kernel
            _kernel = builder.Build();
            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();


            // Add your telemetry plugin
            //_kernel.Plugins.AddFromType<TelemetryService>("Telemetry");

        }

        public Kernel Kernel => _kernel;

        public IChatCompletionService ChatCompletionService => _chatCompletionService;
    }
}
