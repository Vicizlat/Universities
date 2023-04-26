using System;
using System.Linq;
using System.Windows;
using Universities.Handlers;
using Universities.Utils;

namespace Universities.Controller
{
    public static class DataReader
    {
        public static event EventHandler? OnDocumentFound;
        public static event EventHandler? OnOrganizationFound;
        public static event EventHandler? OnPersonFound;

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

        public static bool ReadOrganizationsFile(string filePath, out string message)
        {
            try
            {
                if (!ReadLines(filePath, out string[] lines, out message)) return false;
                if (!lines[0].StartsWith("OrganizationID"))
                {
                    message = Constants.MissmatchData;
                    return false;
                }
                Settings.Instance.Separator = lines[0][14];
                Settings.Instance.WriteSettingsFile();
                foreach (string line in lines.Skip(1))
                {
                    string[] lineArr = StringSplit.SkipStrings(line, Settings.Instance.Separator, '\"');
                    OnOrganizationFound?.Invoke(lineArr, EventArgs.Empty);
                }
                Logging.Instance.WriteLine($"Finished processing {lines.Length - 1} Organizations.");
                return true;
            }
            catch (Exception e)
            {
                message = "Error loading Organizations File!";
                Logging.Instance.WriteLine($"{message} {e.Message}");
                return false;
            }
        }

        public static bool ReadPeopleFile(string filePath, out string message)
        {
            try
            {
                if (!ReadLines(filePath, out string[] lines, out message)) return false;
                if (!lines[0].StartsWith("PersonID"))
                {
                    message = Constants.MissmatchData;
                    return false;
                }
                Settings.Instance.Separator = lines[0][8];
                Settings.Instance.WriteSettingsFile();
                foreach (string line in lines.Skip(1))
                {
                    string[] lineArr = StringSplit.SkipStrings(line, Settings.Instance.Separator, '\"');
                    OnPersonFound?.Invoke(lineArr, EventArgs.Empty);
                }
                Logging.Instance.WriteLine($"Finished processing {lines.Length - 1} People.");
                return true;
            }
            catch (Exception e)
            {
                message = "Error loading People File!";
                Logging.Instance.WriteLine($"{message} {e.Message}");
                return false;
            }
        }

        private static bool ReadLines(string filePath, out string[] lines, out string message)
        {
            message = string.Empty;
            if (FileHandler.ReadAllLines(filePath, out lines)) return true;
            message = $"Error reading file {filePath}. The file is missing or may be in use by another program.";
            MessageBox.Show(message);
            return false;
        }
    }
}