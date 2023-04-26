using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Universities.Controller;
using Universities.Utils;

namespace Universities.Views
{
    public partial class LoginWindow
    {
        public bool SetAdmin { get; set; }

        public LoginWindow(bool addUser)
        {
            InitializeComponent();
            DataContext = this;
            if (addUser)
            {
                AddUser.Visibility = Visibility.Visible;
                SetAdminCheck.Visibility = Visibility.Visible;
                Title.Content = "Create new User:";
            }
            else
            {
                Confirm.Visibility = Visibility.Visible;
                Title.Content = "Change password for User:";
                SetAdminCheck.Visibility = Visibility.Hidden;
                Username.IsEnabled = false;
                Username.Text = Settings.Instance.Username;
                Password.Label = "New Password";
            }
        }

        private void Text_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Confirm.IsEnabled = !string.IsNullOrEmpty(Username.Text) && !string.IsNullOrEmpty(Password.PassBox.Password);
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Confirm.IsEnabled = !string.IsNullOrEmpty(Username.Text) && !string.IsNullOrEmpty(Password.PassBox.Password);
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            SqlCommands.AddUser(Username.Text, Password.PassBox.Password, SetAdmin);
            SetAdmin = SetAdminCheck.IsChecked == true;
            DialogResult = true;
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            SqlCommands.UpdateUserPassword(Username.Text, Password.PassBox.Password);
            Settings.Instance.Password = Password.PassBox.Password;
            DialogResult = Settings.Instance.WriteSettingsFile();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Confirm.IsEnabled)
            {
                ConfirmButton_Click(this, e);
            }
            if (e.Key == Key.Escape)
            {
                Cancel_Click(this, e);
            }
        }
    }
}