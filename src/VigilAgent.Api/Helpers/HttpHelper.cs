using System.Net.Http;
using System.Text;
using System.Text.Json;
using BloggerAgent.Domain.Commons;

namespace VigilAgent.Api.Services
{
    public class HttpHelper
    {
        private readonly HttpClient _httpClient;

        public HttpHelper(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<HttpResponseMessage> SendRequestAsync(ApiRequest request)
        {
            var httpRequest = new HttpRequestMessage(request.Method, request.Url);

            // Add headers
            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                {
                    httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Add body if present
            if (request.Body != null)
            {
                string json = JsonSerializer.Serialize(request.Body);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return await _httpClient.SendAsync(httpRequest);
        }
    }
}
