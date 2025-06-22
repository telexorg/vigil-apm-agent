using BloggerAgent.Domain.Commons;
using Microsoft.Extensions.Options;
using VigilAgent.Api.Commons;
using VigilAgent.Api.Helpers;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.IServices;

namespace VigilAgent.Api.Services
{
    public class PushNotification
    {
        private ILogger<VigilAgentService> _logger;
        private string _webhookUrl;
        private readonly HttpHelper _httpHelper;

            

        public PushNotification(IOptions<TelexApiSettings> telexSettings, ILogger<VigilAgentService> logger, HttpHelper httpHelper)
        {
            _webhookUrl = telexSettings.Value.WebhookUrl;            
            _logger = logger;
            _httpHelper = httpHelper;
        }

        public async Task<bool> SendResponseAsync(string taskResponse, TelemetryTask taskRequest)
        {

            if (string.IsNullOrEmpty(taskRequest.ContextId) || string.IsNullOrEmpty(_webhookUrl))
            {
                throw new Exception("Channel ID is null");
            }

            var apiRequest = new ApiRequest()
            {
                Url = $"{_webhookUrl}/{taskRequest.ContextId}",
                Body = new
                {
                    channel_id = taskRequest.ContextId,
                    org_id = taskRequest.MessageId,
                    thread_id = taskRequest.TaskId,
                    message = taskResponse,
                    reply = false,
                    username = "Blogger Agent"
                },
                Method = HttpMethod.Post,
            };

            var telexResponse = await _httpHelper.SendRequestAsync(apiRequest);

            if ((int)telexResponse.StatusCode != StatusCodes.Status202Accepted || !telexResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Failed to send response to telex");
                return false;
            }

            string responseContent = await telexResponse.Content.ReadAsStringAsync();

            _logger.LogInformation($"Response successfully sent to telex: {responseContent}");

            return true;
        }
    }
}
