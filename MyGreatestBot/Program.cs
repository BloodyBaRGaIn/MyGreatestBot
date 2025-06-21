global using AllowNullAttribute = System.Diagnostics.CodeAnalysis.AllowNullAttribute;
global using DisallowNullAttribute = System.Diagnostics.CodeAnalysis.DisallowNullAttribute;
global using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace MyGreatestBot
{
    /// <summary>
    /// Main class
    /// </summary>
    internal static class Program
    {
        public static bool Release => !Debug;
        public static bool Debug { get; } =
#if DEBUG
                true
#else
                false
#endif
                ;

        private static readonly Semaphore CancellationEventSemaphore = new(1, 1);

        /// <summary>
        /// Process initialization
        /// </summary>
        static Program()
        {
            string? projectName = System.Reflection.Assembly.GetCallingAssembly().GetName().Name;
            if (string.IsNullOrWhiteSpace(projectName))
            {
                projectName = "MyGreatestBot";
            }

            Console.Title = Debug
                ? $"{projectName} | {nameof(Debug)}"
                : Release
                ? $"{projectName} | {nameof(Release)}"
                : $"{projectName} | Unknown configuration";

            _ = Process.GetCurrentProcess()
                .SetHighestAvailableProcessPriority(ProcessPriorityClass.High,
                                                    ProcessPriorityClass.Normal);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.OutputEncoding = Encoding.UTF8;
        }

        /// <summary>
        /// Main entry point
        /// </summary>
        private static int Main()
        {
            Thread.CurrentThread.SetHighestAvailableTheadPriority(
                ThreadPriority.AboveNormal,
                ThreadPriority.Normal);

            Console.CancelKeyPress += Console_CancelKeyPress;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            // YoutubeExplode won't work without specific environment variable
            YoutubeExplodeBypass.Bypass();

            #region Music APIs
            ApiManager.Add(YoutubeApiWrapper.Instance);
            ApiManager.Add(YandexApiWrapper.Instance);
            ApiManager.Add(VkApiWrapper.Instance);
            ApiManager.Add(SpotifyApiWrapper.Instance);
            #endregion Music APIs

            #region Services
            ApiManager.Add(LiteDbWrapper.Instance);
            #endregion Services

            ApiManager.Add(DiscordWrapper.Instance);

            ApiManager.InitApis(); // ApiIntents.Discord

            if (ApiManager.IsAnyEssentialApiFailed)
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    "Essential API(s) failed.");
            }
            else
            {
                DiscordWrapper.Run();
            }

            AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;

            ApiManager.DeinitApis();

            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;

            return 0;
        }

        /// <inheritdoc cref="Console.CancelKeyPress"/>
        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            _ = sender;
            e.Cancel = true;

            if (!CancellationEventSemaphore.TryWaitOne(0))
            {
                DiscordWrapper.CurrentDomainLogHandler.Send("Application is already closing.", LogLevel.Warning);
                return;
            }

            DiscordWrapper.CurrentDomainLogHandler.Send("CancelKey pressed. Closing...", LogLevel.Warning);

            try
            {
                ConnectionHandler.Logout().Wait();
            }
            catch { }
        }

        /// <inheritdoc cref="AppDomain.ProcessExit"/>
        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            _ = sender;
            _ = e;

            if (!CancellationEventSemaphore.TryWaitOne(0))
            {
                DiscordWrapper.CurrentDomainLogHandler.Send("Application is already closing.", LogLevel.Warning);
                return;
            }

            DiscordWrapper.CurrentDomainLogHandler.Send("Close button pressed. Closing...", LogLevel.Warning);

            bool success = false;
            try
            {
                success = ConnectionHandler.Logout().Wait(10000);
            }
            catch { }

            while (success)
            {
                try
                {
                    Thread.Sleep(1);
                }
                catch
                {
                    break;
                }
            }
        }

        /// <inheritdoc cref="AppDomain.UnhandledException"/>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _ = sender;

            if (!CancellationEventSemaphore.TryWaitOne(0))
            {
                return;
            }

            string? message = (e.ExceptionObject as Exception)?.GetExtendedMessage();
            if (string.IsNullOrWhiteSpace(message))
            {
                message = "Cannot get exception message";
            }

            try
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    string.Join(Environment.NewLine,
                        "Unhandled exception was thrown",
                        message));
            }
            catch { }
        }
    }
}
