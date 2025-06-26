using BloggerAgent.Domain.Commons;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System.Reflection.Metadata;
using VigilAgent.Api.Commons;
using VigilAgent.Api.Helpers;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.Data
{
    public class TelexDbContext
    {
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly HttpHelper _httphelper;
        private const string _collectionName = "test_collection";
        private readonly string tagName;

        public TelexDbContext(IOptions<TelexApiSettings> options, HttpHelper httphelper)
        {
            _baseUrl = options.Value.BaseUrl;
            _apiKey = options.Value.ApiKey;
            _httphelper = httphelper;

            if (_apiKey == null)
            {
                throw new ArgumentNullException(nameof(_apiKey));
            }

        }


        public async Task<TelexApiResponse<T>> CreateCollection<T>()
        {
            var apiRequest = new ApiRequest()
            {
                Method = HttpMethod.Post,
                Url = $"{_baseUrl}/agent_db/collections",
                Body = new
                {
                    collection_name = _collectionName,
                },
                Headers = new Dictionary<string, string>()
               {
                   {TelexApiSettings.Header, _apiKey}
               }
            };

            var response = await _httphelper.SendRequestAsync(apiRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Fallback: if something goes wrong with the HTTP call itself
            if (!response.IsSuccessStatusCode)
            {
                var telexResponse = TelexApiResponse<T>.ErrorResponse(responseContent);
            }

            return TelexApiResponse<T>.ExtractResponse(responseContent);
        }


        public async Task<TelexApiResponse<List<Document<T>>>> GetAll<T>(object filter = null)
        {
            var apiRequest = new ApiRequest()
            {
                Method = HttpMethod.Get,
                Url = $"{_baseUrl}/agent_db/collections/{_collectionName}/documents",
                Body = new
                {
                    Filter = filter
                },
                Headers = new Dictionary<string, string>()
                {
                   {TelexApiSettings.Header, _apiKey}
                }
            };

            var response = await _httphelper.SendRequestAsync(apiRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Fallback: if something goes wrong with the HTTP call itself
            if (!response.IsSuccessStatusCode)
            {
                return TelexApiResponse<List<Document<T>>>.ErrorResponse(responseContent);
            }

            return TelexApiResponse<List<Document<T>>>.ExtractResponse(responseContent);

        }


        public async Task<TelexApiResponse<Document<T>>> GetSingle<T>(string id)
        {
            var apiRequest = new ApiRequest()
            {
                Method = HttpMethod.Get,
                Url = $"{_baseUrl}/agent_db/collections/{_collectionName}/documents/{id}",
                Headers = new Dictionary<string, string>()
                {
                   {TelexApiSettings.Header, _apiKey}
                }
            };

            var response = await _httphelper.SendRequestAsync(apiRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            //var telexResponse = TelexApiResponse<Document<T>>.ExtractResponse(responseContent);

            // Fallback: if something goes wrong with the HTTP call itself
            if (!response.IsSuccessStatusCode)
            {
                var telexResponse = TelexApiResponse<T>.ErrorResponse(responseContent);
            }

            return TelexApiResponse<Document<T>>.ExtractResponse(responseContent);

        }


        public async Task<TelexApiResponse<Document<T>>> AddAsync<T>(T document)
        {
            string tagName = nameof(T);
            if (typeof(T) == typeof(Error)) tagName = nameof(T);
            else if (typeof(T) == typeof(Organization)) tagName = CollectionType.Organization;
            else if (typeof(T) == typeof(Message)) tagName = CollectionType.Message;

            var apiRequest = new ApiRequest()
            {
                Method = HttpMethod.Post,
                Url = $"{_baseUrl}/agent_db/collections/{_collectionName}/documents",
                Body = new
                {
                    Document = new
                    {
                        tag = tagName,
                        data = document
                    }
                },
                Headers = new Dictionary<string, string>()
                {
                   {TelexApiSettings.Header, _apiKey}
                }
            };

            var response = await _httphelper.SendRequestAsync(apiRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Fallback: if something goes wrong with the HTTP call itself
            if (!response.IsSuccessStatusCode)
            {
                var telexResponse = TelexApiResponse<T>.ErrorResponse(responseContent);
            }

            return TelexApiResponse<Document<T>>.ExtractResponse(responseContent);
        }


        public async Task<TelexApiResponse<Document<T>>> UpdateAsync<T>(string id, object document)
        {

            var apiRequest = new ApiRequest()
            {
                Method = HttpMethod.Put,
                Url = $"{_baseUrl}/agent_db/collections/{_collectionName}/documents/{id}",
                Body = new
                {
                    Document = document
                },
                Headers = new Dictionary<string, string>()
                {
                   {TelexApiSettings.Header, _apiKey}
                }
            };

            var response = await _httphelper.SendRequestAsync(apiRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Fallback: if something goes wrong with the HTTP call itself
            if (!response.IsSuccessStatusCode)
            {
                var telexResponse = TelexApiResponse<T>.ErrorResponse(responseContent);
            }

            return TelexApiResponse<Document<T>>.ExtractResponse(responseContent);
        }


        public async Task<TelexApiResponse<Document<T>>> DeleteAsync<T>(string id)
        {
            var apiRequest = new ApiRequest()
            {
                Method = HttpMethod.Delete,
                Url = $"{_baseUrl}/agent_db/collections/{_collectionName}/documents/{id}",
                Headers = new Dictionary<string, string>()
                {
                   {TelexApiSettings.Header, _apiKey}
                }
            };

            var response = await _httphelper.SendRequestAsync(apiRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Fallback: if something goes wrong with the HTTP call itself
            if (!response.IsSuccessStatusCode)
            {
                var telexResponse = TelexApiResponse<T>.ErrorResponse(responseContent);
            }

            return TelexApiResponse<Document<T>>.ExtractResponse(responseContent);
        }
    }
}
