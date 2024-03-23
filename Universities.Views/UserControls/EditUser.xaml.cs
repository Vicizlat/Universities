using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Universities.Handlers;
using Universities.Utils;

namespace Universities.Views.UserControls
{
    public partial class EditUser : UserControl
    {
        public bool IsPasswordChanged { get; set; }
        private List<string[]> UsersList = new List<string[]>();

        public EditUser()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                IsPasswordChanged = false;
                UsersList.Clear();
                if (!User.Role.Contains("admin")) UsersList.Add(User.ToArray());
                else UsersList.AddRange((await PhpHandler.GetFromTableAsync("users")).Select(u => u.Split(";")));
                SuperAdmin.Visibility = User.Role == "superadmin" ? Visibility.Visible : Visibility.Hidden;
                cbRemoveUser.Visibility = User.Role.Contains("admin") ? Visibility.Visible : Visibility.Hidden;
                cbSetRole.Visibility = User.Role.Contains("admin") ? Visibility.Visible : Visibility.Hidden;
                Users.ItemsSource = UsersList.Select(u => u[1]);
                Users.SelectedItem = User.Username;
            }
        }

        private void Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Password.Text = string.Empty;
            cbRemoveUser.IsChecked = false;
            Item0.Content = $"Unchanged ({UsersList.ElementAtOrDefault(Users.SelectedIndex)?[3]})";
            cbSetRole.SelectedIndex = 0;
            if (UsersList.ElementAtOrDefault(Users.SelectedIndex)?[1] != User.Username)
            {
                cbRemoveUser.IsEnabled = true;
                cbSetRole.IsEnabled = true;
            }
            else
            {
                cbRemoveUser.IsEnabled = false;
                cbSetRole.IsEnabled = false;
            }
            Confirm.IsEnabled = IsConfirmEnabled();
        }

        private void Text_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            cbRemoveUser.IsEnabled = string.IsNullOrEmpty(Password.Text);
            cbSetRole.IsEnabled = string.IsNullOrEmpty(Password.Text);
            Confirm.IsEnabled = IsConfirmEnabled();
        }

        private void cbRemoveUser_Click(object sender, RoutedEventArgs e)
        {
            Password.IsEnabled = cbRemoveUser.IsChecked == false;
            cbSetRole.IsEnabled = cbRemoveUser.IsChecked == false;
            Confirm.IsEnabled = IsConfirmEnabled();
        }

        private void cbSetRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Password.IsEnabled = cbSetRole.SelectedIndex == 0;
            cbRemoveUser.IsEnabled = cbSetRole.SelectedIndex == 0;
            Confirm.IsEnabled = IsConfirmEnabled();
        }

        private bool IsConfirmEnabled()
        {
            bool changingPassword = !string.IsNullOrEmpty(Password.Text) && cbRemoveUser.IsChecked == false && cbSetRole.SelectedIndex == 0;
            bool removingUser = string.IsNullOrEmpty(Password.Text) && cbRemoveUser.IsChecked == true && cbSetRole.SelectedIndex == 0;
            bool changingPrivileges = string.IsNullOrEmpty(Password.Text) && cbRemoveUser.IsChecked == false && cbSetRole.SelectedIndex > 0;
            return Users.SelectedItem != null && (changingPassword || removingUser || changingPrivileges);
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string[] selUser = UsersList.ElementAtOrDefault(Users.SelectedIndex);
            if (cbRemoveUser.IsChecked == true)
            {
                string msg = $"This will permanently delete user {selUser[1]}. It will not change any of the documents this user is assigned to. If you want to change these assignments you will have to do it manually.";
                string question = $"Are you sure you want to permanently delete user {selUser[1]}?";
                if (PromptBox.Question(msg, question))
                {
                    bool result = await PhpHandler.DeleteFromTable("users", int.Parse(selUser[0]));
                    if (result) PromptBox.Information($"User {selUser[1]} has been removed!");
                    else PromptBox.Error("There was an error removing user. Check log for more details.");
                }
            }
            else if (cbSetRole.SelectedIndex > 0)
            {
                bool result = await PhpHandler.UpdateInTable("users", "Role", cbSetRole.Text.ToLower(), id: int.Parse(selUser[0]));
                if (result) PromptBox.Information($"{selUser[1]}'s role was changed successfully!");
                else PromptBox.Error("There was an error updating user. Check log for more details.");
            }
            else if (!string.IsNullOrEmpty(Password.Text))
            {
                bool result = await PhpHandler.UpdateInTable("users", "Password", Password.Text, id: int.Parse(selUser[0]));
                if (result)
                {
                    if (selUser[1] == User.Username)
                    {
                        Settings.Instance.DecryptedPassword = Password.Text;
                        IsPasswordChanged = true;
                    }
                    PromptBox.Information($"{selUser[1]}'s password was changed successfully!");
                }
                else PromptBox.Error("There was an error updating user. Check log for more details.");
            }
            Visibility = Visibility.Hidden;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Visibility = Visibility.Hidden;
    }
}