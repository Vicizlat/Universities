using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Universities.Handlers;
using Universities.Utils;

namespace Universities.Views
{
    public partial class SettingsWindow
    {
        private IEnumerable<string[]> MainOrgs = new List<string[]>();

        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = Settings.Instance;
            GetMainOrgs(MainOrg.Preffix);
            RegexPatternBox.ToolTip = Settings.Instance.RegexPattern;
            AddMainOrgButton.Visibility = User.Role.Contains("admin") ? Visibility.Visible : Visibility.Hidden;
            AddUserIcon.Visibility = User.Role.Contains("admin") ? Visibility.Visible : Visibility.Hidden;
            ResetAutoIncrement.Visibility = User.Role == "superadmin" ? Visibility.Visible : Visibility.Hidden;
            SaveButton.IsEnabled = false;
        }

        private async void GetMainOrgs(string selectedOrg)
        {
            string[] mainOrgs = await PhpHandler.GetFromTableAsync("main_organizations");
            MainOrgs = mainOrgs.Select(o => o.Split(";"));
            MainOrgSelect.ItemsSource = MainOrgs.Select(o => o[1]);
            if (MainOrgs.Any() && !string.IsNullOrEmpty(selectedOrg))
            {
                MainOrgSelect.SelectedValue = MainOrgs.FirstOrDefault(o => o[2] == selectedOrg)?[1];
            }
        }

        private void MainOrg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SaveButton == null) return;
            SaveButton.IsEnabled = IsSaveEnabled();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SaveButton == null) return;
            SaveButton.IsEnabled = ValidateTextBox((TextBox)sender) && IsSaveEnabled();
        }

        private bool ValidateTextBox(TextBox sender)
        {
            bool isValid = !string.IsNullOrEmpty(sender.Text);
            sender.Background = isValid ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Bisque);
            return isValid;
        }

        private void cbShowParentOrg_Click(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = IsSaveEnabled();
        }

        private bool IsSaveEnabled()
        {
            bool serverChanged = Server.Text != Settings.Instance.Server;
            bool usernameChanged = Username.Text != Settings.Instance.Username;
            bool passwordChanged = Password.Text != Settings.Instance.DecryptedPassword;
            bool mainOrgChanged = (string)MainOrgSelect.SelectedValue != MainOrg.Name;
            bool backupDaysChanged = !string.IsNullOrEmpty(BackupDays.Text) && int.Parse(BackupDays.Text) != Settings.Instance.BackupDaysToKeep;
            bool backupsPerDayChanged = !string.IsNullOrEmpty(BackupsPerDay.Text) && int.Parse(BackupsPerDay.Text) != Settings.Instance.BackupsPerDayToKeep;
            bool backupsChanged = backupDaysChanged || backupsPerDayChanged;
            bool separatorChanged = Separator.Text != Settings.Instance.Separator;
            bool showParentOrgChanged = cbShowParentOrg.IsChecked.GetValueOrDefault() != Settings.Instance.ShowParentOrganization;
            return serverChanged || usernameChanged || passwordChanged || mainOrgChanged || backupsChanged || separatorChanged || showParentOrgChanged;
        }

        private void EditMainOrgButton_Click(object sender, RoutedEventArgs e) => AddMainOrg.Visibility = Visibility.Visible;

        private void AddUserIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => AddUser.Visibility = Visibility.Visible;

        private void EditUserIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => EditUsers.Visibility = Visibility.Visible;

        private void OtherOptions_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (EditUsers.Visibility == Visibility.Hidden && EditUsers.IsPasswordChanged)
            {
                Password.Text = Settings.Instance.DecryptedPassword;
                Settings.Instance.WriteSettingsFile();
            }
            if (AddMainOrg.Visibility == Visibility.Visible || AddUser.Visibility == Visibility.Visible || EditUsers.Visibility == Visibility.Visible)
            {
                SaveButton.IsEnabled = false;
                CancelButton.IsEnabled = false;
            }
            else
            {
                GetMainOrgs(AddMainOrg.MainOrgName.Text);
                SaveButton.IsEnabled = IsSaveEnabled();
                CancelButton.IsEnabled = true;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && SaveButton.IsEnabled) SaveButton_Click(sender, e);
            if (e.Key == Key.Escape && CancelButton.IsEnabled) CancelButton_Click(sender, e);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (User.CheckUser(await PhpHandler.CreateOrVerifyUserAsync("verify", Username.Text, Password.Text)))
            {
                string selectedMainOrg = (string)MainOrgSelect.SelectedValue;
                if (!string.IsNullOrEmpty(selectedMainOrg) && selectedMainOrg != MainOrg.Name)
                {
                    string selMainOrgPref = MainOrgs.FirstOrDefault(o => o[1] == selectedMainOrg)?[2];
                    MainOrg.CheckMainOrg(await PhpHandler.GetFromTableAsync("main_organizations"), selMainOrgPref);
                    await PhpHandler.UpdateInTable("users", "LastMainOrg", selMainOrgPref, id: User.Id);
                }
                Settings.Instance.ShowParentOrganization = cbShowParentOrg.IsChecked.GetValueOrDefault();
                Settings.Instance.WriteSettingsFile();
                DialogResult = PromptBox.Information("All settings saved successfully.");
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Numbers_KeyDown(object sender, KeyEventArgs e) => e.Handled = !KeyPressHandler.Numbers(e.Key);

        private async void AddToPatternButton_Click(object sender, RoutedEventArgs e)
        {
            string newPattern = AddToPattern.Text;
            string[] regex = await PhpHandler.GetFromTableAsync("regex_patterns");
            Settings.Instance.RegexPattern = string.Join("|", regex.Select(rp => rp.Split(";")[1]));
            if (!string.IsNullOrEmpty(newPattern) && !Settings.Instance.RegexPattern.Contains(newPattern))
            {
                if (await PhpHandler.AddToTable("regex_patterns", "regex", newPattern))
                {
                    AddToPattern.Text = string.Empty;
                    (FindResource("Animation") as Storyboard).Begin();
                    Settings.Instance.RegexPattern += $"|{newPattern}";
                    RegexPatternBox.ToolTip = Settings.Instance.RegexPattern;
                    Settings.Instance.WriteSettingsFile();
                }
            }
        }

        private async void ResetAutoIncrement_Click(object sender, RoutedEventArgs e)
        {
            PromptBox.Information(await PhpHandler.ResetAutoIncrement("regex_patterns"));
            PromptBox.Information(await PhpHandler.ResetAutoIncrement("users"));
            PromptBox.Information(await PhpHandler.ResetAutoIncrement("main_organizations"));
        }
    }
}