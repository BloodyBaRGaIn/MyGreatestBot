using DicordNET.Bot;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal static partial class PlayerManager
    {
        internal static void RequestSeek(TimeSpan span)
        {
            if (!IsPlaying || currentTrack == null)
            {
                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Seek",
                    Description = "Nothing to seek"
                });

                return;
            }

            if (currentTrack == null
                || currentTrack.IsLiveStream
                || currentTrack.Duration == TimeSpan.Zero
                || currentTrack.Duration <= span)
            {
                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Seek",
                    Description = "Cannot seek"
                });

                return;
            }

            lock (currentTrack)
            {
                Seek = span;
                currentTrack.Seek = Seek;

                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Purple,
                    Title = "Seek",
                    Description = currentTrack.GetMessage(),
                    Thumbnail = currentTrack.GetThumbnail()
                });
            }

            IsPaused = true;

            Task.Yield().GetAwaiter().GetResult();

            SeekRequested = true;
        }
    }
}
