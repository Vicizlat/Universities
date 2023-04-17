using System;
using System.Windows;
using System.Windows.Input;
using Universities.Utils;

namespace Universities.Views
{
    public partial class SettingsWindow
    {
        public SettingsWindow(bool allowAddUser = false)
        {
            InitializeComponent();
            Server.Text = Settings.Instance.Server;
            Port.Text = Settings.Instance.Port;
            Database.Text = Settings.Instance.Database;
            Username.Text = Settings.Instance.Username;
            Password.PassBox.Password = Settings.Instance.Password;
            Separator.Text = Settings.Instance.Separator.ToString();
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.Server = Server.Text;
            Settings.Instance.Port = Port.Text;
            Settings.Instance.Database = Database.Text;
            Settings.Instance.Username = Username.Text;
            Settings.Instance.Password = Password.PassBox.Password;
            Settings.Instance.Separator = Separator.Text[0];
            Settings.Instance.WriteSettingsFile();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}