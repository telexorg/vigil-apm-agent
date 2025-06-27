using Microsoft.Extensions.Options;
namespace VigilAgent.Api.Commons
{
    public class TelexApiSettings
    {
       
        public const string Header = "X-AGENT-API-KEY";
        public const string DatabaseName = "VigilAgent";

        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public string WebhookUrl { get; set; }
        public string WebSocketUrl { get; set; }

    }

}
