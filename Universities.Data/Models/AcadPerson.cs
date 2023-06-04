using Universities.Utils;

namespace Universities.Data.Models
{
    public class AcadPerson
    {
        public int Id { get; set; }
        public string FirstNames { get; set; }
        public string LastNames { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Notes { get; set; }
        public string Comments { get; set; }

        public AcadPerson()
        {

        }

        public AcadPerson(string[] lineArr) : this()
        {
            FirstNames = lineArr[2];
            LastNames = lineArr[3];
            Faculty = lineArr[4];
            if (Faculty.StartsWith("\"") && Faculty.EndsWith("\"") && Faculty.Contains("\"\""))
            {
                Faculty = Faculty.Substring(1, Faculty.Length - 2).Replace("\"\"", "\"");
            }
            Department = lineArr[5];
            Notes = lineArr[9];
            Comments = lineArr[10];
        }

        public string[] ToArray()
        {
            return new string[]
            {
                string.Join(Settings.Instance.Separator, FirstNames),
                string.Join(Settings.Instance.Separator, LastNames),
                Faculty,
                Department,
                string.Join(Settings.Instance.Separator, Notes),
                string.Join(Settings.Instance.Separator, Comments)
            };
        }

        public override string ToString()
        {
            return string.Join(Settings.Instance.Separator, ToArray());
        }
    }
}