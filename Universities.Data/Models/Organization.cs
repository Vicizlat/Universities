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

        public string GetDisplayName(IEnumerable<Organization> organizations)
        {
            string? parent = organizations.FirstOrDefault(o => o.OrganizationId == ParentId)?.OrganizationName ?? null;
            string parentName = Settings.Instance.ShowParentOrganization && parent != null ? $" ({parent})" : string.Empty;
            return OrganizationName + parentName;
        }

        public string[] ToArray()
        {
            return new string[] { $"{OrganizationId}", OrganizationName, $"{ParentId}", $"{Id}" };
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