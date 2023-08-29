using MyGreatestBot.ApiClasses;
using System;
using System.Diagnostics;
using System.IO;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        private static class FFMPEG
        {
            internal const string FFMPEG_PATH = "ffmpeg_binaries/ffmpeg.exe";

            static FFMPEG()
            {
                if (!File.Exists(FFMPEG_PATH))
                {
                    throw new FileNotFoundException($"ffmpeg executable file not found{Environment.NewLine}" +
                        "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip",
                        FFMPEG_PATH);
                }
            }

            internal static Process StartProcess(ITrackInfo track)
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

            internal static void StopProcess(Process ffmpeg)
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
}
