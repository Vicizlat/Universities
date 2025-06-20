using Universities.Utils;

namespace Universities.Models
{
    public class AcadPerson
    {
        public int Id { get; set; }
        public string FirstNames { get; set; }
        public string LastNames { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string AuthorId { get; set; }
        public string Notes { get; set; }
        public string Comments { get; set; }

        public AcadPerson(string[] lineArr)
        {
            FirstNames = lineArr[1];
            LastNames = lineArr[2];
            Faculty = lineArr[3];
            if (Faculty.StartsWith("\"") && Faculty.EndsWith("\"") && Faculty.Contains("\"\""))
            {
                Faculty = Faculty.Substring(1, Faculty.Length - 2).Replace("\"\"", "\"");
            }
            Department = lineArr[4];
            if (Department.StartsWith("\"") && Department.EndsWith("\"") && Department.Contains("\"\""))
            {
                Department = Department.Substring(1, Department.Length - 2).Replace("\"\"", "\"");
            }
            AuthorId = lineArr[5];
            Notes = lineArr[6];
            Comments = lineArr[7];
        }

        public string[] ToArray()
        {
            return new string[]
            {
                FirstNames,
                LastNames,
                Faculty,
                Department,
                AuthorId,
                Notes,
                Comments
            };
        }

        public override string ToString()
        {
            return string.Join(Constants.ImportSeparator, ToArray());
        }
    }
}