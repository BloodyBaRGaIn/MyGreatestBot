using System;

namespace MyGreatestBot.Extensions
{
    /// <summary>
    /// <see cref="string"/> extensions
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Replace cyrillic with transletters
        /// </summary>
        /// <param name="input">Input text</param>
        /// <returns>Transletters text</returns>
        internal static string ToTransletters(this string input)
        {
            bool has_quotes = input.Contains('\'') || input.Contains('`');
            string result = NickBuhro.Translit.Transliteration.CyrillicToLatin(input);
            return has_quotes ? result : result.Replace("`", "");
        }
    }
}
