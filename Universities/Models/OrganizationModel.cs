using Universities.Utils;

namespace Universities.Models
{
    public class OrganizationModel
    {
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public int? ParentOrgId { get; set; }

        public OrganizationModel(int organizationId, string organizationName, int? parentOrgId)
        {
            OrganizationId = organizationId;
            OrganizationName = organizationName;
            ParentOrgId = parentOrgId;
        }

        public override string ToString()
        {
            return $"{OrganizationId}{Settings.Instance.Separator}\"{OrganizationName}\"{Settings.Instance.Separator}{ParentOrgId}";
        }
    }
}