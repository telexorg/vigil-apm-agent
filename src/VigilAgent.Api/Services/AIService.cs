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
        private readonly IMongoRepository<Message> _repository;

        private readonly KernelProvider _kernelProvider;

     
        public AIService(IOptions<TelexApiSettings> dataConfig, KernelProvider kernelProvider, IOptions<TelexApiSettings> telexSettings, ILogger<VigilAgentService> logger, IConversationRepository messageRepository, HttpHelper httpHelper, IMongoRepository<Message> repository)
        {
            _apiKey = dataConfig.Value.ApiKey;
            _baseUrl = dataConfig.Value.BaseUrl;
            _logger = logger;
            _messageRepository = messageRepository;
            _httpHelper = httpHelper;
            _kernelProvider = kernelProvider;
            _repository = repository;
        }


        public async Task<string> GetIntentAsync(string message)
        {
            try
            {
                var kernel = _kernelProvider.Kernel;
                var chatService = _kernelProvider.ChatCompletionService;

                // Build prompt for intent detection
                var systemMessage = PromptBuilder.BuildIntentDetectionPrompt(message);

                // Create chat history for this turn
                var history = new ChatHistory();
                history.AddSystemMessage(systemMessage);
                history.AddUserMessage(message);

                // Get AI reply
                var result = await chatService.GetChatMessageContentAsync(history);

                var intent = result.Content?.Trim().ToLowerInvariant() ?? "unknown";

                _logger.LogInformation("AI detected intent: {Intent} for message: {Message}", intent, message);

                return intent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetIntentAsync()");
                return "unknown";
            }
        }


        public async Task<string> ChatWithHistoryAsync(TelemetryTask taskRequest)
        {
            try
            {
                var kernel = _kernelProvider.Kernel;
                var chatService = _kernelProvider.ChatCompletionService;

                var newMessage = await _repository.Create(new Message()
                {
                    ContextId = taskRequest.ContextId,
                    Content = taskRequest.Message,
                    Role = Roles.User,
                    TaskId = taskRequest.TaskId,
                    
                });

                // Load prior conversation messages (if any)                
                var previousMessages = await _repository.GetAllAsync(m => m.ContextId == taskRequest.ContextId);
                var orderedMessages = previousMessages.OrderBy(m => m.Timestamp).ToList();

                var history = new ChatHistory();

                // Add system message to guide the assistant
                history.AddSystemMessage(PromptBuilder.BuildSystemMessage());                               

                // Add prior conversation messages
                history.AddRange(orderedMessages.Select(m => new ChatMessageContent()
                {
                    Role = new AuthorRole(m.Role),
                    Content = m.Content
                }));


                // Get AI reply
                var result = await chatService.GetChatMessageContentAsync(history);

                // Save AI reply to db
                var newAIMessage = await _repository.Create(new Message()
                {
                    ContextId = taskRequest.ContextId,
                    Content = result.Content,
                    Role = Roles.Assistant,
                    TaskId = taskRequest.TaskId,

                });

                _logger.LogInformation("ChatWithHistory reply: {Reply}", result.Content);

                return result.Content ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ChatWithHistoryAsync()");
                return "Sorry, something went wrong.";
            }
        }



        public async Task<string> ChatWithTools(TelemetryTask taskRequest)
        {
            try
            {
                var kernel = _kernelProvider.Kernel;
                var chatService = _kernelProvider.ChatCompletionService;

                // Save AI reply (optional)
                await _repository.Create(new Message
                {
                    ContextId = taskRequest.ContextId,
                    Content = taskRequest.Message,
                    Role = Roles.Assistant,
                    TaskId = taskRequest.TaskId,
                });

                //var previousMessages = await _repository.GetLastNAsync(m => m.ContextId == taskRequest.ContextId, 10);
                //var orderedMessages = previousMessages.OrderBy(m => m.Timestamp).ToList();

                var history = new ChatHistory();

                // Add system message to guide the assistant
                history.AddSystemMessage(PromptBuilder.BuildSystemToolingMessage());

                //// Add prior conversation messages
                //history.AddRange(orderedMessages.Select(m => new ChatMessageContent()
                //{
                //    Role = new AuthorRole(m.Role),
                //    Content = m.Content
                //}));
                history.AddUserMessage(taskRequest.Message);

                // Enable Function Calling
                var executionSettings = new GeminiPromptExecutionSettings
                {
                    ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions
                };

                var result = await chatService.GetChatMessageContentAsync(
                    history,
                    executionSettings: executionSettings,
                    kernel: kernel
                );

                // Save AI reply (optional)
                await _repository.Create(new Message
                {
                    ContextId = taskRequest.ContextId,
                    Content = result.Content,
                    Role = Roles.Assistant,
                    TaskId = taskRequest.TaskId,
                });

                return result.Content ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Chat()");
                return "Sorry, something went wrong.";
            }
        }



        public async Task<string> Chat(string request, string responseContext, TelemetryTask? taskRequest = null)
        {
            try
            {
                var kernel = _kernelProvider.Kernel;
                var chatService = _kernelProvider.ChatCompletionService;

                var history = new ChatHistory();

                // Build system message
                var systemMessage = PromptBuilder.BuildUserResponsePrompt(request, responseContext);
                history.AddSystemMessage(systemMessage);

                // If in conversation flow — load prior messages
                if (taskRequest.IsConversation)
                {
                    var previousMessages = await _repository.GetAllAsync(m => m.ContextId == taskRequest.ContextId);

                    history.AddRange(previousMessages.Select(m => new ChatMessageContent
                    {
                        Role = new AuthorRole(m.Role),
                        Content = m.Content
                    }));

                }
                    // Save user message
                    await _repository.Create(new Message
                    {
                        ContextId = taskRequest.ContextId,
                        Content = request,
                        Role = Roles.User,
                        TaskId = taskRequest.TaskId
                    });

                // Add current user message
                history.AddUserMessage(request);

                // AI reply
                var result = await chatService.GetChatMessageContentAsync(history);

                // Save AI reply
                if (taskRequest != null)
                {
                    await _repository.Create(new Message
                    {
                        ContextId = taskRequest.ContextId,
                        Content = result.Content,
                        Role = Roles.Assistant,
                        TaskId = taskRequest.TaskId
                    });
                }

                _logger.LogInformation("Chat reply: {Reply}", result.Content);

                return result.Content ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Chat()");
                return "Sorry, something went wrong.";
            }
        }


        public async Task<string> GenerateResponse(string message, string systemMessage, TelemetryTask task)
        {            
            await _messageRepository.AddMessageAsync(message, task, "user");

            var conversations = await _messageRepository.GetMessagesAsync(task.ContextId);

            var chatHistory = new List<TelexChatMessage>();

            // Add system message
            chatHistory.Add(new TelexChatMessage() { Role = Roles.System, Content = systemMessage });

            chatHistory.AddRange(conversations);                       

            // Add user message
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

            var response = await _httpHelper.SendRequestAsync<TelexChatResponse>(apiRequest);
           
            if (response.Status == "error")
            {              
                return $"An error occurred while communicating with Telex AI: {response.Message}";
            }

            _logger.LogInformation("Message successfully generated from the Telex AI");            

            string aiResponse = response.Data.Messages.Content;

            await _messageRepository.AddMessageAsync(aiResponse, task, "assistant");

            return aiResponse;
        }
    }
}
