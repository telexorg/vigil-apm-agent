using VigilAgent.Api.Commons;

namespace VigilAgent.Api.IServices
{
    public interface ITaskContextProvider
    {
        TaskContext? Get();
    }
}