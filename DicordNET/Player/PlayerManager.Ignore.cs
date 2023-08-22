using DicordNET.Bot;
using DicordNET.Commands;
using DicordNET.DB;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal partial class PlayerManager
    {
        internal static void Ignore(CommandActionSource source = CommandActionSource.None)
        {
            if (!IsPlaying || currentTrack == null)
            {
                if ((source & CommandActionSource.Mute) == 0)
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Nothing to ignore"
                    });
                }

                return;
            }

            DataBaseManager.AddIgnoredTrack(currentTrack);

            Skip(0, CommandActionSource.Mute);

            if ((source & CommandActionSource.Mute) == 0)
            {
                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Track ignored"
                });
            }
        }

        internal static void IgnoreArtist(int index = -1, CommandActionSource source = CommandActionSource.None)
        {
            if (!IsPlaying || currentTrack == null)
            {
                if ((source & CommandActionSource.Mute) == 0)
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Nothing to ignore"
                    });
                }

                return;
            }

            if (currentTrack.ArtistArr.Length > 1)
            {
                if (index < 0)
                {
                    throw new ArgumentException("Provide artist index");
                }
            }
            else
            {
                if (index < 0)
                {
                    index = 0;
                }
            }

            DataBaseManager.AddIgnoredArtist(currentTrack, index);

            Skip(0, CommandActionSource.Mute);

            if ((source & CommandActionSource.Mute) == 0)
            {
                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Artist ignored"
                });
            }
        }
    }
}
