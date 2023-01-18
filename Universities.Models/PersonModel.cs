using System;
using Universities.Utils;

namespace Universities.Models
{
    public class PersonModel
    {
        public int PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int OrgId { get; set; }
        public string DocId { get; set; }
        public int SeqNo { get; set; }
        public int? AuthorId { get; set; }
        public string EmailAddress { get; set; }
        public string OtherNames { get; set; }
        public string FormerInstitution { get; set; }
        public int LastPersonId { get; set; }
        private readonly string[] personArray = new string[10];

        public PersonModel(string[] lineArr)
        {
            PersonId = int.Parse(lineArr[0]);
            FirstName = lineArr[1];
            LastName = lineArr[2];
            OrgId = int.Parse(lineArr[3]);
            DocId = lineArr[4];
            SeqNo = int.Parse(lineArr[5]);
            AuthorId = int.TryParse(lineArr[6], out int authorId) ? authorId : null;
            EmailAddress = lineArr[7];
            OtherNames = lineArr[8];
            FormerInstitution = lineArr[9];
            LastPersonId = -1;
            Array.Copy(lineArr, personArray, 10);
            for (int i = 1; i < 5; i++)
            {
                if (i == 3) continue;
                personArray[i] = $"\"{personArray[i]}\"";
            }
        }

        public string[] ToStringArray()
        {
            return new[] { $"{PersonId}", FirstName, LastName, $"{OrgId}", DocId, $"{SeqNo}", null, null, null, null };
        }

        public override string ToString()
        {
            return string.Join(Settings.Instance.Separator, personArray);
        }
    }
}