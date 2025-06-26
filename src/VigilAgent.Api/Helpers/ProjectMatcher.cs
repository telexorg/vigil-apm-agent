using System.Text.Json;

namespace VigilAgent.Api.Helpers
{
    public static class ProjectMatcher
    {
        public static string? ResolveBestKey(IEnumerable<string> cacheKeys, string orgId, string userInputProjectName)
        {
            var prefix = $"{orgId}:";
            Console.WriteLine($"Cachekeys: {string.Join(", ", cacheKeys)}");
            var matches = cacheKeys
                .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(k => new
                {
                    Key = k,
                    Score = StringSimilarity.Score(
                        Normalize(k.Split(':')[1]),
                        Normalize(userInputProjectName))
                })
                .OrderByDescending(m => m.Score)
                .ToList();

            return matches.FirstOrDefault()?.Key;
        }

        //public static string? ResolveBestKey(IEnumerable<string> cacheKeys, string orgId, string userInputProjectName)
        //{
        //    var prefix = $"{orgId}:";
        //    var normalizedInput = userInputProjectName.ToLowerInvariant().Replace("-", "").Replace(" ", "");

        //    var matches = cacheKeys
        //        .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        //        .FirstOrDefault(k =>
        //        {
        //            var projectName = k.Split(':')[1]
        //                .ToLowerInvariant()
        //                .Replace("-", "")
        //                .Replace(" ", "");

        //            return projectName.Contains(normalizedInput);
        //        });

        //    return matches;
        //}
        public static string Normalize(string input)
                 => input.ToLowerInvariant().Replace("-", " ").Trim();
    }

    public static class StringSimilarity
    {
        public static double Score(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
                return 0;

            a = a.Trim().ToLowerInvariant();
            b = b.Trim().ToLowerInvariant();

            int stepsToSame = ComputeLevenshteinDistance(a, b);
            return 1.0 - (double)stepsToSame / Math.Max(a.Length, b.Length);
        }

        private static int ComputeLevenshteinDistance(string s, string t)
        {
            var d = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++)
                d[i, 0] = i;

            for (int j = 0; j <= t.Length; j++)
                d[0, j] = j;

            for (int j = 1; j <= t.Length; j++)
            {
                for (int i = 1; i <= s.Length; i++)
                {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s.Length, t.Length];
        }
    }
}
