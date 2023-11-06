﻿using MyGreatestBot.ApiClasses.Music;
using System;
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

                CancellationTokenSource cancellation = new();
                Task<bool> task = Task.Run(() => StandardError?.EndOfStream ?? true, cancellation.Token);
                if (!task.Wait(100))
                {
                    cancellation.Cancel();
                }
                else if (!task.Result)
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
