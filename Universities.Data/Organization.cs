using Universities.Utils;

namespace Universities.Models
{
    public class Organization
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public int? ParentId { get; set; }
        public string OrgaNameWithParent { get; set; } = string.Empty;

        public Organization(string[] lineArr, string nameWithParent = "")
        {
            Id = int.Parse(lineArr[0]);
            OrganizationId = int.Parse(lineArr[1]);
            OrganizationName = lineArr[2];
            if (OrganizationName.StartsWith("\"") && OrganizationName.EndsWith("\"") && OrganizationName.Contains("\"\""))
            {
                OrganizationName = OrganizationName.Substring(1, OrganizationName.Length - 2).Replace("\"\"", "\"");
            }
            ParentId = int.TryParse(lineArr[3], out int pId) ? pId : null;
            OrgaNameWithParent = nameWithParent;
        }

        public string[] ToArray()
        {
            return new string[] { $"{Id}", $"{OrganizationId}", OrganizationName, $"{ParentId}", $"{OrgaNameWithParent}" };
        }

        public override string ToString()
        {
            return string.Join(Constants.ImportSeparator, new[] { $"{OrganizationId}", OrganizationName, $"{ParentId}" });
        }
    }
}