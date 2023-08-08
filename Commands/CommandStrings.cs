namespace DicordNET.Commands
{
    internal static class CommandStrings
    {
        internal const string CommonCategoryName = "common";
        internal const string ConnectionCategoryName = "connection";
        internal const string PlayerCategoryName = "player";
        internal const string DebugCategoryName = "debug";

        internal static readonly string[] CategoriesOrder = new string[]
        {
            CommonCategoryName,
            ConnectionCategoryName,
            PlayerCategoryName,
            DebugCategoryName
        };
    }
}
