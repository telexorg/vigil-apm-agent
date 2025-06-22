namespace VigilAgent.Api.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string HeaderName = "X-AGENT-API-Key";
        private const string ValidApiKey = ""; // Store securely

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(HeaderName, out var key) || key != ValidApiKey)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized - Invalid API Key");
                return;
            }

            await _next(context);
        }    

    }
}
