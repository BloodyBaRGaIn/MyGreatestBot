using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using Swan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyGreatestBot.Commands
{
    /// <summary>
    /// Connection commands.
    /// </summary>
    [Category(CommandStrings.ConnectionCategoryName)]
    internal class ConnectionCommands : BaseCommandModule
    {
        [Command("join"), Aliases("j")]
        [Description("Join voice channel")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task JoinCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler != null)
            {
                await handler.Join(ctx);
            }
        }

        [Command("leave"), Aliases("l")]
        [Description("Leave voice channel")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task LeaveCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler != null)
            {
                await handler.Leave(ctx);
            }
        }

        [Command("apistatus"), Aliases("status")]
        [Description("Get APIs status")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task StatusCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            string result = ApiManager.GetRegisteredApiStatus();

            handler.Message.Send(string.IsNullOrEmpty(result)
                ? new ApiStatusCommandException("No APIs initialized")
                : new ApiStatusCommandException(result).WithSuccess());

            await Task.Delay(1);
        }

        [Command("apiinit"), Aliases("init")]
        [Description("Force API initialization")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task InitCommand(CommandContext ctx,
            string api)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            EnsureUserIsOwner(ctx.User);

            api = api.ToLowerInvariant().FirstCharToUpper();

            if (!Enum.TryParse(api, out ApiIntents intents))
            {
                handler.Message.Send(new ApiStatusCommandException($"Cannot find API \"{api}\""));
                return;
            }

            if (intents == ApiIntents.None)
            {
                handler.Message.Send(new ApiStatusCommandException("No API provided"));
                return;
            }

            ApiManager.InitApis(intents);

            await Task.Delay(1);
        }

        [Command("apideinit"), Aliases("deinit")]
        [Description("Force API deinitialization")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task DeinitCommand(CommandContext ctx,
            string api)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            EnsureUserIsOwner(ctx.User);

            api = api.ToLowerInvariant().FirstCharToUpper();

            if (!Enum.TryParse(api, out ApiIntents intents))
            {
                handler.Message.Send(new ApiStatusCommandException($"Cannot find API \"{api}\""));
                return;
            }

            if (intents == ApiIntents.None)
            {
                handler.Message.Send(new ApiStatusCommandException("No API provided"));
                return;
            }

            ApiManager.DeinitApis(intents);

            await Task.Delay(1);
        }

        [Command("apireload"), Aliases("reload")]
        [Description("Reload failed APIs")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task ReloadCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            if (!ApiManager.IsAnyApiFailed)
            {
                throw new ReloadCommandException("No failed APIs to reload");
            }

            ApiManager.ReloadFailedApis();

            if (!ApiManager.IsAnyApiFailed)
            {
                handler.Message.Send(new ReloadCommandException("Reload success").WithSuccess());
            }
            else
            {
                throw new ReloadCommandException("Reload failed");
            }

            await Task.Delay(1);
        }

        [Command("playerstatus"), Aliases("plst")]
        [Description("Get player status")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task PlayerStatusCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.GetStatus(CommandActionSource.Command));
        }

        [Command("beep")]
        [Description("Sends a test beep sound")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task BeepTask(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            await handler.Join(ctx);

            const int durationSeconds = 5;

            // Generate proper PCM sine wave
            const int sampleRate = 48000;
            const short amplitude = 15000; // Max for 16-bit is 32767
            const double frequency = 440.0; // A4 note

            // Calculate total samples needed
            int totalSamples = sampleRate * durationSeconds;
            byte[] pcmData = new byte[totalSamples * 4]; // 2 channels * 2 bytes per sample

            for (int i = 0; i < totalSamples; i++)
            {
                double time = (double)i / sampleRate;
                short sample = (short)(amplitude * Math.Sin(2 * Math.PI * frequency * time));

                // Little endian PCM for both channels
                int byteIndex = i * 4;

                // Left channel (little endian)
                pcmData[byteIndex] = (byte)(sample & 0xFF);
                pcmData[byteIndex + 1] = (byte)((sample >> 8) & 0xFF);

                // Right channel (little endian)
                pcmData[byteIndex + 2] = (byte)(sample & 0xFF);
                pcmData[byteIndex + 3] = (byte)((sample >> 8) & 0xFF);
            }

            using MemoryStream memoryStream = new MemoryStream(pcmData);
            Console.WriteLine($"Generated {pcmData.Length} bytes of PCM data");

            // Write in chunks to simulate real audio streaming
            memoryStream.Position = 0;
            byte[] buffer = new byte[3840]; // Standard Opus frame size

            DiscordVoiceState? voiceState = await ctx.Guild.GetCurrentUserVoiceStateAsync();
            if (voiceState is not null)
            {
                await handler.Log.SendAsync(string.Join(Environment.NewLine,
                    $"Discord Voice State - Channel: {voiceState.ChannelId}",
                    $"Is Server Muted: {voiceState.IsServerMuted}",
                    $"Is Server Deafened: {voiceState.IsServerDeafened}",
                    $"Is Self Muted: {voiceState.IsSelfMuted}",
                    $"Is Self Deafened: {voiceState.IsSelfDeafened}"));
            }

            await handler.Log.SendAsync(
                string.Join(Environment.NewLine,
                handler.Voice.Connection.AudioFormat.Stringify(),
                handler.Voice.Connection.GetTransmitSink().Stringify(),
                "VoiceNextConnection.IsPlaying : " + handler.Voice.Connection.IsPlaying.ToString(),
                "VoiceNextConnection.UdpPing : " + handler.Voice.Connection.UdpPing.ToString(),
                "VoiceNextConnection.WebSocketPing : " + handler.Voice.Connection.WebSocketPing.ToString()));

            int bytesRead;
            while ((bytesRead = await memoryStream.ReadAsync(buffer)) > 0)
            {
                if (bytesRead < buffer.Length)
                {
                    // Last chunk - pad with silence if needed
                    Array.Clear(buffer, bytesRead, buffer.Length - bytesRead);
                }

                _ = await handler.Voice.WriteAsync(buffer, buffer.Length);
                await Task.Delay(20); // Simulate real-time audio
            }
        }

        [Command("logout"), Aliases("exit", "quit", "bye", "bb")]
        [Description("Logout and exit")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task LogoutCommand(CommandContext ctx)
        {
            await LogoutGeneric(ctx, CommandActionSource.LogoutBye);
        }

        [Command("shutdown")]
        [Description("Force shutdown")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task ShutdownCommand(CommandContext ctx)
        {
            await LogoutGeneric(ctx, CommandActionSource.LogoutShut);
        }

        private static async Task LogoutGeneric(CommandContext ctx, CommandActionSource source)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            EnsureUserIsOwner(ctx.User);

            await ConnectionHandler.Logout(source);
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> 
        /// if provided <paramref name="user"/> is not application owner.
        /// </summary>
        /// <param name="user">
        /// User to be cheched.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// An exception with "not allowed action" message.
        /// </exception>
        private static void EnsureUserIsOwner(DiscordUser user)
        {
            IEnumerable<DiscordUser>? owners = DiscordWrapper.Client?.CurrentApplication?.Owners;
            if (owners != null && !owners.Select(x => x.Id).Contains(user.Id))
            {
                throw new InvalidOperationException("You are not allowed to execute this command");
            }
        }
    }
}
