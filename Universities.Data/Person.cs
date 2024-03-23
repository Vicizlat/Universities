using Universities.Utils;

namespace Universities.Models
{
    public class Person
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int OrgId { get; set; }
        public string DocId { get; set; }
        public string AuthorId { get; set; }
        public string EmailAddress { get; set; }
        public string OtherNames { get; set; }
        public string FormerInstitution { get; set; }

        public Person(string[] lineArr)
        {
            Id = int.Parse(lineArr[0]);
            PersonId = int.Parse(lineArr[1]);
            FirstName = lineArr[2];
            LastName = lineArr[3];
            OrgId = int.Parse(lineArr[4]);
            DocId = lineArr[5];
            AuthorId = lineArr[6];
            EmailAddress = lineArr[7];
            OtherNames = lineArr[8];
            FormerInstitution = lineArr[9];
        }

        public string[] ToArray()
        {
            return new string[]
            {
                $"{Id}",
                $"{PersonId}",
                FirstName,
                LastName,
                $"{OrgId}",
                DocId,
                AuthorId,
                EmailAddress,
                OtherNames,
                FormerInstitution
            };
        }

        public override string ToString()
        {
            return string.Join(Settings.Instance.Separator, ToArray().Skip(1));
        }
    }
}