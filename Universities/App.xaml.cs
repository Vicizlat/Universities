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
                string oldPath1 = Directory.CreateDirectory(Path.Combine(Path.Combine(Constants.WorkingFolder, $"app-0.{previousVersion}"), "Settings")).FullName;
                string oldPath2 = Directory.CreateDirectory(Path.Combine(Path.Combine(Constants.WorkingFolder, "app-0.1.3"), "Settings")).FullName;
                string oldPath3 = Directory.CreateDirectory(Path.Combine(Path.Combine(Constants.WorkingFolder, "app-0.1.2"), "Settings")).FullName;
                if (FileHandler.FileExists(Path.Combine(oldPath1, "Settings.xml")))
                {
                    FileHandler.CopyFile(oldPath1, Constants.SettingsPath, "Settings.xml");
                }
                else if (FileHandler.FileExists(Path.Combine(oldPath2, "Settings.xml")))
                {
                    FileHandler.CopyFile(oldPath2, Constants.SettingsPath, "Settings.xml");
                }
                else if (FileHandler.FileExists(Path.Combine(oldPath3, "Settings.xml")))
                {
                    FileHandler.CopyFile(oldPath3, Constants.SettingsPath, "Settings.xml");
                }
                else
                {
                    Settings.Instance.WriteSettingsFile();
                }
            }

            if (!Settings.Instance.ReadSettingsFile() || !DBAccess.Context.Database.CanConnect())
            {
                if (!PromptForSettingsDetails()) return;
            }
            MainController controller = new MainController();
            MainWindow = new MainWindow(controller) { Title = $"Universities v. {installedVersion}     Current user: {SqlCommands.CurrentUser.Item1}" };
            CloseWaitWindow();
            MainWindow.Show();
            MainWindow.Closed += delegate { Shutdown(); };
        }

        public bool PromptForSettingsDetails()
        {
            PromptBox.Warning(Constants.ConnectionDetailsWarning);
            while (new SettingsWindow().ShowDialog().Value)
            {
                if (DBAccess.Context.Database.CanConnect())
                {
                    return PromptBox.Information("Connected to Database");
                }
            }
            Shutdown();
            return !PromptBox.Error("Can't connect to Database");
        }

        private async Task CheckForUpdate()
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

        private void Application_Exit(object sender, ExitEventArgs e) => Logging.Instance.Close();

        public void ShowWaitWindow(string text = null)
        {
            WaitWindow = new WaitWindow(text);
            WaitWindow.Show();
        }

        public void CloseWaitWindow()
        {
            if (WaitWindow != null) WaitWindow.Close();
        }
    }
}