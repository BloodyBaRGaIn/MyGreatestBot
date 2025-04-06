using System;
using System.Collections.Generic;

namespace MyGreatestBot.Extensions
{
    /// <summary>
    /// <see cref="string"/> extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Replace cyrillic with transletters
        /// </summary>
        /// <param name="input">Input text</param>
        /// <returns>Transletters text</returns>
        public static string ToTransletters(this string input)
        {
            bool has_quotes = input.Contains('\'') || input.Contains('`');
            string result = NickBuhro.Translit.Transliteration.CyrillicToLatin(input);
            return has_quotes ? result : result.Replace("`", "");
        }

        public static string FirstCharToUpper(this string input)
        {
            return string.IsNullOrEmpty(input)
                ? string.Empty
                : $"{char.ToUpperInvariant(input[0])}{input[1..]}";
        }

        public static IEnumerable<string> EnsureStrings(IEnumerable<string?> input)
        {
            return EnsureStrings([.. input]);
        }

        public static IEnumerable<string> EnsureStrings(params string?[]? strings)
        {
            if (strings == null || strings.Length == 0)
            {
                yield break;
            }
            foreach (string? value in strings)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    yield return value;
                }
            }
        }

        public static string EnsureIdentifier(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }
            string result = input.Replace("\r", "").Replace("\n", " ");
            string[] lines = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                result = lines[0];
            }
            return result;
        }
    }
}
