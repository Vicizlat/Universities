using System.Collections.Generic;
using System.Data;
using System.Linq;
using Universities.Data.Models;

namespace Universities.Controller
{
    public static class CollectionSorter
    {
        public static IEnumerable<Document> GetDocumentsOrderedBy(MainController controller, string sortBy, string direction)
        {
            return sortBy switch
            {
                "Ut" => direction == "Ascending" ? controller.Documents.OrderBy(d => d.Ut) : controller.Documents.OrderByDescending(d => d.Ut),
                "City" => direction == "Ascending" ? controller.Documents.OrderBy(d => d.City) : controller.Documents.OrderByDescending(d => d.City),
                "Street" => direction == "Ascending" ? controller.Documents.OrderBy(d => d.Street) : controller.Documents.OrderByDescending(d => d.Street),
                "AddrNo" => direction == "Ascending" ? controller.Documents.OrderBy(d => d.Addr_no) : controller.Documents.OrderByDescending(d => d.Addr_no),
                "OrgaName" => direction == "Ascending" ? controller.Documents.OrderBy(d => d.OrgaName) : controller.Documents.OrderByDescending(d => d.OrgaName),
                "OrgaName1" => direction == "Ascending" ? controller.Documents.OrderBy(d => d.OrgaName1) : controller.Documents.OrderByDescending(d => d.OrgaName1),
                "SubOrgaName" => direction == "Ascending" ? controller.Documents.OrderBy(d => d.SubOrgaName) : controller.Documents.OrderByDescending(d => d.SubOrgaName),
                "SeqNo" => direction == "Ascending" ? controller.Documents.OrderBy(d => d.SeqNo) : controller.Documents.OrderByDescending(d => d.SeqNo),
                "LastName" => direction == "Ascending" ? controller.Documents.OrderBy(d => d.LastName) : controller.Documents.OrderByDescending(d => d.LastName),
                "FirstName" => direction == "Ascending" ? controller.Documents.OrderBy(d => d.FirstName) : controller.Documents.OrderByDescending(d => d.FirstName),
                "AssignedTo" => direction == "Ascending" ? controller.Documents.OrderBy(d => d.AssignedToUser) : controller.Documents.OrderByDescending(d => d.AssignedToUser),
                "Processed" => direction == "Ascending" ? controller.Documents.OrderBy(d => d.Processed) : controller.Documents.OrderByDescending(d => d.Processed),
                _ => controller.Documents
            };
        }

        public static IEnumerable<Organization> GetOrganizationsOrderedBy(MainController controller, string sortBy, string direction)
        {
            return sortBy switch
            {
                "OrganizationId" => direction == "Ascending" ? controller.Organizations.OrderBy(d => d.OrganizationId) : controller.Organizations.OrderByDescending(d => d.OrganizationId),
                "OrganizationName" => direction == "Ascending" ? controller.Organizations.OrderBy(d => d.OrganizationName) : controller.Organizations.OrderByDescending(d => d.OrganizationName),
                "ParentId" => direction == "Ascending" ? controller.Organizations.OrderBy(d => d.ParentId) : controller.Organizations.OrderByDescending(d => d.ParentId),
                _ => controller.Organizations
            };
        }

        public static IEnumerable<Person> GetPeopleOrderedBy(MainController controller, string sortBy, string direction)
        {
            return sortBy switch
            {
                "PersonId" => direction == "Ascending" ? controller.People.OrderBy(d => d.PersonId) : controller.People.OrderByDescending(d => d.PersonId),
                "FirstName" => direction == "Ascending" ? controller.People.OrderBy(d => d.FirstName) : controller.People.OrderByDescending(d => d.FirstName),
                "LastName" => direction == "Ascending" ? controller.People.OrderBy(d => d.LastName) : controller.People.OrderByDescending(d => d.LastName),
                "OrgId" => direction == "Ascending" ? controller.People.OrderBy(d => d.OrgId) : controller.People.OrderByDescending(d => d.OrgId),
                "DocId" => direction == "Ascending" ? controller.People.OrderBy(d => d.DocId) : controller.People.OrderByDescending(d => d.DocId),
                "SeqNo" => direction == "Ascending" ? controller.People.OrderBy(d => d.SeqNo) : controller.People.OrderByDescending(d => d.SeqNo),
                _ => controller.People
            };
        }
    }
}