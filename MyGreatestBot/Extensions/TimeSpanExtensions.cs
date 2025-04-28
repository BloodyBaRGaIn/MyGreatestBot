using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyGreatestBot.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string GetCustomTime(this TimeSpan time, bool withMilliseconds = false)
        {
            static string GetPaddedValue(double x, int pad = 2)
            {
                return $"{(int)x}".PadLeft(pad, '0');
            }

            return string.Join(':',
                GetPaddedValue(time.TotalHours),
                GetPaddedValue(time.Minutes),
                GetPaddedValue(time.Seconds),
                withMilliseconds
                ? GetPaddedValue(time.Milliseconds, 3)
                : string.Empty).Trim(':');
        }
    }

    public static partial class TimeSpanRegexProvider
    {
        private const string Delimiter = ":";

        private const string Expression =
            $"^([\\d]+){Delimiter}([\\d]{{2}})({Delimiter}[\\d]{{2}})?$";

        internal const string HoursMinutesSecondsFormat = $"\"H+{Delimiter}MM{Delimiter}SS\"";
        internal const string MinutesSecondsFormat = $"\"M+{Delimiter}SS\"";

        [GeneratedRegex(Expression)]
        private static partial Regex GenerateTimeSpanRegex();

        private static readonly Regex TimeSpanRegex = GenerateTimeSpanRegex();

        internal static TimeSpan GetTimeSpan(in string input)
        {
            Match match;

            try
            {
                match = TimeSpanRegex.Match(input);
            }
            catch
            {
                return TimeSpan.MinValue;
            }

            List<int> pureValues = [];

            IEnumerable<string> rawCollection = match.Groups.Values
                .Skip(1)
                .Select(static v => v.Value.Trim(':'))
                .Where(static s => !string.IsNullOrWhiteSpace(s));

            foreach (string rawValue in rawCollection)
            {
                if (int.TryParse(rawValue, out int pureResult) && pureResult >= 0)
                {
                    pureValues.Add(pureResult);
                }
            }

            if (pureValues.Count is 0 or > 3)
            {
                return TimeSpan.MinValue;
            }

            while (pureValues.Count < 3)
            {
                pureValues.Insert(0, 0);
            }

            return new(pureValues[0], pureValues[1], pureValues[2]);
        }
    }
}
