using System;
using System.IO;
using System.Linq;

namespace Universities.Utils
{
    public class Logging
    {
        public static Logging Instance => thisInstance ?? new Logging();
        private static Logging? thisInstance;
        private readonly TextWriter writer;
        private static readonly string WorkingFolder = string.Join('\\', Environment.CurrentDirectory.Split('\\').ToList().SkipLast(1));
        private static readonly string LogsPath = Directory.CreateDirectory(Path.Combine(WorkingFolder, "Logs")).FullName;

        public Logging()
        {
            string logFileName = $"Log-{DateTime.Now:[yyyy-MM-dd][HH-mm-ss]}.txt";
            writer = new StreamWriter(Path.Combine(LogsPath, logFileName));
            thisInstance = this;
        }

        public void WriteLine(string text, bool noTimeCode = false)
        {
            writer.WriteLine(noTimeCode ? text : $"{DateTime.Now:dd.MM.yyyy HH:mm:ss:ffff}: {text}");
            writer.Flush();
        }

        public void Close()
        {
            WriteLine("Logging ended");
            writer.Dispose();
            ManageLogFiles();
        }

        private static void ManageLogFiles()
        {
            string[] files = Directory.GetFiles(LogsPath);
            while (files.Length > 50)
            {
                File.Delete(files[0]);
                files = Directory.GetFiles(LogsPath);
            }
        }
    }
}