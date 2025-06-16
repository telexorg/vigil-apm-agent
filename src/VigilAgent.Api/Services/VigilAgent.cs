using Microsoft.AspNetCore.Http;
using BloggerAgent.Domain.Commons;
using VigilAgent.Api.IServices;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.Helpers;
using Microsoft.Extensions.Options;
using VigilAgent.Api.Commons;
using VigilAgent.Api.Dtos;

namespace VigilAgent.Api.Services
{
    public class VigilAgent
    {
        //private static readonly ConcurrentDictionary<string, List<ChatMessage>> conversations = new(); // Group messages by channelId

        private ILogger<VigilAgent> _logger;
        private readonly IRequestProcessingService _requestService;
        private readonly IConversationRepository _messageRepository;
        private readonly IAIService _aiService;
        private readonly HttpHelper _httpHelper;


        private readonly RequestRouter _router;

        public VigilAgent()
        {
            var handlers = new Dictionary<string, IAgentCommandHandler>
            {
                { "show-logs", new ShowLogsHandler() },
                { "explain-errors", new ExplainErrorsHandler() },
                { "show-metrics", new ShowRuntimeMetrics() },
                { "recommend-fix", new RecommendFixHandler() }
            };

            _router = new RequestRouter(handlers);
        }

        public VigilAgent(ILogger<VigilAgent> logger, IRequestProcessingService requestService, IConversationRepository messageRepository, IAIService aiRepository, HttpHelper httpHelper)
        {
            _requestService = requestService;
            _logger = logger;
            _messageRepository = messageRepository;
            _aiService = aiRepository;
            _httpHelper = httpHelper;
        }       

       

        public async Task<MessageResponse> HandleUserInput(TaskRequest taskRequest)
        {
            try
            {
                var newTaskRequest = TelemetryTask.ExtractTaskData(taskRequest);
                            
                string response = await _router.RouteAsync(newTaskRequest.Message);

                return DataExtract.ConstructResponse(taskRequest, response);

            }
            catch (Exception ex)
            {
                // Log the error and rethrow the exception
                _logger.LogError(ex, "Failed to generate response");
                throw;
            }
}

       
    }
}
