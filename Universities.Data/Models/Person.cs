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
        public int? AuthorId { get; set; }
        public string EmailAddress { get; set; }
        public string OtherNames { get; set; }
        public string FormerInstitution { get; set; }

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
            AuthorId = int.TryParse(lineArr[5], out int authorId) ? authorId : null;
            EmailAddress = lineArr[6];
            OtherNames = lineArr[7];
            FormerInstitution = lineArr[8];
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
                $"{AuthorId}",
                EmailAddress,
                OtherNames,
                FormerInstitution,
                $"{Id}"
            };
        }

        public string ToExportString()
        {
            return string.Join(Settings.Instance.Separator, ToArray().SkipLast(1));
        }

        public override string ToString()
        {
            return string.Join(Settings.Instance.Separator, ToArray());
        }
    }
}