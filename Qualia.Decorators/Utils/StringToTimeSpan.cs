using System.Text.RegularExpressions;

namespace Qualia.Decorators.Utils
{
    internal static partial class StringToTimeSpan
    {
        public static TimeSpan Parse(string input)
        {
            var match = TimeSpanFromStringRegex().Match(input);
            if (!match.Success)
            {
                throw new ArgumentException("Invalid timespan format. Use '1s', '1m', '1h', or '1d'.");
            }

            int value = int.Parse(match.Groups["value"].Value);
            string unit = match.Groups["unit"].Value.ToLower();

            return unit switch
            {
                "s" => TimeSpan.FromSeconds(value),
                "m" => TimeSpan.FromMinutes(value),
                "h" => TimeSpan.FromHours(value),
                "d" => TimeSpan.FromDays(value),
                _ => throw new ArgumentException("Invalid timespan unit. Use 's' for seconds, 'm' for minutes, 'h' for hours, or 'd' for days.")
            };
        }

        [GeneratedRegex(@"^(?<value>\d+)(?<unit>[smhd])$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex TimeSpanFromStringRegex();
    }
}
