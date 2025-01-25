using DSharpPlus.CommandsNext;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyGreatestBot.Commands
{
    internal class ConsoleCommands : ConsoleCommandModule
    {
        [ConsoleCommand("test")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public void ConsoleTestCommand()
        {
            DiscordWrapper.CurrentDomainLogHandler.Send(
                "Hello World from .NET");
        }

        [ConsoleCommand("name")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public void ConsoleNameCommand()
        {
            if (DiscordWrapper.Client == null)
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    "Cannot get my username");
                return;
            }

            DiscordWrapper.CurrentDomainLogHandler.Send(
                $"My user name is {DiscordWrapper.Client.CurrentUser.Username}");
        }

        [ConsoleCommand("echo")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public void ConsoleEchoCommand(string? word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            DiscordWrapper.CurrentDomainLogHandler.Send(word);
        }

        [ConsoleCommand("help")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public void ConsoleHelpCommand(string? command = null)
        {
            List<CustomHelpFormatter> collection = [];

            if (string.IsNullOrWhiteSpace(command)
                || string.Equals(command, "all", StringComparison.CurrentCultureIgnoreCase))
            {
                collection.AddRange(CustomHelpFormatter.WithAllCommands());
            }
            else if (DiscordWrapper.RegisteredCommands.TryGetValue(command.ToLowerInvariant(),
                                                                   out Command? cmd))
            {
                collection.Add(new CustomHelpFormatter().WithCommand(cmd));
            }
            else
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    "Invalid command");
                return;
            }

            DiscordWrapper.CurrentDomainLogHandler.Send(
                string.Join(
                    Environment.NewLine,
                    StringExtensions.EnsureStrings(collection.Select(c => c.Build().Content))));
        }

        [ConsoleCommand("status")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public void StatusCommand()
        {
            string result = ApiManager.GetRegisteredApiStatus();

            DiscordWrapper.CurrentDomainLogHandler.Send(string.IsNullOrEmpty(result)
                ? "No APIs initialized"
                : result);
        }

        [ConsoleCommand("init")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public void InitCommand(string api)
        {
            api = api.ToLowerInvariant().FirstCharToUpper();

            if (!Enum.TryParse(api, out ApiIntents intents))
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    $"Cannot find API \"{api}\"");
                return;
            }

            if (intents == ApiIntents.None)
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    "No API provided");
                return;
            }

            ApiManager.InitApis(intents);
        }

        [ConsoleCommand("deinit")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public void DeinitCommand(string api)
        {
            api = api.ToLowerInvariant().FirstCharToUpper();

            if (!Enum.TryParse(api, out ApiIntents intents))
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    $"Cannot find API \"{api}\"");
                return;
            }

            if (intents == ApiIntents.None)
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    "No API provided");
                return;
            }

            ApiManager.DeinitApis(intents);
        }

        [ConsoleCommand("reload")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public void ReloadCommand()
        {
            if (!ApiManager.IsAnyApiFailed)
            {
                DiscordWrapper.CurrentDomainLogHandler.Send(
                    "No failed APIs to reload");
                return;
            }

            ApiManager.ReloadFailedApis();

            if (!ApiManager.IsAnyApiFailed)
            {
                DiscordWrapper.CurrentDomainLogHandler.Send(
                    "Reload success");
            }
            else
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    "Reload failed");
            }
        }
    }
}
