using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Exceptions;
using MyGreatestBot.ApiClasses.Services.Sql;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using System;
using System.Runtime.Versioning;

namespace MyGreatestBot.Player
{
    [SupportedOSPlatform("windows")]
    internal partial class Player
    {
        internal void IgnoreTrack(CommandActionSource source)
        {
            bool mute = source.HasFlag(CommandActionSource.Mute);
            if (!ApiManager.InitIntents.HasFlag(ApiIntents.Sql))
            {
                throw new SqlApiException();
            }

            if (!IsPlaying || currentTrack == null)
            {
                if (!mute)
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

            Skip(0, source | CommandActionSource.Mute);

            if (!mute)
            {
                Handler.Message.Send(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Track ignored"
                });
            }
        }

        internal void IgnoreArtist(int index, CommandActionSource source)
        {
            bool mute = source.HasFlag(CommandActionSource.Mute);
            if (!ApiManager.InitIntents.HasFlag(ApiIntents.Sql))
            {
                throw new SqlApiException();
            }

            if (!IsPlaying || currentTrack == null)
            {
                if (!mute)
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

            Skip(0, source | CommandActionSource.Mute);

            if (!mute)
            {
                Handler.Message.Send(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Artist ignored"
                });
            }
        }
    }
}
