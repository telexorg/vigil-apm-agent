namespace VigilAgent.Api.Helpers
{
    public static class CacheKey
    {
        public static string For(string orgId, string projectName) =>
            $"{orgId}:{projectName}".ToLowerInvariant();
    }
}
