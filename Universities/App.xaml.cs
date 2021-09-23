using System;
using System.IO;
using System.Windows;
using Universities.Controller;
using Universities.Utils;

namespace Universities
{
    public partial class App
    {
        private MainController controller { get; set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Logging.Instance.WriteLine("Logging started");
            string[] files = Directory.GetFiles(Constants.LogsPath);
            while (files.Length > 50)
            {
                File.Delete(files[0]);
                files = Directory.GetFiles(Constants.LogsPath);
            }
            controller = new MainController();
            MainWindow = new MainWindow(controller);
            MainWindow.Show();
            MainWindow.Closed += CallShutdown;
        }

        private void CallShutdown(object sender, EventArgs e)
        {
            Logging.Instance.Close();
            Shutdown();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
        }
    }
}