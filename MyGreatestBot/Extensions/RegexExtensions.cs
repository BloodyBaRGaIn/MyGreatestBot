using System.Text.RegularExpressions;

namespace DicordNET.Extensions
{
    /// <summary>
    /// <see cref="Regex"/> extensions
    /// </summary>
    internal static class RegexExtensions
    {
        internal static string? GetMatchValue(this Regex regex, string query)
        {
            return regex.GetMatchValue(query, 1)[0];
        }

        internal static string?[] GetMatchValue(this Regex regex, string query, params int[] groups)
        {
            string?[] result = new string?[groups.Length];

            Match match = regex.Match(query);

            if (match.Success)
            {
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
            }

            return result;
        }
    }
}
