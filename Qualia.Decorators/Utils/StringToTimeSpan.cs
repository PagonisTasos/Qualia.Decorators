using System;
using System.Text.RegularExpressions;

namespace Qualia.Decorators.Utils
{
    internal static partial class StringToTimeSpan
    {
        public static TimeSpan Parse(string input)
        {
            var match = new Regex(@"^(?<value>\d+)(?<unit>[smhd])$", RegexOptions.IgnoreCase).Match(input);
            if (!match.Success)
            {
                throw new ArgumentException("Invalid timespan format. Use '1s', '1m', '1h', or '1d'.");
            }

            int value = int.Parse(match.Groups["value"].Value);
            string unit = match.Groups["unit"].Value.ToLower();

            if (unit == "s") return TimeSpan.FromSeconds(value);
            if (unit == "m") return TimeSpan.FromMinutes(value);
            if (unit == "h") return TimeSpan.FromHours(value);
            if (unit == "d") return TimeSpan.FromDays(value);

            throw new ArgumentException("Invalid timespan unit. Use 's' for seconds, 'm' for minutes, 'h' for hours, or 'd' for days.");
        }
    }
}
