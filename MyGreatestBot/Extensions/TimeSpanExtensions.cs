using System;

namespace MyGreatestBot.Extensions
{
    internal static class TimeSpanExtensions
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
                withMilliseconds ? GetPaddedValue(time.Milliseconds, 3) : string.Empty).Trim(':');
        }
    }
}