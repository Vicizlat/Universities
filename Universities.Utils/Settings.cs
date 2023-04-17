using System;
using System.IO;
using System.Xml.Serialization;

namespace Universities.Utils
{
    public class Settings
    {
        private static Settings thisInstance;
        public static Settings Instance => thisInstance ?? new Settings();
        public string Server { get; set; }
        public string Port { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DataSetFilePath { get; set; }
        public string OrganizationsFilePath { get; set; }
        public string PeopleFilePath { get; set; }
        public char Separator { get; set; }
        private XmlSerializer Serializer { get; }

        public Settings()
        {
            Serializer = new XmlSerializer(typeof(Settings));
            thisInstance = this;
        }

        public bool ReadSettingsFile()
        {
            try
            {
                using TextReader reader = new StreamReader(Constants.SettingsFilePath);
                thisInstance = (Settings)Serializer.Deserialize(reader);
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
                Serializer.Serialize(writer, thisInstance);
                return true;
            }
            catch (Exception e)
            {
                Logging.Instance.WriteLine(e.Message);
                return false;
            }
        }

        public string GetConnectionString()
        {
            if (string.IsNullOrEmpty(Server) || string.IsNullOrEmpty(Port) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password)) return string.Empty;
            return $"Server={Server};Port={Port};User={Username};Password={Password};";
        }
    }
}