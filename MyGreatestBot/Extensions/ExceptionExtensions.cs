using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using System;
using System.Drawing;
using VkNet.Model;

namespace MyGreatestBot.Extensions
{
    /// <summary>
    /// <see cref="Exception"/> extensions
    /// </summary>
    internal static class ExceptionExtensions
    {
        internal static string GetExtendedMessage(this Exception exception)
        {
            if (string.IsNullOrWhiteSpace(exception.Message))
            {
                return exception.GetType().Name;
            }
            string result = $"{exception.GetType().Name} : {exception.Message}";
            if (exception.InnerException != null)
            {
                result += $"{Environment.NewLine}{exception.InnerException.GetExtendedMessage()}";
            }
            return result;
        }

        internal static DiscordEmbedBuilder GetDiscordEmbed(this Exception exception)
        {
            DiscordEmbedBuilder builder = new();
            builder = exception switch
            {
                CommandExecutionException command => builder.WithColor(command.Color).WithTitle(command.Title),
                _ => builder.WithColor(DiscordColor.Red).WithTitle(exception.GetType().Name),
            };
            return builder.WithDescription(exception.Message);
        }
    }
}
