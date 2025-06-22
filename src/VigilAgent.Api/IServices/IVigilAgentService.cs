using VigilAgent.Api.Dtos;

namespace VigilAgent.Api.IServices
{
    public interface IVigilAgentService
    {
        Task<MessageResponse> HandleUserInput(TaskRequest taskRequest);
    }
}
