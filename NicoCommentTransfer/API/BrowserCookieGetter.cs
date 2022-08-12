using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;
using System.Windows;

namespace NicoCommentTransfer.API
{
    class BrowserCookieGetter
    {
        public static string[] GetChromeCookie()
        {
            long expiresunixtime = 0;
            string usersession = "";
            string usersessions = "";
            try
            {
                string sqlPath = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\User Data\\Default\\Cookies";
                var sqlSb = new SQLiteConnectionStringBuilder { DataSource = sqlPath };
                byte[] key = GetKey();
                using (var cn = new SQLiteConnection(sqlSb.ToString()))
                {
                    try
                    {
                        cn.Open();
                        SQLiteCommand command = cn.CreateCommand();
                        command.CommandText = "select * from cookies where host_key = '.nicovideo.jp' and name = 'user_session'";
                        SQLiteDataReader sdr = command.ExecuteReader();
                        while (sdr.Read() == true)
                        {
                            if (sdr.HasRows)
                            {
                                byte[] encryptedData = GetBytes(sdr, 12);
                                expiresunixtime = sdr.GetInt64(5) / 1000000 - 11644473600;
                                byte[] nonce, ciphertextTag;
                                Prepare(encryptedData, out nonce, out ciphertextTag);
                                usersession = Decrypt(ciphertextTag, key, nonce);
                            }
                        }
                        sdr.Close();
                        command.CommandText = "select * from cookies where host_key = '.nicovideo.jp' and name = 'user_session_secure'";
                        SQLiteDataReader sdr2 = command.ExecuteReader();
                        while (sdr2.Read() == true)
                        {
                            if (sdr2.HasRows)
                            {
                                byte[] encryptedData = GetBytes(sdr2, 12);
                                byte[] nonce, ciphertextTag;
                                Prepare(encryptedData, out nonce, out ciphertextTag);
                                usersessions = Decrypt(ciphertextTag, key, nonce);
                            }
                        }
                        sdr2.Close();
                    }
                    catch
                    {
                        //
                    }
                    finally
                    {
                        cn.Close();
                    }
                }
            }
            catch
            {
                MessageBox.Show("エラーが発生しました。B75", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            return new string[] { usersession, usersessions, expiresunixtime.ToString() };
        }
        public static string[] GetFirefoxCookie()
        {
            long expiresunixtime = 0;
            string usersession = "";
            string usersessions = "";
            try
            {
                string sqlPath = Directory.GetDirectories(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Mozilla\\Firefox\\Profiles\\", "*release")[0] + "\\cookies.sqlite";
                var sqlSb = new SQLiteConnectionStringBuilder { DataSource = sqlPath };
                using (var cn = new SQLiteConnection(sqlSb.ToString()))
                {
                    cn.Open();
                    SQLiteCommand command = cn.CreateCommand();
                    command.CommandText = "select * from moz_cookies where host = '.nicovideo.jp' and name = 'user_session'";
                    SQLiteDataReader sdr = command.ExecuteReader();
                    while (sdr.Read() == true)
                    {
                        if (sdr.HasRows)
                        {
                            expiresunixtime = sdr.GetInt64(6);
                            usersession = sdr.GetString(3);
                        }
                    }
                    sdr.Close();
                    command.CommandText = "select * from moz_cookies where host = '.nicovideo.jp' and name = 'user_session_secure'";
                    SQLiteDataReader sdr2 = command.ExecuteReader();
                    while (sdr2.Read() == true)
                    {
                        if (sdr2.HasRows)
                        {
                            usersessions = sdr2.GetString(3);
                        }
                    }
                    sdr2.Close();
                }
            }
            catch
            {
                MessageBox.Show("エラーが発生しました。B118", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            return new string[] { usersession, usersessions, expiresunixtime.ToString() };
        }
        public static long ToUnixTime(DateTime dt)
        {
            var dto = new DateTimeOffset(dt.Ticks, new TimeSpan(+09, 00, 00));
            return dto.ToUnixTimeSeconds();
        }
        public static DateTime ConvertUnixToDateTime(long unixtime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixtime).ToLocalTime().DateTime;
        }
        private static byte[] GetBytes(SQLiteDataReader reader, int columnIndex)
        {
            const int CHUNK_SIZE = 2 * 1024;
            byte[] buffer = new byte[CHUNK_SIZE];
            long bytesRead;
            long fieldOffset = 0;
            using (MemoryStream stream = new MemoryStream())
            {
                while ((bytesRead = reader.GetBytes(columnIndex, fieldOffset, buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, (int)bytesRead);
                    fieldOffset += bytesRead;
                }
                return stream.ToArray();
            }
        }
        public static byte[] GetKey()
        {
            // AppDataのパスを取得
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // Local Stateのパスを取得
            var path = Path.GetFullPath(appdata + "\\..\\Local\\Google\\Chrome\\User Data\\Local State");

            // Local StateをJsonとして読み込む
            string v = File.ReadAllText(path);
            dynamic json = JsonConvert.DeserializeObject(v);
            string key = json.os_crypt.encrypted_key;

            // Base64エンコード
            byte[] src = Convert.FromBase64String(key);
            // 文字列「DPAPI」をスキップ
            byte[] encryptedKey = src.Skip(5).ToArray();

            // DPAPIで復号化
            byte[] decryptedKey = ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.CurrentUser);

            return decryptedKey;
        }
        public static void Prepare(byte[] encryptedData, out byte[] nonce, out byte[] ciphertextTag)
        {
            nonce = new byte[12];
            ciphertextTag = new byte[encryptedData.Length - 3 - nonce.Length];

            System.Array.Copy(encryptedData, 3, nonce, 0, nonce.Length);
            System.Array.Copy(encryptedData, 3 + nonce.Length, ciphertextTag, 0, ciphertextTag.Length);
        }
        public static string Decrypt(byte[] encryptedBytes, byte[] key, byte[] iv)
        {
            string sR = "";
            try
            {
#pragma warning disable CS0618 // 型またはメンバーが旧型式です
                GcmBlockCipher cipher = new GcmBlockCipher(new AesFastEngine());
#pragma warning restore CS0618 // 型またはメンバーが旧型式です
                AeadParameters parameters = new AeadParameters(new KeyParameter(key), 128, iv, null);
                cipher.Init(false, parameters);
                byte[] plainBytes = new byte[cipher.GetOutputSize(encryptedBytes.Length)];
                Int32 retLen = cipher.ProcessBytes(encryptedBytes, 0, encryptedBytes.Length, plainBytes, 0);
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
    }
}
