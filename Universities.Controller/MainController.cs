using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using Universities.Data;
using Universities.Data.Models;
using Universities.Handlers;
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
            Context = context;
            CurrentUser = currentUser;
            IsAdmin = isAdmin;
            InstalledVersion = installedVersion;
            //Context.Database.EnsureDeleted();
            //Context.Database.EnsureCreated();
            UpdateDocuments();
            UpdateOrganizations();
            People = GetPeople().ToList();
            if (isAdmin)
            {
                DuplicateDocuments = GetDuplicateDocuments().ToList();
                IncompleteDocuments = GetIncompleteDocuments().ToList();
            }
            DataReader.OnDocumentFound += DataReader_OnDocumentFound;
        }

        public IEnumerable<Person> GetPeople()
        {
            foreach (Person person in Context.People)
            {
                yield return person;
            }
        }

        public IEnumerable<DuplicateDocument> GetDuplicateDocuments()
        {
            foreach (DuplicateDocument dupDoc in Context.DuplicateDocuments)
            {
                yield return dupDoc;
            }
        }

        public IEnumerable<IncompleteDocument> GetIncompleteDocuments()
        {
            foreach (IncompleteDocument emptyDoc in Context.IncompleteDocuments)
            {
                yield return emptyDoc;
            }
        }

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

        public void ImportDataset(string filePath)
        {
            if (DataReader.ReadDataSetFile(filePath, out string message))
            {
                OnDocumentsChanged?.Invoke(this, EventArgs.Empty);
                message = $"Successfully loaded {Documents.Count} Documents.";
            }
            MessageBox.Show(message);
        }

        public void UpdateDocuments()
        {
            Documents = Context.Documents.Where(d => d.AssignedToUser == CurrentUser).Where(d => !d.Processed).ToList();
            OnDocumentsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ImportOrganizations(string filePath)
        {
            Organizations = DataReader.ReadOrganizationsFile(this, filePath, out string message);
            MessageBox.Show(message);
            OnOrganizationsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateOrganizations()
        {
            Organizations = Context.Organizations.ToList();
            OnOrganizationsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ImportPeople(string filePath)
        {
            People = DataReader.ReadPeopleFile(this, filePath, out string message);
            MessageBox.Show(message);
            OnPeopleChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AssignUserToDocuments(string[] doc, string user)
        {
            Document document = Context.Documents.FirstOrDefault(d => d.Ut == doc[0] && d.FirstName == doc[20] && d.LastName == doc[17]);
            if (document != null)
            {
                document.AssignedToUser = user;
                Context.SaveChanges();
            }
        }

        public void SetDocumentProcessedStatus(string[] doc, bool processed)
        {
            Document document = Context.Documents.FirstOrDefault(d => d.Ut == doc[0] && d.FirstName == doc[20] && d.LastName == doc[17]);
            if (document != null)
            {
                document.Processed = processed;
                Context.SaveChanges();
            }
        }

        public string GetOrganizationName(Organization o)
        {
            Organization? parent = Organizations.FirstOrDefault(or => or.OrganizationId == o.ParentId);
            return $"{o.OrganizationName} ({parent?.OrganizationName})";
        }

        public string[] GetDocumentArray(int docId)
        {
            return docId >= 0 && docId < Documents.Count ? Documents[docId].ToArray() : Array.Empty<string>();
        }

        public bool SaveDocuments()
        {
            List<string> exportDocuments = new List<string> { string.Join(Settings.Instance.Separator, Constants.ExportDocumentsHeader) };
            exportDocuments.AddRange(Documents.Select(d => d.ToString()));
            return FileHandler.WriteAllLines(Settings.Instance.DataSetFilePath, exportDocuments);
        }

        public bool AddOrganization(int organizationId, string organizationName, int parentOrganizationId)
        {
            Organizations.Add(new Organization(organizationId, organizationName, parentOrganizationId));
            OnOrganizationsChanged?.Invoke(this, EventArgs.Empty);
            return SaveOrganizations();
        }

        public bool SaveOrganizations()
        {
            List<string> exportOrganizations = new List<string> { string.Join(Settings.Instance.Separator, Constants.ExportOrganizationsHeader) };
            exportOrganizations.AddRange(Organizations.Select(o => o.ToString()));
            return FileHandler.WriteAllLines(Settings.Instance.OrganizationsFilePath, exportOrganizations);
        }

        public int GetPersonId(string firstName, string lastName)
        {
            Person? findPerson = People.Find(p => p.FirstName == firstName && p.LastName == lastName);
            return findPerson?.PersonId ?? (People.Count == 0 ? 1 : People.OrderBy(p => p.PersonId).Last().PersonId + 1);
        }

        public void AddPerson(int personId, string firstName, string lastName, int orgId, string docId, int seqNo)
        {
            string[] lineArr = new string[] { $"{personId}", firstName, lastName, $"{orgId}", docId, $"{seqNo}", string.Empty, string.Empty, string.Empty, string.Empty };
            Person newPerson = new Person(lineArr);
            People.Add(newPerson);
            Context.People.Add(newPerson);
            Context.SaveChanges();
            OnDocumentsChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool ExportPeople()
        {
            List<string> exportPeople = new List<string> { string.Join(Settings.Instance.Separator, Constants.ExportPeopleHeader) };
            exportPeople.AddRange(People.Select(p => p.ToString()));
            if (string.IsNullOrEmpty(Settings.Instance.PeopleFilePath))
            {
                if (!FileDialogHandler.ShowSaveFileDialog("Save People", out string filePath)) return false;
                Settings.Instance.PeopleFilePath = filePath;
            }
            return FileHandler.WriteAllLines(Settings.Instance.PeopleFilePath, exportPeople);
        }
    }
}