using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Universities.Data.Models;
using Universities.Utils;

namespace Universities.Controller
{
    public class MainController
    {
        public event EventHandler? OnDocumentsChanged;
        public event EventHandler? OnOrganizationsChanged;
        public event EventHandler? OnPeopleChanged;
        public List<Document> Documents { get; set; }
        public List<DuplicateDocument> DuplicateDocuments { get; set; }
        public List<IncompleteDocument> IncompleteDocuments { get; set; }
        public List<Organization> Organizations { get; set; }
        public List<string> OrganizationsDisplayNames { get; set; }
        public List<Person> People { get; set; }

        public MainController()
        {
            CollectionSorter.Controller = this;
            ImportExport.Controller = this;
            //Context.Database.EnsureDeleted();
            //Context.Database.EnsureCreated();
            UpdateDocuments();
            UpdateOrganizations();
            UpdatePeople();
            if (SqlCommands.CurrentUser.Item2)
            {
                DuplicateDocuments = DBAccess.Context.DuplicateDocuments.ToList();
                IncompleteDocuments = DBAccess.Context.IncompleteDocuments.ToList();
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
                DBAccess.Context.IncompleteDocuments.Add(emptyDoc);
                DBAccess.Context.SaveChanges();
            }
            else if (Documents.Any(d => d.Ut == docArr[0] && d.FirstName == docArr[20] && d.LastName == docArr[17]))
            {
                DuplicateDocument dupDoc = new DuplicateDocument(docArr);
                DuplicateDocuments.Add(dupDoc);
                DBAccess.Context.DuplicateDocuments.Add(dupDoc);
                DBAccess.Context.SaveChanges();
            }
            else
            {
                Document doc = new Document(docArr);
                Documents.Add(doc);
                DBAccess.Context.Documents.Add(doc);
                DBAccess.Context.SaveChanges();
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
            AddPerson((string[])sender);
        }

        public void UpdateDocuments()
        {
            Documents = DBAccess.Context.Documents.Where(d => d.AssignedToUser == SqlCommands.CurrentUser.Item1 || SqlCommands.CurrentUser.Item1 == "root").Where(d => !d.Processed).ToList();
            OnDocumentsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateOrganizations()
        {
            Organizations = DBAccess.Context.Organizations.ToList();
            OrganizationsDisplayNames = Organizations.Select(o => o.GetDisplayName(Organizations)).ToList();
            OnOrganizationsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdatePeople()
        {
            People = DBAccess.Context.People.ToList();
            OnPeopleChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateDocument(string[] doc, object updateData)
        {
            Document? document = DBAccess.Context.Documents.FirstOrDefault(d => d.Ut == doc[0] && d.FirstName == doc[20] && d.LastName == doc[17]);
            if (document != null)
            {
                if (updateData is bool) document.Processed = (bool)updateData;
                if (updateData is string) document.AssignedToUser = (string)updateData;
                DBAccess.Context.SaveChanges();
                OnDocumentsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Organization? GetOrganizationByIndex(int index)
        {
            return index < Organizations.Count ? Organizations[index] : null;
        }

        public void AddOrganization(string[] orgArr)
        {
            try
            {
                DBAccess.Context.Organizations.Add(new Organization(orgArr));
                DBAccess.Context.SaveChanges();
                UpdateOrganizations();
            }
            catch
            {
                PromptBox.Error($"Failed to Add Organization: {orgArr[1]}!");
            }
        }

        public int GetPersonId(string firstName, string lastName, int orgId)
        {
            Person? findPerson = DBAccess.Context.People.FirstOrDefault(p => p.FirstName == firstName && p.LastName == lastName && p.OrgId == orgId);
            if (findPerson == null)
            {
                if (!DBAccess.Context.People.Any()) return Settings.Instance.PeopleStartId;
                else return DBAccess.Context.People.OrderBy(p => p.PersonId).Last().PersonId + 1;
            }
            else return findPerson.PersonId;
        }

        public void AddPerson(string[] personArray)
        {
            try
            {
                DBAccess.Context.People.Add(new Person(personArray));
                DBAccess.Context.SaveChanges();
                UpdatePeople();
            }
            catch
            {
                PromptBox.Error($"Failed to Save Person: {personArray[1]} {personArray[2]}!");
            }
        }

        public Person? FindPerson(string firstName, string lastName, string docId)
        {
            return DBAccess.Context.People.FirstOrDefault(p => p.FirstName == firstName && p.LastName == lastName && p.DocId == docId);
        }
    }
}