using VigilAgent.Api.Models;

namespace VigilAgent.Api.Contracts
{
    public interface IApiKeyValidator
    {
        Task<Project?> ValidateAsync(string apiKey);
    }
}