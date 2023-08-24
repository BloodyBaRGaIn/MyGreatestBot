﻿using DicordNET.Commands;
using DicordNET.Config;
using DicordNET.Extensions;
using DicordNET.Player;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace DicordNET.Bot
{
    [SupportedOSPlatform("windows")]
    internal sealed class DiscordBot
    {
        internal DiscordClient? Client { get; private set; }
        internal InteractivityExtension? Interactivity { get; private set; }
        internal CommandsNextExtension? Commands { get; private set; }
        internal ServiceProvider ServiceProvider { get; private set; } = new ServiceCollection().BuildServiceProvider();

        private string prefix = string.Empty;

        internal async Task RunAsync()
        {
            DiscordConfigJSON config_js = ConfigManager.GetDiscordConfigJSON();

            prefix = config_js.Prefix;

            DiscordConfiguration discordConfig = new()
            {
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Error,
                Intents = DiscordIntents.All,
                Token = config_js.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordConfig);

            Client.Ready += Client_Ready;
            Client.VoiceStateUpdated += Client_VoiceStateUpdated;
            Client.ClientErrored += Client_ClientErrored;
            Client.SocketErrored += Client_SocketErrored;

            _ = Client.UseInteractivity(new()
            {
                Timeout = TimeSpan.FromMinutes(20)
            });

            CommandsNextConfiguration commandsConfig = new()
            {
                StringPrefixes = new string[] { config_js.Prefix },
                CaseSensitive = false,
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
                Services = ServiceProvider,
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.SetHelpFormatter<CustomHelpFormatter>();
            Commands.RegisterCommands<ConnectionCommands>();
            Commands.RegisterCommands<PlayerCommands>();
            Commands.RegisterCommands<DebugCommands>();

            Commands.CommandErrored += Commands_CommandErrored;

            await Client.ConnectAsync();
            _ = Client.UseVoiceNext();

            while (true)
            {
                await Task.Delay(Timeout.Infinite);
            }
        }

        private Task Client_SocketErrored(DiscordClient sender, SocketErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.GetExtendedMessage());
            return Task.CompletedTask;
        }

        private Task Client_ClientErrored(DiscordClient sender, ClientErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.GetExtendedMessage());
            return Task.CompletedTask;
        }

        private Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            if (e.User.Id == client.CurrentUser.Id && e.User.IsBot && e.After?.Channel != e.Before?.Channel)
            {
                if (e.After?.Channel == null)
                {
                    PlayerManager.Pause(CommandActionSource.Mute | CommandActionSource.External);
                }
                else
                {
                    BotWrapper.VoiceChannel = e.After?.Channel;
                    PlayerManager.Resume(CommandActionSource.Mute | CommandActionSource.External);
                }
            }
            return Task.CompletedTask;
        }

        private async Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
        {
            await sender.UpdateStatusAsync(new()
            {
                ActivityType = ActivityType.ListeningTo,
                Name = $"{prefix}help"
            }, UserStatus.Online);

            Console.WriteLine("### READY ###");
        }

        private async Task Commands_CommandErrored(
            CommandsNextExtension sender,
            CommandErrorEventArgs args)
        {
            _ = await args.Context.Channel.SendMessageAsync(args.Exception.GetExtendedMessage());
        }
    }
}
