namespace VigilAgent.Api.Commons.AgentCardSpecs
{
    public class SecurityScheme
    {
        public string Type { get; set; } = "";
        public string Scheme { get; set; }              // e.g. "bearer" (only for http)
        public string BearerFormat { get; set; }        // e.g. "JWT" (optional)
        public string OpenIdConnectUrl { get; set; }    // for openIdConnect

    }
    public class OpenIdConnectSecurityScheme : SecurityScheme
    {
        public string OpenIdConnectUrl { get; set; }
    }


    public class APIKeySecurityScheme : SecurityScheme
    {
        public string Name { get; set; }        // Name of the header/query/cookie
        public string In { get; set; }          // "query", "header", "cookie"
    }

    public class OAuth2SecurityScheme : SecurityScheme
    {
        public Dictionary<string, OAuthFlow> Flows { get; set; }
    }

    public class OAuthFlow
    {
        public string AuthorizationUrl { get; set; }
        public string TokenUrl { get; set; }
        public string RefreshUrl { get; set; }
        public Dictionary<string, string> Scopes { get; set; }
    }

    public class HTTPAuthSecurityScheme : SecurityScheme
    {
        public string Scheme { get; set; }          // e.g. "basic", "bearer"
        public string BearerFormat { get; set; }    // e.g. "JWT" (optional)
    }
}
