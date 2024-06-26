﻿using System.Text.RegularExpressions;

namespace MyGreatestBot.Extensions
{
    /// <summary>
    /// <see cref="Regex"/> extensions
    /// </summary>
    public static class RegexExtensions
    {
        public static string? GetMatchValue(this Regex regex, string query)
        {
            return regex.GetMatchValue(query, 1)[0];
        }

        public static string?[] GetMatchValue(this Regex regex, string query, params int[] groups)
        {
            string?[] result = new string?[groups.Length];

            Match match = regex.Match(query);

            if (!match.Success)
            {
                return result;
            }

            for (int i = 0; i < groups.Length; i++)
            {
                try
                {
                    result[i] = match.Groups[groups[i]].Value;
                }
                catch
                {
                    result[i] = null;
                }
            }

            return result;
        }
    }
}
