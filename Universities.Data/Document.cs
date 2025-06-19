using Universities.Utils;

namespace Universities.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Ut { get; set; }
        public string Country { get; set; }
        public string Location { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Addr_No { get; set; }
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
        public string AuthorId { get; set; }
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
            Country = lineArr[2];
            Location = lineArr[3];
            ZipCode = lineArr[4];
            City = lineArr[5];
            Street = lineArr[6];
            Addr_No = lineArr[7];
            FullAddress = lineArr[8];
            OrgaName = lineArr[9];
            OrgaName1 = lineArr[10];
            OrgaName2 = lineArr[11];
            OrgaName3 = lineArr[12];
            OrgaName4 = lineArr[13];
            SubOrgaName = lineArr[14];
            Full_name = lineArr[15];
            LastName = lineArr[16];
            Display_name = lineArr[17];
            Wos_standard = lineArr[18];
            AuthorId = lineArr[19];
            FirstName = lineArr[20];
            PrefFullName = lineArr[21];
            PrefLastName = lineArr[22];
            PrefFirstName = lineArr[23];
            AssignedToUser = lineArr[24];
            Processed = lineArr[25] == "1";
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
            Country = lineDict["Country"] >= 0 ? lineArr[lineDict["Country"]] : string.Empty;
            Location = lineDict["Location"] >= 0 ? lineArr[lineDict["Location"]] : string.Empty;
            ZipCode = lineDict["Zip Code"] >= 0 ? lineArr[lineDict["Zip Code"]] : string.Empty;
            City = lineDict["City"] >= 0 ? lineArr[lineDict["City"]] : string.Empty;
            Street = lineDict["Street"] >= 0 ? lineArr[lineDict["Street"]] : string.Empty;
            Addr_No = lineDict["Address No"] >= 0 ? lineArr[lineDict["Address No"]] : string.Empty;
            FullAddress = lineDict["Full Address"] >= 0 ? lineArr[lineDict["Full Address"]] : string.Empty;
            OrgaName = lineDict["Organisation names (concatenated)"] >= 0 ? lineArr[lineDict["Organisation names (concatenated)"]] : string.Empty;
            OrgaName1 = lineDict["1st Enhanced Organisation name"] >= 0 ? lineArr[lineDict["1st Enhanced Organisation name"]] : string.Empty;
            OrgaName2 = lineDict["2nd Enhanced Organisation name"] >= 0 ? lineArr[lineDict["2nd Enhanced Organisation name"]] : string.Empty;
            OrgaName3 = lineDict["3rd Enhanced Organisation name"] >= 0 ? lineArr[lineDict["3rd Enhanced Organisation name"]] : string.Empty;
            OrgaName4 = lineDict["Enhanced Organisation Names (concatenated)"] >= 0 ? lineArr[lineDict["Enhanced Organisation Names (concatenated)"]] : string.Empty;
            SubOrgaName = lineDict["Sub-organisation names (concatenated)"] >= 0 ? lineArr[lineDict["Sub-organisation names (concatenated)"]] : string.Empty;
            Full_name = lineDict["Full Name"] >= 0 ? lineArr[lineDict["Full Name"]] : string.Empty;
            Display_name = lineDict["Display Name"] >= 0 ? lineArr[lineDict["Display Name"]] : string.Empty;
            Wos_standard = lineDict["WOS Standard Name"] >= 0 ? lineArr[lineDict["WOS Standard Name"]] : string.Empty;
            AuthorId = lineDict["Distinct Author ID"] >= 0 ? lineArr[lineDict["Distinct Author ID"]] : string.Empty;
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
                if (string.IsNullOrEmpty(PrefFirstName)) FirstName = GetNames(Full_name, Display_name, Wos_standard, PrefFullName)[0];
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
            else return new string[] { string.Empty, string.Empty };
            string[] parts = nameToUse.Split(',');
            if (parts.Length == 2) return new string[] { parts[0].Trim(), parts[1].Trim() };
            else if (parts.Length == 1) return new string[] { parts[0].Trim(), string.Empty };
            else return new string[] { string.Empty, string.Empty };
        }

        public string[] ToArray()
        {
            return new string[]
            {
                $"{Id}",
                Ut,
                Country,
                Location,
                ZipCode,
                City,
                Street,
                Addr_No,
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
                AuthorId,
                FirstName,
                PrefFullName,
                PrefLastName,
                PrefFirstName,
                AssignedToUser,
                Processed.ToString()
            };
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