using System.IO;
using System.Windows;
using System.Threading.Tasks;
using Squirrel;
using Universities.Controller;
using Universities.Handlers;
using Universities.Utils;
using Universities.Views;
using System.Windows.Threading;
using System;

namespace Universities
{
    public partial class App
    {
        public WaitWindow WaitWindow { get; set; }
        private string installedVersion;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            Logging.Instance.WriteLine("Logging started");
            ShowWaitWindow();
            FileHandler.ManageLogFiles();
            await CheckForUpdate();

            if (!FileHandler.FileExists(Constants.SettingsFilePath) && installedVersion != "Debug")
            {
                double previousVersion = ((double)(double.Parse(installedVersion[2..]) * 10) - 1) / 10;
                string oldPath1 = Directory.CreateDirectory(Path.Combine(Path.Combine(Constants.WorkingFolder, $"app-0.{previousVersion:f1}"), "Settings")).FullName;
                string oldPath2 = Directory.CreateDirectory(Path.Combine(Path.Combine(Constants.WorkingFolder, "app-0.1.2"), "Settings")).FullName;
                if (FileHandler.FileExists(Path.Combine(oldPath1, "Settings.xml")))
                {
                    FileHandler.CopyFile(oldPath1, Constants.SettingsPath, "Settings.xml");
                }
                else if (FileHandler.FileExists(Path.Combine(oldPath2, "Settings.xml")))
                {
                    FileHandler.CopyFile(oldPath2, Constants.SettingsPath, "Settings.xml");
                }
                else
                {
                    Settings.Instance.WriteSettingsFile();
                }
            }

            if (!Settings.Instance.ReadSettingsFile() || !CheckSettings() || !CheckConnection())
            {
                if (!PromptForSettingsDetails()) return;
            }
            MainController controller = new MainController();
            MainWindow = new MainWindow(controller, installedVersion);
            CloseWaitWindow();
            MainWindow.Show();
        }

        private bool CheckSettings()
        {
            bool checkServer = !string.IsNullOrEmpty(Settings.Instance.Server);
            bool checkPort = Settings.Instance.Port != 0;
            bool checkUser = !string.IsNullOrEmpty(Settings.Instance.Username);
            bool checkPass = !string.IsNullOrEmpty(Settings.Instance.Password);
            bool checkDatabase = !string.IsNullOrEmpty(Settings.Instance.Database);
            return checkServer && checkPort && checkUser && checkPass && checkDatabase;
        }

        private bool CheckConnection()
        {
            try
            {
                if (DBAccess.GetContext(true).Database.CanConnect())
                {
                    DBAccess.GetContext().Database.EnsureCreated();
                    return true;
                }
                else return false;
            }
            catch (Exception e)
            {
                Logging.Instance.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                PromptBox.Error("Can't connect to Database!");
                Shutdown(3);
                return false;
            }
        }

        public bool PromptForSettingsDetails()
        {
            PromptBox.Warning(Constants.ConnectionDetailsWarning);
            while (new SettingsWindow().ShowDialog().Value)
            {
                if (Settings.Instance.ReadSettingsFile() && CheckConnection())
                {
                    return PromptBox.Information("Connected to Database");
                }
            }
            PromptBox.Error("Settings are wrong or incomplete!");
            Shutdown(1);
            return false;
        }

        private async Task CheckForUpdate()
        {
            try
            {
                using UpdateManager manager = await UpdateManager.GitHubUpdateManager(@"https://github.com/Vicizlat/Universities");
                installedVersion = manager.CurrentlyInstalledVersion()?.ToString() ?? "Debug";
                if (installedVersion == "Debug") return;
                UpdateInfo updateInfo = await manager.CheckForUpdate();
                string newVersion = updateInfo.FutureReleaseEntry.Version.ToString();
                if (updateInfo.ReleasesToApply.Count > 0)
                {
                    await manager.UpdateApp();
                    Logging.Instance.WriteLine($"Succesfuly Updated from v.{installedVersion} to v.{newVersion}!");
                    UpdateManager.RestartApp("Universities.exe");
                }
            }
            catch (Exception e)
            {
                Logging.Instance.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                PromptBox.Error("No internet connection!");
                Shutdown(2);
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            switch (e.ApplicationExitCode)
            {
                case (int)Constants.ExitCodes.NoErrors:
                    Logging.Instance.WriteLine($"Application exited with error code {e.ApplicationExitCode}: No Error");
                    break;
                case (int)Constants.ExitCodes.NoSettings:
                    Logging.Instance.WriteLine($"Application exited with error code {e.ApplicationExitCode}: Settings are incomplete");
                    break;
                case (int)Constants.ExitCodes.NoInternet:
                    Logging.Instance.WriteLine($"Application exited with error code {e.ApplicationExitCode}: No internet connection");
                    break;
                case (int)Constants.ExitCodes.NoDatabase:
                    Logging.Instance.WriteLine($"Application exited with error code {e.ApplicationExitCode}: Can't connect to Database");
                    break;
            }
            Logging.Instance.Close();
        }

        public void ShowWaitWindow(string text = null)
        {
            WaitWindow = new WaitWindow(text);
            WaitWindow.Show();
        }

        public void CloseWaitWindow()
        {
            if (WaitWindow != null) WaitWindow.Close();
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string message = e.Exception.Message + Environment.NewLine + e.Exception.StackTrace;
            MessageBox.Show(message);
            Logging.Instance.WriteLine(message);
            e.Handled = true;
        }
    }
}