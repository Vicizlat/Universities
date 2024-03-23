using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Universities.Utils
{
    public class Settings
    {
        private static Settings? thisInstance;
        private readonly XmlSerializer Serializer = new XmlSerializer(typeof(Settings));
        private static readonly string WorkingFolder = string.Join('\\', Environment.CurrentDirectory.Split('\\').ToList().SkipLast(1));
        private static readonly string SettingsPath = Directory.CreateDirectory(Path.Combine(WorkingFolder, "Settings")).FullName;
        [XmlIgnore]
        public static readonly string SettingsFilePath = Path.Combine(SettingsPath, "Settings.xml");
        public static Settings Instance => thisInstance ?? new Settings();
        public string Server { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        [XmlIgnore]
        public string DecryptedPassword { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Separator { get; set; } = ",";
        public string RegexPattern { get; set; } = string.Empty;
        public bool ShowParentOrganization { get; set; } = false;
        public int BackupDaysToKeep { get; set; } = 5;
        public int BackupsPerDayToKeep { get; set; } = 5;

        public Settings()
        {
            thisInstance = this;
        }

        public bool ReadSettingsFile()
        {
            try
            {
                using TextReader reader = new StreamReader(SettingsFilePath);
                thisInstance = Serializer.Deserialize(reader) as Settings;
                Instance.DecryptedPassword = DecryptString(GetCpuId(), Instance.Password);
                return true;
            }
            catch (Exception e)
            {
                Logging.Instance.WriteLine(e.Message);
                return false;
            }
        }

        public bool WriteSettingsFile()
        {
            try
            {
                Instance.Password = EncryptString(GetCpuId(), Instance.DecryptedPassword);
                using TextWriter writer = new StreamWriter(SettingsFilePath);
                Serializer.Serialize(writer, this);
                return true;
            }
            catch (Exception e)
            {
                Logging.Instance.WriteLine(e.Message);
                return false;
            }
        }

        private static string GetCpuId()
        {
            ManagementObjectSearcher mbs = new ManagementObjectSearcher("Select ProcessorID From Win32_processor");
            return mbs.Get().Cast<ManagementObject>().FirstOrDefault(mo => mo != null)?["ProcessorID"].ToString() ?? string.Empty;
        }

        public static string EncryptString(string key, string plainText)
        {
            try
            {
                string result = string.Empty;
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = (new byte[16]);
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    using MemoryStream memoryStream = new MemoryStream();
                    using CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }
                    result = Convert.ToBase64String(memoryStream.ToArray());
                }
                return result;
            }
            catch
            {
                return plainText;
            }
        }

        public static string DecryptString(string key, string cipherText)
        {
            try
            {
                string result = string.Empty;
                byte[] buffer = Convert.FromBase64String(cipherText);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = (new byte[16]);
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using MemoryStream memoryStream = new MemoryStream(buffer);
                    using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                    using StreamReader streamReader = new StreamReader(cryptoStream);
                    result = streamReader.ReadToEnd();
                }
                return result;
            }
            catch
            {
                return cipherText;
            }
        }
    }
}