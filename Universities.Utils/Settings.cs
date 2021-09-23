using System;
using System.IO;
using System.Xml.Serialization;

namespace Universities.Utils
{
    public class Settings
    {
        private static Settings thisInstance;
        public static Settings Instance => thisInstance ?? new Settings();
        public string DataSetFilePath { get; set; }
        public string OrganizationsFilePath { get; set; }
        public string PeopleFilePath { get; set; }
        public char Separator { get; set; }
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(Settings));
        private readonly string settingsPath;

        public Settings()
        {
            settingsPath = Path.Combine(Constants.SettingsPath, Constants.SettingsFileName);
            thisInstance = this;
        }

        public bool ReadSettingsFile()
        {
            try
            {
                if (!File.Exists(settingsPath)) return false;
                using TextReader reader = new StreamReader(settingsPath);
                thisInstance = (Settings)Serializer.Deserialize(reader);
                return true;
            }
            catch { return false; }
        }

        public bool WriteSettingsFile()
        {
            try
            {
                using TextWriter writer = new StreamWriter(settingsPath);
                Serializer.Serialize(writer, thisInstance);
                return true;
            }
            catch (Exception e)
            {
                
                Logging.Instance.WriteLine(e.Message);
                return false;
            }
        }
    }
}