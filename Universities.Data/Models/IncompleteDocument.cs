using System.Diagnostics;
using Universities.Utils;

namespace Universities.Data.Models
{
    public class IncompleteDocument
    {
        public int Id { get; set; }
        public string Ut { get; set; }
        public string Country { get; set; }
        public string ZipLocation { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Addr_no { get; set; }
        public string FullAddress { get; set; }
        public string OrgaName { get; set; }
        public string OrgaName1 { get; set; }
        public string OrgaName2 { get; set; }
        public string OrgaName3 { get; set; }
        public string OrgaName4 { get; set; }
        public string SubOrgaName { get; set; }
        public int? SeqNo { get; set; }
        public string Full_name { get; set; }
        public string Role { get; set; }
        public string LastName { get; set; }
        public string Display_name { get; set; }
        public string Wos_standard { get; set; }
        public string FirstName { get; set; }
        public string AssignedToUser { get; set; }
        public bool Processed { get; set; }

        public IncompleteDocument()
        {

        }

        public IncompleteDocument(string[] lineArr) : this()
        {
            Ut = lineArr[0];
            Country = lineArr[1];
            ZipLocation = lineArr[2];
            ZipCode = lineArr[3];
            City = lineArr[4];
            Street = lineArr[5];
            Addr_no = lineArr[6];
            FullAddress = lineArr[7];
            OrgaName = lineArr[8];
            OrgaName1 = lineArr[9];
            OrgaName2 = lineArr[10];
            OrgaName3 = lineArr[11];
            OrgaName4 = lineArr[12];
            SubOrgaName = lineArr[13];
            SeqNo = int.TryParse(lineArr[14], out int seqNo) ? seqNo : null;
            Full_name = lineArr[15];
            Role = lineArr[16];
            LastName = lineArr[17];
            Display_name = lineArr[18];
            Wos_standard = lineArr[19];
            FirstName = string.IsNullOrEmpty(lineArr[20]) ? lineArr[19].Split(',')[1].Trim() : lineArr[20].Trim();
            AssignedToUser = string.Empty;
            Processed = false;
        }

        public string[] ToArray()
        {
            return new string[]
            {
                Ut,
                Country,
                ZipLocation,
                ZipCode,
                City,
                Street,
                Addr_no,
                FullAddress,
                OrgaName,
                OrgaName1,
                OrgaName2,
                OrgaName3,
                OrgaName4,
                SubOrgaName,
                $"{SeqNo}",
                Full_name,
                Role,
                LastName,
                Display_name,
                Wos_standard,
                FirstName,
                AssignedToUser,
                Processed.ToString()
            };
        }

        public override string ToString()
        {
            string[] documentArray = ToArray();
            string[] docArray = new string[documentArray.Length];
            for (int i = 0; i < documentArray.Length; i++)
            {
                docArray[i] = $"\"{documentArray[i]}\"";
            }
            return string.Join(Settings.Instance.Separator, docArray);
        }
    }
}