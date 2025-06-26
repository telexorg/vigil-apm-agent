using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Apm.Utils
{
    public static class ApiSignatureUtility
    {
        public static string CreateSignature(string payloadJson, string apiKey)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(payloadJson + apiKey);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
