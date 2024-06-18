global using AllowNullAttribute = System.Diagnostics.CodeAnalysis.AllowNullAttribute;
global using DisallowNullAttribute = System.Diagnostics.CodeAnalysis.DisallowNullAttribute;
global using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;
using MyGreatestBot.ApiClasses;
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

            ApiManager.Add(LiteDbWrapper.Instance);
            ApiManager.Add(YoutubeApiWrapper.Instance);
            ApiManager.Add(YandexApiWrapper.Instance);
            ApiManager.Add(VkApiWrapper.Instance);
            ApiManager.Add(SpotifyApiWrapper.Instance);
            ApiManager.Add(DiscordWrapper.Instance);

            ApiManager.InitApis();

            if (ApiManager.IsAnyEssentialApiFailed)
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    "Essential API(s) failed.");
            }
            else
            {
                DiscordWrapper.Run(connectionTimeout: 10000,
                                   disconnectionTimeout: 500);
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

            DiscordWrapper.CurrentDomainLogHandler.Send("CancelKey pressed. Closing...", LogLevel.Warning);

            Console.CancelKeyPress -= Console_CancelKeyPress;
            e.Cancel = true;

            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;

            try
            {
                ConnectionHandler.Logout(false).Wait();
            }
            catch { }
        }

        /// <inheritdoc cref="AppDomain.ProcessExit"/>
        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            _ = sender;
            _ = e;

            DiscordWrapper.CurrentDomainLogHandler.Send("Close button pressed. Closing...", LogLevel.Warning);

            AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;

            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;

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

        /// <inheritdoc cref="AppDomain.UnhandledException"/>
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
                    string.Join(Environment.NewLine,
                        "Unhandled exception was thrown",
                        message));
            }
            catch { }
        }
    }
}
