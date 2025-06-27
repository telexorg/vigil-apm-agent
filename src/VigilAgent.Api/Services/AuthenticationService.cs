using VigilAgent.Api.Dtos;
using VigilAgent.Api.Helpers;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.IServices;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IMongoRepository<Project> _projects;
        private readonly IApiKeyManager _apiKeyManager;

        public AuthenticationService(IMongoRepository<Project> projects, IApiKeyManager apiKeyManager)
        {
            _projects = projects;
            _apiKeyManager = apiKeyManager;
        }

        public async Task<(bool isConflict, Project created, string rawApiKey)> RegisterProjectAsync(ProjectRegistrationRequest request)
        {
            var existing = await _projects.FindOneAsync(p =>
                p.OrgId == request.OrgId &&
                p.ProjectName == request.ProjectName);

            if (existing != null)
                return (true, null, null);

            var projectId = $"proj_{Guid.NewGuid():N}"[..8];
            var apiKey = _apiKeyManager.GenerateApiKey(projectId, request.OrgId);
            var hash = _apiKeyManager.HashApiKey(apiKey);

            var project = new Project
            {
                Id = projectId,
                ProjectName = request.ProjectName,
                OrgId = request.OrgId,
                ApiKey = hash,
                CreatedAt = DateTime.UtcNow
            };

            await _projects.Create(project);
            return (false, project, apiKey);
        }
    }
}
