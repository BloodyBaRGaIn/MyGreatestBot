using DicordNET.TrackClasses;
using System.Diagnostics;

namespace DicordNET.Player
{
    internal static class TrackManager
    {
        internal const string FFMPEG_PATH = "ffmpeg_binaries/ffmpeg.exe";

        internal static Process StartFFMPEG(ITrackInfo track)
        {
            Process process = Process.Start(new ProcessStartInfo()
            {
                FileName = FFMPEG_PATH,
                Arguments = track.Arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false
            }) ?? throw new InvalidOperationException("ffmpeg not started");

            process.PriorityClass = ProcessPriorityClass.RealTime;

            return process;
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
