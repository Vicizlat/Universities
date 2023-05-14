using System.Windows;
using System.Windows.Input;
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

        private void Numbers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.D0 && e.Key != Key.D1 && e.Key != Key.D2 && e.Key != Key.D3 && e.Key != Key.D4 &&
                e.Key != Key.D5 && e.Key != Key.D6 && e.Key != Key.D7 && e.Key != Key.D8 && e.Key != Key.D9 &&
                e.Key != Key.NumPad0 && e.Key != Key.NumPad1 && e.Key != Key.NumPad2 && e.Key != Key.NumPad3 &&
                e.Key != Key.NumPad4 && e.Key != Key.NumPad5 && e.Key != Key.NumPad6 && e.Key != Key.NumPad7 &&
                e.Key != Key.NumPad8 && e.Key != Key.NumPad9) e.Handled = true;
        }
    }
}