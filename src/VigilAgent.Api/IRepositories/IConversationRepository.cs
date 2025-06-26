using System.Reflection.Metadata;
using VigilAgent.Api.Commons;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.IRepositories
{
    public interface IConversationRepository : ITelexRepository<Message>
    {
        Task<Document<Message>> GetConversationsByUserAsync(string userId);
        Task<List<TelexChatMessage>> GetMessagesAsync(string contextId);

        Task<bool> AddMessageAsync(string message, TelemetryTask task, string role);
    }
}