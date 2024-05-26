using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            private readonly string guildName;

            [AllowNull] private Process Process;
            [AllowNull] private StreamReader? StandardOutput => Process?.StandardOutput;
            [AllowNull] private StreamReader? StandardError => Process?.StandardError;

            /// <summary>
            /// <inheritdoc cref="Process.HasExited"/>
            /// </summary>
            internal bool HasExited => Process?.HasExited ?? true;

            internal FFMPEG(string guildName)
            {
                if (string.IsNullOrWhiteSpace(guildName))
                {
                    guildName = "Unknown guild";
                }

                this.guildName = guildName;
            }

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
                }) ?? throw new InvalidOperationException($"{nameof(FFMPEG)} not started");

                try
                {
                    process.PriorityClass = ProcessPriorityClass.RealTime;
                }
                catch { }

                Process = process;
            }

            /// <inheritdoc cref="Stream.ReadAsync(byte[], int, int, CancellationToken)"/>
            internal Task<int>? ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return StandardOutput?.BaseStream?.ReadAsync(buffer, offset, count, cancellationToken);
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
                    Thread.CurrentThread.SetHighestAvailableTheadPriority(
                        ThreadPriority.Highest,
                        ThreadPriority.Normal);

                    Thread.CurrentThread.Name = $"{nameof(GetErrorMessage)} {guildName} {++ErrorCount}";
                    if (StandardError != null && !StandardError.EndOfStream)
                    {
                        ErrorQueue.Enqueue(StandardError.ReadToEnd());
                    }
                }, cts.Token);

                return !ErrorQueue.TryDequeue(out string? result) || string.IsNullOrWhiteSpace(result)
                    ? string.Empty
                    : result;
            }

            internal bool TryLoad(int milliseconds)
            {
                if (Process == null || StandardOutput == null)
                {
                    return false;
                }

                bool exit = WaitForExit(1);

                if (HasExited && exit && StandardOutput.EndOfStream)
                {
                    return false;
                }

                double ticks = 0;

                DateTime start = DateTime.Now;

                while (StandardOutput.EndOfStream)
                {
                    exit = WaitForExit(1);
                    if (HasExited && exit)
                    {
                        return false;
                    }
                    ticks += (DateTime.Now - start).TotalMilliseconds;
                    if (ticks > milliseconds)
                    {
                        return false;
                    }
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

                Process = null;
            }
        }
    }
}
