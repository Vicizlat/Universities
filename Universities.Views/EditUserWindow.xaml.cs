using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Universities.Utils;

namespace Universities.Views
{
    public partial class EditUserWindow
    {
        private string currentUser;
        private string selectedUser;
        private List<Tuple<string, bool>> users;

        public EditUserWindow()
        {
            InitializeComponent();
            DataContext = this;
            currentUser = SqlCommands.CurrentUser.Item1;
            cbSetAdmin.SelectedIndex = 0;
            users = SqlCommands.GetUsers();
            if (SqlCommands.CurrentUser.Item2)
            {
                Users.ItemsSource = users.Select(t => t.Item1);
                cbRemoveUser.Visibility = Visibility.Visible;
                cbSetAdmin.Visibility = Visibility.Visible;
            }
            else
            {
                Users.ItemsSource = users.Where(u => u.Item1 == currentUser).Select(t => t.Item1);
                cbRemoveUser.Visibility = Visibility.Hidden;
                cbSetAdmin.Visibility = Visibility.Hidden;
            }
            Users.SelectedItem = currentUser;
            selectedUser = currentUser;
        }

        private void Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedUser = Users.SelectedItem.ToString();
            Password.Text = string.Empty;
            cbRemoveUser.IsChecked = false;
            cbRemoveUser.IsEnabled = selectedUser != currentUser;
            Item0.Content = $"Unchanged ({(users[Users.SelectedIndex].Item2 ? "Admin" : "Not Admin")})";
            cbSetAdmin.SelectedIndex = 0;
            cbSetAdmin.IsEnabled = selectedUser != currentUser;
            Confirm.IsEnabled = IsConfirmEnabled();
        }

        private void Text_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            cbRemoveUser.IsEnabled = string.IsNullOrEmpty(Password.Text);
            cbSetAdmin.IsEnabled = string.IsNullOrEmpty(Password.Text);
            Confirm.IsEnabled = IsConfirmEnabled();
        }

        private void cbRemoveUser_Click(object sender, RoutedEventArgs e)
        {
            Password.IsEnabled = cbRemoveUser.IsChecked == false;
            cbSetAdmin.IsEnabled = cbRemoveUser.IsChecked == false;
            Confirm.IsEnabled = IsConfirmEnabled();
        }

        private void cbSetAdmin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Password.IsEnabled = cbSetAdmin.SelectedIndex == 0;
            cbRemoveUser.IsEnabled = cbSetAdmin.SelectedIndex == 0;
            Confirm.IsEnabled = IsConfirmEnabled();
        }

        private bool IsConfirmEnabled()
        {
            bool changingPassword = !string.IsNullOrEmpty(Password.Text) && cbRemoveUser.IsChecked == false && cbSetAdmin.SelectedIndex == 0;
            bool removingUser = string.IsNullOrEmpty(Password.Text) && cbRemoveUser.IsChecked == true && cbSetAdmin.SelectedIndex == 0;
            bool changingPrivileges = string.IsNullOrEmpty(Password.Text) && cbRemoveUser.IsChecked == false && cbSetAdmin.SelectedIndex > 0;
            return !string.IsNullOrEmpty(selectedUser) && (changingPassword || removingUser || changingPrivileges);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (cbRemoveUser.IsChecked == true)
            {
                if (PromptBox.Question($"Are you sure you want to remove User '{selectedUser}'?")) SqlCommands.RemoveUser(selectedUser);
            }
            else if (cbSetAdmin.SelectedIndex > 0)
            {
                SqlCommands.ChangeUserPrivileges(selectedUser, cbSetAdmin.SelectedIndex == 2);
            }
            else if (!string.IsNullOrEmpty(Password.Text))
            {
                SqlCommands.ChangeUserPassword(selectedUser, Password.Text);
                if (selectedUser == currentUser)
                {
                    Settings.Instance.Password = Password.Text;
                    Settings.Instance.WriteSettingsFile();
                }
            }
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
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