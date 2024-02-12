using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using System;

namespace MyGreatestBot.Extensions
{
    /// <summary>
    /// <see cref="Exception"/> extensions
    /// </summary>
    public static class ExceptionExtensions
    {
        public static string GetExtendedMessage(this Exception? exception)
        {
            if (exception == null)
            {
                return string.Empty;
            }
            if (string.IsNullOrWhiteSpace(exception.Message))
            {
                return exception.GetType().Name;
            }

            string result = $"{exception.GetType().Name} : {exception.Message} {exception.GetStackFrame()}";

            if (exception.InnerException != null)
            {
                result += $"{Environment.NewLine}{exception.InnerException.GetExtendedMessage()}";
            }

            return result;
        }

        public static string GetStackFrame(this Exception? exception)
        {
#if DEBUG
            if (exception == null)
            {
                return string.Empty;
            }
            System.Diagnostics.StackTrace st = new(exception, true);
            if (st == null)
            {
                return string.Empty;
            }
            System.Diagnostics.StackFrame? frame = st.GetFrame(0);
            if (frame == null)
            {
                return string.Empty;
            }
            string? name = frame.GetFileName();
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }
            int line = frame.GetFileLineNumber();
            return $"({new System.IO.FileInfo(name).Name} : {line})";
#else
            // Full stack trace available in Debug build configuration
            return string.Empty;
#endif
        }

        public static DiscordEmbedBuilder GetDiscordEmbed(this Exception exception)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithDescription(exception.Message);

            return exception switch
            {
                CommandExecutionException cmd => builder
                .WithColor(cmd.Color)
                .WithTitle(cmd.Title),

                _ => builder
                .WithColor(DiscordColor.Red)
                .WithTitle(exception.GetType().Name)
            };
        }
    }
}
