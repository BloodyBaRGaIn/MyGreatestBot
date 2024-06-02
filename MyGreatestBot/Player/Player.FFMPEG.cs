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
        /// <summary>
        /// Audio decoder handling class
        /// </summary>
        private sealed class FFMPEG : IDisposable
        {
            internal const string FFMPEG_PATH = "ffmpeg_binaries/ffmpeg.exe";

            private ulong ErrorCount;

            private readonly Queue<string> ErrorQueue = new();

            private Task? ErrorTask;
            private CancellationTokenSource? ErrorTaskCts;
            private readonly Semaphore ErrorSemaphore = new(1, 1);

            private readonly string guildName;

            /// <summary>
            /// FFMPEG process.
            /// </summary>
            [AllowNull] private Process Process;

            /// <inheritdoc cref="Process.StandardOutput"/>
            [AllowNull] private StreamReader? StandardOutput => Process?.StandardOutput;

            /// <inheritdoc cref="Process.StandardError"/>
            [AllowNull] private StreamReader? StandardError => Process?.StandardError;

            /// <summary>
            /// <inheritdoc cref="Process.HasExited"/>
            /// </summary>
            internal bool HasExited
            {
                get
                {
                    try
                    {
                        return Process?.HasExited ?? true;
                    }
                    catch
                    {
                        return true;
                    }
                }
            }

            /// <summary>
            /// <inheritdoc cref="Process.ExitCode"/>
            /// </summary>
            internal int ExitCode
            {
                get
                {
                    if (!HasExited)
                    {
                        return 0;
                    }
                    try
                    {
                        return Process?.ExitCode ?? 0;
                    }
                    catch
                    {
                        return 0;
                    }
                }
            }

            /// <summary>
            /// Default class constructor.
            /// </summary>
            /// 
            /// <param name="guildName">
            /// Guild name referenced to the <see cref="ConnectionHandler"/> instance.
            /// </param>
            internal FFMPEG(string guildName)
            {
                if (string.IsNullOrWhiteSpace(guildName))
                {
                    guildName = "Unknown guild";
                }

                this.guildName = guildName;
            }

            /// <summary>
            /// Checks if executable file exists.
            /// </summary>
            /// 
            /// <returns>
            /// True if exists, otherwise false.
            /// </returns>
            internal static bool CheckForExecutableExists()
            {
                return File.Exists(FFMPEG_PATH);
            }

            /// <summary>
            /// Starts track audio decoding.
            /// </summary>
            /// 
            /// <param name="track">
            /// Track to decode.
            /// </param>
            /// 
            /// <exception cref="InvalidOperationException"></exception>
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

                ErrorTaskCts = new();
                ErrorTask = Task.Factory.StartNew(() =>
                {
                    Thread.CurrentThread.SetHighestAvailableTheadPriority();
                    Thread.CurrentThread.Name = $"{nameof(FFMPEG)}_{nameof(ErrorTask)} {guildName} {ErrorCount}";
                    while (true)
                    {
                        if (ErrorTaskCts.IsCancellationRequested || StandardError == null)
                        {
                            return;
                        }
                        if (!WaitForExit(1))
                        {
                            continue;
                        }
                        ++ErrorCount;
                        if (!StandardError.EndOfStream && HasExited)
                        {
                            _ = ErrorSemaphore.TryWaitOne();
                            ErrorQueue.Enqueue(StandardError.ReadToEnd());
                            _ = ErrorSemaphore.TryRelease();
                        }
                    }
                }, ErrorTaskCts.Token);
            }

            /// <inheritdoc cref="Stream.ReadAsync(byte[], int, int, CancellationToken)"/>
            internal Task<int>? ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return StandardOutput?.BaseStream?.ReadAsync(buffer, offset, count, cancellationToken);
            }

            /// <inheritdoc cref="Process.WaitForExit(int)"/>
            internal bool WaitForExit(int milliseconds)
            {
                return Process == null || Process.WaitForExit(milliseconds);
            }

            /// <summary>
            /// Gets last FFMPEG error message.
            /// </summary>
            /// 
            /// <returns>
            /// Message string or empty string if there were no errors occured.
            /// </returns>
            internal string GetErrorMessage()
            {
                _ = ErrorSemaphore.TryWaitOne();
                if (Process == null || ErrorQueue.TryDequeue(out string? result) || string.IsNullOrWhiteSpace(result))
                {
                    result = string.Empty;
                }
                _ = ErrorSemaphore?.TryRelease();
                return result;
            }

            /// <summary>
            /// Tries to get at least one byte from decoding process.
            /// </summary>
            /// 
            /// <param name="milliseconds">
            /// 
            /// </param>
            /// <returns></returns>
            internal bool TryLoad(int milliseconds)
            {
                if (Process == null || StandardOutput == null)
                {
                    return false;
                }

                _ = WaitForExit(1);

                Stopwatch stopwatch = Stopwatch.StartNew();

                while (StandardOutput.EndOfStream)
                {
                    bool exit = WaitForExit(1);
                    if (HasExited && exit || stopwatch.ElapsedMilliseconds >= milliseconds)
                    {
                        stopwatch.Stop();
                        return false;
                    }
                }

                stopwatch.Stop();

                string errorMessage = GetErrorMessage();

                return string.IsNullOrWhiteSpace(errorMessage);
            }

            /// <summary>
            /// Stops decoding.
            /// </summary>
            internal void Stop()
            {
                ErrorTaskCts?.Cancel();
                ErrorTask?.Wait();

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
                ErrorTaskCts = null;
            }

            public void Dispose()
            {
                Stop();
                try
                {
                    ErrorTask?.Dispose();
                }
                catch { }

                ErrorSemaphore.TryDispose();
            }
        }
    }
}
