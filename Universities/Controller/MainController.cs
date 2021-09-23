﻿using System;
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
        public event EventHandler OnDocumentsChanged;
        public event EventHandler OnOrganizationsChanged;
        public List<DocumentModel> Documents { get; set; }
        public List<OrganizationModel> Organizations { get; set; }
        public List<PersonModel> People { get; set; }
        private const string Separators = ",;";

        public MainController()
        {
            Documents = new List<DocumentModel>();
            Organizations = new List<OrganizationModel>();
            People = new List<PersonModel>();
            if (FileHandler.FileExists(Constants.SettingsPath, Constants.SettingsFileName) && Settings.Instance.ReadSettingsFile())
            {
                LoadFiles();
            }
            else
            {
                string message = "There are no saved file locations. Do you want to open the Settings window to choose file locations?";
                if (QuestionBox(message, "Question")) ShowSettingsWindow();
            }
        }

        public void LoadFiles()
        {
            StringBuilder sb = new StringBuilder();
            Documents = ReadDataSetFile(Settings.Instance.DataSetFilePath, out string message);
            sb.AppendLine(message);
            Organizations = ReadOrganizationsFile(Settings.Instance.OrganizationsFilePath, out message);
            sb.AppendLine(message);
            People = ReadPeopleFile(Settings.Instance.PeopleFilePath, out message);
            sb.AppendLine(message);
            MessageBox.Show(sb.ToString());
            OnDocumentsChanged?.Invoke(this, EventArgs.Empty);
            OnOrganizationsChanged?.Invoke(this, EventArgs.Empty);
        }

        private List<DocumentModel> ReadDataSetFile(string filePath, out string message)
        {
            try
            {
                if (!FileHandler.ReadAllLines(filePath, out string[] lines))
                {
                    message = $"Error reading file {filePath}. The file is missing or may be in use by another program.";
                    MessageBox.Show(message);
                    return new List<DocumentModel>();
                }
                List<DocumentModel> documents = new List<DocumentModel>();
                List<string> duplicateEntries = new List<string>();
                List<string> emptyEntries = new List<string>();
                foreach (string line in lines)
                {
                    if (line.StartsWith("UT"))
                    {
                        Settings.Instance.Separator = line[2];
                        continue;
                    }
                    string[] lineArr = StringSplit.SkipStrings(line, Separators[0], '\"');
                    if (lineArr.Length < 20) lineArr = StringSplit.SkipStrings(line, Separators[1], '\"');
                    if (string.IsNullOrEmpty(lineArr[14]))
                    {
                        emptyEntries.Add(line);
                        continue;
                    }
                    string firstName = string.IsNullOrEmpty(lineArr[20]) ? lineArr[19].Split(',')[1] : lineArr[20];
                    string lastName = lineArr[17];
                    if (documents.Any(d => d.Ut == lineArr[0] && d.FirstName == firstName && d.LastName == lastName))
                    {
                        duplicateEntries.Add(line);
                        continue;
                    }
                    documents.Add(new DocumentModel(lineArr));
                }
                if (duplicateEntries.Count > 0)
                {
                    Logging.Instance.WriteLine($"Found {duplicateEntries.Count} Duplicate entries.");
                    StringBuilder sb = new StringBuilder($"There are {duplicateEntries.Count} duplicate entries in the document.")
                        .AppendLine("Do you want to save them to a file?").AppendLine()
                        .AppendLine("WARNING: Only the duplicate line will be saved!");
                    bool save = QuestionBox(sb.ToString(), "Warning");
                    duplicateEntries.Insert(0, lines[0]);
                    if (save && FileDialogHandler.ShowSaveFileDialog("Save Duplicate entries", out string duplicatesFilePath))
                    {
                        if (FileHandler.WriteAllLines(duplicatesFilePath, duplicateEntries))
                        {
                            Logging.Instance.WriteLine($"{duplicateEntries.Count - 1} entries successfully saved to {duplicatesFilePath}.");
                            RemoveLinesFromDataSet(filePath, ref lines, duplicateEntries);
                        }
                    }
                    else
                    {
                        Logging.Instance.WriteLine("Duplicate entries were NOT saved to a file.");
                    }
                }
                if (emptyEntries.Count > 0)
                {
                    Logging.Instance.WriteLine($"Found {emptyEntries.Count} Empty entries.");
                    StringBuilder sb = new StringBuilder($"There are {emptyEntries.Count} entries with missing data in the document.")
                        .AppendLine("Do you want to save them to a file?").AppendLine();
                    bool save = QuestionBox(sb.ToString(), "Stop");
                    emptyEntries.Insert(0, lines[0]);
                    if (save && FileDialogHandler.ShowSaveFileDialog("Save Empty entries", out string emptiesFilePath))
                    {
                        if (FileHandler.WriteAllLines(emptiesFilePath, duplicateEntries))
                        {
                            Logging.Instance.WriteLine($"{emptyEntries.Count - 1} entries successfully saved to {emptiesFilePath}.");
                            RemoveLinesFromDataSet(filePath, ref lines, emptyEntries);
                        }
                    }
                    else
                    {
                        Logging.Instance.WriteLine("Incomplete entries were NOT saved to a file.");
                    }
                }
                Logging.Instance.WriteLine($"Finished processing {lines.Length - 1} Documents.");
                message = $"Successfully loaded {documents.Count} Documents.";
                return documents.OrderBy(d => d.LastName).ThenBy(d => d.FirstName).ToList();
            }
            catch (Exception e)
            {
                message = "Error loading DataSet File!";
                Logging.Instance.WriteLine($"{message} {e.Message}");
                return new List<DocumentModel>();
            }
        }

        private void RemoveLinesFromDataSet(string filePath, ref string[] lines, List<string> entries)
        {
            if (!QuestionBox(Constants.RemoveMessage, "Question")) return;
            List<string> newLines = lines.ToList();
            foreach (string entry in entries.Skip(1))
            {
                newLines.Remove(entry);
            }
            lines = newLines.ToArray();
            FileHandler.WriteAllLines(filePath, lines);
            Logging.Instance.WriteLine($"{lines.Length - 1} entries successfully saved to {filePath}.");
        }

        private List<OrganizationModel> ReadOrganizationsFile(string filePath, out string message)
        {
            try
            {
                if (!FileHandler.ReadAllLines(filePath, out string[] lines))
                {
                    message = $"Error reading file {filePath}. The file is missing or may be in use by another program.";
                    MessageBox.Show(message);
                    return new List<OrganizationModel>();
                }
                List<OrganizationModel> organizations = new List<OrganizationModel>();
                foreach (string line in lines)
                {
                    if (line.StartsWith("OrganizationID"))
                    {
                        Settings.Instance.Separator = line[14];
                        continue;
                    }
                    string[] lineArr = StringSplit.SkipStrings(line, Separators[0], '\"');
                    if (lineArr.Length < 3) lineArr = StringSplit.SkipStrings(line, Separators[1], '\"');
                    int id = int.Parse(lineArr[0]);
                    string name = lineArr[1];
                    int? parentId = int.TryParse(lineArr[2], out int pId) ? pId : null;
                    organizations.Add(new OrganizationModel(id, name, parentId));
                }
                Logging.Instance.WriteLine($"Finished processing {lines.Length - 1} Organizations.");
                message = $"Successfully loaded {organizations.Count} Organizations.";
                return organizations;
            }
            catch (Exception e)
            {
                message = "Error loading Organizations File!";
                Logging.Instance.WriteLine($"{message} {e.Message}");
                return new List<OrganizationModel>();
            }
        }

        private List<PersonModel> ReadPeopleFile(string filePath, out string message)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    message = "No People file selected.";
                    return new List<PersonModel>();
                }
                if (!FileHandler.ReadAllLines(filePath, out string[] lines))
                {
                    message = $"Error reading file {filePath}. The file is missing or may be in use by another program.";
                    MessageBox.Show(message);
                    return new List<PersonModel>();
                }
                List<PersonModel> people = new List<PersonModel>();
                foreach (string line in lines)
                {
                    if (line.StartsWith("PersonID"))
                    {
                        Settings.Instance.Separator = line[8];
                        continue;
                    }
                    string[] lineArr = StringSplit.SkipStrings(line, Separators[0], '\"');
                    if (lineArr.Length < 10) lineArr = StringSplit.SkipStrings(line, Separators[1], '\"');
                    people.Add(new PersonModel(lineArr));
                }
                Logging.Instance.WriteLine($"Finished processing {lines.Length - 1} People.");
                message = $"Successfully loaded {people.Count} People.";
                return people;
            }
            catch (Exception e)
            {
                message = "Error loading People File!";
                Logging.Instance.WriteLine($"{message} {e.Message}");
                return new List<PersonModel>();
            }
        }

        public string GetOrganizationName(OrganizationModel o)
        {
            OrganizationModel parent = Organizations.FirstOrDefault(or => or.OrganizationId == o.ParentOrgId);
            return $"{o.OrganizationName} ({parent?.OrganizationName})";
        }

        /// <summary>
        /// Shows MessageBox with Caption = "Attention!", YesNo buttons and configurable image.
        /// </summary>
        /// <param name="message">Message to show...</param>
        /// <param name="imageText">Accepted values: None, Stop, Question, Warning, Information</param>
        /// <returns>True if Yes button is pressed. False if No button is pressed or box is closed.</returns>
        public bool QuestionBox(string message, string imageText)
        {
            MessageBoxImage image = Enum.Parse<MessageBoxImage>(imageText);
            return MessageBox.Show(message, "Attention!", MessageBoxButton.YesNo, image) == MessageBoxResult.Yes;
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
            PersonModel findPerson = People.Find(p => p.FirstName == firstName && p.LastName == lastName);
            int id = findPerson?.PersonId ?? (People.Count == 0 ? 1 : People.Last().PersonId + 1);
            string[] personArray = { $"{id}", firstName, lastName, $"{orgId}", docId, $"{seqNo}", null, null, null, null };
            People.Add(new PersonModel(personArray));
            return SavePeople();
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
                PersonModel findPerson = shiftedPeople.Find(p => p.LastPersonId == person.PersonId);
                person.LastPersonId = person.PersonId;
                person.PersonId = findPerson?.PersonId ?? (shiftedPeople.Count == 0 ? newStartingId : shiftedPeople.Last().PersonId + 1);
                shiftedPeople.Add(new PersonModel(person.ToStringArray()) { LastPersonId = person.LastPersonId});
            }
            List<string> exportPeople = new List<string> { string.Join(Settings.Instance.Separator, Constants.ExportPeopleHeader) };
            exportPeople.AddRange(shiftedPeople.Select(p => p.ToString()));
            if (!FileDialogHandler.ShowSaveFileDialog("Save People as new file", out string filePath)) return false;
            return FileHandler.WriteAllLines(filePath, exportPeople);
        }

        public void ShowSettingsWindow() => new SettingsWindow(this).ShowDialog();
    }
}