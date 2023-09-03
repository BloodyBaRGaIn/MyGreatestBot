using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using System;
using System.Diagnostics;

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
            // Full stack trace available in Debug build configuration
            if (exception == null)
            {
                return string.Empty;
            }
            StackTrace st = new(exception, true);
            if (st == null)
            {
                return string.Empty;
            }
            StackFrame? frame = st.GetFrame(0);
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
        }

        public static DiscordEmbedBuilder GetDiscordEmbed(this Exception exception, bool is_executed_successfully = false)
        {
            return exception is CommandExecutionException cmd
                ? new DiscordEmbedBuilder()
                {
                    Color = is_executed_successfully ? cmd.ExecutedColor : cmd.ErroredColor,
                    Title = cmd.Title,
                    Description = exception.Message
                }
                : new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = exception.GetType().Name,
                    Description = exception.Message
                };
        }
    }
}
