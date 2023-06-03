using System.Windows;
using System.Windows.Input;
using Universities.Handlers;
using Universities.Utils;

namespace Universities.Views
{
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = Settings.Instance;
            Databases.ItemsSource = SqlCommands.GetDatabases();
            Databases.SelectedValue = Settings.Instance.Database;
            EditUserButton.Content = SqlCommands.CurrentUser.Item2 ? "Edit Users" : "Change Password";
            if (!SqlCommands.CurrentUser.Item2) AddDatabaseButton.Visibility = Visibility.Hidden;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.Database = (string)Databases.SelectedValue;
            Settings.Instance.WriteSettingsFile();
            MessageBox.Show("All settings saved successfully. Please note that most settings require a restart to take effect.");
            DialogResult = true;
            Close();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            new EditUserWindow().ShowDialog();
            Password.Text = Settings.Instance.Password;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && NewDbName.Visibility == Visibility.Hidden)
            {
                SaveButton_Click(sender, e);
            }
            if (e.Key == Key.Escape && NewDbName.Visibility == Visibility.Hidden)
            {
                CancelButton_Click(this, e);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Numbers_KeyDown(object sender, KeyEventArgs e) => e.Handled = KeyPressHandler.NotNumbers(e);

        private void AddDatabase_Click(object sender, RoutedEventArgs e)
        {
            NewDbName.Visibility = Visibility.Visible;
            NewDbName.Focus();
        }

        private void NewDbName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SqlCommands.AddDatabase(NewDbName.Text);
                Databases.ItemsSource = SqlCommands.GetDatabases();
                NewDbName.Text = string.Empty;
                NewDbName.Visibility = Visibility.Hidden;
            }
            if (e.Key == Key.Escape)
            {
                NewDbName.Text = string.Empty;
                NewDbName.Visibility = Visibility.Hidden;
            }
        }
    }
}