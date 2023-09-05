using MyGreatestBot.ApiClasses.Music.Spotify;
using MyGreatestBot.ApiClasses.Music.Vk;
using MyGreatestBot.ApiClasses.Music.Yandex;
using MyGreatestBot.ApiClasses.Music.Youtube;

namespace MyGreatestBot.ApiClasses.Music
{
    internal static class GenericTrackInfo
    {
        internal static ITrackInfo? GetTrack(ApiIntents api, string id)
        {
            return api switch
            {
                ApiIntents.Youtube => YoutubeApiWrapper.GetTrack(id),
                ApiIntents.Yandex => YandexApiWrapper.GetTrack(id),
                ApiIntents.Vk => VkApiWrapper.GetTrack(id),
                ApiIntents.Spotify => SpotifyApiWrapper.GetTrack(id),
                _ => null,
            };
        }
    }
}
