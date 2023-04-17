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

        public Organization(int id, string name, int? parentId) : this()
        {
            OrganizationId = id;
            OrganizationName = name;
            ParentId = parentId;
        }

        public override string ToString()
        {
            char separator = Settings.Instance.Separator;
            return $"\"{OrganizationId}\"{separator}\"{OrganizationName}\"{separator}\"{ParentId}\"";
        }

        public string[] ToArray()
        {
            return new string[] { $"{OrganizationId}", OrganizationName, $"{ParentId}" };
        }
    }
}