using DicordNET.Utils;

namespace DicordNET.TrackClasses
{
    internal interface ITrackInfo
    {
        ITrackInfo Base { get; }

        internal HyperLink TrackName { get; }
        internal HyperLink[] ArtistArr { get; }
        internal HyperLink? AlbumName { get; }
        internal HyperLink? PlaylistName { get; }
        internal string Id { get; }
        internal TimeSpan Duration { get; }
        internal TimeSpan Seek { get; set; }
        internal string? CoverURL { get; }
        protected internal string AudioURL { get; protected set; }
        protected internal bool IsLiveStream { get; protected set; }

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
                result += $"Album: {AlbumName}\n";
            }

            if (PlaylistName != null)
            {
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
    }
}
