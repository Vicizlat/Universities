using System.Windows;
using System.Windows.Input;
using Universities.Handlers;
using Universities.Utils;

namespace Universities.Views
{
    public partial class SettingsWindow
    {
        public string Database { get; set; }

        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = Settings.Instance;
            EditUserButton.Content = SqlCommands.CurrentUser.Item2 ? "Edit Users" : "Change Password";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
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

        private void Numbers_KeyDown(object sender, KeyEventArgs e) => e.Handled = KeyPressHandler.NotNumbers(e);
    }
}