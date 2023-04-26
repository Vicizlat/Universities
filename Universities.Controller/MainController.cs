using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Universities.Data;
using Universities.Data.Models;
using Universities.Utils;

namespace Universities.Controller
{
    public class MainController
    {
        public event EventHandler? OnDocumentsChanged;
        public event EventHandler? OnOrganizationsChanged;
        public event EventHandler? OnPeopleChanged;
        public UniversitiesContext Context { get; }
        public string CurrentUser { get; }
        public bool IsAdmin { get; }
        public string InstalledVersion { get; }
        public List<Document> Documents { get; set; }
        public List<DuplicateDocument> DuplicateDocuments { get; set; }
        public List<IncompleteDocument> IncompleteDocuments { get; set; }
        public List<Organization> Organizations { get; set; }
        public List<Person> People { get; set; }

        public MainController(UniversitiesContext context, string currentUser, bool isAdmin, string installedVersion)
        {
            CollectionSorter.Controller = this;
            SqlCommands.Controller = this;
            ImportExport.Controller = this;
            DBAccess.Controller = this;
            Context = context;
            CurrentUser = currentUser;
            IsAdmin = isAdmin;
            InstalledVersion = installedVersion;
            //Context.Database.EnsureDeleted();
            //Context.Database.EnsureCreated();
            UpdateDocuments();
            UpdateOrganizations();
            UpdatePeople();
            if (isAdmin)
            {
                DuplicateDocuments = Context.DuplicateDocuments.ToList();
                IncompleteDocuments = Context.IncompleteDocuments.ToList();
            }
            DataReader.OnDocumentFound += DataReader_OnDocumentFound;
            DataReader.OnOrganizationFound += DataReader_OnOrganizationFound;
            DataReader.OnPersonFound += DataReader_OnPersonFound;
            ImportExport.OnDocumentsChanged += OnDocumentsChanged_Triggered;
            ImportExport.OnOrganizationsChanged += OnOrganizationsChanged_Triggered;
            ImportExport.OnPeopleChanged += OnPeopleChanged_Triggered;
            DBAccess.OnPeopleChanged += OnPeopleChanged_Triggered;
        }

        private void OnDocumentsChanged_Triggered(object? sender, EventArgs e) => UpdateDocuments();

        private void OnOrganizationsChanged_Triggered(object? sender, EventArgs e) => UpdateOrganizations();

        private void OnPeopleChanged_Triggered(object? sender, EventArgs e) => UpdatePeople();

        private void DataReader_OnDocumentFound(object? sender, EventArgs e)
        {
            if (sender == null) return;
            string[] docArr = (string[])sender;
            if (string.IsNullOrEmpty(docArr[17]))
            {
                IncompleteDocument emptyDoc = new IncompleteDocument(docArr);
                IncompleteDocuments.Add(emptyDoc);
                Context.IncompleteDocuments.Add(emptyDoc);
                Context.SaveChanges();
            }
            else if (Documents.Any(d => d.Ut == docArr[0] && d.FirstName == docArr[20] && d.LastName == docArr[17]))
            {
                DuplicateDocument dupDoc = new DuplicateDocument(docArr);
                DuplicateDocuments.Add(dupDoc);
                Context.DuplicateDocuments.Add(dupDoc);
                Context.SaveChanges();
            }
            else
            {
                Document doc = new Document(docArr);
                Documents.Add(doc);
                Context.Documents.Add(doc);
                Context.SaveChanges();
            }
        }

        private void DataReader_OnOrganizationFound(object? sender, EventArgs e)
        {
            if (sender == null) return;
            AddOrganization((string[])sender);
        }

        private void DataReader_OnPersonFound(object? sender, EventArgs e)
        {
            if (sender == null) return;
            string[] personArr = (string[])sender;
            Person person = new Person(personArr);
            People.Add(person);
            Context.People.Add(person);
            Context.SaveChanges();
        }

