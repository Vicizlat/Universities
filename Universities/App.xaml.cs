using System;
using System.IO;
using System.Windows;
using Universities.Controller;
using Universities.Handlers;
using Universities.Utils;
using Universities.Views;

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
            if (FileHandler.FileExists(Constants.SettingsFilePath) && Settings.Instance.ReadSettingsFile())
            {
                controller.LoadFiles();
            }
            else
            {
                string message = "There are no saved file locations. Do you want to open the Settings window to choose file locations?";
                if (PromptBox.Question(message)) new SettingsWindow(controller).ShowDialog();
            }
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