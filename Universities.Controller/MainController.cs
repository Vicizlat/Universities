using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Universities.Handlers;
using Universities.Models;
using Universities.Utils;

namespace Universities.Controller
{
    public class MainController
    {
        public event EventHandler? OnDocumentsChanged;
        public event EventHandler? OnOrganizationsChanged;
        public List<DocumentModel> Documents { get; set; }
        public List<OrganizationModel> Organizations { get; set; }
        public List<PersonModel> People { get; set; }
        public string Version;

        public MainController(string version)
        {
            Documents = new List<DocumentModel>();
            Organizations = new List<OrganizationModel>();
            People = new List<PersonModel>();
            Version = version;
        }

        public void LoadFiles()
        {
            StringBuilder sb = new StringBuilder();
            Documents = DataReader.ReadDataSetFile(Settings.Instance.DataSetFilePath, out string message);
            sb.AppendLine(message);
            Organizations = DataReader.ReadOrganizationsFile(Settings.Instance.OrganizationsFilePath, out message);
            sb.AppendLine(message);
            People = DataReader.ReadPeopleFile(Settings.Instance.PeopleFilePath, out message);
            sb.AppendLine(message);
            MessageBox.Show(sb.ToString());
            OnDocumentsChanged?.Invoke(this, EventArgs.Empty);
            OnOrganizationsChanged?.Invoke(this, EventArgs.Empty);
        }

        public string GetOrganizationName(OrganizationModel o)
        {
            OrganizationModel? parent = Organizations.FirstOrDefault(or => or.OrganizationId == o.ParentOrgId);
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
            return docId < 0 || docId >= Documents.Count ? Array.Empty<string>() : Documents[docId].documentArray;
        }

        public bool SaveDocuments()
        {
            List<string> exportDocuments = new List<string> { string.Join(Settings.Instance.Separator, Constants.ExportDocumentsHeader) };
            exportDocuments.AddRange(Documents.Select(d => d.ToString()));
            return FileHandler.WriteAllLines(Settings.Instance.DataSetFilePath, exportDocuments);
        }

        public bool AddOrganization(int organizationId, string organizationName, int parentOrganizationId)
        {
            Organizations.Add(new OrganizationModel(organizationId, organizationName, parentOrganizationId));
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
            PersonModel? findPerson = People.Find(p => p.FirstName == firstName && p.LastName == lastName);
            int id = findPerson?.PersonId ?? (People.Count == 0 ? 1 : People.OrderBy(p => p.PersonId).Last().PersonId + 1);
            string[] personArray = { $"{id}", firstName, lastName, $"{orgId}", docId, $"{seqNo}", null, null, null, null };
            People.Add(new PersonModel(personArray));
            return SavePeople();
        }

        public void FindAndRemovePerson(string fName, string lName, string dId, int seqNo)
        {
            PersonModel? person = People.Find(p => p.FirstName == fName && p.LastName == lName && p.DocId == dId && p.SeqNo == seqNo);
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
            List<PersonModel> shiftedPeople = new List<PersonModel>();
            foreach (PersonModel person in People)
            {
                PersonModel? findPerson = shiftedPeople.Find(p => p.LastPersonId == person.PersonId);
                person.LastPersonId = person.PersonId;
                person.PersonId = findPerson?.PersonId ?? (shiftedPeople.Count == 0 ? newStartingId : shiftedPeople.Last().PersonId + 1);
                shiftedPeople.Add(new PersonModel(person.ToStringArray()) { LastPersonId = person.LastPersonId });
            }
            List<string> exportPeople = new List<string> { string.Join(Settings.Instance.Separator, Constants.ExportPeopleHeader) };
            exportPeople.AddRange(shiftedPeople.Select(p => p.ToString()));
            if (!FileDialogHandler.ShowSaveFileDialog("Save People as new file", out string filePath)) return false;
            return FileHandler.WriteAllLines(filePath, exportPeople);
        }
    }
}