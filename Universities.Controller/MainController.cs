using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using Universities.Data;
using Universities.Data.Models;
using Universities.Handlers;
using Universities.Models;
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
        public List<DocumentModel> Documents { get; set; }
        //public List<Document> Documents { get; set; }
        public List<DuplicateDocument> DuplicateDocuments { get; set; }
        public List<IncompleteDocument> IncompleteDocuments { get; set; }
        public List<Organization> Organizations { get; set; }
        public List<Person> People { get; set; }

        public MainController(UniversitiesContext context, string currentUser, bool isAdmin, string installedVersion)
        {
            Context = context;
            CurrentUser = currentUser;
            IsAdmin = isAdmin;
            InstalledVersion = installedVersion;
            //Context.Database.EnsureDeleted();
            //Context.Database.EnsureCreated();
            Documents = new List<DocumentModel>();
            DuplicateDocuments = GetDuplicateDocuments().ToList();
            IncompleteDocuments = new List<IncompleteDocument>();
            Organizations = GetOrganizations().ToList();
            People = GetPeople().ToList();
            DataReader.OnDocumentFound += DataReader_OnDocumentFound;
            DataReader.OnIncompleteDocumentFound += DataReader_OnIncompleteDocumentFound;
        }

        public IEnumerable<DuplicateDocument> GetDuplicateDocuments()
        {
            foreach (DuplicateDocument dupDoc in Context.DuplicateDocuments)
            {
                yield return dupDoc;
            }
        }

        public IEnumerable<Organization> GetOrganizations()
        {
            foreach (Organization organization in Context.Organizations)
            {
                yield return organization;
            }
        }

        public IEnumerable<Person> GetPeople()
        {
            foreach (Person person in Context.People)
            {
                yield return person;
            }
        }

        private void DataReader_OnDocumentFound(object? sender, EventArgs e)
        {
            if (sender == null) return;
            Document doc = new Document((string[])sender);
            if (Documents.Any(d => d.Ut == doc.Ut && d.FirstName == doc.FirstName && d.LastName == doc.LastName))
            {
                DuplicateDocument dupDoc = new DuplicateDocument((string[])sender);
                DuplicateDocuments.Add(dupDoc);
                //Context.DuplicateDocuments.Add(dupDoc);
                //Context.SaveChanges();
            }
            else
            {
                Documents.Add(new DocumentModel((string[])sender));
                //Context.Documents.Add(doc);
                //Context.SaveChanges();
            }
        }

        private void DataReader_OnIncompleteDocumentFound(object? sender, EventArgs e)
        {
            if (sender == null) return;
            IncompleteDocuments.Add(new IncompleteDocument((string[])sender));
            //Context.IncompleteDocuments.Add(new IncompleteDocument(lineArr));
            //Context.SaveChanges();
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

        public void ImportOrganizations(string filePath)
        {
            Organizations = DataReader.ReadOrganizationsFile(this, filePath, out string message);
            MessageBox.Show(message);
            OnOrganizationsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ImportPeople(string filePath)
        {
            People = DataReader.ReadPeopleFile(this, filePath, out string message);
            MessageBox.Show(message);
            OnPeopleChanged?.Invoke(this, EventArgs.Empty);
        }

        public List<string> GetUsers()
        {
            try
            {
                List<string> users = new List<string>();
                using (MySqlConnection Connection = new MySqlConnection(Settings.Instance.GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand("SELECT * FROM mysql.user;", Connection))
                {
                    Connection.Open();
                    MySqlDataReader reader = Command.ExecuteReader();
                    while (reader.Read())
                    {
                        users.Add((string)reader.GetValue("User"));
                    }
                }
                return users;
            }
            catch
            {
                return new List<string>() { "Unable to get Users" };
            }
        }

        public string GetOrganizationName(Organization o)
        {
            Organization? parent = Organizations.FirstOrDefault(or => or.OrganizationId == o.ParentId);
            return $"{o.OrganizationName} ({parent?.OrganizationName})";
        }

        public void AddDocument(int docId, string[] documentArray)
        {
            Documents.Insert(docId, new DocumentModel(documentArray));
        }

        public bool RemoveDocument(int docId)
        {
            DocumentModel? document = docId < 0 || docId >= Documents.Count ? null : Documents[docId];
            return document != null && Documents.Remove(document);
        }

        public string[] GetDocumentArray(int docId)
        {
            return docId < 0 || docId >= Documents.Count ? Array.Empty<string>() : Documents[docId].ToArray();
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

        public bool AddPerson(string firstName, string lastName, int orgId, string docId, int seqNo)
        {
            Person? findPerson = People.Find(p => p.FirstName == firstName && p.LastName == lastName);
            int id = findPerson?.PersonId ?? (People.Count == 0 ? 1 : People.OrderBy(p => p.PersonId).Last().PersonId + 1);
            string[] personArray = { $"{id}", firstName, lastName, $"{orgId}", docId, $"{seqNo}", string.Empty, string.Empty, string.Empty, string.Empty };
            People.Add(new Person(personArray));
            OnPeopleChanged?.Invoke(this, EventArgs.Empty);
            return SavePeople();
        }

        public void FindAndRemovePerson(string fName, string lName, string dId, int seqNo)
        {
            Person? person = People.Find(p => p.FirstName == fName && p.LastName == lName && p.DocId == dId && p.SeqNo == seqNo);
            if (person != null) People.Remove(person);
        }

        public bool SavePeople()
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

        public bool ShiftPeopleIds(int newStartingId)
        {
            List<Person> shiftedPeople = new List<Person>();
            foreach (Person person in People)
            {
                Person? findPerson = shiftedPeople.Find(p => p.LastPersonId == person.PersonId);
                person.LastPersonId = person.PersonId;
                person.PersonId = findPerson?.PersonId ?? (shiftedPeople.Count == 0 ? newStartingId : shiftedPeople.Last().PersonId + 1);
                shiftedPeople.Add(new Person(person.ToArray()) { LastPersonId = person.LastPersonId });
            }
            List<string> exportPeople = new List<string> { string.Join(Settings.Instance.Separator, Constants.ExportPeopleHeader) };
            exportPeople.AddRange(shiftedPeople.Select(p => p.ToString()));
            if (!FileDialogHandler.ShowSaveFileDialog("Save People as new file", out string filePath)) return false;
            return FileHandler.WriteAllLines(filePath, exportPeople);
        }

        public void AddUser(string username, string password, bool isAdmin)
        {
            if (!string.IsNullOrEmpty(CurrentUser) && CurrentUser == "root")
            {
                string command = $"CREATE USER '{username}' IDENTIFIED BY '{password}';";
                if (isAdmin) command += $"GRANT ALL ON *.* TO '{username}';";
                else command += $"GRANT SELECT, INSERT, UPDATE ON *.* TO '{username}';";
                using (MySqlConnection Connection = new MySqlConnection(Settings.Instance.GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand(command, Connection))
                {
                    Connection.Open();
                    Command.ExecuteNonQuery();
                }
                MessageBox.Show($"User '{username}' added successfully!", CurrentUser, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else MessageBox.Show("You don't have permission to add users!", CurrentUser, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public IEnumerable<DocumentModel> GetDocumentsOrderedBy(string sortBy, string direction)
        {
            return sortBy switch
            {
                "Ut" => direction == "Ascending" ? Documents.OrderBy(d => d.Ut) : Documents.OrderByDescending(d => d.Ut),
                "City" => direction == "Ascending" ? Documents.OrderBy(d => d.City) : Documents.OrderByDescending(d => d.City),
                "Street" => direction == "Ascending" ? Documents.OrderBy(d => d.Street) : Documents.OrderByDescending(d => d.Street),
                "AddrNo" => direction == "Ascending" ? Documents.OrderBy(d => d.Addr_no) : Documents.OrderByDescending(d => d.Addr_no),
                "OrgaName" => direction == "Ascending" ? Documents.OrderBy(d => d.OrgaName) : Documents.OrderByDescending(d => d.OrgaName),
                "OrgaName1" => direction == "Ascending" ? Documents.OrderBy(d => d.OrgaName1) : Documents.OrderByDescending(d => d.OrgaName1),
                "SubOrgaName" => direction == "Ascending" ? Documents.OrderBy(d => d.SubOrgaName) : Documents.OrderByDescending(d => d.SubOrgaName),
                "SeqNo" => direction == "Ascending" ? Documents.OrderBy(d => d.SeqNo) : Documents.OrderByDescending(d => d.SeqNo),
                "LastName" => direction == "Ascending" ? Documents.OrderBy(d => d.LastName) : Documents.OrderByDescending(d => d.LastName),
                "FirstName" => direction == "Ascending" ? Documents.OrderBy(d => d.FirstName) : Documents.OrderByDescending(d => d.FirstName),
                _ => Documents
            };
        }

        public IEnumerable<Organization> GetOrganizationsOrderedBy(string sortBy, string direction)
        {
            return sortBy switch
            {
                "OrganizationId" => direction == "Ascending" ? Organizations.OrderBy(d => d.OrganizationId) : Organizations.OrderByDescending(d => d.OrganizationId),
                "OrganizationName" => direction == "Ascending" ? Organizations.OrderBy(d => d.OrganizationName) : Organizations.OrderByDescending(d => d.OrganizationName),
                "ParentId" => direction == "Ascending" ? Organizations.OrderBy(d => d.ParentId) : Organizations.OrderByDescending(d => d.ParentId),
                _ => Organizations
            };
        }

        public IEnumerable<Person> GetPeopleOrderedBy(string sortBy, string direction)
        {
            return sortBy switch
            {
                "PersonId" => direction == "Ascending" ? People.OrderBy(d => d.PersonId) : People.OrderByDescending(d => d.PersonId),
                "FirstName" => direction == "Ascending" ? People.OrderBy(d => d.FirstName) : People.OrderByDescending(d => d.FirstName),
                "LastName" => direction == "Ascending" ? People.OrderBy(d => d.LastName) : People.OrderByDescending(d => d.LastName),
                "OrgId" => direction == "Ascending" ? People.OrderBy(d => d.OrgId) : People.OrderByDescending(d => d.OrgId),
                "DocId" => direction == "Ascending" ? People.OrderBy(d => d.DocId) : People.OrderByDescending(d => d.DocId),
                "SeqNo" => direction == "Ascending" ? People.OrderBy(d => d.SeqNo) : People.OrderByDescending(d => d.SeqNo),
                _ => People
            };
        }
    }
}