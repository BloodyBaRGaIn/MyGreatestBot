using MyGreatestBot.ApiClasses.Music;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        private sealed class FFMPEG
        {
            internal const string FFMPEG_PATH = "ffmpeg_binaries/ffmpeg.exe";

            private static ulong ErrorCount;

            private static readonly Queue<string> ErrorQueue = new();

            [AllowNull]
            private Process Process;

            [AllowNull]
            internal StreamReader? StandardOutput => Process?.StandardOutput;

            [AllowNull]
            internal StreamReader? StandardError => Process?.StandardError;

            internal bool HasExited => Process?.HasExited ?? true;

            internal static bool CheckForExecutableExists()
            {
                return File.Exists(FFMPEG_PATH);
            }

            internal void Start(ITrackInfo track)
            {
                Stop();

                Process process = Process.Start(new ProcessStartInfo()
                {
                    FileName = FFMPEG_PATH,
                    Arguments = track.Arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    LoadUserProfile = true,
                    WorkingDirectory = "ffmpeg_binaries"
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

            internal string GetErrorMessage()
            {
                if (Process == null)
                {
                    return string.Empty;
                }

                CancellationTokenSource cts = new();
                Task task = Task.Factory.StartNew(() =>
                {
                    Thread.CurrentThread.Name = $"{nameof(GetErrorMessage)}{++ErrorCount}";
                    if (StandardError != null && !StandardError.EndOfStream)
                    {
                        ErrorQueue.Enqueue(StandardError.ReadToEnd());
                    }
                }, cts.Token);

                return ErrorQueue.TryDequeue(out string? result) && !string.IsNullOrWhiteSpace(result) ? result : string.Empty;
            }

            internal bool TryLoad(int milliseconds)
            {
                if (Process == null || StandardOutput == null)
                {
                    return false;
                }

                bool exit = WaitForExit(milliseconds);

                Task.Yield().GetAwaiter().GetResult();

                if (HasExited && exit && StandardOutput.EndOfStream)
                {
                    return false;
                }

                string errorMessage = GetErrorMessage();

                return string.IsNullOrWhiteSpace(errorMessage);
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
