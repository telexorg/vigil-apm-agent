using VigilAgent.Api.Helpers;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.IServices;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.Services
{
    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly IMongoRepository<Project> _repo;
        private readonly IApiKeyManager _apiKeyManager;

        public ApiKeyValidator(IMongoRepository<Project> repo, IApiKeyManager apiKeyManager)
        {
            _repo = repo;
            _apiKeyManager = apiKeyManager;
        }

        public async Task<Project?> ValidateAsync(string apiKey)
        {
            var ids = _apiKeyManager.ExtractIds(apiKey);
            if (ids is not (string projectId, string orgId)) return null;

            var project = await _repo.FindOneAsync(p => p.Id == projectId && p.OrgId == orgId);
            if (project is null) return null;

            return _apiKeyManager.Validate(apiKey, project.ApiKey) ? project : null;
        }
    }
}
