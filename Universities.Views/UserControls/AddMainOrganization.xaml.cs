using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Universities.Handlers;
using Universities.Utils;

namespace Universities.Views.UserControls
{
    public partial class AddMainOrganization : UserControl
    {
        private IEnumerable<string[]> MainOrgs = new List<string[]>();

        public AddMainOrganization()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                string[] mainOrgs = await PhpHandler.GetFromTableAsync("main_organizations");
                MainOrgs = mainOrgs.Select(o => o.Split(";"));
                DelMainOrgSelect.ItemsSource = MainOrgs.Select(o => o[1]);
            }
            else
            {
                MainOrgName.Text = string.Empty;
                MainOrgPreffix.Text = string.Empty;
                DelMainOrgSelect.SelectedItem = null;
            }
        }

        private void AddMainOrg_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            bool isValid = !string.IsNullOrEmpty(MainOrgName.Text);
            MainOrgName.Background = isValid ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Bisque);
            Confirm.IsEnabled = isValid;
            MainOrgPreffix.Text = MainOrgName.Text.Replace(" ", "_").ToLower();
        }

        public async void AddMainOrgConfirm_Click(object sender, RoutedEventArgs e)
        {
            string result = await PhpHandler.AddMainOrgAsync(MainOrgName.Text, MainOrgPreffix.Text);
            PromptBox.Information(result);
            if (result.StartsWith("Error")) return;
            Visibility = Visibility.Hidden;
        }

        private void DelMainOrgSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DelMainOrgButton.IsEnabled = true;
        }

        private async void DelMainOrgButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedMainOrg = DelMainOrgSelect.SelectedItem.ToString();
            if (PromptBox.Question(string.Format(Constants.InfDeleteMainOrg, selectedMainOrg), Constants.QuestionAreYouSure))
            {
                if (int.TryParse(MainOrgs.FirstOrDefault(mo => mo[1] == selectedMainOrg)[0], out int mainOrgId))
                {
                    await PhpHandler.DeleteFromTable("main_organizations", mainOrgId);
                    await PhpHandler.DeleteMainOrg(MainOrgs.FirstOrDefault(mo => mo[1] == selectedMainOrg)[2]);
                    Visibility = Visibility.Hidden;
                }
            }
        }

        public void AddMainOrgCancel_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }
    }
}