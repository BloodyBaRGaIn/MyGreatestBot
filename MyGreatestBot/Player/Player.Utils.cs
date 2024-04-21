using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        private static DiscordEmbedBuilder GetPlayingMessage<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(ITrackInfo track, string state)
            where T : CommandExecutionException
        {
            T exception;

            try
            {
                exception = (typeof(T).GetConstructors()
                .First(c =>
                {
                    ParameterInfo[] parameters = c.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
                })
                .Invoke(new[] { track.GetMessage(state) }) as T)
                ?? throw new ArgumentException("Cannot create message");
            }
            catch
            {
                throw;
            }

            _ = exception.WithSuccess();
            DiscordEmbedBuilder message = exception.GetDiscordEmbed();
            message.Thumbnail = track.GetThumbnail();
            return message;
        }

        private void Reset()
        {
            Thread.BeginCriticalRegion();

            currentTrack = null;
            IsPlaying = false;
            IsPaused = false;
            StopRequested = false;
            SeekRequested = false;
            PlayerTimePosition = TimeSpan.Zero;
            tracksQueue?.Clear();
            ffmpeg?.Stop();

            Thread.EndCriticalRegion();
        }

        private static void Wait(int delay = 1)
        {
            Task.Delay(delay).Wait();
            Task.Yield().GetAwaiter().GetResult();
        }

        private void WaitForStatus(PlayerStatus status)
        {
            while (true)
            {
                if ((Status & status) != PlayerStatus.None)
                {
                    break;
                }
                Wait();
            }
        }

        private void WaitForFinish()
        {
            WaitForStatus(PlayerStatus.Finish | PlayerStatus.InitOrIdle | PlayerStatus.DeinitOrError);
        }
    }
}