        public void UpdateDocuments()
        {
            Documents = Context.Documents.Where(d => d.AssignedToUser == CurrentUser || CurrentUser == "root").Where(d => !d.Processed).ToList();
            OnDocumentsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateOrganizations()
        {
            Organizations = Context.Organizations.ToList();
            OnOrganizationsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdatePeople()
        {
            People = Context.People.ToList();
            OnPeopleChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateDocument(string[] doc, object updateData)
        {
            Document? document = Context.Documents.FirstOrDefault(d => d.Ut == doc[0] && d.FirstName == doc[20] && d.LastName == doc[17]);
            if (document != null)
            {
                if (updateData is bool) document.Processed = (bool)updateData;
                if (updateData is string) document.AssignedToUser = (string)updateData;
                Context.SaveChanges();
                OnDocumentsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string GetOrganizationDisplayName(int orgId)
        {
            Organization? org = Organizations.FirstOrDefault(o => o.OrganizationId == orgId);
            Organization? parent = Organizations.FirstOrDefault(o => o.OrganizationId == org?.ParentId);
            return $"{org?.OrganizationName} ({parent?.OrganizationName})";
        }

        public string GetOrganizationName(int index)
        {
            return ((Organization?)Organizations[index])?.OrganizationName ?? string.Empty;
        }

        public void AddOrganization(string[] orgArr)
        {
            try
            {
                Organization newOrganization = new Organization(orgArr);
                Organizations.Add(newOrganization);
                Context.Organizations.Add(newOrganization);
                Context.SaveChanges();
                OnOrganizationsChanged?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                PromptBox.Error($"Failed to Add Organization: {orgArr[1]}!");
            }
        }

        public int? GetOrganizationId(string organizationName)
        {
            return Context.Organizations.FirstOrDefault(o => o.OrganizationName == organizationName)?.OrganizationId;
        }

        public int GetPersonId(string firstName, string lastName, int orgId)
        {
            Person? findPerson = Context.People.FirstOrDefault(p => p.FirstName == firstName && p.LastName == lastName && p.OrgId == orgId);
            return findPerson?.PersonId ?? (Context.People.Count() == 0 ? 1 : Context.People.OrderBy(p => p.PersonId).Last().PersonId + 1);
        }

        public void AddPerson(string[] personArray)
        {
            try
            {
                Person newPerson = new Person(personArray);
                People.Add(newPerson);
                Context.People.Add(newPerson);
                Context.SaveChanges();
                OnPeopleChanged?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                PromptBox.Error($"Failed to Save Person: {personArray[1]} {personArray[2]}!");
            }
        }

        public Person? FindPerson(string firstName, string lastName, string docId)
        {
            return Context.People.FirstOrDefault(p => p.FirstName == firstName && p.LastName == lastName && p.DocId == docId);
        }
    }

    public static class DBAccess
    {
        public static MainController? Controller;
        public static event EventHandler? OnPeopleChanged;

        public static void EditPersonId(string[] personArr)
        {
            if (Controller == null) return;
            Person? person = Controller.FindPerson(personArr[1], personArr[2], personArr[4]);
            if (person == null) return;
            person.PersonId = int.Parse(personArr[0]);
            //Controller.Context.SaveChanges();
            //OnPeopleChanged?.Invoke(personArr[0], EventArgs.Empty);
        }

        public static int GetNextFreePersonId(int startId)
        {
            if (Controller == null) return 0;
            while (Controller.Context.People.Any(p => p.PersonId == startId))
            {
                startId++;
            }
            return startId;
        }

        public static void DeletePerson(string[] personArr)
        {
            if (Controller == null) return;
            Person? person = Controller.Context.People.FirstOrDefault(p => p.Id == int.Parse(personArr[9]));
            if (person == null) return;
            Controller.Context.People.Remove(person);
            Controller.Context.SaveChanges();
            Logging.Instance.WriteLine(person.ToString(), true);
            OnPeopleChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}