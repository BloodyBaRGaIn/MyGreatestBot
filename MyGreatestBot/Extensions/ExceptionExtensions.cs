using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using System;
using System.Diagnostics;
using System.IO;

namespace MyGreatestBot.Extensions
{
    /// <summary>
    /// <see cref="Exception"/> extensions
    /// </summary>
    public static class ExceptionExtensions
    {
        public static string GetTypeName(this Exception? exception)
        {
            return exception?.GetType()?.Name ?? string.Empty;
        }

        public static string GetNonEmptyMessage(this Exception? exception)
        {
            if (exception == null)
            {
                return string.Empty;
            }
            if (!string.IsNullOrWhiteSpace(exception.Message))
            {
                return exception.Message;
            }
            return $"{exception.GetTypeName()} was thrown";
        }

        public static string GetExtendedMessage(this Exception? exception)
        {
            if (exception == null)
            {
                return string.Empty;
            }

            string typeName = exception.GetTypeName();

            string result = $"{typeName} : {exception.GetNonEmptyMessage()} {exception.GetStackFrame()}";

            if (exception.InnerException != null)
            {
                result += $"{Environment.NewLine}{exception.InnerException.GetExtendedMessage()}";
            }

            return result;
        }

        public static string GetStackFrame(this Exception? exception)
        {
            if (Program.Release || exception == null)
            {
                return string.Empty;
            }

            StackTrace? st = null;
            try
            {
                st = new(exception, true);
            }
            catch { }

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

            FileInfo? fileInfo = null;
            try
            {
                fileInfo = new(name);
            }
            catch { }

            if (fileInfo == null)
            {
                return string.Empty;
            }

            string fileName = fileInfo.Name;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return string.Empty;
            }

            int line = frame.GetFileLineNumber();
            return line == 0 ? $"(at {fileName})" : $"(at {fileName} : line {line})";
        }

        public static DiscordEmbedBuilder GetDiscordEmbed(this Exception exception)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithDescription(exception.GetNonEmptyMessage());

            return exception switch
            {
                CommandExecutionException cmd => builder
                    .WithColor(cmd.Color)
                    .WithTitle(cmd.Title),

                _ => builder
                    .WithColor(DiscordColor.Red)
                    .WithTitle(exception.GetTypeName())
            };
        }
    }
}
