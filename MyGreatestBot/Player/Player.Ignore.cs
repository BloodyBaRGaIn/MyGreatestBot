using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Exceptions;
using MyGreatestBot.ApiClasses.Services.Sql;
using MyGreatestBot.Commands;
using MyGreatestBot.Commands.Exceptions;
using System;
using System.Runtime.Versioning;

namespace MyGreatestBot.Player
{
    [SupportedOSPlatform("windows")]
    internal partial class Player
    {
        internal void IgnoreTrack(CommandActionSource source = CommandActionSource.None)
        {
            if (!ApiManager.InitIntents.HasFlag(ApiIntents.Sql))
            {
                throw new SqlApiException();
            }

            if (!IsPlaying || currentTrack == null)
            {
                if (!source.HasFlag(CommandActionSource.Mute))
                {
                    throw new IgnoreException("Nothing to ignore");
                }

                return;
            }

            try
            {
                SqlServerWrapper.AddIgnoredTrack(currentTrack);
            }
            catch (Exception ex)
            {
                throw new IgnoreException("Failed to ignore track", ex);
            }

            Skip(0, CommandActionSource.Mute);

            if (!source.HasFlag(CommandActionSource.Mute))
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
            if (!ApiManager.InitIntents.HasFlag(ApiIntents.Sql))
            {
                throw new SqlApiException();
            }

            if (!IsPlaying || currentTrack == null)
            {
                if (!source.HasFlag(CommandActionSource.Mute))
                {
                    throw new IgnoreException("Nothing to ignore");
                }

                return;
            }

            if (index < 0)
            {
                if (currentTrack.ArtistArr.Length > 1)
                {
                    throw new IgnoreException("Provide artist index");
                }
                else
                {
                    index = 0;
                }
            }

            try
            {
                SqlServerWrapper.AddIgnoredArtist(currentTrack, index);
            }
            catch (Exception ex)
            {
                throw new IgnoreException("Failed to ignore artist", ex);
            }

            Skip(0, CommandActionSource.Mute);

            if (!source.HasFlag(CommandActionSource.Mute))
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
