using VigilAgent.Api.Dtos;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.IServices
{
    public interface IAuthenticationService
    {
        Task<(bool isConflict, Project created, string rawApiKey)> RegisterProjectAsync(ProjectRegistrationRequest request);
    }
}