using System;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using Squirrel;
using Universities.Controller;
using Universities.Handlers;
using Universities.Utils;
using Universities.Views;

namespace Universities
{
    public partial class App
    {
        private MainController controller { get; set; }
        private UpdateManager manager;
        private UpdateInfo updateInfo;
        private string installedVersion;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            Logging.Instance.WriteLine("Logging started");
            ManageLogFiles(Constants.LogsPath);
            await CheckForUpdate();
            controller = new MainController(installedVersion);
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

        private async Task CheckForUpdate()
        {
            manager = await UpdateManager.GitHubUpdateManager(@"https://github.com/Vicizlat/Universities");
            installedVersion = manager.CurrentlyInstalledVersion()?.ToString() ?? "Debug";
            if (installedVersion == "Debug") return;
            updateInfo = await manager.CheckForUpdate();
            string newVersion = updateInfo.FutureReleaseEntry.Version.ToString();
            if (updateInfo.ReleasesToApply.Count > 0)
            {
                await manager.UpdateApp();
                Logging.Instance.WriteLine($"Succesfuly Updated from v.{installedVersion} to v.{newVersion}!");
                UpdateManager.RestartApp("Universities.exe");
            }
        }

        private void ManageLogFiles(string logsPath)
        {
            string[] files = Directory.GetFiles(logsPath);
            while (files.Length > 50)
            {
                File.Delete(files[0]);
                files = Directory.GetFiles(logsPath);
            }
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