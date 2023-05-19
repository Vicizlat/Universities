﻿using System;
using System.Linq;
using Universities.Data;
using Universities.Data.Models;
using Universities.Utils;

namespace Universities.Controller
{
    public static class DBAccess
    {
        public static event EventHandler? OnPeopleChanged;
        public static event EventHandler? OnDocumentsChanged;
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

        public static bool EditPersonId(string[] personArr, int newPersonId)
        {
            if (Context == null) return false;
            Person? person = Context.People.FirstOrDefault(p => p.Id == int.Parse(personArr[9]));
            if (person == null) return false;
            person.PersonId = newPersonId;
            Context.SaveChanges();
            OnPeopleChanged?.Invoke(person, EventArgs.Empty);
            return true;
        }

        public static bool EditPersonOrgId(string[] personArr, int newOrgId)
        {
            if (Context == null) return false;
            Person? person = Context.People.FirstOrDefault(p => p.Id == int.Parse(personArr[9]));
            if (person == null) return false;
            person.OrgId = newOrgId;
            Context.SaveChanges();
            OnPeopleChanged?.Invoke(person, EventArgs.Empty);
            return true;
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
            Logging.Instance.WriteLine(person.ToExportString(), true);
            OnPeopleChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void DeleteDocument(string[] docArr)
        {
            if (Context == null) return;
            Document? doc = Context.Documents.FirstOrDefault(d => d.Ut == docArr[0] && d.FirstName == docArr[20] && d.LastName == docArr[17]);
            if (doc == null) return;
            Context.Documents.Remove(doc);
            Context.SaveChanges();
            Logging.Instance.WriteLine(doc.ToString(), true);
            OnDocumentsChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}