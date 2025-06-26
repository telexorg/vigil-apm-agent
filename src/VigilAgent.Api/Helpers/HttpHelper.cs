using System.Net.Http;
using System.Text;
using System.Text.Json;
using BloggerAgent.Domain.Commons;
using VigilAgent.Api.Commons;

namespace VigilAgent.Api.Helpers
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
        
        
        public async Task<TelexApiResponse<T>> SendRequestAsync<T>(ApiRequest request)
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

            var response =  await _httpClient.SendAsync(httpRequest);

            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return TelexApiResponse<T>.ErrorResponse(responseString);               
            }

            return TelexApiResponse<T>.ExtractResponse(responseString);

        }
    }
}
