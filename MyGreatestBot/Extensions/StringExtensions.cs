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
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            return $"{char.ToUpperInvariant(input[0])}{input[1..]}";
        }
    }
}
