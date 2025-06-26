namespace VigilAgent.Api.Helpers
{
    public class TimeFormatter
    {

        public static bool TryParseTimeRange(string input, out TimeSpan span)
        {
            span = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            try
            {
                input = input.Trim().ToLowerInvariant();

                if (input.EndsWith("d") && int.TryParse(input.TrimEnd('d'), out int days))
                {
                    span = TimeSpan.FromDays(days);
                    return true;
                }

                if (input.EndsWith("h") && int.TryParse(input.TrimEnd('h'), out int hours))
                {
                    span = TimeSpan.FromHours(hours);
                    return true;
                }

                if (input.EndsWith("m") && int.TryParse(input.TrimEnd('m'), out int mins))
                {
                    span = TimeSpan.FromMinutes(mins);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

    }
}
