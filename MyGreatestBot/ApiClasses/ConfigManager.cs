using MyGreatestBot.ApiClasses.ConfigStructs;
using Newtonsoft.Json;
using System;
using System.IO;

namespace MyGreatestBot.ApiClasses
{
    /// <summary>
    /// JSON reader class
    /// </summary>
    internal static class ConfigManager
    {
        
        private const string DiscordConfigNameKey = "DiscordConfigName";
        private const string DbNosqlConfigNameKey = "DbNosqlConfigName";
        private const string DbSqlConfigNameKey = "DbSqlConfigName";
        private const string DbSqlServiceConfigNameKey = "DbSqlServiceConfigName";
        private const string GoogleCredentialsConfigNameKey = "GoogleCredentialsConfigName";
        private const string GoogleAppSecretsConfigNameKey = "GoogleAppSecretsConfigName";
        private const string SpotifyCredentialsConfigNameKey = "SpotifyCredentialsConfigName";
        private const string VkCredentialsConfigNameKey = "VkCredentialsConfigName";
        private const string YandexCredentialsConfigNameKey = "YandexCredentialsConfigName";

        private static readonly JsonConfigDescriptor DiscordConfigDescriptor;
        private static readonly JsonConfigDescriptor DbNosqlConfigDescriptor;
        private static readonly JsonConfigDescriptor DbSqlConfigDescriptor;
        private static readonly JsonConfigDescriptor DbSqlServiceConfigDescriptor;
        private static readonly JsonConfigDescriptor GoogleCredentialsConfigDescriptor;
        private static readonly JsonConfigDescriptor GoogleAppSecretsConfigDescriptor;
        private static readonly JsonConfigDescriptor SpotifyCredentialsConfigDescriptor;
        private static readonly JsonConfigDescriptor VkCredentialsConfigDescriptor;
        private static readonly JsonConfigDescriptor YandexCredentialsConfigDescriptor;

        static ConfigManager()
        {
            DiscordConfigDescriptor = new(DiscordConfigNameKey);
            DbNosqlConfigDescriptor = new(DbNosqlConfigNameKey);
            DbSqlConfigDescriptor = new(DbSqlConfigNameKey);
            DbSqlServiceConfigDescriptor = new(DbSqlServiceConfigNameKey);
            GoogleCredentialsConfigDescriptor = new(GoogleCredentialsConfigNameKey);
            GoogleAppSecretsConfigDescriptor = new(GoogleAppSecretsConfigNameKey);
            SpotifyCredentialsConfigDescriptor = new(SpotifyCredentialsConfigNameKey);
            VkCredentialsConfigDescriptor = new(VkCredentialsConfigNameKey);
            YandexCredentialsConfigDescriptor = new(YandexCredentialsConfigNameKey);
        }

        /// <summary>
        /// Reads JSON on path
        /// </summary>
        /// 
        /// <typeparam name="T">
        /// Return struct type
        /// </typeparam>
        /// 
        /// <param name="descriptor">
        /// JSON file path
        /// </param>
        /// 
        /// <returns>
        /// Deserialized object instance
        /// </returns>
        /// 
        /// <exception cref="InvalidOperationException"></exception>
        private static T ReadConfig<T>(BaseConfigDescriptor descriptor) where T : struct
        {
            string content = string.Empty;

            try
            {
                using FileStream file = GetFileStream(descriptor);
                using StreamReader reader = new(file);
                content = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Cannot read file {descriptor.FullPath}", ex);
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Invalid file content", ex);
            }
        }

        /// <summary>
        /// Reads JSON on path
        /// </summary>
        /// 
        /// <param name="descriptor">
        /// JSON file path
        /// </param>
        /// 
        /// <returns>
        /// <inheritdoc cref="File.OpenRead(string)"/>
        /// </returns>
        /// 
        /// <inheritdoc cref="File.OpenRead(string)" path="/exception"/>
        private static FileStream GetFileStream(BaseConfigDescriptor descriptor)
        {
            return !Directory.Exists(descriptor.Root.Directory)
                ? throw new DirectoryNotFoundException(
                    $"Config directory not found: {descriptor.Root.Directory}")
                : !File.Exists(descriptor.FullPath)
                ? throw new FileNotFoundException("Config file not found", descriptor.FullPath)
                : File.OpenRead(descriptor.FullPath);
        }

        /// <summary>
        /// Reads Discord bot config
        /// </summary>
        /// 
        /// <returns>
        /// Discord bot config
        /// </returns>
        internal static DiscordConfigJSON GetDiscordConfigJSON()
        {
            return ReadConfig<DiscordConfigJSON>(DiscordConfigDescriptor);
        }

        /// <summary>
        /// Reads NoSql database config
        /// </summary>
        /// 
        /// <returns>
        /// NoSql database config
        /// </returns>
        internal static NoSqlDatabaseConfigJSON GetNoSqlDatabaseConfigJSON()
        {
            return ReadConfig<NoSqlDatabaseConfigJSON>(DbNosqlConfigDescriptor);
        }

        /// <summary>
        /// Reads Sql database config
        /// </summary>
        /// 
        /// <returns>
        /// Sql database config
        /// </returns>
        internal static SqlDatabaseConfigJSON GetSqlDatabaseConfigJSON()
        {
            return ReadConfig<SqlDatabaseConfigJSON>(DbSqlConfigDescriptor);
        }

        /// <summary>
        /// Reads Sql service config
        /// </summary>
        /// 
        /// <returns>
        /// Sql service config
        /// </returns>
        internal static SqlServiceConfigJSON GetSqlServiceConfigJSON()
        {
            return ReadConfig<SqlServiceConfigJSON>(DbSqlServiceConfigDescriptor);
        }

        /// <summary>
        /// Reads Google credentials
        /// </summary>
        /// 
        /// <returns>
        /// Google credentials
        /// </returns>
        internal static GoogleCredentialsJSON GetGoogleCredentialsJSON()
        {
            return ReadConfig<GoogleCredentialsJSON>(GoogleCredentialsConfigDescriptor);
        }

        /// <summary>
        /// Reads Google client secret
        /// </summary>
        /// 
        /// <returns>
        /// Google client secret filestream
        /// </returns>
        internal static FileStream GetGoogleClientSecretsFileStream()
        {
            return GetFileStream(GoogleAppSecretsConfigDescriptor);
        }

        /// <summary>
        /// Reads Spotify credentials
        /// </summary>
        /// 
        /// <returns>
        /// Spotify credentials
        /// </returns>
        internal static SpotifyCredentialsJSON GetSpotifyClientSecretsJSON()
        {
            return ReadConfig<SpotifyCredentialsJSON>(SpotifyCredentialsConfigDescriptor);
        }

        /// <summary>
        /// Reads Vk credentials
        /// </summary>
        /// 
        /// <returns>
        /// Vk credentials
        /// </returns>
        internal static VkCredentialsJSON GetVkCredentialsJSON()
        {
            return ReadConfig<VkCredentialsJSON>(VkCredentialsConfigDescriptor);
        }

        /// <summary>
        /// Reads Yandex credentials
        /// </summary>
        /// 
        /// <returns>
        /// Yandex credentials
        /// </returns>
        internal static YandexCredentialsJSON GetYandexCredentialsJSON()
        {
            return ReadConfig<YandexCredentialsJSON>(YandexCredentialsConfigDescriptor);
        }
    }
}
