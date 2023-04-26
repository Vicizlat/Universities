using System.Collections.Generic;
using System.Data;
using System.Linq;
using Universities.Data.Models;

namespace Universities.Controller
{
    public static class CollectionSorter
    {
        public static MainController Controller;

        public static IEnumerable<Document> GetDocumentsOrderedBy(string sortBy, string direction)
        {
            return sortBy switch
            {
                "Ut" => direction == "Ascending" ? Controller.Documents.OrderBy(d => d.Ut) : Controller.Documents.OrderByDescending(d => d.Ut),
                "City" => direction == "Ascending" ? Controller.Documents.OrderBy(d => d.City) : Controller.Documents.OrderByDescending(d => d.City),
                "Street" => direction == "Ascending" ? Controller.Documents.OrderBy(d => d.Street) : Controller.Documents.OrderByDescending(d => d.Street),
                "AddrNo" => direction == "Ascending" ? Controller.Documents.OrderBy(d => d.Addr_no) : Controller.Documents.OrderByDescending(d => d.Addr_no),
                "OrgaName" => direction == "Ascending" ? Controller.Documents.OrderBy(d => d.OrgaName) : Controller.Documents.OrderByDescending(d => d.OrgaName),
                "OrgaName1" => direction == "Ascending" ? Controller.Documents.OrderBy(d => d.OrgaName1) : Controller.Documents.OrderByDescending(d => d.OrgaName1),
                "SubOrgaName" => direction == "Ascending" ? Controller.Documents.OrderBy(d => d.SubOrgaName) : Controller.Documents.OrderByDescending(d => d.SubOrgaName),
                "SeqNo" => direction == "Ascending" ? Controller.Documents.OrderBy(d => d.SeqNo) : Controller.Documents.OrderByDescending(d => d.SeqNo),
                "LastName" => direction == "Ascending" ? Controller.Documents.OrderBy(d => d.LastName) : Controller.Documents.OrderByDescending(d => d.LastName),
                "FirstName" => direction == "Ascending" ? Controller.Documents.OrderBy(d => d.FirstName) : Controller.Documents.OrderByDescending(d => d.FirstName),
                "AssignedTo" => direction == "Ascending" ? Controller.Documents.OrderBy(d => d.AssignedToUser) : Controller.Documents.OrderByDescending(d => d.AssignedToUser),
                "Processed" => direction == "Ascending" ? Controller.Documents.OrderBy(d => d.Processed) : Controller.Documents.OrderByDescending(d => d.Processed),
                _ => Controller.Documents
            };
        }

        public static IEnumerable<Organization> GetOrganizationsOrderedBy(string sortBy, string direction)
        {
            return sortBy switch
            {
                "OrganizationId" => direction == "Ascending" ? Controller.Organizations.OrderBy(d => d.OrganizationId) : Controller.Organizations.OrderByDescending(d => d.OrganizationId),
                "OrganizationName" => direction == "Ascending" ? Controller.Organizations.OrderBy(d => d.OrganizationName) : Controller.Organizations.OrderByDescending(d => d.OrganizationName),
                "ParentId" => direction == "Ascending" ? Controller.Organizations.OrderBy(d => d.ParentId) : Controller.Organizations.OrderByDescending(d => d.ParentId),
                _ => Controller.Organizations
            };
        }

        public static IEnumerable<Person> GetPeopleOrderedBy(string sortBy, string direction)
        {
            return sortBy switch
            {
                "PersonId" => direction == "Ascending" ? Controller.People.OrderBy(d => d.PersonId) : Controller.People.OrderByDescending(d => d.PersonId),
                "FirstName" => direction == "Ascending" ? Controller.People.OrderBy(d => d.FirstName) : Controller.People.OrderByDescending(d => d.FirstName),
                "LastName" => direction == "Ascending" ? Controller.People.OrderBy(d => d.LastName) : Controller.People.OrderByDescending(d => d.LastName),
                "OrgId" => direction == "Ascending" ? Controller.People.OrderBy(d => d.OrgId) : Controller.People.OrderByDescending(d => d.OrgId),
                "DocId" => direction == "Ascending" ? Controller.People.OrderBy(d => d.DocId) : Controller.People.OrderByDescending(d => d.DocId),
                _ => Controller.People
            };
        }
    }
}