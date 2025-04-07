using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Yandex.Music.Api.Extensions.API;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Track;

namespace MyGreatestBot.ApiClasses.Music.Yandex
{
    /// <summary>
    /// Yandex track info implementation
    /// </summary>
    public sealed class YandexTrackInfo : BaseTrackInfo
    {
        public override ApiIntents TrackType => ApiIntents.Yandex;

        private readonly YTrack origin;

        /// <summary>
        /// Yandex track info constructor
        /// </summary>
        /// <param name="track">Track instance from Yandex API</param>
        /// <param name="playlist">Playlist instance from Yandex API</param>
        /// <param name="transletters">Make transletters from cyrillic</param>
        internal YandexTrackInfo(YTrack track, YPlaylist? playlist = null, bool transletters = false)
        {
            origin = track;

            TrackName = new HyperLink(track.Title, $"{Domain}track/{track.Id}")
                .WithId(GetCompositeId(track.Id));

            ArtistArr = [.. track.Artists.Select(a =>
                new HyperLink(
                    transletters ? a.Name.ToTransletters() : a.Name,
                    $"{Domain}artist/{a.Id}")
                .WithId(GetCompositeId(a.Id)))];

            Duration = TimeSpan.FromMilliseconds(track.DurationMs);

            YAlbum? album = track.Albums?.FirstOrDefault();
            if (album != null)
            {
                AlbumName = new(album.Title, $"{Domain}album/{album.Id}");
            }

            if (playlist != null)
            {
                string title = playlist.Title;
                if (title == string.Empty)
                {
                    title = "Playlist";
                }
                PlaylistName = new(title, $"{Domain}users/{playlist.Owner.Login}/playlists/{playlist.Kind}");
            }

            List<string?> cover_uris = [track.CoverUri];

            if (track.Albums != null && track.Albums.Count != 0)
            {
                cover_uris.AddRange(track.Albums
                    .Where(static a => a != null)
                    .Select(static a => a.CoverUri));
            }

            if (track.Artists != null && track.Artists.Count != 0)
            {
                cover_uris.AddRange(track.Artists
                    .Where(static a => a != null)
                    .Select(a => a.OgImage));
            }

            CoverUrlCollection = cover_uris.Select(static cover => $"https://{cover?.Replace("/%%", "/100x100")}");
        }

        protected override void ObtainAudioURLInternal(CancellationTokenSource cts)
        {
            _ = cts;
            AudioURL = origin.GetLink();
        }
    }
}
