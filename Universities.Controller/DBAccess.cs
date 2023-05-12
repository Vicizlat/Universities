using System;
using System.Linq;
using Universities.Data;
using Universities.Data.Models;
using Universities.Utils;

namespace Universities.Controller
{
    public static class DBAccess
    {
        public static event EventHandler? OnPeopleChanged;
        private static UniversitiesContext? context;

        public static UniversitiesContext Context
        {
            get
            {
                if (context == null || !context.Database.CanConnect())
                {
                    string database = $"Database={Settings.Instance.Database};";
                    string connectionString = $"{SqlCommands.GetConnectionString()}{database}";
                    context = new UniversitiesContext(connectionString);
                }
                return context;
            }
        }

        public static Organization? GetOrganization(int id)
        {
            return Context.Organizations.FirstOrDefault(o => o.OrganizationId == id);
        }

        public static int? GetOrganizationId(string organizationName)
        {
            return Context.Organizations.FirstOrDefault(o => o.OrganizationName == organizationName)?.OrganizationId;
        }

        public static void EditPersonId(string[] personArr, int newPersonId)
        {
            if (Context == null) return;
            Person? person = Context.People.FirstOrDefault(p => p.Id == int.Parse(personArr[9]));
            if (person == null) return;
            person.PersonId = newPersonId;
            Context.SaveChanges();
            OnPeopleChanged?.Invoke(person, EventArgs.Empty);
        }

        public static void EditPersonOrgId(string[] personArr, int newOrgId)
        {
            if (Context == null) return;
            Person? person = Context.People.FirstOrDefault(p => p.Id == int.Parse(personArr[9]));
            if (person == null) return;
            person.OrgId = newOrgId;
            Context.SaveChanges();
            OnPeopleChanged?.Invoke(person, EventArgs.Empty);
        }

        public static int GetNextFreePersonId(int startId)
        {
            if (Context == null) return Settings.Instance.PeopleStartId;
            while (Context.People.Any(p => p.PersonId == startId))
            {
                startId++;
            }
            return startId;
        }

        public static void DeletePerson(string[] personArr)
        {
            if (Context == null) return;
            Person? person = Context.People.FirstOrDefault(p => p.Id == int.Parse(personArr[9]));
            if (person == null) return;
            Context.People.Remove(person);
            Context.SaveChanges();
            Logging.Instance.WriteLine(person.ToString(), true);
            OnPeopleChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}