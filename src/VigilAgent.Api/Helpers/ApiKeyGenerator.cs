using System.Security.Cryptography;
using System.Text;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;


namespace VigilAgent.Api.Helpers
{
            
    public static class ApiKeyManager
    {
        private static string _encryptionKey;
        private static string _hashPepper;

        public static void Configure(IConfiguration config)
        {
            _encryptionKey = config["ApiSecrets:EncryptionKey"]
                             ?? throw new InvalidOperationException("EncryptionKey not configured.");
            _hashPepper = config["ApiSecrets:ApiKeyPepper"]
                          ?? throw new InvalidOperationException("ApiKeyPepper not configured.");
        }

        // 🚀 Generate: encrypted payload + suffix
        public static string GenerateApiKey(string projectId, string orgId)
        {
            var payload = $"{projectId}:{orgId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var encrypted = EncryptToBase64(payload);
            var suffix = Guid.NewGuid().ToString("N")[..8];
            return $"{encrypted}.{suffix}";
        }

        // 🔐 Hash with pepper
        public static string HashApiKey(string rawKey)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(rawKey + _hashPepper);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        // ✅ Validate key against stored hash
        public static bool Validate(string inputKey, string storedHash)
        {
            var hash = HashApiKey(inputKey);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(hash),
                Encoding.UTF8.GetBytes(storedHash)
            );
        }

        // 🧠 Extract org + project from key
        public static (string projectId, string orgId)? ExtractIds(string apiKey)
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

        // 🔒 Encryption + base64
        private static string EncryptToBase64(string plaintext)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32)[..32]);
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plaintext);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        // 🔓 Decryption from base64 input
        private static string DecryptFromBase64(string encryptedText)
        {
            var fullCipher = Convert.FromBase64String(encryptedText);

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32)[..32]);

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
