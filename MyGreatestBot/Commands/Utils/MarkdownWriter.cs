using DSharpPlus.CommandsNext;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyGreatestBot.Commands.Utils
{
    /// <summary>
    /// Markdown syntax workaround
    /// </summary>
    public static class MarkdownWriter
    {
        private const string FilePath = "../../../Commands.md";

        /// <summary>
        /// Creates a Github markdown file that contains all registered commands
        /// </summary>
        public static void GenerateFile(MarkdownType mdType = MarkdownType.Github)
        {
            using FileStream markdown = File.Open(FilePath, FileMode.Create, FileAccess.Write);
            using StreamWriter streamWriter = new(markdown);

            streamWriter.Write($"# Commands{Environment.NewLine}{Environment.NewLine}");
            streamWriter.Write($"Commands are organized into categories for better readability  {Environment.NewLine}");
            streamWriter.Write($"Default command prefix is {DiscordWrapper.DefaultPrefix}  {Environment.NewLine}");

            if (DiscordWrapper.Commands == null)
            {
                return;
            }

            foreach (string item in GetFullCommandsString(mdType))
            {
                streamWriter.Write(item);
            }
        }

        /// <summary>
        /// Get all registered commands descriptions with
        /// specified markdown type for correct presentation
        /// </summary>
        /// <param name="mdType">Desired markdown type</param>
        /// <returns></returns>
        public static IEnumerable<string> GetFullCommandsString(MarkdownType mdType)
        {
            foreach (IGrouping<string, Command> category in DiscordWrapper.RegisteredCommands.Values
                .DistinctBy(c => c.Name)
                .GroupBy(c => c.Category ?? string.Empty))
            {
                string result = string.Empty;

                result += GetCategoryHeaderString(category.Key);

                foreach (Command command in category)
                {
                    result += GetCommandString(command, mdType);
                }

                yield return result;
            }
        }

        /// <summary>
        /// Get command description with
        /// specified markdown type for correct presentation
        /// </summary>
        /// <param name="command">Desired command instance</param>
        /// <param name="mdType">Desired markdown type</param>
        /// <returns></returns>
        public static string GetCommandString(Command command, MarkdownType mdType)
        {
            string result = string.Empty;

            result += $"```{Environment.NewLine}{command.Name}";
            if (command.Aliases.Any())
            {
                result += $" ({string.Join(", ", command.Aliases)})";
            }

            if (!string.IsNullOrWhiteSpace(command.Description))
            {
                result += $" - {command.Description}";
            }

            result += Environment.NewLine;

            string pad = GetListPad(1, mdType);

            IReadOnlyList<CommandArgument> arguments = command.Overloads[0].Arguments;

            if (arguments.Any())
            {
                result += $"Arguments:{Environment.NewLine}";

                foreach (CommandArgument argument in arguments)
                {
                    result += $"{pad}{argument.Name} ({argument.Type.Name})";
                    if (!string.IsNullOrWhiteSpace(argument.Description))
                    {
                        string[] split = argument.Description.Split(Environment.NewLine);

                        result += " - ";

                        for (int i = 0; i < split.Length; i++)
                        {
                            result += split[i].Replace("\t", pad);
                            if (i < split.Length - 1)
                            {
                                result += Environment.NewLine;
                            }
                        }
                    }
                    if (argument.IsOptional)
                    {
                        result += " (optional)";
                    }
                    result += $"{Environment.NewLine}";
                }
            }

            if (command.CustomAttributes
                .FirstOrDefault(static attr => attr.GetType() == typeof(ExampleAttribute)) is ExampleAttribute example
                && !string.IsNullOrWhiteSpace(example.Example))
            {
                result += $"Examples:{Environment.NewLine}";
                result += $"{pad}{example.Example}{Environment.NewLine}";
            }

            result += $"```{Environment.NewLine}";

            return result;
        }

        /// <summary>
        /// Get command category as markdown header
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <returns></returns>
        private static string GetCategoryHeaderString(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                categoryName = "Unnamed";
            }
            categoryName = categoryName.FirstCharToUpper();
            return $"{Environment.NewLine}## {categoryName} commands{Environment.NewLine}{Environment.NewLine}";
        }

        /// <summary>
        /// Get padding string for the list
        /// </summary>
        /// <param name="depth">The list depth</param>
        /// <param name="mdType">Desired markdown type</param>
        /// <returns></returns>
        private static string GetListPad(int depth, MarkdownType mdType)
        {
            if (depth < 0)
            {
                depth = 0;
            }

            return string.Empty.PadLeft((int)mdType * depth, ' ');
        }
    }
}
