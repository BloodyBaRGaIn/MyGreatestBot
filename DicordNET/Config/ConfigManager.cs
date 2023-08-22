using Newtonsoft.Json;

namespace DicordNET.Config
{
    /// <summary>
    /// JSON reader class
    /// </summary>
    internal static class ConfigManager
    {
        private const string CONFIG_DIR = "Config/Json";

        private const string DISCORD_JSON_PATH = $"{CONFIG_DIR}/config.json";

        private const string GOOGLE_CREDENTIALS_JSON_PATH = $"{CONFIG_DIR}/google_cred.json";
        private const string YANDEX_CREDENTIALS_JSON_PATH = $"{CONFIG_DIR}/yandex_cred.json";
        private const string VK_CREDENTIALS_JSON_PATH = $"{CONFIG_DIR}/vk_cred.json";

        private const string GOOGLE_CLIENT_SECRETS_JSON_PATH = $"{CONFIG_DIR}/google_secret.json";
        private const string SPOTIFY_CLIENT_SECRETS_JSON_PATH = $"{CONFIG_DIR}/spotify_secret.json";

        /// <summary>
        /// Reads JSON on path
        /// </summary>
        /// <typeparam name="T">Return struct type</typeparam>
        /// <param name="filepath">JSON file path</param>
        /// <returns>Deserialized object instance</returns>
        /// <exception cref="InvalidOperationException"></exception>
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

        /// <summary>
        /// Reads JSON on path
        /// </summary>
        /// <param name="filepath">JSON file path</param>
        /// <returns>Filestream</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
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

        /// <summary>
        /// Reads Discord bot config
        /// </summary>
        /// <returns>Discord bot config</returns>
        internal static DiscordConfigJSON GetDiscordConfigJSON()
        {
            return ReadConfig<DiscordConfigJSON>(DISCORD_JSON_PATH);
        }

        /// <summary>
        /// Reads Google credentials
        /// </summary>
        /// <returns>Google credentials</returns>
        internal static GoogleCredentialsJSON GetGoogleCredentialsJSON()
        {
            return ReadConfig<GoogleCredentialsJSON>(GOOGLE_CREDENTIALS_JSON_PATH);
        }

        /// <summary>
        /// Reads Yandex credentials
        /// </summary>
        /// <returns>Yandex credentials</returns>
        internal static YandexCredentialsJSON GetYandexCredentialsJSON()
        {
            return ReadConfig<YandexCredentialsJSON>(YANDEX_CREDENTIALS_JSON_PATH);
        }

        /// <summary>
        /// Reads Vk credentials
        /// </summary>
        /// <returns>Vk credentials</returns>
        internal static VkCredentialsJSON GetVkCredentialsJSON()
        {
            return ReadConfig<VkCredentialsJSON>(VK_CREDENTIALS_JSON_PATH);
        }

        /// <summary>
        /// Reads Google client secret
        /// </summary>
        /// <returns>Google client secret filestream</returns>
        internal static FileStream GetGoogleClientSecretsFileStream()
        {
            return GetFileStream(GOOGLE_CLIENT_SECRETS_JSON_PATH);
        }

        /// <summary>
        /// Reads Spotify client secret
        /// </summary>
        /// <returns>Spotify client secret</returns>
        internal static SpotifyClientSecretsJSON GetSpotifyClientSecretsJSON()
        {
            return ReadConfig<SpotifyClientSecretsJSON>(SPOTIFY_CLIENT_SECRETS_JSON_PATH);
        }
    }
}
