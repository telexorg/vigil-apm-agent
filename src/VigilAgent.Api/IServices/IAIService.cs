using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VigilAgent.Api.Commons;

namespace VigilAgent.Api.IServices
{
    public interface IAIService
    {
        Task<string> GenerateResponse(string message, string systemMessage, TaskContext taskRequest);
        Task<string> Chat(string request, string responseContext, TaskContext taskRequest = null);
        Task<string> GetIntentAsync(string message);
        Task<string> ChatWithHistoryAsync(TaskContext taskRequest);

        Task<string> ChatWithTools(TaskContext taskRequest);
    }
}
