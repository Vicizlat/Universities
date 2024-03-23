using Squirrel;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Universities.Controller;
using Universities.Handlers;
using Universities.Utils;
using Universities.Views;

namespace Universities
{
    public partial class App
    {
        private enum ExitCodes
        {
            NoErrors = 0,
            NoSettings = 1,
            NoInternet = 2,
            NoDatabase = 3
        };

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            Logging.Instance.WriteLine("Logging started");
            WaitWindow waitWindow = new WaitWindow("Starting program...");
            waitWindow.Show();
            string installedVersion = await CheckForUpdate();
            if (!CheckSettings() || !await CheckConnection() || !await GetValidUserAsync() || !await GetValidMainOrgAsync())
            {
                if (!await PromptForSettingsDetails()) return;
            }

            MainController controller = new MainController();
            MainWindow MainWindow = new MainWindow(installedVersion);
            MainWindow.Controller = controller;
            waitWindow?.Close();
            MainWindow.Show();
        }

        private bool CheckSettings()
        {
            if (!FileHandler.FileExists(Settings.SettingsFilePath)) return PromptBox.Error(Constants.ErrorNoSettingsFile);
            if (!Settings.Instance.ReadSettingsFile()) return PromptBox.Error(string.Format(Constants.ErrorFileRead, Settings.SettingsFilePath));
            bool checkServer = !string.IsNullOrEmpty(Settings.Instance.Server);
            bool checkUser = !string.IsNullOrEmpty(Settings.Instance.Username);
            bool checkPass = !string.IsNullOrEmpty(Settings.Instance.DecryptedPassword);
            if (checkServer && checkUser && checkPass) return true;
            return PromptBox.Error(Constants.ErrorSettings);
        }

        public async Task<bool> PromptForSettingsDetails()
        {
            while (new SettingsWindow().ShowDialog().Value)
            {
                if (CheckSettings() && await CheckConnection() && await GetValidUserAsync() && await GetValidMainOrgAsync()) return true;
            }
            Shutdown(1);
            return PromptBox.Error(Constants.ErrorSettings);
        }

        private async Task<bool> CheckConnection() => await PhpHandler.CreateCommonTables() || PromptBox.Error("Can't connect to Server!");
        private async Task<bool> GetValidUserAsync() => User.CheckUser(await PhpHandler.CreateOrVerifyUserAsync("verify"));
        private async Task<bool> GetValidMainOrgAsync() => MainOrg.CheckMainOrg(await PhpHandler.GetFromTableAsync("main_organizations"), User.LastMainOrg);

        public async Task<string> CheckForUpdate()
        {
            try
            {
                using UpdateManager manager = await UpdateManager.GitHubUpdateManager(@"https://github.com/Vicizlat/Universities");
                string installedVersion = manager.CurrentlyInstalledVersion()?.ToString() ?? "Debug";
                if (installedVersion != "Debug")
                {
                    UpdateInfo updateInfo = await manager.CheckForUpdate();
                    string newVersion = updateInfo.FutureReleaseEntry.Version.ToString();
                    if (updateInfo.ReleasesToApply.Count > 0)
                    {
                        await manager.UpdateApp();
                        Logging.Instance.WriteLine($"Succesfuly Updated from v.{installedVersion} to v.{newVersion}!");
                        UpdateManager.RestartApp("Universities.exe");
                    }
                }
                return installedVersion;
            }
            catch (Exception e)
            {
                Logging.Instance.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                PromptBox.Error("No internet connection!");
                Shutdown(2);
                return "No internet connection!";
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            string logMsg = $"Application exited with error code {e.ApplicationExitCode}: ";
            if (e.ApplicationExitCode == (int)ExitCodes.NoErrors) logMsg += "No Error";
            else if (e.ApplicationExitCode == (int)ExitCodes.NoSettings) logMsg += "Settings are incomplete";
            else if (e.ApplicationExitCode == (int)ExitCodes.NoInternet) logMsg += "No internet connection";
            else if (e.ApplicationExitCode == (int)ExitCodes.NoDatabase) logMsg += "Can't connect to Server";
            Logging.Instance.WriteLine(logMsg);
            Logging.Instance.Close();
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string message = e.Exception.Message + Environment.NewLine + "Please, check log file for more information.";
            MessageBox.Show(message);
            Logging.Instance.WriteLine(e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
            e.Handled = true;
        }
    }
}