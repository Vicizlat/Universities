using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Universities.Handlers;
using Universities.Utils;

namespace Universities.Views.UserControls
{
    public partial class AddNewUser : UserControl
    {
        public AddNewUser()
        {
            InitializeComponent();
            DataContext = this;
            if (User.Role == "superadmin") cbRole.Items.Add(User.Role);
        }

        private void Text_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Username.Background = string.IsNullOrEmpty(Username.Text) ? new SolidColorBrush(Colors.Bisque) : new SolidColorBrush(Colors.White);
            Password.Background = string.IsNullOrEmpty(Password.Text) ? new SolidColorBrush(Colors.Bisque) : new SolidColorBrush(Colors.White);
            Confirm.IsEnabled = !string.IsNullOrEmpty(Username.Text) && !string.IsNullOrEmpty(Password.Text);
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string result = await PhpHandler.CreateOrVerifyUserAsync("create", Username.Text, Password.Text, cbRole.Text);
            PromptBox.Information(result);
            Visibility = Visibility.Hidden;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Visibility = Visibility.Hidden;
    }
}