using Microsoft.Extensions.Options;
namespace VigilAgent.Api.Commons
{
    public class TelexApiSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; }
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }

        public const string Header = "X-TELEX-API-KEY";
        public string WebhookUrl { get; set; }

    }

}
