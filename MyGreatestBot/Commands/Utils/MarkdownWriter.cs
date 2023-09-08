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

            if (DoscordWrapper.Commands == null)
            {
                return;
            }

            foreach (string item in GetFullCommandsString())
            {
                streamWriter.Write(item);
            }
        }

        public static IEnumerable<string> GetFullCommandsString()
        {
            foreach (IGrouping<string, Command> category in DoscordWrapper.Commands.RegisteredCommands.Values
                .DistinctBy(c => c.Name)
                .GroupBy(c => c.Category ?? string.Empty))
            {
                string result = string.Empty;

                result += GetCategoryHeaderString(category.Key);

                foreach (Command command in category)
                {
                    result += GetCommandString(command);
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

        public static string GetCommandString(Command command)
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

            foreach (CommandArgument argument in overload.Arguments)
            {
                result += $"    - ```{argument.Name} ({argument.Type.Name})";
                if (!string.IsNullOrWhiteSpace(argument.Description))
                {
                    result += $" - {argument.Description}";
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
