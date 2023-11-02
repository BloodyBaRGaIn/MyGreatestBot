using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Services.Sql;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using System;
using System.Runtime.Versioning;

namespace MyGreatestBot.Player
{
    [SupportedOSPlatform("windows")]
    internal sealed partial class Player
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
                SqlServerWrapper.Instance.AddIgnoredTrack(currentTrack, Handler.GuildId);
            }
            catch (Exception ex)
            {
                throw new IgnoreException("Failed to ignore track", ex);
            }

            Skip(0, source | CommandActionSource.Mute);

            if (!mute)
            {
                Handler.Message.Send(new IgnoreException("Track ignored"), true);
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

            AddIgnoredArtistBody(index);

            Skip(0, source | CommandActionSource.Mute);

            if (!mute)
            {
                Handler.Message.Send(new IgnoreException("Artist ignored"), true);
            }
        }

        private void AddIgnoredArtistBody(int index)
        {
            if (currentTrack == null)
            {
                return;
            }

            int start, max;

            if (index < 0)
            {
                start = 0;
                max = currentTrack.ArtistArr.Length;
            }
            else
            {
                start = index;
                max = index;
            }

            for (int i = start; i < max; i++)
            {
                try
                {
                    SqlServerWrapper.Instance.AddIgnoredArtist(currentTrack, i, Handler.GuildId);
                }
                catch (Exception ex)
                {
                    throw new IgnoreException("Failed to ignore artist", ex);
                }
            }
        }
    }
}
