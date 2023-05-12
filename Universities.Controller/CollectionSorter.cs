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
            return direction == "Ascending" ?
                Controller.Documents.OrderBy(d => d.GetType().GetProperty(sortBy)?.GetValue(d)) :
                Controller.Documents.OrderByDescending(d => d.GetType().GetProperty(sortBy)?.GetValue(d));
        }

        public static IEnumerable<Organization> GetOrganizationsOrderedBy(string sortBy, string direction)
        {
            return direction == "Ascending" ?
                Controller.Organizations.OrderBy(o => o.GetType().GetProperty(sortBy)?.GetValue(o)) :
                Controller.Organizations.OrderByDescending(o => o.GetType().GetProperty(sortBy)?.GetValue(o));
        }

        public static IEnumerable<Person> GetPeopleOrderedBy(string sortBy, string direction)
        {
            return direction == "Ascending" ?
                Controller.People.OrderBy(p => p.GetType().GetProperty(sortBy)?.GetValue(p)) :
                Controller.People.OrderByDescending(p => p.GetType().GetProperty(sortBy)?.GetValue(p));
        }
    }
}