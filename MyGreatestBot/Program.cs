using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music.Spotify;
using MyGreatestBot.ApiClasses.Music.Vk;
using MyGreatestBot.ApiClasses.Music.Yandex;
using MyGreatestBot.ApiClasses.Music.Youtube;
using MyGreatestBot.ApiClasses.Services.Sql;
using MyGreatestBot.Bot;
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
        /// Main entry point
        /// </summary>
        private static void Main()
        {
            try
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            }
            catch { }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            AuthActions.SetApiOrder(
                ApiIntents.Sql,
                ApiIntents.Youtube,
                ApiIntents.Yandex,
                ApiIntents.Vk,
                ApiIntents.Spotify);

            AuthActions.AddOrReplace(
                ApiIntents.Sql,
                SqlServerWrapper.Open, SqlServerWrapper.Close);

            AuthActions.AddOrReplace(
                ApiIntents.Youtube,
                YoutubeApiWrapper.PerformAuth, YoutubeApiWrapper.Logout);

            AuthActions.AddOrReplace(
                ApiIntents.Yandex,
                YandexApiWrapper.PerformAuth, YandexApiWrapper.Logout);

            AuthActions.AddOrReplace(
                ApiIntents.Vk,
                VkApiWrapper.PerformAuth, VkApiWrapper.Logout);

            AuthActions.AddOrReplace(
                ApiIntents.Spotify,
                SpotifyApiWrapper.PerformAuth, SpotifyApiWrapper.Logout);

            ApiManager.InitApis();

            BotWrapper.Run(connection_timeout: 10000);

            ApiManager.DeinitApis();
        }
    }
}
