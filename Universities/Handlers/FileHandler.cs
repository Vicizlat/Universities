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
    }
}