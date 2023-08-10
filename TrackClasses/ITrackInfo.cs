using DicordNET.ApiClasses;
using DicordNET.Utils;

namespace DicordNET.TrackClasses
{
    internal interface ITrackInfo
    {
        protected ITrackInfo Base { get; }

        protected string Domain { get; }

        internal ApiIntents TrackType { get; }

        internal string Id { get; }

        internal HyperLink TrackName { get; }
        internal HyperLink[] ArtistArr { get; }
        internal HyperLink? AlbumName { get; }
        internal HyperLink? PlaylistName { get; }

        internal string Title { get; }
        
        internal TimeSpan Duration { get; }
        internal protected TimeSpan Seek { get; protected set; }

        internal string? CoverURL { get; }
        internal string AudioURL { get; }
        internal bool IsLiveStream { get; }

        internal bool TrySeek(TimeSpan span)
        {
            if (IsLiveStream || Duration == TimeSpan.Zero || span > Duration)
            {
                return false;
            }

            lock (this)
            {
                Seek = span;
            }

            return true;
        }

        internal string GetMessage()
        {
            string result = string.Empty;
            result += $"Playing: {TrackName}\n";
            result += "Author: ";

            result += ArtistArr[0].ToString();

            for (int i = 1; i < ArtistArr.Length; i++)
            {
                result += $", {ArtistArr[i]}";
            }

            result += '\n';

            if (Duration != TimeSpan.Zero)
            {
                result += $"Duration: {Duration:hh\\:mm\\:ss}\n";
            }

            if (AlbumName != null)
            {
                if (!string.IsNullOrWhiteSpace(AlbumName.Title))
                    result += $"Album: {AlbumName}\n";
            }

            if (PlaylistName != null)
            {
                if (!string.IsNullOrWhiteSpace(PlaylistName.Title))
                    result += $"Playlist: {PlaylistName}\n";
            }

            if (Seek != TimeSpan.Zero)
            {
                result += $"Position: {Seek:hh\\:mm\\:ss}\n";
            }

            return result.Trim('\n');
        }

        internal DSharpPlus.Entities.DiscordEmbedBuilder.EmbedThumbnail? GetThumbnail()
        {
            if (string.IsNullOrWhiteSpace(CoverURL))
            {
                return null;
            }

            return new DSharpPlus.Entities.DiscordEmbedBuilder.EmbedThumbnail()
            {
                Url = CoverURL
            };
        }

        internal abstract void ObtainAudioURL();

        internal abstract void Reload();

        internal string Arguments => $"-loglevel fatal {(Seek == TimeSpan.Zero ? "" : $"-ss {Seek} ")}" +
                                     $"-i \"{AudioURL}\" -f s16le -ac 2 -ar 48000 -filter:a \"volume = 0.25\" pipe:1";

        internal int CompareTo(ITrackInfo? other)
        {
            System.Numerics.BigInteger result = 0;
            if (this is null || other is null) return int.MaxValue;
            int name = other.TrackName.CompareTo(TrackName);
            int album;
            if (other.AlbumName is null)
            {
                album = int.MaxValue;
            }
            else
            {
                album = other.AlbumName.CompareTo(AlbumName);
            }
            int artist = 0;
            if (other.ArtistArr.Length != ArtistArr.Length)
            {
                artist = int.MaxValue;
            }
            else
            {
                for (int i = 0; i < ArtistArr.Length; i++)
                {
                    artist += other.ArtistArr[i].CompareTo(ArtistArr[i]);
                }
            }

            result += name;
            result += album;
            result += artist;

            if (result > int.MaxValue) return int.MaxValue;
            if (result < int.MinValue) return int.MinValue;
            return (int)result;
        }
    }
}
