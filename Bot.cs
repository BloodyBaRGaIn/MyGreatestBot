using DicordNET.Commands;
using DicordNET.Config;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;

namespace DicordNET
{
    [Flags]
    internal enum ActionSource : uint
    {
        None = 0x00,
        Command = 0x01,
        External = 0x02,


        Mute = 0x10000000
    }
    internal static class StaticBotInstanceContainer
    {
        private const int SEND_MESSAGE_WAIT_MS = 1000;

        internal static readonly Bot BotInstance = new();

        internal static DiscordClient? Client => BotInstance.Client;
        internal static CommandsNextExtension? Commands => BotInstance.Commands;

        internal static VoiceNextExtension? VoiceNext;
        internal static VoiceNextConnection? VoiceConnection;
        internal static DiscordChannel? TextChannel;
        internal static DiscordChannel? VoiceChannel;
        internal static VoiceTransmitSink? TransmitSink;

        internal static void Run() => BotInstance.RunAsync().GetAwaiter().GetResult();

        internal static VoiceNextConnection? GetVoiceConnection(DiscordGuild guild)
        {
            try
            {
                return VoiceNext?.GetConnection(guild);
            }
            catch
            {
                return VoiceConnection;
            }
        }

        internal static async Task SendMessageAsync(DiscordEmbedBuilder embed)
        {
            if (TextChannel != null)
            {
                await TextChannel.SendMessageAsync(embed);
            }
        }

        internal static async Task SendMessageAsync(string message)
        {
            if (TextChannel != null)
            {
                await TextChannel.SendMessageAsync(message);
            }
        }

        internal static void SendMessage(DiscordEmbedBuilder embed)
        {
            _ = SendMessageAsync(embed).Wait(SEND_MESSAGE_WAIT_MS);
        }

        internal static void SendMessage(string message)
        {
            _ = SendMessageAsync(message).Wait(SEND_MESSAGE_WAIT_MS);
        }

        internal static void SendSpeaking(bool speaking)
        {
            try
            {
                _ = VoiceConnection?.SendSpeakingAsync(speaking).Wait(100);
            }
            catch { }
        }

        internal static void Connect()
        {
            if (VoiceNext != null)
            {
                VoiceNext.ConnectAsync(VoiceChannel).Wait(1000);
            }
        }

        internal static void Disconnect()
        {
            try
            {
                VoiceConnection?.Disconnect();
            }
            catch { }
        }
    }

    internal class Bot
    {
        internal DiscordClient? Client { get; private set; }
        internal InteractivityExtension? Interactivity { get; private set; }
        internal CommandsNextExtension? Commands { get; private set; }

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

            Client.UseInteractivity(new()
            {
                Timeout = TimeSpan.FromMinutes(20)
            });

            CommandsNextConfiguration commandsConfig = new()
            {
                StringPrefixes = new string[] { config_js.Prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<VoiceCommands>();
            Commands.RegisterCommands<FunCommands>();

            Commands.CommandErrored += Commands_CommandErrored;

            await Client.ConnectAsync();
            Client.UseVoiceNext();

            while (true)
            {
                await Task.Delay(Timeout.Infinite);
            }
        }

        private Task Client_SocketErrored(DiscordClient sender, SocketErrorEventArgs args)
        {
            Console.WriteLine($"{args.Exception.GetType().Name} : {args.Exception.Message}");
            return Task.CompletedTask;
        }

        private Task Client_ClientErrored(DiscordClient sender, ClientErrorEventArgs args)
        {
            Console.WriteLine($"{args.Exception.GetType().Name} : {args.Exception.Message}");
            return Task.CompletedTask;
        }

        private Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            if (e.User.Id == client.CurrentUser.Id && e.User.IsBot && e.After?.Channel != e.Before?.Channel)
            {
                if (e.After?.Channel == null)
                {
                    PlayerManager.Pause(ActionSource.Mute | ActionSource.External);
                }
                else
                {
                    PlayerManager.Resume(ActionSource.Mute | ActionSource.External);
                }
            }
            return Task.CompletedTask;
        }

        private async Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
        {
            await sender.UpdateStatusAsync(new()
            {
                ActivityType = ActivityType.ListeningTo,
                Name = prefix
            }, UserStatus.Online);

            Console.WriteLine("### READY ###");
        }

        private async Task Commands_CommandErrored(
            CommandsNextExtension sender,
            CommandErrorEventArgs args)
        {
            await args.Context.Channel.SendMessageAsync($"{args.Exception.GetType().Name} : {args.Exception.Message}");
        }
    }
}
