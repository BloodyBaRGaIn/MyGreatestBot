using Newtonsoft.Json;

namespace DicordNET.Config
{
    internal static class ConfigManager
    {
        private const string CONFIG_DIR = "Config/Json";

        private const string DISCORD_JSON_PATH = $"{CONFIG_DIR}/config.json";

        private const string GOOGLE_CREDENTIALS_JSON_PATH = $"{CONFIG_DIR}/google_cred.json";
        private const string YANDEX_CREDENTIALS_JSON_PATH = $"{CONFIG_DIR}/yandex_cred.json";
        private const string VK_CREDENTIALS_JSON_PATH = $"{CONFIG_DIR}/vk_cred.json";

        private const string GOOGLE_CLIENT_SECRETS_JSON_PATH = $"{CONFIG_DIR}/google_secret.json";
        private const string SPOTIFY_CLIENT_SECRETS_JSON_PATH = $"{CONFIG_DIR}/spotify_secret.json";

        private static T ReadConfig<T>(string filepath) where T : struct
        {
            string content = string.Empty;

            try
            {
                using FileStream file = GetFileStream(filepath);
                using StreamReader reader = new(file);
                content = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Cannot read file {filepath}", ex);
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch
            {
                throw new InvalidOperationException("Invalid file content");
            }
        }

        private static FileStream GetFileStream(string filepath)
        {
            if (!Directory.Exists(CONFIG_DIR))
            {
                throw new DirectoryNotFoundException($"Config directory not found: {CONFIG_DIR}");
            }
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException("Config file not found", filepath);
            }

            return File.OpenRead(filepath);
        }

        internal static DiscordConfigJSON GetDiscordConfigJSON()
        {
            return ReadConfig<DiscordConfigJSON>(DISCORD_JSON_PATH);
        }

        internal static GoogleCredentialsJSON GetGoogleCredentialsJSON()
        {
            return ReadConfig<GoogleCredentialsJSON>(GOOGLE_CREDENTIALS_JSON_PATH);
        }

        internal static YandexCredentialsJSON GetYandexCredentialsJSON()
        {
            return ReadConfig<YandexCredentialsJSON>(YANDEX_CREDENTIALS_JSON_PATH);
        }

        internal static VkCredentialsJSON GetVkCredentialsJSON()
        {
            return ReadConfig<VkCredentialsJSON>(VK_CREDENTIALS_JSON_PATH);
        }

        internal static FileStream GetGoogleClientSecretsFileStream()
        {
            return GetFileStream(GOOGLE_CLIENT_SECRETS_JSON_PATH);
        }

        internal static SpotifyClientSecretsJSON GetSpotifyClientSecretsJSON()
        {
            return ReadConfig<SpotifyClientSecretsJSON>(SPOTIFY_CLIENT_SECRETS_JSON_PATH);
        }
    }
}
