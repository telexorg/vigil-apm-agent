using VigilAgent.Api.Commons;
using VigilAgent.Api.IServices;

namespace VigilAgent.Api.Services
{
    public class TaskContextProvider : ITaskContextProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TaskContextProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public TaskContext? Get()
        {
            var ctx = _httpContextAccessor.HttpContext;
            return ctx.Items.TryGetValue("TaskContext", out var taskContext) && taskContext is TaskContext task
                ? task
                : null;
        }
    }
}
