using Universities.Utils;

namespace Universities.Data.Models
{
    public class Person
    {
        public int Id { get; set; }
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

        public Person()
        {

        }

        public Person(string[] lineArr) : this()
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
        }

        public string[] ToArray()
        {
            return new string[]
            {
                $"{PersonId}",
                FirstName,
                LastName,
                $"{OrgId}",
                DocId,
                $"{SeqNo}",
                $"{AuthorId}",
                EmailAddress,
                OtherNames,
                FormerInstitution
            };
        }

        public override string ToString()
        {
            string[] personArray = ToArray();
            string[] pArray = new string[personArray.Length];
            for (int i = 1; i < personArray.Length; i++)
            {
                pArray[i] = $"\"{personArray[i]}\"";
            }
            return string.Join(Settings.Instance.Separator, pArray);
        }
    }
}