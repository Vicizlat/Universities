using System;
using System.Linq;
using Universities.Data;
using Universities.Data.Models;
using Universities.Utils;

namespace Universities.Controller
{
    public static class DBAccess
    {
        public static event EventHandler? OnDocumentsChanged;
        public static event EventHandler? OnOrganizationsChanged;
        public static event EventHandler? OnPeopleChanged;
        private static UniversitiesContext? context;

        public static UniversitiesContext GetContext(bool forceNewContext = false)
        {
            if (context == null || forceNewContext)
            {
                string database = $"Database={Settings.Instance.Database};";
                string connectionString = $"{SqlCommands.GetConnectionString()}{database}";
                context = new UniversitiesContext(connectionString);
            }
            return context;
        }

        public static Organization? GetOrganization(int? id)
        {
            if (id == null) return null;
            return GetContext().Organizations.FirstOrDefault(o => o.OrganizationId == id);
        }

        public static int? GetOrganizationId(string organizationName)
        {
            return GetContext().Organizations.FirstOrDefault(o => o.OrganizationName == organizationName)?.OrganizationId;
        }

        public static bool EditPersonId(string[] personArr, int newPersonId)
        {
            if (GetContext() == null) return false;
            Person? person = GetContext().People.FirstOrDefault(p => p.Id == int.Parse(personArr[9]));
            if (person == null) return false;
            person.PersonId = newPersonId;
            GetContext().SaveChanges();
            OnPeopleChanged?.Invoke(person, EventArgs.Empty);
            return true;
        }

        public static bool EditPersonOrgId(string[] personArr, int newOrgId)
        {
            if (GetContext() == null) return false;
            Person? person = GetContext().People.FirstOrDefault(p => p.Id == int.Parse(personArr[9]));
            if (person == null) return false;
            person.OrgId = newOrgId;
            GetContext().SaveChanges();
            OnPeopleChanged?.Invoke(person, EventArgs.Empty);
            return true;
        }

        public static int GetNextFreePersonId(int startId)
        {
            if (GetContext() == null) return Settings.Instance.PeopleStartId;
            while (GetContext().People.Any(p => p.PersonId == startId))
            {
                startId++;
            }
            return startId;
        }

        public static void DeleteDocument(string[] docArr)
        {
            if (GetContext() == null) return;
            Document? doc = GetContext().Documents.FirstOrDefault(d => d.Ut == docArr[0] && d.FirstName == docArr[20] && d.LastName == docArr[17]);
            if (doc == null) return;
            GetContext().Documents.Remove(doc);
            GetContext().SaveChanges();
            Logging.Instance.WriteLine(doc.ToString(), true);
            OnDocumentsChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void DeleteOrganization(string[] orgArr)
        {
            if (GetContext() == null) return;
            Organization? org = GetContext().Organizations.FirstOrDefault(o => o.Id == int.Parse(orgArr[3]));
            if (org == null) return;
            GetContext().Organizations.Remove(org);
            GetContext().SaveChanges();
            Logging.Instance.WriteLine(org.ToExportString(), true);
            OnOrganizationsChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void DeletePerson(string[] personArr)
        {
            if (GetContext() == null) return;
            Person? person = GetContext().People.FirstOrDefault(p => p.Id == int.Parse(personArr[9]));
            if (person == null) return;
            GetContext().People.Remove(person);
            GetContext().SaveChanges();
            Logging.Instance.WriteLine(person.ToExportString(), true);
            OnPeopleChanged?.Invoke(null, EventArgs.Empty);
        }

        public static AcadPerson? LastAcadPerson
        {
            get
            {
                if (GetContext() == null) return null;
                return GetContext().AcadPersonnel.OrderBy(p => p.Id).LastOrDefault();
            }
        }

        public static void AddRegexPattern(string pattern)
        {
            GetContext().RegexPatterns.Add(new RegexPattern() { Pattern = pattern });
            GetContext().SaveChanges();
        }
    }
}