using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Universities.Data.Models;
using Universities.Handlers;
using Universities.Models;
using Universities.Utils;

namespace Universities.Controller
{
    public static class DataReader
    {
        public static event EventHandler? OnDocumentFound;
        public static event EventHandler? OnIncompleteDocumentFound;

        public static bool ReadDataSetFile(string filePath, out string message)
        {
            try
            {
                if (!ReadLines(filePath, out string[] lines, out message)) return false;
                if (!lines[0].StartsWith("UT"))
                {
                    message = Constants.MissmatchData;
                    return false;
                }
                Settings.Instance.Separator = lines[0][2];
                Settings.Instance.WriteSettingsFile();
                foreach (string line in lines.Skip(1))
                {
                    string[] lineArr = StringSplit.SkipStrings(line, Settings.Instance.Separator, '\"');
                    if (string.IsNullOrEmpty(lineArr[14]))
                    {
                        OnIncompleteDocumentFound?.Invoke(lineArr, EventArgs.Empty);
                        continue;
                    }
                    OnDocumentFound?.Invoke(lineArr, EventArgs.Empty);
                }
                Logging.Instance.WriteLine($"Finished processing {lines.Length - 1} Documents.");
                return true;
            }
            catch (Exception e)
            {
                message = "Error loading DataSet File!";
                Logging.Instance.WriteLine($"{message} {e.Message}");
                return false;
            }
        }

        public static List<Organization> ReadOrganizationsFile(MainController controller, string filePath, out string message)
        {
            try
            {
                List<Organization> organizations = new List<Organization>();
                if (!ReadLines(filePath, out string[] lines, out message)) return organizations;
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
                    organizations.Add(new Organization(id, name, parentId));
                    //controller.Context.Organizations.Add(new Organization(id, name, parentId));
                    //controller.Context.SaveChanges();
                }
                Logging.Instance.WriteLine($"Finished processing {lines.Length - 1} Organizations.");
                message = $"Successfully loaded {organizations.Count} Organizations.";
                return organizations;
            }
            catch (Exception e)
            {
                message = "Error loading Organizations File!";
                Logging.Instance.WriteLine($"{message} {e.Message}");
                return new List<Organization>();
            }
        }

        public static List<Person> ReadPeopleFile(MainController controller, string filePath, out string message)
        {
            try
            {
                List<Person> people = new List<Person>();
                if (string.IsNullOrEmpty(filePath))
                {
                    message = "No People file selected.";
                    return people;
                }
                if (!ReadLines(filePath, out string[] lines, out message)) return people;
                foreach (string line in lines)
                {
                    if (line.StartsWith("PersonID"))
                    {
                        Settings.Instance.Separator = line[8];
                        continue;
                    }
                    string[] lineArr = StringSplit.SkipStrings(line, Constants.Separators[0], '\"');
                    if (lineArr.Length < 10) lineArr = StringSplit.SkipStrings(line, Constants.Separators[1], '\"');
                    people.Add(new Person(lineArr));
                    //controller.Context.People.Add(new Person(lineArr));
                    //controller.Context.SaveChanges();
                }
                Logging.Instance.WriteLine($"Finished processing {lines.Length - 1} People.");
                message = $"Successfully loaded {people.Count} People.";
                return people;
            }
            catch (Exception e)
            {
                message = "Error loading People File!";
                Logging.Instance.WriteLine($"{message} {e.Message}");
                return new List<Person>();
            }
        }

        private static bool ReadLines(string filePath, out string[] lines, out string message)
        {
            bool readResult = FileHandler.ReadAllLines(filePath, out lines);
            message = string.Empty;
            if (!readResult)
            {
                message = $"Error reading file {filePath}. The file is missing or may be in use by another program.";
                MessageBox.Show(message);
            }
            return readResult;
        }
    }
}