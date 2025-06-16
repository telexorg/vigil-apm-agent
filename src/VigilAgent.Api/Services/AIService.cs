using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VigilAgent.Api.IServices;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.Helpers;
using VigilAgent.Api.Commons;
using BloggerAgent.Domain.Commons;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.Services
{
    public class AIService : IAIService
    {
        private ILogger<VigilAgent> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly IConversationRepository _messageRepository;
        private readonly HttpHelper _httpHelper;


        public AIService(IOptions<TelexApiSettings> dataConfig, IOptions<TelexApiSettings> telexSettings, ILogger<VigilAgent> logger, IConversationRepository messageRepository, HttpHelper httpHelper)
        {
            _apiKey = dataConfig.Value.ApiKey;
            _baseUrl = dataConfig.Value.BaseUrl;
            _logger = logger;
            _messageRepository = messageRepository;
            _httpHelper = httpHelper;
        }
        public async Task<string> GenerateResponse(string message, string systemMessage, TelemetryTask task)
        {
            var messages = new List<TelexChatMessage>()
            {
                new TelexChatMessage() { Role = Roles.System, Content = systemMessage }
            };

            var conversations = await _messageRepository.GetMessagesAsync(task.ContextId);

            if (conversations.Count > 0 || conversations != null)
            {
                messages.AddRange(conversations);
            }

            messages.Add(new TelexChatMessage { Role = "user", Content = message });

            var apiRequest = new ApiRequest()
            {
                Url = $"{_baseUrl}/telexai/chat",
                Body = new { messages },
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

            await AddNewMessagesAsync(message, task, generatedResponse);

            return generatedResponse;
        }

        private async Task AddNewMessagesAsync(string message, TelemetryTask blogDto, string generatedResponse)
        {
            var newMessages = new List<Message>()
            {
                new Message { Id = Guid.NewGuid().ToString(), Content = message, TaskId = blogDto.TaskId, ContextId = blogDto.ContextId, Role = Roles.User },
                new Message() { Id = Guid.NewGuid().ToString(), Content = generatedResponse, TaskId = blogDto.TaskId, ContextId = blogDto.ContextId, Role = Roles.Assistant }
            };

            foreach (Message newMessage in newMessages)
            {
                bool isAdded = await _messageRepository.CreateAsync(newMessage);

                if (!isAdded)
                    _logger.LogInformation($"Failed to add {newMessage.Role} message to database");
            }
        }
    }
}
