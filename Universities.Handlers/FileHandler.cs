using System;
using System.Collections.Generic;
using System.IO;
using Universities.Utils;

namespace Universities.Handlers
{
    public static class FileHandler
    {
        public static bool ReadAllLines(string fileName, out string[] contents)
        {
            try
            {
                contents = File.ReadAllLines(fileName);
                Logging.Instance.WriteLine($"Successfully opened \"{fileName}\"!");
                return true;
            }
            catch
            {
                Logging.Instance.WriteLine($"Failed to open \"{fileName}\"!");
                contents = Array.Empty<string>();
                return false;
            }
        }

        public static bool WriteAllLines(string fileName, IEnumerable<string> contents)
        {
            try
            {
                File.WriteAllLines(fileName, contents);
                Logging.Instance.WriteLine($"Successfully saved \"{fileName}\"!");
                return true;
            }
            catch
            {
                Logging.Instance.WriteLine($"Failed to save \"{fileName}\"!");
                return false;
            }
        }

        public static bool FileExists(string path, string fileName = "")
        {
            string fullPath = Path.Combine(path, fileName);
            return File.Exists(fullPath);
        }

        public static void CopyFile(string path1, string path2, string fileName)
        {
            string fullPath1 = Path.Combine(path1, fileName);
            string fullPath2 = Path.Combine(path2, fileName);
            File.Copy(fullPath1, fullPath2);
        }

        public static void ManageLogFiles()
        {
            string[] files = Directory.GetFiles(Constants.LogsPath);
            while (files.Length > 50)
            {
                File.Delete(files[0]);
                files = Directory.GetFiles(Constants.LogsPath);
            }
        }

        public static void ManageBackupFiles()
        {
            string[] foldersDays = Directory.GetDirectories(Constants.BackupsPath);
            while (foldersDays.Length > Constants.BackupDaysToKeep)
            {
                Directory.Delete(foldersDays[0], true);
                foldersDays = Directory.GetDirectories(Constants.BackupsPath);
            }
            foreach (string folder in foldersDays)
            {
                string[] foldersHours = Directory.GetDirectories(folder);
                while (foldersHours.Length > Constants.BackupsPerDayToKeep)
                {
                    Directory.Delete(foldersHours[0], true);
                    foldersHours = Directory.GetDirectories(folder);
                }
            }
        }
    }
}