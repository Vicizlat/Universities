using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Universities.Utils;

namespace Universities.Views
{
    public partial class LoginWindow
    {
        public string WindowTitle { get; set; }
        public string ConfirmButtonText { get; set; }
        public bool IsLogin { get; }
        public bool IsAdmin { get; set; }

        public string UsernameText = string.Empty;
        public string PasswordText = string.Empty;

        public LoginWindow(int attempts, bool isLogin)
        {
            InitializeComponent();
            DataContext = this;
            IsLogin = isLogin;
            if (isLogin)
            {
                WindowTitle = "Please enter username and password to login";
                ConfirmButtonText = "Login";
                if (attempts < 3)
                {
                    Alert1.Visibility = Visibility.Visible;
                    Alert2.Visibility = Visibility.Visible;
                    Alert2.Text += attempts;
                }
            }
            else
            {
                SetAdmin.Visibility = Visibility.Visible;
                SetAdmin.IsChecked = false;
                OpenSettingsButton.Visibility = Visibility.Hidden;
                WindowTitle = "Enter username and password for the new User";
                ConfirmButtonText = "Add User";
            }
        }

        private void Text_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Login.IsEnabled = !string.IsNullOrEmpty(Username.Text) && !string.IsNullOrEmpty(Password.PassBox.Password);
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Login.IsEnabled = !string.IsNullOrEmpty(Username.Text) && !string.IsNullOrEmpty(Password.PassBox.Password);
        }

        private void OpenSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = new SettingsWindow().ShowDialog().Value;
            Close();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            UsernameText = Username.Text;
            PasswordText = Password.PassBox.Password;
            if (IsLogin)
            {
                Settings.Instance.Username = Username.Text;
                Settings.Instance.Password = Password.PassBox.Password;
                Settings.Instance.WriteSettingsFile();
            }
            IsAdmin = SetAdmin.IsChecked.HasValue ? SetAdmin.IsChecked.Value : false;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Login.IsEnabled)
            {
                Login_Click(this, e);
            }
            if (e.Key == Key.Escape)
            {
                Cancel_Click(this, e);
            }
        }
    }
}