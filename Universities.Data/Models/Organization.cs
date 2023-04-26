using Universities.Utils;

namespace Universities.Data.Models
{
    public class Organization
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public int? ParentId { get; set; }

        public Organization()
        {

        }

        public Organization(string[] lineArr) : this()
        {
            OrganizationId = int.Parse(lineArr[0]);
            OrganizationName = lineArr[1];
            ParentId = int.TryParse(lineArr[2], out int pId) ? pId : null;
        }

        public string[] ToArray()
        {
            return new string[] { $"{OrganizationId}", OrganizationName, $"{ParentId}" };
        }

        public override string ToString()
        {
            return string.Join(Settings.Instance.Separator, ToArray());
        }
    }
}