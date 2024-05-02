using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music.Spotify;
using MyGreatestBot.ApiClasses.Music.Vk;
using MyGreatestBot.ApiClasses.Music.Yandex;
using MyGreatestBot.ApiClasses.Music.Youtube;
using MyGreatestBot.ApiClasses.Services.Discord;
using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.ApiClasses.Services.Sql;
using MyGreatestBot.Extensions;
using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace MyGreatestBot
{
    /// <summary>
    /// Main class
    /// </summary>
    [SupportedOSPlatform("windows")]
    [UnsupportedOSPlatform("linux")]
    [UnsupportedOSPlatform("macos")]
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("ios")]
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
                : projectName;

            try
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            }
            catch { }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.OutputEncoding = Encoding.Unicode;
        }

        /// <summary>
        /// Main entry point
        /// </summary>
        private static int Main()
        {
            Thread.CurrentThread.SetHighestAvailableTheadPriority(
                ThreadPriority.Highest,
                ThreadPriority.Normal);

            Console.CancelKeyPress += Console_CancelKeyPress;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            ApiManager.Add(SqlServerWrapper.Instance);
            ApiManager.Add(YoutubeApiWrapper.MusicInstance);
            ApiManager.Add(YandexApiWrapper.MusicInstance);
            ApiManager.Add(VkApiWrapper.MusicInstance);
            ApiManager.Add(SpotifyApiWrapper.MusicInstance);
            ApiManager.Add(DiscordWrapper.Instance);

            ApiManager.InitApis();

            DiscordWrapper.Run(connectionTimeout: 10000,
                               disconnectionTimeout: 500);

            AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;

            ApiManager.DeinitApis();

            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;

            return 0;
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            _ = sender;

            DiscordWrapper.CurrentDomainLogHandler.Send("CancelKey pressed. Closing...", LogLevel.Warning);

            Console.CancelKeyPress -= Console_CancelKeyPress;
            e.Cancel = true;

            try
            {
                ConnectionHandler.Logout(false).Wait();
            }
            catch { }
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            _ = sender;
            _ = e;

            DiscordWrapper.CurrentDomainLogHandler.Send("Close button pressed. Closing...", LogLevel.Warning);

            AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;

            try
            {
                ConnectionHandler.Logout(false).Wait();
            }
            catch { }

            while (true)
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

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _ = sender;

            string? message = (e.ExceptionObject as Exception)?.GetExtendedMessage();
            if (string.IsNullOrWhiteSpace(message))
            {
                message = "Cannot get exception message";
            }

            try
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    $"Unhandled exception was thrown{Environment.NewLine}{message}");
            }
            catch { }
        }
    }
}
