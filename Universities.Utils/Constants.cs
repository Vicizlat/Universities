namespace Universities.Utils
{
    public static class Constants
    {
        public const string ErrorSettings = "Settings are wrong or incomplete!";
        public const string ImportSeparator = ";";
        public const string ErrorFileRead = "Error reading file {0}. The file is missing or in use by another program.";
        public const string ErrorNoSeparator = "No obvious separator found in header line. Please, use ',' or ';' as separator.";
        public const string ErrorManySeparators = "Detected multiple possible separators in header line. Please, check the uploaded file and use only one separator.";
        public const string ErrorWrongFile = "The file you selected has different columns than expected. Did you select the wrong file?";
        public const string ErrorNoSettingsFile = "Settings file not found. Please, fill in all required information.";
        public const string SuccessImport = "Successfully imported {0} {1} from {2}.";
        public const string SuccessExport = "Successfully exported all {0} to {1}.";
        public const string FailExport = "Failed to export {0} to {1}.";
        public const string InfDifferentColums = "Line {0} has different number of columns than the header line.";
        public const string InfClearTable = "This will completly clear the {0} table for {1}!";
        public const string InfDeleteMainOrg = "This will delete Main Organization '{0}' and all tables associated with it!";
        public const string InfDeleteSelected = "This will permanently delete all selected items from {0}!";
        public const string QuestionAreYouSure = "Are you sure you want to do that?";
        public const string QuestionImportContinue = "Do you want to skip this line and continue with the import?";
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
            "FormerInstitution",
            "Added_By_User",
            "Added_On_Date"
        };
        public static readonly string[] ExportAcadPersonnelHeader =
        {
            "FirstNames",
            "LastNames",
            "Faculty",
            "Department",
            "AuthorId",
            "Notes",
            "Comments"
        };
        public static readonly string[] ExportDocumentsHeader =
        {
            "Accession Number (UT)",
            "Full Address",
            "Organisation names (concatenated)",
            "1st Enhanced Organisation name",
            "2nd Enhanced Organisation name",
            "3rd Enhanced Organisation name",
            "Enhanced Organisation Names (concatenated)",
            "Sub-organisation names (concatenated)",
            "Full Name",
            "Last Name",
            "Display Name",
            "WOS Standard Name",
            "First Name",
            "Preferred Full Name",
            "Preferred Last Name",
            "Preferred First Name",
            "AssignedToUser",
            "Processed"
        };
    }
}