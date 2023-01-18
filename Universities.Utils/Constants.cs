using System;
using System.IO;

namespace Universities.Utils
{
    public static class Constants
    {
        private static readonly string WorkingFolder = Environment.CurrentDirectory;
        public static readonly string SettingsPath = Directory.CreateDirectory(Path.Combine(WorkingFolder, "Settings")).FullName;
        public static readonly string SettingsFilePath = Path.Combine(SettingsPath, "Settings.xml");
        public static readonly string LogsPath = Directory.CreateDirectory(Path.Combine(WorkingFolder, "Logs")).FullName;
        public static readonly string LogFileName = $"Log-{DateTime.Now:[yyyy-MM-dd][HH-mm-ss]}.txt";
        public static readonly string LogFilePath = Path.Combine(LogsPath, LogFileName);
        public static readonly char[] Separators =
        {
            ',',
            ';'
        };
        public static readonly string[] ExportOrganizationsHeader =
        {
            "OrganizationID",
            "OrganizationName",
            "ParentOrgaID"
        };
        public static readonly string[] ExportPeopleHeader =
        {
            "PersonID",
            "FirstName",
            "LastName",
            "OrganizationID",
            "DocumentID",
            "seq_No",
            "AuthorID",
            "EmailAddress",
            "OtherNames",
            "FormerInstitution"
        };
        public static readonly string[] ExportDocumentsHeader =
        {
            "UT",
            "country",
            "zipLocation",
            "zipCode",
            "city",
            "street",
            "addr_no",
            "full_address",
            "orgaNameConcatenated",
            "orgaEnhancedName_1",
            "orgaEnhancedName_2",
            "orgaEnhancedName_3",
            "orgaEnhancedName_4",
            "subOrgaNameConcatenated",
            "seq_no",
            "full_name",
            "role",
            "last_name",
            "display_name",
            "wos_standard",
            "first_name"
        };
    }
}