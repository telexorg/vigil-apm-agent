namespace VigilAgent.Api.IServices
{
    public interface ITaskStreamHandler
    {
        Task StreamResponseAsync(IEnumerable<string> tokens);
    }
}