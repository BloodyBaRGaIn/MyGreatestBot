﻿using MyGreatestBot.ApiClasses.ConfigClasses.Descriptors;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    internal sealed partial class PlayerHandler
    {
        /// <summary>
        /// Audio decoder handling class
        /// </summary>
        private sealed class FFMPEG : IDisposable
        {
            private const string FfmpegFileNameKey = "FfmpegFileName";

            private static readonly FfmpegDescriptor FfmpegDescriptor;

            static FFMPEG()
            {
                FfmpegDescriptor = new(FfmpegFileNameKey);
            }

            private static float VolumeRatio { get; } = 0.25f;
            private static uint FrequencyHz { get; } = 48000;

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
            private StreamReader StandardOutput => Process?.StandardOutput ?? StreamReader.Null;

            /// <inheritdoc cref="Process.StandardError"/>
            private StreamReader StandardError => Process?.StandardError ?? StreamReader.Null;

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
                return File.Exists(FfmpegDescriptor.FullPath);
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
            internal void Start(BaseTrackInfo track)
            {
                Stop();

                Process process = Process.Start(new ProcessStartInfo()
                {
                    FileName = FfmpegDescriptor.FullPath,
                    Arguments = GetTrackArguments(track),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    LoadUserProfile = true,
                    WorkingDirectory = "ffmpeg_binaries"
                }) ?? throw new InvalidOperationException($"{nameof(FFMPEG)} not started");

                process.SetHighestAvailableProcessPriority(ProcessPriorityClass.RealTime,
                                                           ProcessPriorityClass.Normal);

                Process = process;

                ErrorTaskCts = new();
                ErrorTask = Task.Factory.StartNew(() =>
                {
                    Thread.CurrentThread.SetHighestAvailableTheadPriority();
                    Thread.CurrentThread.Name = $"{nameof(FFMPEG)}_{nameof(ErrorTask)} \"{guildName}\" {ErrorCount}";
                    while (true)
                    {
                        if (ErrorTaskCts.IsCancellationRequested)
                        {
                            return;
                        }
                        if (!WaitForExit(1))
                        {
                            continue;
                        }

                        _ = ErrorSemaphore.TryWaitOne();

                        try
                        {
                            Thread.Sleep(10);
                        }
                        catch { }

                        if (!StandardError.EndOfStream)
                        {
                            ++ErrorCount;
                            ErrorQueue.Enqueue(StandardError.ReadToEnd());
                        }

                        _ = ErrorSemaphore.TryRelease();
                    }
                }, ErrorTaskCts.Token);
            }

            /// <inheritdoc cref="Stream.ReadAsync(byte[], int, int, CancellationToken)"/>
            internal Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return StandardOutput.BaseStream.ReadAsync(buffer, offset, count, cancellationToken);
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
                _ = ErrorSemaphore?.TryWaitOne();
                if (!ErrorQueue.TryDequeue(out string? result) || string.IsNullOrWhiteSpace(result))
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
            /// <param name="timeoutMs">
            /// Time to await in milliseconds.
            /// </param>
            /// <returns>
            /// True if succeeded, otherwise false.
            /// </returns>
            internal bool TryLoad(int timeoutMs)
            {
                if (Process == null)
                {
                    return false;
                }

                _ = WaitForExit(1);

                Stopwatch stopwatch = Stopwatch.StartNew();

                while (StandardOutput.EndOfStream)
                {
                    bool exit = WaitForExit(1);
                    if ((HasExited && exit) || (stopwatch.ElapsedMilliseconds >= timeoutMs))
                    {
                        break;
                    }
                }

                stopwatch.Stop();

                string errorMessage = GetErrorMessage();

                return string.IsNullOrWhiteSpace(errorMessage) && !StandardOutput.EndOfStream;
            }

            /// <summary>
            /// Stops decoding.
            /// </summary>
            internal void Stop()
            {
                ErrorTaskCts?.Cancel();
                Wait(10);
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

            /// <summary>
            /// Gets argument string for FFMPEG
            /// </summary>
            private static string GetTrackArguments(BaseTrackInfo track)
            {
                string log_level = "-loglevel warning";

                string time_pos = track.TimePosition != TimeSpan.Zero && !track.IsLiveStream
                    ? $"-ss {track.TimePosition}"
                    : string.Empty;

                string multiple_requests = "-multiple_requests 1";

                string reconnect_on_network_error = "-reconnect_on_network_error 1";

                string reconnect_streamed = "-reconnect_streamed 1";

                string reconnect_delay_max = "-reconnect_delay_max 1";

                string reconnect_max_retries = "-reconnect_max_retries 5";

                string audio_url = $"-i \"{track.AudioURL}\"";

                string io_format = "-f s16le";

                string audio_channels = "-ac 2";

                string sampling_frequency = $"-ar {FrequencyHz.ToString(CultureInfo.InvariantCulture)}";

                string volume = (VolumeRatio is > 0 and < 1)
                    ? $"-filter:a \"volume = {VolumeRatio.ToString("0.00", CultureInfo.InvariantCulture)}\""
                    : string.Empty;

                string pipe = "pipe:1";

                return string.Join(' ',
                    log_level,
                    time_pos,
                    multiple_requests,
                    reconnect_on_network_error, reconnect_streamed,
                    reconnect_delay_max, reconnect_max_retries,
                    audio_url,
                    io_format,
                    audio_channels,
                    sampling_frequency,
                    volume,
                    pipe);
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
