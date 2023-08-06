using DicordNET.ApiClasses;
using DicordNET.TrackClasses;
using System.Diagnostics;

namespace DicordNET
{
    internal static class TrackManager
    {
        internal const string FFMPEG_PATH = "ffmpeg_binaries/ffmpeg.exe";

        internal static List<ITrackInfo> GetAll(string query)
        {
            List<ITrackInfo> tracks = new();

            if (query.Contains("https://www.youtube.com/"))
            {
                tracks.AddRange(YoutubeApiWrapper.GetTracks(query));
            }
            else if (query.Contains("https://music.yandex.by/") || query.Contains("https://music.yandex.ru/"))
            {
                tracks.AddRange(YandexApiWrapper.GetTracks(query));
            }
            else if (query.Contains("https://vk.com/"))
            {
                tracks.AddRange(VkApiWrapper.GetTracks(query));
            }
            else
            {
                // Unknown query type
                ;
            }

            return tracks;
        }

        internal static Process StartFFMPEG(ITrackInfo track)
        {
            return Process.Start(new ProcessStartInfo()
            {
                FileName = FFMPEG_PATH,
                Arguments = track.Arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false
            }) ?? throw new InvalidOperationException("ffmpeg not started");
        }

        internal static void DisposeFFMPEG(Process ffmpeg)
        {
            if (!ffmpeg.HasExited)
            {
                try
                {
                    ffmpeg.Kill();
                }
                catch { }
            }

            try
            {
                ffmpeg.Dispose();
            }
            catch { }
        }
    }
}
