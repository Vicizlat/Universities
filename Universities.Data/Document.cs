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
        public string SeqNo { get; set; }
        public string Full_name { get; set; }
        public string Role { get; set; }
        public string Profile { get; set; }
        public string ReprintContract { get; set; }
        public string Addr_No2 { get; set; }
        public string LastName { get; set; }
        public string Display_name { get; set; }
        public string Wos_standard { get; set; }
        public string AuthorId { get; set; }
        public string FirstName { get; set; }
        public string ResearcherId { get; set; }
        public string OtherResId { get; set; }
        public string ORCID_Id { get; set; }
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
            SeqNo = lineArr[15];
            Full_name = lineArr[16];
            Role = lineArr[17];
            Profile = lineArr[18];
            ReprintContract = lineArr[19];
            Addr_No2 = lineArr[20];
            string displayNameLast = lineArr[22].Contains(',') ? lineArr[22].Split(',')[0] : string.Empty;
            string displayNameFirst = lineArr[22].Contains(',') ? lineArr[22].Split(',')[1] : string.Empty;
            LastName = string.IsNullOrEmpty(lineArr[21]) ? displayNameLast.Trim() : lineArr[21].Trim();
            Display_name = lineArr[22];
            Wos_standard = lineArr[23];
            AuthorId = lineArr[24];
            FirstName = string.IsNullOrEmpty(lineArr[25]) ? displayNameFirst.Trim() : lineArr[25].Trim();
            ResearcherId = lineArr[26];
            OtherResId = lineArr[27];
            ORCID_Id = lineArr[28];
            PrefFullName = lineArr[29];
            PrefLastName = lineArr[30];
            PrefFirstName = lineArr[31];
            AssignedToUser = lineArr[32];
            Processed = lineArr[33] == "1";
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
            SeqNo = lineDict["Researcher/Author SeqNo (position)"] >= 0 ? lineArr[lineDict["Researcher/Author SeqNo (position)"]] : string.Empty;
            Full_name = lineDict["Full Name"] >= 0 ? lineArr[lineDict["Full Name"]] : string.Empty;
            Role = lineDict["Role"] >= 0 ? lineArr[lineDict["Role"]] : string.Empty;
            Profile = lineDict["Claimed Web of Science Researcher Profile"] >= 0 ? lineArr[lineDict["Claimed Web of Science Researcher Profile"]] : string.Empty;
            ReprintContract = lineDict["Reprint contact"] >= 0 ? lineArr[lineDict["Reprint contact"]] : string.Empty;
            Addr_No2 = lineDict["Address No (2)"] >= 0 ? lineArr[lineDict["Address No (2)"]] : string.Empty;
            if (lineDict["Last Name"] >= 0) LastName = lineArr[lineDict["Last Name"]];
            else
            {
                if (lineDict["Display Name"] >= 0 && lineArr[lineDict["Display Name"]].Contains(','))
                {
                    LastName = lineArr[lineDict["Display Name"]].Split(',')[0].Trim();
                }
                else LastName = string.Empty;
            }
            Display_name = lineDict["Display Name"] >= 0 ? lineArr[lineDict["Display Name"]] : string.Empty;
            Wos_standard = lineDict["WOS Standard Name"] >= 0 ? lineArr[lineDict["WOS Standard Name"]] : string.Empty;
            AuthorId = lineDict["Distinct Author ID"] >= 0 ? lineArr[lineDict["Distinct Author ID"]] : string.Empty;
            if (lineDict["First Name"] >= 0) FirstName = lineArr[lineDict["First Name"]];
            else
            {
                if (lineDict["Display Name"] >= 0 && lineArr[lineDict["Display Name"]].Contains(','))
                {
                    FirstName = lineArr[lineDict["Display Name"]].Split(',')[1].Trim();
                }
                else FirstName = string.Empty;
            }
            ResearcherId = lineDict["ResearcherID"] >= 0 ? lineArr[lineDict["ResearcherID"]] : string.Empty;
            OtherResId = lineDict["Other ResearcherID"] >= 0 ? lineArr[lineDict["Other ResearcherID"]] : string.Empty;
            ORCID_Id = lineDict["ORCID ID"] >= 0 ? lineArr[lineDict["ORCID ID"]] : string.Empty;
            PrefFullName = lineDict["Preferred Full Name"] >= 0 ? lineArr[lineDict["Preferred Full Name"]] : string.Empty;
            PrefLastName = lineDict["Preferred Last Name"] >= 0 ? lineArr[lineDict["Preferred Last Name"]] : string.Empty;
            PrefFirstName = lineDict["Preferred First Name"] >= 0 ? lineArr[lineDict["Preferred First Name"]] : string.Empty;
            AssignedToUser = lineDict["AssignedToUser"] >= 0 ? lineArr[lineDict["AssignedToUser"]] : string.Empty;
            Processed = lineDict["Processed"] >= 0 && (lineArr[lineDict["Processed"]] == "1" || lineArr[lineDict["Processed"]].ToLower() == "true");
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
                SeqNo,
                Full_name,
                Role,
                Profile,
                ReprintContract,
                Addr_No2,
                LastName,
                Display_name,
                Wos_standard,
                AuthorId,
                FirstName,
                ResearcherId,
                OtherResId,
                ORCID_Id,
                PrefFullName,
                PrefLastName,
                PrefFirstName,
                AssignedToUser,
                Processed.ToString()
            };
        }

        public override string ToString()
        {
            return string.Join(Settings.Instance.Separator, ToArray().Skip(1));
        }
    }
}