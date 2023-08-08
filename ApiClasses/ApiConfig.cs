using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicordNET.ApiClasses
{
    internal static class ApiConfig
    {
        private static ApiIntents InitIntents;

        internal static void InitApis(ApiIntents intents = ApiIntents.All)
        {
            InitIntents = intents;

            if ((InitIntents & ApiIntents.Youtube) == ApiIntents.Youtube)
            {
                YoutubeApiWrapper.PerformAuth();
            }

            if ((InitIntents & ApiIntents.Yandex) == ApiIntents.Yandex)
            {
                YandexApiWrapper.PerformAuth();
            }

            if ((InitIntents & ApiIntents.Vk) == ApiIntents.Vk)
            {
                VkApiWrapper.PerformAuth();
            }

            if ((InitIntents & ApiIntents.Spotify) == ApiIntents.Spotify)
            {
                SpotifyApiWrapper.PerformAuth();
            }
        }

        internal static void DeinitApis(ApiIntents intents = ApiIntents.All)
        {
            if ((InitIntents & ApiIntents.Youtube) == ApiIntents.Youtube
                && (intents & ApiIntents.Youtube) == ApiIntents.Youtube)
            {
                YoutubeApiWrapper.Logout();
            }

            if ((InitIntents & ApiIntents.Yandex) == ApiIntents.Yandex
                && (intents & ApiIntents.Yandex) == ApiIntents.Yandex)
            {
                YandexApiWrapper.Logout();
            }

            if ((InitIntents & ApiIntents.Vk) == ApiIntents.Vk
                && (intents & ApiIntents.Vk) == ApiIntents.Vk)
            {
                VkApiWrapper.Logout();
            }

            if ((InitIntents & ApiIntents.Spotify) == ApiIntents.Spotify
                && (intents & ApiIntents.Spotify) == ApiIntents.Spotify)
            {
                SpotifyApiWrapper.Logout();
            }
        }
    }
}
