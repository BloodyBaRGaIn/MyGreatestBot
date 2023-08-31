using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.ConfigStructs;
using MyGreatestBot.Commands;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Extensions;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace MyGreatestBot.Bot
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

        internal async Task RunAsync()
        {
            try
            {
                DiscordConfigJSON config_js = ConfigManager.GetDiscordConfigJSON();

                DiscordConfiguration discordConfig = new()
                {
                    MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Error,
                    Intents = DiscordIntents.All,
                    Token = config_js.Token,
                    TokenType = TokenType.Bot,
                    AutoReconnect = true
                };

                Client = new DiscordClient(discordConfig);

                Client.Ready += async (sender, args) =>
                {
                    await sender.UpdateStatusAsync(new()
                    {
                        ActivityType = ActivityType.ListeningTo,
                        Name = $"{config_js.Prefix}{CommandStrings.HelpCommandName}"
                    }, UserStatus.Online);

                    Console.WriteLine("Discord SUCCESS");
                };

                Client.ClientErrored += async (sender, args) =>
                {
                    await Console.Error.WriteLineAsync(args.Exception.GetExtendedMessage());
                };

                Client.SocketErrored += async (sender, args) =>
                {
                    await Console.Error.WriteLineAsync(args.Exception.GetExtendedMessage());
                };

                Client.VoiceStateUpdated += Client_VoiceStateUpdated;

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
                try
                {
                    await Task.Delay(1);
                }
                catch
                {
                    return;
                }
            }
        }

        private async Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            if (e.User.Id == client.CurrentUser.Id && e.User.IsBot && e.After?.Channel != e.Before?.Channel)
            {
                ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(e.Guild);
                if (handler != null)
                {
                    if (e.After?.Channel != null)
                    {
                        await handler.Join(e);
                        await handler.WaitForConnectionAsync();
                        handler.Update(e.Guild);
                    }
                    else
                    {
                        if (!handler.IsManualDisconnect)
                        {
                            handler.SendMessage(new DiscordEmbedBuilder()
                            {
                                Color = DiscordColor.Red,
                                Title = "Kicked from voice channel"
                            });
                            handler.PlayerInstance.Stop(CommandActionSource.Mute);
                        }
                        handler.IsManualDisconnect = false;
                    }

                    handler.Update(e.Guild);
                }
            }

            await Task.Delay(1);
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
                result += $"{args.Command.Name}";
            }

            if (!string.IsNullOrWhiteSpace(args.Context.RawArgumentString))
            {
                result += $" {args.Context.RawArgumentString}";
            }

            return result;
        }

        private async Task Commands_CommandExecuted(
            CommandsNextExtension sender,
            CommandExecutionEventArgs args)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(args.Context.Guild);
            if (handler != null)
            {
                await handler.LogAsync(GetCommandInfo(args));
            }
        }

        private async Task Commands_CommandErrored(
            CommandsNextExtension sender,
            CommandErrorEventArgs args)
        {
            string extended_message = args.Exception.GetExtendedMessage();

            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(args.Context.Guild);
            if (handler != null)
            {
                await handler.LogErrorAsync($"{GetCommandInfo(args)}{Environment.NewLine}" +
                    $"Command errored{Environment.NewLine}" +
                    $"{extended_message}");

                handler.SendMessage(args.Exception);
            }
        }
    }
}
