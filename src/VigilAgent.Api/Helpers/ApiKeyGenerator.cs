using System.Security.Cryptography;
using System.Text;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using VigilAgent.Api.Commons;
using MongoDB.Driver;


namespace VigilAgent.Api.Helpers
{

    public class ApiKeyManager : IApiKeyManager
    {
        public string EncryptionKey { get; }
        public string HashPepper { get; }

        public ApiKeyManager(IOptions<ApiSecret> options)
        {
            var value = options.Value;

            EncryptionKey = !string.IsNullOrWhiteSpace(value.EncryptionKey)
            ? value.EncryptionKey
            : throw new InvalidOperationException("EncryptionKey is required.");

            HashPepper = !string.IsNullOrWhiteSpace(value.ApiKeyPepper)
            ? value.ApiKeyPepper
            : throw new InvalidOperationException("ApiKeyPepper is required.");
        }


        public string GenerateApiKey(string projectId, string orgId)
        {
            var payload = $"{projectId}:{orgId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var encrypted = EncryptToBase64(payload);
            var suffix = Guid.NewGuid().ToString("N")[..8];
            return $"{encrypted}.{suffix}";
        }


        public string HashApiKey(string rawKey)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(rawKey + HashPepper);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }


        public bool Validate(string inputKey, string storedHash)
        {
            var hash = HashApiKey(inputKey);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(hash),
                Encoding.UTF8.GetBytes(storedHash)
            );
        }


        public (string projectId, string orgId)? ExtractIds(string apiKey)
        {
            var parts = apiKey.Split('.');
            if (parts.Length != 2) return null;

            try
            {
                var decrypted = DecryptFromBase64(parts[0]);
                var fields = decrypted.Split(':');
                if (fields.Length < 2) return null;
                return (fields[0], fields[1]);
            }
            catch
            {
                return null;
            }
        }


        private string EncryptToBase64(string plaintext)
        {
            using var aes = Aes.Create();
            aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(EncryptionKey));
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plaintext);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }


        private string DecryptFromBase64(string encryptedText)
        {
            var fullCipher = Convert.FromBase64String(encryptedText);

            using var aes = Aes.Create();
            aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(EncryptionKey));

            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }

    public static class ApiKeyGenerator
    {
        public static string GenerateKey()
        {
            var keyBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(keyBytes);
            return Convert.ToBase64String(keyBytes);
        }

        public static string HashKey(string apiKey, string secretKey)
        {
            using var sha256 = SHA256.Create();
            var saltedKey = Encoding.UTF8.GetBytes(apiKey + secretKey);
            var hash = sha256.ComputeHash(saltedKey);
            return Convert.ToHexString(hash);
        }
    }
}
