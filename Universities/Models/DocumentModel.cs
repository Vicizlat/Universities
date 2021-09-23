using System;
using Universities.Utils;

namespace Universities.Models
{
    public class DocumentModel
    {
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
        public int SeqNo { get; set; }
        public string Full_name { get; set; }
        public string Role { get; set; }
        public string LastName { get; set; }
        public string Display_name { get; set; }
        public string Wos_standard { get; set; }
        public string FirstName { get; set; }
        private readonly string[] documentArray = new string[21];

        public DocumentModel(string[] lineArr)
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
            SeqNo = int.Parse(lineArr[14]);
            Full_name = lineArr[15];
            Role = lineArr[16];
            LastName = lineArr[17];
            Display_name = lineArr[18];
            Wos_standard = lineArr[19];
            FirstName = string.IsNullOrEmpty(lineArr[20]) ? lineArr[19].Split(',')[1] : lineArr[20];
            Array.Copy(lineArr, documentArray, 20);
            documentArray[20] = FirstName;
            for (int index = 0; index < documentArray.Length; index++)
            {
                documentArray[index] = $"\"{documentArray[index]}\"";
            }
        }

        public override string ToString() => string.Join(Settings.Instance.Separator, documentArray);
    }
}