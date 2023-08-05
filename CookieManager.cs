using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System.Net;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace DicordNET
{
    [SupportedOSPlatform("windows")]
    internal sealed class CookieManager
    {
        internal string CookieFilePath { get; init; }
        internal string KeyFilePath { get; init; }

        internal CookieManager(string CookieFilePath, string KeyFilePath)
        {
            this.CookieFilePath = CookieFilePath;
            this.KeyFilePath = KeyFilePath;
        }

        public List<Cookie> GetCookies(string hostname)
        {
            List<Cookie> data = new();
            if (File.Exists(CookieFilePath))
            {
                try
                {
                    using SqliteConnection conn = new($"Data Source={CookieFilePath}");
                    using SqliteCommand cmd = conn.CreateCommand();
                    cmd.CommandText = $"SELECT name,encrypted_value,host_key FROM cookies WHERE host_key = '{hostname}'";
                    byte[] key = AesGcm256.GetKey(KeyFilePath);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!data.Any(a => a.Name == reader.GetString(0)))
                            {
                                byte[] encryptedData = GetBytes(reader, 1);
                                AesGcm256.Prepare(encryptedData, out byte[] nonce, out byte[] ciphertextTag);
                                string value = AesGcm256.Decrypt(ciphertextTag, key, nonce);

                                data.Add(new Cookie()
                                {
                                    Name = reader.GetString(0),
                                    Value = value,
                                    Domain = reader.GetString(2),
                                    Path = "/"
                                });
                            }
                        }
                    }
                    conn.Close();
                }
                catch { }
            }
            return data;

        }

        private static byte[] GetBytes(SqliteDataReader reader, int columnIndex)
        {
            const int CHUNK_SIZE = 2 * 1024;
            byte[] buffer = new byte[CHUNK_SIZE];
            long bytesRead;
            long fieldOffset = 0;
            using MemoryStream stream = new();
            while ((bytesRead = reader.GetBytes(columnIndex, fieldOffset, buffer, 0, buffer.Length)) > 0)
            {
                stream.Write(buffer, 0, (int)bytesRead);
                fieldOffset += bytesRead;
            }
            return stream.ToArray();
        }

        [SupportedOSPlatform("windows")]
        private class AesGcm256
        {
            public static byte[] GetKey(string KeyFilePath)
            {
                string v = File.ReadAllText(KeyFilePath);

#pragma warning disable CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.
                dynamic json = JsonConvert.DeserializeObject(v);
#pragma warning restore CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.

                if (json == null)
                {
                    throw new ArgumentNullException(nameof(json));
                }

                string key = json.os_crypt.encrypted_key;

                byte[] src = Convert.FromBase64String(key);
                byte[] encryptedKey = src.Skip(5).ToArray();

                byte[] decryptedKey = ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.CurrentUser);

                return decryptedKey;
            }

            public static string Decrypt(byte[] encryptedBytes, byte[] key, byte[] iv)
            {
                string sR = string.Empty;
                try
                {
                    GcmBlockCipher cipher = new(new AesEngine());
                    AeadParameters parameters = new(new KeyParameter(key), 128, iv, null);

                    cipher.Init(false, parameters);
                    byte[] plainBytes = new byte[cipher.GetOutputSize(encryptedBytes.Length)];
                    int retLen = cipher.ProcessBytes(encryptedBytes, 0, encryptedBytes.Length, plainBytes, 0);
                    cipher.DoFinal(plainBytes, retLen);

                    sR = Encoding.UTF8.GetString(plainBytes).TrimEnd("\r\n\0".ToCharArray());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }

                return sR;
            }

            public static void Prepare(byte[] encryptedData, out byte[] nonce, out byte[] ciphertextTag)
            {
                nonce = new byte[12];
                ciphertextTag = new byte[encryptedData.Length - 3 - nonce.Length];

                Array.Copy(encryptedData, 3, nonce, 0, nonce.Length);
                Array.Copy(encryptedData, 3 + nonce.Length, ciphertextTag, 0, ciphertextTag.Length);
            }
        }
    }
}
