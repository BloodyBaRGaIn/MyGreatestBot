using MyGreatestBot.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    internal sealed partial class PlayerHandler
    {
        private enum LowPlayerResult : int
        {
            NotStarted = -2,
            TrackNull = -1,
            Success = 0,
            RestartSeek,
            RestartRead,
            RestartWrite
        }

        private enum ReadBytesResult : int
        {
            NotStarted = -2,
            TaskNull = -1,
            Success = 0,
            ZeroLength,
            Timeout
        }

        [Flags]
        private enum PlayerStatus : uint
        {
            None = 0x0000U,
            Init = 0x0001U,
            Idle = 0x0002U,
            InitOrIdle = Init | Idle,
            Start = 0x0004U,
            Obtaining = 0x0008U,
            Loading = 0x0010U,
            Playing = 0x0020U,
            Paused = 0x0040U,
            Finish = 0x0080U,
            Deinit = 0x0100U,
            Error = 0x0200U,
            DeinitOrError = Deinit | Error
        }

        /// <summary>
        /// Resets the player states.
        /// </summary>
        private void Reset()
        {
            Thread.BeginCriticalRegion();

            currentTrack = null;
            IsPlaying = false;
            IsPaused = false;
            StopRequested = false;
            RewindRequested = false;
            PlayerTimePosition = TimeSpan.Zero;
            tracksQueue?.Clear();
            FfmpegInstance?.Stop();

            Thread.EndCriticalRegion();
        }

        /// <summary>
        /// Delay with yield.
        /// </summary>
        /// <param name="delay">The number of milliseconds to delay.</param>
        private static void Wait(int delay = 1)
        {
            Task.Delay(delay).Wait();
            Task.Yield().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Method for waiting for status.<br/>
        /// Returned when the logical AND result<br/>
        /// between the player status and the passed status is non-zero.
        /// </summary>
        /// <param name="status">Status to await.</param>
        /// <param name="waitAction">Additional action to invoke while waiting.</param>
        private void WaitForStatus(PlayerStatus status, Action? waitAction = null)
        {
            while (true)
            {
                if ((Status & status) != PlayerStatus.None)
                {
                    break;
                }
                waitAction?.Invoke();
                Wait();
            }
        }

        /// <summary>
        /// Waiting for player status is "finished", "idle", or "errored".
        /// </summary>
        private void WaitForFinish()
        {
            WaitForStatus(PlayerStatus.Finish | PlayerStatus.InitOrIdle | PlayerStatus.DeinitOrError);
        }

        /// <summary>
        /// Logs ffmpeg status.
        /// </summary>
        /// <param name="lowPlayerResult"></param>
        /// <param name="readBytesResult"></param>
        private void PrintErrorMessage(LowPlayerResult lowPlayerResult, ReadBytesResult readBytesResult)
        {
            Handler.Log.Send(
                Environment.NewLine +
                string.Join(Environment.NewLine,
                    $"{nameof(FFMPEG.HasExited)} {FfmpegInstance.HasExited}",
                    $"{nameof(FFMPEG.ExitCode)} {FfmpegInstance.ExitCode}",
                    $"{nameof(LowPlayerResult)} {lowPlayerResult}",
                    $"{nameof(ReadBytesResult)} {readBytesResult}",
                    $"{nameof(PlayerTimePosition)} {PlayerTimePosition.GetCustomTime(withMilliseconds: true)}",
                    $"{nameof(TimeRemaining)} {TimeRemaining.GetCustomTime(withMilliseconds: true)}") +
                Environment.NewLine,
                LogLevel.Debug);

            string errorMessage = FfmpegInstance.GetErrorMessage();

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                Handler.Log.Send("Unexpected process exit", LogLevel.Debug);
            }
            else
            {
                Handler.Log.Send(
                    Environment.NewLine +
                    string.Join(Environment.NewLine,
                        $"Error message begin{Environment.NewLine}",
                        errorMessage,
                        "Error message end") +
                    Environment.NewLine, LogLevel.Debug);
            }
        }
    }
}
