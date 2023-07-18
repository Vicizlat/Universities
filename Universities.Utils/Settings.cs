using System;
using System.IO;
using System.Xml.Serialization;

namespace Universities.Utils
{
    public class Settings
    {
        private static Settings thisInstance;
        public static Settings Instance => thisInstance ?? new Settings();
        public string Server { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
        public string Database { get; set; } = "sofia_univ";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Separator { get; set; } = ",";
        public int PeopleStartId { get; set; } = 2000;
        public int OrgaStartId { get; set; } = 1000;
        public int LastDocNo { get; set; } = -1;
        public string RegexPattern { get; set; }
        public bool ShowParentOrganization { get; set; } = false;
        private XmlSerializer Serializer = new XmlSerializer(typeof(Settings));

        public Settings()
        {
            thisInstance = this;
        }

        public bool ReadSettingsFile()
        {
            try
            {
                using TextReader reader = new StreamReader(Constants.SettingsFilePath);
                thisInstance = Serializer.Deserialize(reader) as Settings;
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
                using TextWriter writer = new StreamWriter(Constants.SettingsFilePath);
                Serializer.Serialize(writer, this);
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