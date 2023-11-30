using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music.Spotify;
using MyGreatestBot.ApiClasses.Music.Vk;
using MyGreatestBot.ApiClasses.Music.Yandex;
using MyGreatestBot.ApiClasses.Music.Youtube;
using MyGreatestBot.ApiClasses.Services.Discord;
using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.ApiClasses.Services.Sql;
using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

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
    internal class Program
    {
        /// <summary>
        /// Log handler for unhandled exceptions
        /// </summary>
        private static readonly LogHandler CurrentDomainLogErrorHandler = new(Console.Error, AppDomain.CurrentDomain.FriendlyName);

        /// <summary>
        /// Process initialization
        /// </summary>
        static Program()
        {
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
        private static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            ApiManager.Add(SqlServerWrapper.Instance);
            ApiManager.Add(YoutubeApiWrapper.Instance);
            ApiManager.Add(YandexApiWrapper.Instance);
            ApiManager.Add(VkApiWrapper.Instance);
            ApiManager.Add(SpotifyApiWrapper.Instance);
            ApiManager.Add(DiscordWrapper.Instance);

            ApiManager.InitApis();

            DiscordWrapper.Run(connection_timeout: 10000);

            ApiManager.DeinitApis();
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            try
            {
                ConnectionHandler.Logout(false).Wait();
            }
            catch { }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                CurrentDomainLogErrorHandler.Send(
                    $"Unhandled exception was thrown{Environment.NewLine}" +
                    $"{(e.ExceptionObject as Exception)?.Message ?? string.Empty}");
            }
            catch { }
        }
    }
}
