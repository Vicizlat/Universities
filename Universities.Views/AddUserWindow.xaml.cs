using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Universities.Utils;

namespace Universities.Views
{
    public partial class AddUserWindow
    {
        public AddUserWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Text_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            AddUser.IsEnabled = !string.IsNullOrEmpty(Username.TextBox.Text) && !string.IsNullOrEmpty(Password.TextBox.Text);
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            SqlCommands.AddUser(Username.CustomText, Password.CustomText, cbSetAdmin.SelectedIndex == 1);
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && AddUser.IsEnabled)
            {
                AddUserButton_Click(this, e);
            }
            if (e.Key == Key.Escape)
            {
                Cancel_Click(this, e);
            }
        }
    }
}