namespace VigilAgent.Api.Helpers
{
    public interface IApiKeyManager
    {
        string EncryptionKey { get; }
        string HashPepper { get; }

        (string projectId, string orgId)? ExtractIds(string apiKey);
        string GenerateApiKey(string projectId, string orgId);
        string HashApiKey(string rawKey);
        bool Validate(string inputKey, string storedHash);
    }
}