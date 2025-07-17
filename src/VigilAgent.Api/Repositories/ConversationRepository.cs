using BloggerAgent.Domain.Commons;
using System.Linq.Expressions;
using VigilAgent.Api.Commons;
using VigilAgent.Api.Data;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.Repositories
{
    public class ConversationRepository : TelexRepository<Message>, IConversationRepository
    {
        private readonly TelexDbContext _context;
        private readonly ITelexRepository<Message> _repository;

        public ConversationRepository(TelexDbContext context, ITelexRepository<Message> repository) : base(context)
        {
            _context = context;
            _repository = repository;
        }


        public async Task<Document<Message>> GetConversationsByUserAsync(string userId)
        {
            return await _repository.GetByIdAsync(userId);
        }


        public async Task<List<TelexChatMessage>> GetMessagesAsync(string contextId)
        {
            var conversations = await _repository.FilterAsync(new { tag = CollectionType.Message });
            if (conversations == null)
            {
                throw new Exception("Failed to retrieve messages");
            }

            return conversations
                .Where(c => c.Data.ContextId == contextId)
                .Select(c => new TelexChatMessage()
                {
                    Role = c.Data.Role,
                    Content =  c.Data.Content 
                }).ToList();
        }


        public async Task<bool> AddMessageAsync(string message, TaskContext task, string role)
        {
            var newMessage = new Message
            {
                Id = Guid.NewGuid().ToString(),
                Content = message,
                TaskId = task.TaskId,
                ContextId = task.ContextId,
                Role = role
            };

            return await _repository.CreateAsync(newMessage);             

        }

    }
}

