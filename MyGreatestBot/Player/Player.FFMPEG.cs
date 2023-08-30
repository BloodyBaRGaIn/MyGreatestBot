using MyGreatestBot.ApiClasses;
using System;
using System.Diagnostics;
using System.IO;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        private class FFMPEG
        {
            internal const string FFMPEG_PATH = "ffmpeg_binaries/ffmpeg.exe";
            private Process? Process;

            internal bool HasExited => Process?.HasExited ?? true;
            internal StreamReader? StandardOutput => Process?.StandardOutput;

            static FFMPEG()
            {
                if (!File.Exists(FFMPEG_PATH))
                {
                    throw new FileNotFoundException($"ffmpeg executable file not found{Environment.NewLine}" +
                        "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip",
                        FFMPEG_PATH);
                }
            }

            internal void Start(ITrackInfo track)
            {
                Stop();

                Process process = Process.Start(new ProcessStartInfo()
                {
                    FileName = FFMPEG_PATH,
                    Arguments = track.Arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }) ?? throw new InvalidOperationException("ffmpeg not started");

                try
                {
                    process.PriorityClass = ProcessPriorityClass.RealTime;
                }
                catch { }

                Process = process;
            }

            internal bool WaitForExit(int milliseconds)
            {
                return Process == null || Process.WaitForExit(milliseconds);
            }

            internal bool TryLoad(int milliseconds)
            {
                if (Process == null || StandardOutput == null)
                {
                    return false;
                }
                bool exit = WaitForExit(milliseconds);
                if (HasExited || exit || StandardOutput.EndOfStream)
                {
                    return false;
                }

                return true;
            }

            internal void Stop()
            {
                if (Process == null)
                {
                    return;
                }

                try
                {
                    Process.Kill();
                }
                catch { }

                try
                {
                    Process.Dispose();
                }
                catch { }
            }
        }
    }
}
