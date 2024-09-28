using MyGreatestBot.ApiClasses.ConfigStructs;
using Newtonsoft.Json;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyGreatestBot.ApiClasses
{
    /// <summary>
    /// JSON reader class
    /// </summary>
    internal static class ConfigManager
    {
        private const string ConfigDirKey = "ConfigDir";
        private const string DiscordConfigNameKey = "DiscordConfigName";
        private const string DbNosqlConfigNameKey = "DbNosqlConfigName";
        private const string DbSqlConfigNameKey = "DbSqlConfigName";
        private const string DbSqlServiceConfigNameKey = "DbSqlServiceConfigName";
        private const string GoogleCredentialsConfigNameKey = "GoogleCredentialsConfigName";
        private const string GoogleAppSecretsConfigNameKey = "GoogleAppSecretsConfigName";
        private const string SpotifyCredentialsConfigNameKey = "SpotifyCredentialsConfigName";
        private const string VkCredentialsConfigNameKey = "VkCredentialsConfigName";
        private const string YandexCredentialsConfigNameKey = "YandexCredentialsConfigName";

        private static readonly Dictionary<string, string> propertiesDictionary;

        private static readonly string ConfigDir;

        private class ConfigDescriptor(string name)
        {
            private const string ConfigExtension = "json";

            public string FullName { get; } = $"{name}.{ConfigExtension}";

            public string Path => System.IO.Path.Combine(ConfigDir, FullName);

            public ConfigDescriptor(Dictionary<string, string> props, string key) : this($"{props[key]}")
            {

            }
        }

        private static readonly ConfigDescriptor DiscordConfigDescriptor;
        private static readonly ConfigDescriptor DbNosqlConfigDescriptor;
        private static readonly ConfigDescriptor DbSqlConfigDescriptor;
        private static readonly ConfigDescriptor DbSqlServiceConfigDescriptor;
        private static readonly ConfigDescriptor GoogleCredentialsConfigDescriptor;
        private static readonly ConfigDescriptor GoogleAppSecretsConfigDescriptor;
        private static readonly ConfigDescriptor SpotifyCredentialsConfigDescriptor;
        private static readonly ConfigDescriptor VkCredentialsConfigDescriptor;
        private static readonly ConfigDescriptor YandexCredentialsConfigDescriptor;

        static ConfigManager()
        {
            if (!BuildPropsProvider.GetProperties(out propertiesDictionary))
            {
                throw BuildPropsProvider.LastError
                    ?? new InvalidOperationException("Cannor get properties");
            }

            ConfigDir = $"{propertiesDictionary[ConfigDirKey]}";

            DiscordConfigDescriptor = new(propertiesDictionary, DiscordConfigNameKey);
            DbNosqlConfigDescriptor = new(propertiesDictionary, DbNosqlConfigNameKey);
            DbSqlConfigDescriptor = new(propertiesDictionary, DbSqlConfigNameKey);
            DbSqlServiceConfigDescriptor = new(propertiesDictionary, DbSqlServiceConfigNameKey);
            GoogleCredentialsConfigDescriptor = new(propertiesDictionary, GoogleCredentialsConfigNameKey);
            GoogleAppSecretsConfigDescriptor = new(propertiesDictionary, GoogleAppSecretsConfigNameKey);
            SpotifyCredentialsConfigDescriptor = new(propertiesDictionary, SpotifyCredentialsConfigNameKey);
            VkCredentialsConfigDescriptor = new(propertiesDictionary, VkCredentialsConfigNameKey);
            YandexCredentialsConfigDescriptor = new(propertiesDictionary, YandexCredentialsConfigNameKey);
        }

        /// <summary>
        /// Reads JSON on path
        /// </summary>
        /// 
        /// <typeparam name="T">
        /// Return struct type
        /// </typeparam>
        /// 
        /// <param name="filepath">
        /// JSON file path
        /// </param>
        /// 
        /// <returns>
        /// Deserialized object instance
        /// </returns>
        /// 
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
            catch (Exception ex)
            {
                throw new InvalidOperationException("Invalid file content", ex);
            }
        }

        /// <summary>
        /// Reads JSON on path
        /// </summary>
        /// 
        /// <param name="filepath">
        /// JSON file path
        /// </param>
        /// 
        /// <returns>
        /// <inheritdoc cref="File.OpenRead(string)"/>
        /// </returns>
        /// 
        /// <inheritdoc cref="File.OpenRead(string)" path="/exception"/>
        private static FileStream GetFileStream(string filepath)
        {
            return !Directory.Exists(ConfigDir)
                ? throw new DirectoryNotFoundException($"Config directory not found: {ConfigDir}")
                : !File.Exists(filepath)
                ? throw new FileNotFoundException("Config file not found", filepath)
                : File.OpenRead(filepath);
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
            return ReadConfig<DiscordConfigJSON>(DiscordConfigDescriptor.Path);
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
            return ReadConfig<NoSqlDatabaseConfigJSON>(DbNosqlConfigDescriptor.Path);
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
            return ReadConfig<SqlDatabaseConfigJSON>(DbSqlConfigDescriptor.Path);
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
            return ReadConfig<SqlServiceConfigJSON>(DbSqlServiceConfigDescriptor.Path);
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
            return ReadConfig<GoogleCredentialsJSON>(GoogleCredentialsConfigDescriptor.Path);
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
            return GetFileStream(GoogleAppSecretsConfigDescriptor.Path);
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
            return ReadConfig<SpotifyCredentialsJSON>(SpotifyCredentialsConfigDescriptor.Path);
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
            return ReadConfig<VkCredentialsJSON>(VkCredentialsConfigDescriptor.Path);
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
            return ReadConfig<YandexCredentialsJSON>(YandexCredentialsConfigDescriptor.Path);
        }
    }
}
