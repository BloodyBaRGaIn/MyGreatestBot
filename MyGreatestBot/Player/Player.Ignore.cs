using DSharpPlus.Entities;
using MyGreatestBot.Commands;
using MyGreatestBot.Sql;
using System;
using System.Runtime.Versioning;

namespace MyGreatestBot.Player
{
    [SupportedOSPlatform("windows")]
    internal partial class Player
    {
        internal void IgnoreTrack(CommandActionSource source = CommandActionSource.None)
        {
            if (!IsPlaying || currentTrack == null)
            {
                if ((source & CommandActionSource.Mute) == 0)
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Nothing to ignore"
                    });
                }

                return;
            }

            SqlServerWrapper.AddIgnoredTrack(currentTrack);

            Skip(0, CommandActionSource.Mute);

            if ((source & CommandActionSource.Mute) == 0)
            {
                Handler.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Track ignored"
                });
            }
        }

        internal void IgnoreArtist(int index = -1, CommandActionSource source = CommandActionSource.None)
        {
            if (!IsPlaying || currentTrack == null)
            {
                if ((source & CommandActionSource.Mute) == 0)
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
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

            SqlServerWrapper.AddIgnoredArtist(currentTrack, index);

            Skip(0, CommandActionSource.Mute);

            if ((source & CommandActionSource.Mute) == 0)
            {
                Handler.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Artist ignored"
                });
            }
        }
    }
}
