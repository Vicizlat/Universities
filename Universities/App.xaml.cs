using System.IO;
using System.Windows;
using System.Threading.Tasks;
using Squirrel;
using Universities.Controller;
using Universities.Handlers;
using Universities.Utils;
using Universities.Views;
using Universities.Data;
using MySqlConnector;

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
            if (Settings.Instance.ReadSettingsFile())
            {
                if (!TryGetContext(out context))
                {
                    if (string.IsNullOrEmpty(Settings.Instance.Server) ||
                        string.IsNullOrEmpty(Settings.Instance.Port) ||
                        string.IsNullOrEmpty(Settings.Instance.Database))
                    {
                        if (!PromptForSettingsDetails(out context)) return;
                    }
                    else if (string.IsNullOrEmpty(Settings.Instance.Username) || string.IsNullOrEmpty(Settings.Instance.Password))
                    {
                        for (int i = 3; i >= 0; i--)
                        {
                            if (i > 0 && new LoginWindow(i, true).ShowDialog().Value)
                            {
                                if (TryGetContext(out context))
                                {
                                    PromptBox.Information("Connected to Database", currentUser);
                                    break;
                                }
                            }
                            else
                            {
                                PromptBox.Error("Can't connect to Database");
                                CallShutdown();
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                if (!PromptForSettingsDetails(out context)) return;
            }
            MainController controller = new MainController(context, currentUser, isAdmin, installedVersion);
            MainWindow = new MainWindow(controller);
            MainWindow.Show();
            MainWindow.Closed += delegate { CallShutdown(); };
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
                    CallShutdown();
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
            currentUser = CheckUser();
            return context.Database.CanConnect();
        }

        public string CheckUser()
        {
            try
            {
                string currentUser = null;
                using (MySqlConnection Connection = new MySqlConnection(Settings.Instance.GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand("select current_user;", Connection))
                {
                    Connection.Open();
                    currentUser = (string)Command.ExecuteScalar();
                }
                using (MySqlConnection Connection = new MySqlConnection(Settings.Instance.GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand("SHOW GRANTS;", Connection))
                {
                    Connection.Open();
                    isAdmin = ((string)Command.ExecuteScalar()).StartsWith("GRANT ALL");
                }
                return currentUser?.Remove(currentUser.Length - 2) ?? string.Empty;
            }
            catch
            {
                return "Not connected to DB";
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

        private void CallShutdown()
        {
            Logging.Instance.Close();
            Shutdown();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
        }
    }
}