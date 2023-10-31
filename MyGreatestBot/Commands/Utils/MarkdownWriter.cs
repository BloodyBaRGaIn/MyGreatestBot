using DSharpPlus.CommandsNext;
using MyGreatestBot.ApiClasses.Services.Discord;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyGreatestBot.Commands.Utils
{
    public static class MarkdownWriter
    {
        private const string FilePath = "../../../Commands.md";

        public static void GenerateFile()
        {
            using FileStream markdown = File.Open(FilePath, FileMode.Create, FileAccess.Write);
            using StreamWriter streamWriter = new(markdown);

            streamWriter.Write($"# Commands{Environment.NewLine}{Environment.NewLine}");
            streamWriter.Write($"Commands are organized into categories for better readability{Environment.NewLine}");

            if (DiscordWrapper.Commands == null)
            {
                return;
            }

            foreach (string item in GetFullCommandsString(MarkdownType.Github))
            {
                streamWriter.Write(item);
            }
        }

        public static IEnumerable<string> GetFullCommandsString(MarkdownType mdType)
        {
            foreach (IGrouping<string, Command> category in DiscordWrapper.Commands.RegisteredCommands.Values
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

        public static string GetCategoryHeaderString(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                categoryName = "Unnamed";
            }
            categoryName = categoryName.FirstCharToUpper();
            return $"{Environment.NewLine}## {categoryName} commands{Environment.NewLine}{Environment.NewLine}";
        }

        public static string GetListPad(int depth, MarkdownType mdType)
        {
            return string.Empty.PadLeft((int)mdType * depth, ' ');
        }

        public static string GetCommandString(Command command, MarkdownType mdType)
        {
            string result = string.Empty;

            result += $"- ```{command.Name}";
            if (command.Aliases.Any())
            {
                result += $" ({string.Join(", ", command.Aliases)})";
            }

            if (!string.IsNullOrWhiteSpace(command.Description))
            {
                result += $" - {command.Description}";
            }

            result += $"```  {Environment.NewLine}";

            CommandOverload overload = command.Overloads[0];
            if (!overload.Arguments.Any())
            {
                return result;
            }

            result += $"    Arguments:{Environment.NewLine}";

            string pad = GetListPad(1, mdType);

            foreach (CommandArgument argument in overload.Arguments)
            {
                result += $"{pad}- ```{argument.Name} ({argument.Type.Name})";
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
                result += $"```  {Environment.NewLine}";
            }

            return result;
        }
    }
}
