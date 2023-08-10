namespace DicordNET.Extensions
{
    internal static class StringExtensions
    {
        internal static string ToTransletters(this string input)
        {
            bool has_quotes = input.Contains('\'') || input.Contains('`');
            string result = NickBuhro.Translit.Transliteration.CyrillicToLatin(input);
            return has_quotes ? result : result.Replace("`", "");
        }
    }
}
