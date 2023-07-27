using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Universities.Data.Models;
using Universities.Utils;

namespace Universities.Controller
{
    public class MainController
    {
        public event EventHandler? OnDocumentsChanged;
        public event EventHandler? OnOrganizationsChanged;
        public event EventHandler? OnPeopleChanged;
        public event EventHandler? OnAcadPersonnelChanged;
        public List<Document> Documents { get; set; } = new List<Document> ();
        public List<DuplicateDocument> DuplicateDocuments { get; set; } = new List<DuplicateDocument>();
        public List<IncompleteDocument> IncompleteDocuments { get; set; } = new List<IncompleteDocument>();
        public List<Organization> Organizations { get; set; } = new List<Organization>();
        public List<string> OrganizationsDisplayNames { get; set; } = new List<string>();
        public List<Person> People { get; set; } = new List<Person>();
        public List<AcadPerson> AcadPersonnel { get; set; } = new List<AcadPerson>();

        public MainController()
        {
            CollectionSorter.Controller = this;
            ImportExport.Controller = this;
            Settings.Instance.RegexPattern = string.Join("|", DBAccess.GetContext().RegexPatterns.Select(rp => rp.Pattern));
            Settings.Instance.WriteSettingsFile();
            UpdateDocuments();
            UpdateOrganizations();
            UpdatePeople();
            AcadPersonnel = DBAccess.GetContext().AcadPersonnel.ToList();
            if (SqlCommands.CurrentUser.Item2)
            {
                DuplicateDocuments = DBAccess.GetContext().DuplicateDocuments.ToList();
                IncompleteDocuments = DBAccess.GetContext().IncompleteDocuments.ToList();
            }
            DataReader.OnDocumentFound += DataReader_OnDocumentFound;
            DataReader.OnOrganizationFound += DataReader_OnOrganizationFound;
            DataReader.OnPersonFound += DataReader_OnPersonFound;
            DataReader.OnAcadPersonFound += DataReader_OnAcadPersonFound; ;
            ImportExport.OnDocumentsChanged += OnDocumentsChanged_Triggered;
            ImportExport.OnOrganizationsChanged += OnOrganizationsChanged_Triggered;
            ImportExport.OnPeopleChanged += OnPeopleChanged_Triggered;
            DBAccess.OnDocumentsChanged -= OnDocumentsChanged_Triggered;
            DBAccess.OnOrganizationsChanged -= OnOrganizationsChanged_Triggered;
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
                DBAccess.GetContext().IncompleteDocuments.Add(emptyDoc);
                DBAccess.GetContext().SaveChanges();
            }
            else if (Documents.Any(d => d.Ut == docArr[0] && d.FirstName == docArr[20] && d.LastName == docArr[17]))
            {
                DuplicateDocument dupDoc = new DuplicateDocument(docArr);
                DuplicateDocuments.Add(dupDoc);
                DBAccess.GetContext().DuplicateDocuments.Add(dupDoc);
                DBAccess.GetContext().SaveChanges();
            }
            else
            {
                Document doc = new Document(docArr);
                Documents.Add(doc);
                DBAccess.GetContext().Documents.Add(doc);
                DBAccess.GetContext().SaveChanges();
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

        private void DataReader_OnAcadPersonFound(object? sender, EventArgs e)
        {
            if (sender == null) return;
            AddAcadPerson((string[])sender);
            OnAcadPersonnelChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool UpdateDocument(string[] doc, object updateData)
        {
            Document? document = DBAccess.GetContext().Documents.FirstOrDefault(d => d.Ut == doc[0] && d.FirstName == doc[20] && d.LastName == doc[17]);
            if (document != null)
            {
                if (updateData is bool) document.Processed = (bool)updateData;
                if (updateData is string) document.AssignedToUser = (string)updateData;
                DBAccess.GetContext().SaveChanges();
                OnDocumentsChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public void UpdateDocuments()
        {
            string user = SqlCommands.CurrentUser.Item1;
            bool isRoot = user == "root";
            Documents = DBAccess.GetContext().Documents.Where(d => d.AssignedToUser == user || isRoot).Where(d => !d.Processed).ToList();
            OnDocumentsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateOrganizations()
        {
            Organizations = DBAccess.GetContext().Organizations.ToList();
            OrganizationsDisplayNames = Organizations.Select(o => o.GetDisplayName(Organizations)).ToList();
            OnOrganizationsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdatePeople()
        {
            People = DBAccess.GetContext().People.ToList();
            OnPeopleChanged?.Invoke(this, EventArgs.Empty);
        }

        public Organization? GetOrganizationByIndex(int index)
        {
            return index < Organizations.Count ? Organizations[index] : null;
        }

        public void AddOrganization(string[] orgArr)
        {
            try
            {
                DBAccess.GetContext().Organizations.Add(new Organization(orgArr));
                DBAccess.GetContext().SaveChanges();
                UpdateOrganizations();
            }
            catch
            {
                PromptBox.Error($"Failed to add Organization: {orgArr[1]}!");
            }
        }

        public int GetPersonId(string firstName, string lastName, int orgId)
        {
            Person? findPerson = DBAccess.GetContext().People.FirstOrDefault(p => p.FirstName == firstName && p.LastName == lastName && p.OrgId == orgId);
            if (findPerson == null)
            {
                if (!DBAccess.GetContext().People.Any()) return Settings.Instance.PeopleStartId;
                else return DBAccess.GetContext().People.OrderBy(p => p.PersonId).Last().PersonId + 1;
            }
            else return findPerson.PersonId;
        }

        public void AddPerson(string[] personArray)
        {
            try
            {
                DBAccess.GetContext().People.Add(new Person(personArray));
                DBAccess.GetContext().SaveChanges();
                UpdatePeople();
            }
            catch
            {
                PromptBox.Error($"Failed to add Person: {personArray[1]} {personArray[2]}!");
            }
        }

        public void AddAcadPerson(string[] acadPersonArray)
        {
            try
            {
                AcadPerson acadPerson = new AcadPerson(acadPersonArray);
                if (!DBAccess.GetContext().Organizations.Any(o => o.OrganizationName == acadPerson.Faculty))
                {
                    int orgId = DBAccess.GetContext().Organizations.ToList().LastOrDefault()?.OrganizationId + 1 ?? Settings.Instance.OrgaStartId;
                    DataReader_OnOrganizationFound(new string[3] { $"{orgId}", acadPerson.Faculty, $"{GetOrganizationByIndex(0)?.OrganizationId}" }, EventArgs.Empty);
                }
                if (!string.IsNullOrEmpty(acadPerson.Department) && !DBAccess.GetContext().Organizations.Any(o => o.OrganizationName == acadPerson.Department))
                {
                    int orgId = DBAccess.GetContext().Organizations.ToList().LastOrDefault()?.OrganizationId + 1 ?? Settings.Instance.OrgaStartId;
                    int? parOrgId = DBAccess.GetContext().Organizations.FirstOrDefault(o => o.OrganizationName == acadPerson.Faculty)?.OrganizationId;
                    DataReader_OnOrganizationFound(new string[3] { $"{orgId}", acadPerson.Department, $"{parOrgId}" }, EventArgs.Empty);
                }
                DBAccess.GetContext().AcadPersonnel.Add(acadPerson);
                DBAccess.GetContext().SaveChanges();
            }
            catch
            {
                PromptBox.Error($"Failed to add Acad. Person {acadPersonArray[0]} {acadPersonArray[1]}!");
            }
        }

        public Person? FindPerson(string firstName, string lastName, string docId)
        {
            return DBAccess.GetContext().People.FirstOrDefault(p => p.FirstName == firstName && p.LastName == lastName && p.DocId == docId);
        }

        public bool RegexMatch(string text1, string text2)
        {
            Regex regex = new Regex(string.Join("|", Settings.Instance.RegexPattern), RegexOptions.IgnoreCase);
            text1 = regex.Replace(text1, "");
            text2 = regex.Replace(text2, "");
            return text1 == text2;
        }
    }
}