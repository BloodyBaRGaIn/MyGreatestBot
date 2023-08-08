using DicordNET.TrackClasses;

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

        internal static void ReloadApis(ApiIntents intents = ApiIntents.All)
        {
            if ((InitIntents & ApiIntents.Youtube) == ApiIntents.Youtube
                && (intents & ApiIntents.Youtube) == ApiIntents.Youtube)
            {
                YoutubeApiWrapper.Logout();
                YoutubeApiWrapper.PerformAuth();
            }

            if ((InitIntents & ApiIntents.Yandex) == ApiIntents.Yandex
                && (intents & ApiIntents.Yandex) == ApiIntents.Yandex)
            {
                YandexApiWrapper.Logout();
                YandexApiWrapper.PerformAuth();
            }

            if ((InitIntents & ApiIntents.Vk) == ApiIntents.Vk
                && (intents & ApiIntents.Vk) == ApiIntents.Vk)
            {
                VkApiWrapper.Logout();
                VkApiWrapper.PerformAuth();
            }

            if ((InitIntents & ApiIntents.Spotify) == ApiIntents.Spotify
                && (intents & ApiIntents.Spotify) == ApiIntents.Spotify)
            {
                SpotifyApiWrapper.Logout();
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

        internal static List<ITrackInfo> GetAll(string? query)
        {
            List<ITrackInfo> tracks = new();

            if (string.IsNullOrWhiteSpace(query))
            {
                return tracks;
            }

            if (query.Contains("https://www.youtube.com/"))
            {
                if ((InitIntents & ApiIntents.Youtube) == ApiIntents.Youtube)
                    tracks.AddRange(YoutubeApiWrapper.GetTracks(query));
            }
            else if (query.Contains("https://music.yandex.by/") || query.Contains("https://music.yandex.ru/"))
            {
                if ((InitIntents & ApiIntents.Yandex) == ApiIntents.Yandex)
                    tracks.AddRange(YandexApiWrapper.GetTracks(query));
            }
            else if (query.Contains("https://vk.com/"))
            {
                if ((InitIntents & ApiIntents.Vk) == ApiIntents.Vk)
                    tracks.AddRange(VkApiWrapper.GetTracks(query));
            }
            else if (query.Contains("https://open.spotify.com/"))
            {
                if ((InitIntents & ApiIntents.Spotify) == ApiIntents.Spotify)
                    tracks.AddRange(SpotifyApiWrapper.GetTracks(query));
            }
            else
            {
                // Unknown query type
                ;
            }

            return tracks;
        }
    }
}
