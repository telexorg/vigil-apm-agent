
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TestAi;

//IChatCompletionService chatService = new GoogleAIGeminiChatCompletionService("gemini-2.0-flash-001", "AIzaSyDSZb1QvRGVD8fzh17yAQZZMuLgccnh2Mo");


IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var aiConfig = config.GetSection("AI").Get<AIConfig>();


var builder = Kernel.CreateBuilder();
// ✅ Add console logging with TRACE level
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders(); // Optional: clear default loggers
    logging.AddConsole();     // Add console output
    logging.SetMinimumLevel(LogLevel.Trace); // Log everything (Trace, Debug, Info, etc.)
});

builder.Services.AddSingleton<IChatCompletionService>(new GoogleAIGeminiChatCompletionService( aiConfig.GeminiModel, aiConfig.ApiKey));


Kernel kernel = builder.Build();
kernel.ImportPluginFromType<Demographics>();

PromptExecutionSettings settings = new GeminiPromptExecutionSettings()
{
    ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions
};

var chatService = kernel.GetRequiredService<IChatCompletionService>();

ChatHistory chatHistory = new ChatHistory();

while (true)
{
    Console.Write("Q: ");

    var prompt = Console.ReadLine();
    chatHistory.AddUserMessage(prompt);

    var response = chatService.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel );


    var result = "";
    await foreach (var chatMessage in response)
    {
        Console.Write(chatMessage);
        result += chatMessage;
    }
    chatHistory.AddAssistantMessage(result);

}


class Demographics
{
    [KernelFunction]
    public int GetPersonAge(string name)
    {
        return name switch
        {
            "Fabian" => 29,
            "Elsa" => 21,
            "Anna" => 19,
            _ => 40
        };
    }
}

class AIConfig
{
    public string ApiKey { get; set; }
    public string GeminiModel { get; set; }
}