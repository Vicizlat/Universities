using System.Windows;
using System.Windows.Input;
using Universities.Controller;
using Universities.Utils;

namespace Universities.Views
{
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            Server.Text = Settings.Instance.Server;
            Port.Text = $"{Settings.Instance.Port}";
            Databases.ItemsSource = SqlCommands.GetDatabases();
            Databases.SelectedItem = Settings.Instance.Database;
            Username.Text = Settings.Instance.Username;
            Password.PassBox.Password = Settings.Instance.Password;
            Separator.Text = Settings.Instance.Separator.ToString();
            PeopleStartId.Text = $"{Settings.Instance.PeopleStartId}";
            OrgaStartId.Text = $"{Settings.Instance.OrgaStartId}";
            cbShowParOrg.IsChecked = Settings.Instance.ShowParentOrganization;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.Server = Server.Text;
            Settings.Instance.Port = int.Parse(Port.Text);
            Settings.Instance.Database = (string)Databases.SelectedItem;
            Settings.Instance.Username = Username.Text;
            Settings.Instance.Password = Password.PassBox.Password;
            Settings.Instance.Separator = Separator.Text[0];
            Settings.Instance.PeopleStartId = int.Parse(PeopleStartId.Text);
            Settings.Instance.OrgaStartId = int.Parse(OrgaStartId.Text);
            Settings.Instance.ShowParentOrganization = cbShowParOrg.IsChecked == true;
            DialogResult = Settings.Instance.WriteSettingsFile();
            MessageBox.Show("All settings saved successfully. Please note that most settings require a restart to take effect.");
            Close();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow(false).ShowDialog();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveButton_Click(sender, e);
            }
            if (e.Key == Key.Escape)
            {
                CancelButton_Click(this, e);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}