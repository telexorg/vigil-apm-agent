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
        private readonly IAIService _aiService;
        private readonly ITelemetryService _telemetryHandler;


        private readonly RequestRouter _router;
       

        public VigilAgentService(ILogger<VigilAgentService> logger, IAIService aiRepository, ITelemetryService telemetryHandler)
        {
            _telemetryHandler = telemetryHandler;
           
            _logger = logger;
            _aiService = aiRepository;
            _router = new RequestRouter(_telemetryHandler);
        }



        public async Task<MessageResponse> HandleUserInput(TaskRequest taskRequest)
        {
            try
            {
                var newTaskRequest = DataExtract.ExtractTaskData(taskRequest);

                _logger.LogInformation("HandleUserInput: UserMessage={Message}", newTaskRequest.Message);

                var aiReply = await _aiService.ChatWithTools(newTaskRequest);

                return DataExtract.ConstructResponse(taskRequest, aiReply);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleUserInput()");
                return DataExtract.ConstructResponse(taskRequest, "🤖 Sorry, something went wrong.");
            }
        }

        public async Task<MessageResponse> HandleUserInputWithRouting(TaskRequest task)
        {
            try
            {
                // STEP 1 — Extract task object
                var taskRequest = DataExtract.ExtractTaskData(task);

                // STEP 2 — Try to route message (no tokens, no AI yet)
                var responseContext = await _router.RouteAsync(taskRequest.Message);

                string response;

                if (responseContext != "NO_MATCH")
                {
                    // STEP 3 — Matched! Use stateless chat with backend context
                    _logger.LogInformation("Router matched — using stateless Chat()");
                    response = await _aiService.Chat(taskRequest.Message, responseContext, taskRequest);
                }
                else
                {
                    // STEP 4 — No router match — fallback to AI chat with history
                    _logger.LogWarning("Router failed — fallback to ChatWithHistory()");
                    response = await _aiService.ChatWithHistoryAsync(taskRequest);
                }

                // STEP 5 — Return AI reply
                return DataExtract.ConstructResponse(task, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleUserInput()");
                return DataExtract.ConstructResponse(task, "🤖 Sorry — something went wrong.");
            }
        }

        public async Task<MessageResponse> HandleUserInputWithIntentDetection(TaskRequest task)
        {
            try
            {
                var taskRequest = DataExtract.ExtractTaskData(task);

                // STEP 1 — First try router (cheap, fast)
                var responseContext = await _router.RouteAsync(taskRequest.Message);

                string response;

                if (responseContext != "NO_MATCH")
                {
                    // STEP 2 — Router matched — use stateless Chat
                    _logger.LogInformation("Router matched — using stateless Chat()");
                    response = await _aiService.Chat(taskRequest.Message, responseContext, taskRequest);
                }
                else
                {
                    // STEP 3 — Router failed — now detect intent with AI (token cost)
                    _logger.LogInformation("Router failed — detecting intent via AI");
                    var intent = await _aiService.GetIntentAsync(taskRequest.Message);

                    _logger.LogInformation("AI detected intent: {Intent}", intent);

                    // STEP 4 — If intent matches known type — route again
                    if (intent == "show-logs" || intent == "explain-errors" ||
                        intent == "show-metrics" || intent == "recommend-fix")
                    {
                        _logger.LogInformation("AI intent matched known type — routing");

                        var contextFromIntent = await _router.RouteAsync(intent);

                        if (contextFromIntent != "NO_MATCH")
                        {
                            response = await _aiService.Chat(taskRequest.Message, contextFromIntent, taskRequest);
                        }
                        else
                        {
                            _logger.LogWarning("AI intent could not be routed — fallback to ChatWithHistory()");
                            response = await _aiService.ChatWithHistoryAsync(taskRequest);
                        }
                    }
                    else
                    {
                        // Unknown intent — fallback to ChatWithHistory
                        _logger.LogInformation("Unknown AI intent — fallback to ChatWithHistory()");
                        response = await _aiService.ChatWithHistoryAsync(taskRequest);
                    }
                }

                return DataExtract.ConstructResponse(task, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleUserInput()");
                return DataExtract.ConstructResponse(task, "🤖 Sorry — something went wrong.");
            }
        }


    }
}
