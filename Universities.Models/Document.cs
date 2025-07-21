using Universities.Utils;

namespace Universities.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Ut { get; set; }
        public string FullAddress { get; set; }
        public string OrgaName { get; set; }
        public string OrgaName1 { get; set; }
        public string OrgaName2 { get; set; }
        public string OrgaName3 { get; set; }
        public string OrgaName4 { get; set; }
        public string SubOrgaName { get; set; }
        public string Full_name { get; set; }
        public string LastName { get; set; }
        public string Display_name { get; set; }
        public string Wos_standard { get; set; }
        public string FirstName { get; set; }
        public string PrefFullName { get; set; }
        public string PrefLastName { get; set; }
        public string PrefFirstName { get; set; }
        public string AssignedToUser { get; set; }
        public bool Processed { get; set; }

        public Document(string[] lineArr)
        {
            Id = int.Parse(lineArr[0]);
            Ut = lineArr[1];
            FullAddress = lineArr[2];
            OrgaName = lineArr[3];
            OrgaName1 = lineArr[4];
            OrgaName2 = lineArr[5];
            OrgaName3 = lineArr[6];
            OrgaName4 = lineArr[7];
            SubOrgaName = lineArr[8];
            Full_name = lineArr[9];
            LastName = lineArr[10];
            Display_name = lineArr[11];
            Wos_standard = lineArr[12];
            FirstName = lineArr[13];
            PrefFullName = lineArr[14];
            PrefLastName = lineArr[15];
            PrefFirstName = lineArr[16];
            AssignedToUser = lineArr[17];
            Processed = lineArr[18] == "1";
            if (string.IsNullOrEmpty(FirstName))
            {
                if (string.IsNullOrEmpty(PrefFirstName)) FirstName = GetNames(Full_name, Display_name, Wos_standard, PrefFullName)[1];
                else FirstName = PrefFirstName;
            }
            if (string.IsNullOrEmpty(LastName))
            {
                if (string.IsNullOrEmpty(PrefFirstName)) FirstName = GetNames(Full_name, Display_name, Wos_standard, PrefFullName)[0];
                else LastName = PrefLastName;
            }
        }

        public Document(Dictionary<string, int> lineDict, string[] lineArr)
        {
            Ut = lineDict["Accession Number (UT)"] >= 0 ? lineArr[lineDict["Accession Number (UT)"]] : string.Empty;
            FullAddress = lineDict["Full Address"] >= 0 ? lineArr[lineDict["Full Address"]] : string.Empty;
            OrgaName = lineDict["Organisation names (concatenated)"] >= 0 ? lineArr[lineDict["Organisation names (concatenated)"]] : string.Empty;
            OrgaName1 = lineDict["1st Enhanced Organisation name"] >= 0 ? lineArr[lineDict["1st Enhanced Organisation name"]] : string.Empty;
            OrgaName2 = lineDict["2nd Enhanced Organisation name"] >= 0 ? lineArr[lineDict["2nd Enhanced Organisation name"]] : string.Empty;
            OrgaName3 = lineDict["3rd Enhanced Organisation name"] >= 0 ? lineArr[lineDict["3rd Enhanced Organisation name"]] : string.Empty;
            OrgaName4 = lineDict["Enhanced Organisation Names (concatenated)"] >= 0 ? lineArr[lineDict["Enhanced Organisation Names (concatenated)"]] : string.Empty;
            SubOrgaName = lineDict["Sub-organisation names (concatenated)"] >= 0 ? lineArr[lineDict["Sub-organisation names (concatenated)"]] : string.Empty;
            Full_name = lineDict["Full Name"] >= 0 ? lineArr[lineDict["Full Name"]] : string.Empty;
            LastName = lineDict["Last Name"] >= 0 ? lineArr[lineDict["Last Name"]] : string.Empty;
            Display_name = lineDict["Display Name"] >= 0 ? lineArr[lineDict["Display Name"]] : string.Empty;
            Wos_standard = lineDict["WOS Standard Name"] >= 0 ? lineArr[lineDict["WOS Standard Name"]] : string.Empty;
            FirstName = lineDict["First Name"] >= 0 ? lineArr[lineDict["First Name"]] : string.Empty;
            PrefFullName = lineDict["Preferred Full Name"] >= 0 ? lineArr[lineDict["Preferred Full Name"]] : string.Empty;
            PrefLastName = lineDict["Preferred Last Name"] >= 0 ? lineArr[lineDict["Preferred Last Name"]] : string.Empty;
            PrefFirstName = lineDict["Preferred First Name"] >= 0 ? lineArr[lineDict["Preferred First Name"]] : string.Empty;
            AssignedToUser = lineDict["AssignedToUser"] >= 0 ? lineArr[lineDict["AssignedToUser"]] : string.Empty;
            Processed = lineDict["Processed"] >= 0 && (lineArr[lineDict["Processed"]] == "1" || lineArr[lineDict["Processed"]].ToLower() == "true");
            if (string.IsNullOrEmpty(FirstName))
            {
                if (string.IsNullOrEmpty(PrefFirstName)) FirstName = GetNames(Full_name, Display_name, Wos_standard, PrefFullName)[1];
                else FirstName = PrefFirstName;
            }
            if (string.IsNullOrEmpty(LastName))
            {
                if (string.IsNullOrEmpty(PrefLastName)) LastName = GetNames(Full_name, Display_name, Wos_standard, PrefFullName)[0];
                else LastName = PrefLastName;
            }
        }

        private string[] GetNames(string fullName, string displName, string wosName, string prefFullName)
        {
            string nameToUse = string.Empty;
            if (!string.IsNullOrEmpty(displName)) nameToUse = displName;
            else if (!string.IsNullOrEmpty(wosName)) nameToUse = wosName;
            else if (!string.IsNullOrEmpty(fullName)) nameToUse = fullName;
            else if (!string.IsNullOrEmpty(prefFullName)) nameToUse = prefFullName;
            else return [string.Empty, string.Empty];
            string[] parts = nameToUse.Split(',');
            if (parts.Length == 2) return [parts[0].Trim(), parts[1].Trim()];
            else if (parts.Length == 1) return [parts[0].Trim(), string.Empty];
            else return [string.Empty, string.Empty];
        }

        public string[] ToArray()
        {
            return
            [
                $"{Id}",
                Ut,
                FullAddress,
                OrgaName,
                OrgaName1,
                OrgaName2,
                OrgaName3,
                OrgaName4,
                SubOrgaName,
                Full_name,
                LastName,
                Display_name,
                Wos_standard,
                FirstName,
                PrefFullName,
                PrefLastName,
                PrefFirstName,
                AssignedToUser,
                Processed.ToString()
            ];
        }

        public Dictionary<string, string> ToDict()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string[] keys = new string[Constants.ExportDocumentsHeader.Length + 1];
            keys[0] = "Id";
            Constants.ExportDocumentsHeader.CopyTo(keys, 1);
            for (int i = 0; i < keys.Length; i++)
            {
                result.Add(keys[i], this.ToArray()[i]);
            }
            return result;
        }

        public override string ToString()
        {
            return string.Join(Constants.ImportSeparator, ToArray().Skip(1));
        }
    }
}