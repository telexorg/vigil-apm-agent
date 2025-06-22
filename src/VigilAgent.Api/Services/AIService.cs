using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VigilAgent.Api.IServices;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.Helpers;
using VigilAgent.Api.Commons;
using BloggerAgent.Domain.Commons;
using VigilAgent.Api.Models;
using VigilAgent.Api.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;

namespace VigilAgent.Api.Services
{
    public class AIService : IAIService
    {
        private ILogger<VigilAgentService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly IConversationRepository _messageRepository;
        private readonly HttpHelper _httpHelper;

        private readonly KernelProvider _kernelProvider;

     
        public AIService(IOptions<TelexApiSettings> dataConfig, KernelProvider kernelProvider, IOptions<TelexApiSettings> telexSettings, ILogger<VigilAgentService> logger, IConversationRepository messageRepository, HttpHelper httpHelper)
        {
            _apiKey = dataConfig.Value.ApiKey;
            _baseUrl = dataConfig.Value.BaseUrl;
            _logger = logger;
            _messageRepository = messageRepository;
            _httpHelper = httpHelper;
            _kernelProvider = kernelProvider;
        }
        public async Task<string> GenerateResponse(string message, string systemMessage, TelemetryTask task)
        {
            var chatHistory = new List<TelexChatMessage>()
            {
                new TelexChatMessage() { Role = Roles.System, Content = systemMessage }
            };

            var conversations = await _messageRepository.GetMessagesAsync(task.ContextId);

            if (conversations.Count > 0 || conversations != null)
            {
                chatHistory.AddRange(conversations);
            }
            
            await AddMessageAsync(message, task, "user");


            chatHistory.Add(new TelexChatMessage { Role = "user", Content = message });

            var apiRequest = new ApiRequest()
            {
                Url = $"{_baseUrl}/telexai/chat",
                Body = new { chatHistory },
                Method = HttpMethod.Post,
                Headers = new Dictionary<string, string>
                {
                    {TelexApiSettings.Header, _apiKey },
                    {"X-Model", "google/gemini-2.5-flash-preview-05-20" }
                }
            };

            _logger.LogInformation("Sending message to Telex AI");

            var response = await _httpHelper.SendRequestAsync(apiRequest);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var error = TelexApiResponse<TelexChatMessage>.ExtractResponse(responseString);

                return $"An error occurred while communicating with Telex AI: {error.Message}";
            }

            _logger.LogInformation("Message successfully generated from the Telex AI");

            var generatedData = TelexApiResponse<TelexChatResponse>.ExtractResponse(responseString);

            string generatedResponse = generatedData.Data.Messages.Content;

            await AddMessageAsync(generatedResponse, task, "assistant");

            return generatedResponse;
        }

        private async Task AddMessageAsync(string message, TelemetryTask task, string role)
        {
            var newMessage =
                new Message { Id = Guid.NewGuid().ToString(), Content = message, TaskId = task.TaskId, ContextId = task.ContextId, Role = role };
                          
                bool isAdded = await _messageRepository.CreateAsync(newMessage);

                if (!isAdded)
                    _logger.LogInformation($"Failed to add {newMessage.Role} message to database");
           
        }

        public async Task<string> Chat(string request, string responseContext)
        {
            var kernel = _kernelProvider.Kernel;
            var chatService = _kernelProvider.ChatCompletionService;

            // Build ChatHistory
            var history = new ChatHistory();

            var systemMessage = PromptBuilder.BuildUserResponsePrompt(request,responseContext);

            history.AddSystemMessage(systemMessage);

            // Restore prior conversation (if any)
            //if (history.Count > 0)
            //{
            //    foreach (var item in history)
            //    {
            //        history.AddMessage(item.Role, item.Content);
            //    }
            //}

            // Add current user message
            history.AddUserMessage(request);

            // Settings
            //GeminiPromptExecutionSettings executionSettings = new()
            //{
            //    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            //};

            // Get AI reply
            var result = await chatService.GetChatMessageContentAsync(
                history
                //executionSettings: executionSettings,
                //kernel: kernel
            );

            // Add AI reply to history
            history.AddMessage(result.Role, result.Content ?? string.Empty);

            // Return response with updated history
            var response = new 
            {
                Reply = result.Content ?? "",
                History = history.Select(m => new TelexChatMessage { Role = m.Role.ToString(), Content = m.Content ?? "" }).ToList()
            };

            return result.Content;
        }
    }
}
