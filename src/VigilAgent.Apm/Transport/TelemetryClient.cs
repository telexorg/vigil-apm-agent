using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System.Text;
using System.Text.Json;
using VigilAgent.Apm.Config;
using VigilAgent.Apm.Utils;
using System.Net;
using VigilAgent.Apm.Contracts;

namespace VigilAgent.Apm.Processing
{

    public class TelemetryClient : ITelemetryClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TelemetryClient> _logger;
        private readonly string _apiKey;
        private readonly string _endPoint;
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;


        public TelemetryClient(HttpClient httpClient, IOptions<TelemetryOptions> options, ILogger<TelemetryClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var config = options?.Value ?? throw new ArgumentNullException(nameof(options));

            _apiKey = !string.IsNullOrWhiteSpace(config.ApiKey) ? config.ApiKey
                    : throw new ArgumentException("API key is missing.");

            _endPoint = TelemetryOptions.TelemetryEndpoint;

            _retryPolicy = Policy
                   .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && r.StatusCode != HttpStatusCode.Unauthorized)
                   .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                       (outcome, delay, attempt, _) =>
                       {
                           _logger.LogWarning("[VigilClient] Retry {Attempt} after {Delay} due to {StatusCode}",
                           attempt, delay, outcome.Result?.StatusCode);
                       }
                   );

        }

        public async Task<bool> SendBatchAsync(List<object> events)
        {
            if (events is null || events.Count == 0)
                return false;

            var json = JsonSerializer.Serialize(events, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var signature = ApiSignatureUtility.CreateSignature(json, _apiKey);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.Add("X-VIGIL-API-KEY", _apiKey);
            content.Headers.Add("X-VIGIL-API-SIGNATURE", signature);

            try
            {
                _logger.LogInformation("[VigilClient] Sending telemetry batch: {Count}", events.Count);

                var response = await _retryPolicy.ExecuteAsync(() =>
                    _httpClient.PostAsync(_endPoint, content));

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[VigilClient] Telemetry send failed: {Code} - {Reason}",
                        (int)response.StatusCode, response.ReasonPhrase);

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _logger.LogWarning("[VigilClient] Please check that your API key is valid or request for a new one");
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VigilClient] Telemetry send permanently failed after retries");
            }

            _logger.LogInformation("[VigilClient] Telemetry batch successfully sent");

            return true;
        }

    //    private static readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy =
    //Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    //    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
    //        (outcome, delay, attempt, context) =>
    //        {
    //            Console.WriteLine($"Retry {attempt} after {delay} due to {outcome.Result?.StatusCode}");
    //        });
    }

}
