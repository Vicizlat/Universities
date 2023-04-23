using System.IO;
using System.Windows;
using System.Threading.Tasks;
using Squirrel;
using Universities.Controller;
using Universities.Handlers;
using Universities.Utils;
using Universities.Views;
using Universities.Data;

namespace Universities
{
    public partial class App
    {
        private string installedVersion;
        private string currentUser;
        private bool isAdmin;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            Logging.Instance.WriteLine("Logging started");
            ManageLogFiles(Constants.LogsPath);
            await CheckForUpdate();
            UniversitiesContext context = null;

            if (!FileHandler.FileExists(Constants.SettingsFilePath)) Settings.Instance.WriteSettingsFile();
            if (!Settings.Instance.ReadSettingsFile() || !TryGetContext(out context))
            {
                if (!PromptForSettingsDetails(out context)) return;
            }
            MainController controller = new MainController(context, currentUser, isAdmin, installedVersion);
            MainWindow = new MainWindow(controller);
            MainWindow.Show();
            MainWindow.Closed += delegate { Shutdown(); };
        }

        public bool PromptForSettingsDetails(out UniversitiesContext context)
        {
            PromptBox.Warning(Constants.ConnectionDetailsWarning);
            while (true)
            {
                if (new SettingsWindow().ShowDialog().Value)
                {
                    if (TryGetContext(out context))
                    {
                        PromptBox.Information("Connected to Database", currentUser);
                        return true;
                    }
                }
                else
                {
                    PromptBox.Error("Can't connect to Database");
                    context = null;
                    Shutdown();
                    return false;
                }
            }
        }

        private bool TryGetContext(out UniversitiesContext context)
        {
            string server = $"Server={Settings.Instance.Server};";
            string port = $"Port={Settings.Instance.Port};";
            string database = $"Database={Settings.Instance.Database};";
            string user = $"User={Settings.Instance.Username};";
            string pass = $"Password={Settings.Instance.Password};";
            string connectionString = $"{server}{port}{database}{user}{pass}";
            context = new UniversitiesContext(connectionString);
            currentUser = SqlCommands.GetCurrentUser(out isAdmin);
            return context.Database.CanConnect();
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

        private async Task CheckForUpdate()
        {
            UpdateManager manager = await UpdateManager.GitHubUpdateManager(@"https://github.com/Vicizlat/Universities");
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
    }
}