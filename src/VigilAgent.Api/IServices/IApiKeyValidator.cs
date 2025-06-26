using VigilAgent.Api.Models;

namespace VigilAgent.Api.IServices
{
    public interface IApiKeyValidator
    {
        Task<Project?> ValidateAsync(string apiKey);
    }
}