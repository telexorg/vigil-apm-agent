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
    public class VigilAgentService : IVigilAgentService
    {
        //private static readonly ConcurrentDictionary<string, List<ChatMessage>> conversations = new(); // Group messages by channelId

        private ILogger<VigilAgentService> _logger;
        private readonly IIntentClassifier _requestService;
        private readonly IConversationRepository _messageRepository;
        private readonly IAIService _aiService;
        private readonly HttpHelper _httpHelper;


        private readonly RequestRouter _router;

        //public VigilAgent()
        //{
        //    var handlers = new Dictionary<string, IAgentCommandHandler>
        //    {
        //        { "show-logs", new ShowLogsHandler() },
        //        { "explain-errors", new ExplainErrorsHandler() },
        //        { "show-metrics", new ShowRuntimeMetrics() },
        //        { "recommend-fix", new RecommendFixHandler() }
        //    };

        //    _router = new RequestRouter(handlers);
        //}

        public VigilAgentService(ILogger<VigilAgentService> logger, IIntentClassifier requestService, IConversationRepository messageRepository, IAIService aiRepository, HttpHelper httpHelper)
        {
            var handlers = new Dictionary<string, ITelemetryHandler>
            {
                { "show-logs", new ShowLogsHandler() },
                { "explain-errors", new ExplainErrorsHandler() },
                { "show-metrics", new ShowRuntimeMetrics() },
                { "recommend-fix", new RecommendFixHandler() }
            };

            _requestService = requestService;
            _logger = logger;
            _messageRepository = messageRepository;
            _aiService = aiRepository;
            _httpHelper = httpHelper;


            _router = new RequestRouter(handlers);
        }       

       

        public async Task<MessageResponse> HandleUserInput(TaskRequest taskRequest)
        {
            try
            {
                var newTaskRequest = DataExtract.ExtractTaskData(taskRequest);

                string responseContext = await _router.RouteAsync(newTaskRequest.Message);

                if (string.IsNullOrEmpty(responseContext))
                {
                    _logger.LogWarning("No handler found for the command: {Command}", newTaskRequest.Message);
                    return DataExtract.ConstructResponse(taskRequest, "No handler found for the command.");
                }

                _logger.LogInformation("Routing request to handler: {Handler}", responseContext);
                var intent = await _aiService.Chat(newTaskRequest.Message, responseContext);


                return DataExtract.ConstructResponse(taskRequest, intent);

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
