using DSharpPlus.CommandsNext;
using MyGreatestBot.Bot;
using MyGreatestBot.Extensions;
using System.IO;
using System.Linq;

namespace MyGreatestBot.Commands.Utils
{
    internal static class MarkdownWriter
    {
        private const string FilePath = "../../../Commands.md";

        internal static void GenerateFile()
        {
            using FileStream markdown = File.Open(FilePath, FileMode.Create, FileAccess.Write);
            using StreamWriter streamWriter = new(markdown);

            streamWriter.Write("# Commands\r\n\r\nCommands are organized into categories for better readability\r\n");

            if (BotWrapper.Commands == null)
            {
                return;
            }

            foreach (IGrouping<string, Command> category in BotWrapper.Commands.RegisteredCommands.Values
                .DistinctBy(c => c.Name)
                .GroupBy(c => c.Category ?? string.Empty))
            {
                string categoryName = category.Key;
                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    categoryName = "Unnamed";
                }
                categoryName = categoryName.FirstCharToUpper();
                streamWriter.Write($"\r\n## {categoryName} commands\r\n\r\n");

                foreach (var command in category)
                {
                    streamWriter.Write($"- ```{command.Name}");
                    if (command.Aliases.Any())
                    {
                        streamWriter.Write($" ({string.Join(", ", command.Aliases)})");
                    }

                    if (!string.IsNullOrWhiteSpace(command.Description))
                    {
                        streamWriter.Write($" - {command.Description}");
                    }

                    streamWriter.Write("<```\r\n");

                    CommandOverload overload = command.Overloads[0];
                    if (overload.Arguments.Any())
                    {
                        streamWriter.Write("    Arguments:\r\n");
                    }

                    foreach (var argument in overload.Arguments)
                    {
                        streamWriter.Write($"    - ```{argument.Name} ({argument.Type.Name})");
                        if (!string.IsNullOrWhiteSpace(argument.Description))
                        {
                            streamWriter.Write($" - {argument.Description}");
                        }
                        if (argument.IsOptional)
                        {
                            streamWriter.Write(" (optional)");
                        }
                        streamWriter.Write("```\r\n");
                    }
                }
            }
        }
    }
}
