using System;
using System.IO;

namespace Universities.Utils
{
    public class Logging
    {
        public static Logging Instance => thisInstance ?? new Logging();
        private static Logging thisInstance;
        private readonly TextWriter writer;

        public Logging()
        {
            writer = new StreamWriter(Constants.LogFilePath);
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
        }
    }
}