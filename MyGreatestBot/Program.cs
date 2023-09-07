using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music.Spotify;
using MyGreatestBot.ApiClasses.Music.Vk;
using MyGreatestBot.ApiClasses.Music.Yandex;
using MyGreatestBot.ApiClasses.Music.Youtube;
using MyGreatestBot.ApiClasses.Services.Discord;
using MyGreatestBot.ApiClasses.Services.Sql;
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

            ApiManager.Add(SqlServerWrapper.Instance);
            ApiManager.Add(YoutubeApiWrapper.Instance);
            ApiManager.Add(YandexApiWrapper.Instance);
            ApiManager.Add(VkApiWrapper.Instance);
            ApiManager.Add(SpotifyApiWrapper.Instance);
            ApiManager.Add(BotWrapper.BotInstance);

            ApiManager.InitApis();

            BotWrapper.Run(connection_timeout: 10000);

            ApiManager.DeinitApis();
        }
    }
}
