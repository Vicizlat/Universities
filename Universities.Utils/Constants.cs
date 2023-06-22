using System;
using System.IO;
using System.Linq;

namespace Universities.Utils
{
    public static class Constants
    {
        public static readonly string WorkingFolder = string.Join('\\', Environment.CurrentDirectory.Split('\\').ToList().SkipLast(1));
        public static readonly string SettingsPath = Directory.CreateDirectory(Path.Combine(WorkingFolder, "Settings")).FullName;
        public static readonly string SettingsFilePath = Path.Combine(SettingsPath, "Settings.xml");
        public static readonly string LogsPath = Directory.CreateDirectory(Path.Combine(WorkingFolder, "Logs")).FullName;
        public static readonly string LogFileName = $"Log-{DateTime.Now:[yyyy-MM-dd][HH-mm-ss]}.txt";
        public static readonly string LogFilePath = Path.Combine(LogsPath, LogFileName);
        public const string ConnectionDetailsWarning = "Some connection information is missing in the Settings file." +
            " Please provide connection details in the Settings window.";
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
            "AuthorID",
            "EmailAddress",
            "OtherNames",
            "FormerInstitution"
        };
        public static readonly string[] ExportAcadPersonnelHeader =
        {
            "FirstNames",
            "LastNames",
            "Faculty",
            "Department",
            "Notes",
            "Comments"
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
            "first_name",
            "AssignedToUser",
            "Processed"
        };
        public enum ExitCodes
        {
            NoErrors = 0,
            NoSettings = 1,
            NoInternet = 2,
            NoDatabase = 3
        };
    }
}