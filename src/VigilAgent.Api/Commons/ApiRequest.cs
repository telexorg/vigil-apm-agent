namespace BloggerAgent.Domain.Commons
{
    public class ApiRequest
    {
        public HttpMethod Method { get; set; }
        public string Url { get; set; }
        public object? Body { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();

      
    }
}
