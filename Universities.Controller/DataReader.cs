using System;
using System.Linq;
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
                if (!CheckFileData(lines[0], "UT", out message)) return false;
                foreach (string line in lines.Skip(1))
                {
                    string[] lineArr = line.Split(lines[0][2], StringSplitOptions.TrimEntries);
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
                if (!CheckFileData(lines[0], "OrganizationID", out message)) return false;
                foreach (string line in lines.Skip(1))
                {
                    string[] lineArr = line.Split(lines[0][14], StringSplitOptions.TrimEntries);
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
                if (!CheckFileData(lines[0], "PersonID", out message)) return false;
                foreach (string line in lines.Skip(1))
                {
                    string[] lineArr = line.Split(lines[0][8], StringSplitOptions.TrimEntries);
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

        private static bool CheckFileData(string text, string startsWith, out string message)
        {
            message = string.Empty;
            if (text.StartsWith(startsWith)) return true;
            message = "Data does not match expectations. Did you load the wrong file?";
            return false;
        }

        private static bool ReadLines(string filePath, out string[] lines, out string message)
        {
            message = string.Empty;
            if (FileHandler.ReadAllLines(filePath, out lines)) return true;
            message = $"Error reading file {filePath}. The file is missing or may be in use by another program.";
            return false;
        }
    }
}