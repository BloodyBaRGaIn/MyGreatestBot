using DicordNET.Commands;
using DicordNET.Config;
using DicordNET.Extensions;
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
        private const int DiscordConnectionTimeout = 10000;

        internal DiscordClient? Client { get; private set; }
        internal InteractivityExtension? Interactivity { get; private set; }
        internal CommandsNextExtension? Commands { get; private set; }
        internal VoiceNextExtension? Voice { get; private set; }
        internal ServiceProvider ServiceProvider { get; private set; } = new ServiceCollection().BuildServiceProvider();

        private string prefix = string.Empty;

        internal async Task RunAsync()
        {
            try
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

                Interactivity = Client.UseInteractivity(new()
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
                Commands.CommandExecuted += Commands_CommandExecuted;

                if (!Client.ConnectAsync().Wait(DiscordConnectionTimeout))
                {
                    throw new ApplicationException("Cannot connect to Discord");
                }
                Voice = Client.UseVoiceNext();
            }
            catch (Exception ex)
            {
                try
                {
                    if (Client != null)
                    {
                        await Client.DisconnectAsync();
                        Client.Dispose();
                    }
                }
                catch { }
                Console.Error.WriteLine(ex.GetExtendedMessage());
                Console.WriteLine("Press any key to exit");
                _ = Console.ReadKey(true);
                return;
            }

            while (true)
            {
                await Task.Delay(Timeout.Infinite);
            }
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

        private async Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            if (e.User.Id == client.CurrentUser.Id && e.User.IsBot && e.After?.Channel != e.Before?.Channel)
            {
                ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(e.Guild);
                if (handler != null)
                {
                    if (e.After?.Channel == null)
                    {
                        handler.PlayerInstance.Pause(CommandActionSource.Mute | CommandActionSource.External);
                    }
                    else
                    {
                        handler.VoiceChannel = e.After?.Channel;
                        handler.VoiceConnection = Voice?.GetConnection(e.Guild);
                        handler.UpdateSink();
                        handler.PlayerInstance.Resume(CommandActionSource.Mute | CommandActionSource.External);
                    }
                }
            }

            await Task.Delay(1);
        }

        private async Task Client_SocketErrored(DiscordClient sender, SocketErrorEventArgs args)
        {
            await Console.Error.WriteLineAsync(args.Exception.GetExtendedMessage());
        }

        private async Task Client_ClientErrored(DiscordClient sender, ClientErrorEventArgs args)
        {
            await Console.Error.WriteLineAsync(args.Exception.GetExtendedMessage());
        }

        private static string GetCommandInfo(CommandEventArgs args)
        {
            string result = string.Empty;
            if (args.Context.Member != null)
            {
                result += $"{args.Context.Member.DisplayName} : ";
            }

            if (args.Command != null)
            {
                result += $"{args.Command.Name} ";
            }

            if (!string.IsNullOrWhiteSpace(args.Context.RawArgumentString))
            {
                result += $"{args.Context.RawArgumentString}";
            }

            return result;
        }

        private async Task Commands_CommandExecuted(
            CommandsNextExtension sender,
            CommandExecutionEventArgs args)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(args.Context.Guild);
            if (handler == null)
            {
                return;
            }

            await handler.LogAsync(GetCommandInfo(args));
        }

        private async Task Commands_CommandErrored(
            CommandsNextExtension sender,
            CommandErrorEventArgs args)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(args.Context.Guild);
            if (handler != null)
            {
                await handler.LogErrorAsync($"{GetCommandInfo(args)}{Environment.NewLine}" +
                    $"Command errored{Environment.NewLine}" +
                    $"{args.Exception.GetExtendedMessage()}");
            }
            _ = await args.Context.Channel.SendMessageAsync(args.Exception.GetExtendedMessage());
        }
    }
}
