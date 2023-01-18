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
    public static class DataReader
    {
        public static List<DocumentModel> ReadDataSetFile(string filePath, out string message)
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
                    string[] lineArr = StringSplit.SkipStrings(line, Constants.Separators[0], '\"');
                    if (lineArr.Length < 20) lineArr = StringSplit.SkipStrings(line, Constants.Separators[1], '\"');
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
                    bool save = Warning(sb.ToString());
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
                    bool save = Stop(sb.ToString());
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

        private static void RemoveLinesFromDataSet(string filePath, ref string[] lines, List<string> entries)
        {
            string message = "Do you want to remove these entries from the main document.";
            if (!Question(message)) return;
            List<string> newLines = lines.ToList();
            foreach (string entry in entries.Skip(1))
            {
                newLines.Remove(entry);
            }
            lines = newLines.ToArray();
            FileHandler.WriteAllLines(filePath, lines);
            Logging.Instance.WriteLine($"{lines.Length - 1} entries successfully saved to {filePath}.");
        }

        public static List<OrganizationModel> ReadOrganizationsFile(string filePath, out string message)
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
                    string[] lineArr = StringSplit.SkipStrings(line, Constants.Separators[0], '\"');
                    if (lineArr.Length < 3) lineArr = StringSplit.SkipStrings(line, Constants.Separators[1], '\"');
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

        public static List<PersonModel> ReadPeopleFile(string filePath, out string message)
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
                    string[] lineArr = StringSplit.SkipStrings(line, Constants.Separators[0], '\"');
                    if (lineArr.Length < 10) lineArr = StringSplit.SkipStrings(line, Constants.Separators[1], '\"');
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

        public static bool Question(string message)
        {
            return MessageBox.Show(message, "Attention!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public static bool Warning(string message)
        {
            return MessageBox.Show(message, "Attention!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }

        public static bool Stop(string message)
        {
            return MessageBox.Show(message, "Attention!", MessageBoxButton.YesNo, MessageBoxImage.Stop) == MessageBoxResult.Yes;
        }
    }
}