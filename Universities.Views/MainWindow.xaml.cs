using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Universities.Controller;

namespace Universities.Views
{
    public partial class MainWindow
    {
        private readonly MainController controller;
        private int docId = -1;
        public string[] docArray;

        public MainWindow(MainController controller)
        {
            InitializeComponent();
            DataContext = this;
            this.controller = controller;
            Title = $"Universities v. {controller.InstalledVersion}     Current user: {controller.CurrentUser}";
            if (!controller.IsAdmin)
            {
                AddUser.Visibility = Visibility.Hidden;
                DataManage.Visibility = Visibility.Hidden;
            }
            if (controller.Organizations.Count > 0) SelectOrganization.ItemsSource = controller.Organizations.Select(controller.GetOrganizationName);
            PopulateFields();
            controller.OnDocumentsChanged += OnDocumentsChanged;
            controller.OnOrganizationsChanged += OnOrganizationsChanged;
        }

        private void OnDocumentsChanged(object? sender, EventArgs e)
        {
            PopulateFields();
        }

        private void OnOrganizationsChanged(object? sender, EventArgs e)
        {
            SelectOrganization.ItemsSource = controller.Organizations.Select(controller.GetOrganizationName);
            SelectOrganization.SelectedIndex = -1;
            SaveButton.IsEnabled = IsSaveEnabled();
        }

        private void AddUserIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow(true);
            if (loginWindow.ShowDialog() == true) SqlCommands.AddUser(loginWindow.UsernameText, loginWindow.PasswordText, loginWindow.SetAdmin);
        }

        private void ImportExportIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataManagementWindow dataManagementWindow = new DataManagementWindow(controller);
            dataManagementWindow.ShowDialog();
        }

        private void SettingsIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        private void AddOrganization_OnClick(object sender, RoutedEventArgs e) => new AddOrganization(controller).ShowDialog();

        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            docId--;
            PopulateFields();
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            docId++;
            PopulateFields();
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            int orgId = controller.Organizations[SelectOrganization.SelectedIndex].OrganizationId;
            int personId = controller.GetPersonId(docArray[20], docArray[17]);
            controller.AddPerson(personId, docArray[20], docArray[17], orgId, docArray[0], int.Parse(docArray[14]));
            controller.SetDocumentProcessedStatus(docArray, true);
            foreach (string[] item in lvSimilarAuthors.SelectedItems)
            {
                controller.AddPerson(personId, item[20], item[17], orgId, item[0], int.Parse(item[14]));
                controller.SetDocumentProcessedStatus(item, true);
            }
            //if (controller.AddPerson(personId, firstName, lastName, orgId, docArray[0], int.Parse(docArray[14])))
            //{
            //    if (controller.SaveDocuments())
            //    {
            //        PopulateFields();
            //        return;
            //    }
            //}
            //MessageBox.Show("Failed to save Person. No changes were made.");
        }

        private void PopulateFields()
        {
            docArray = controller.GetDocumentArray(docId);
            SaveButton.IsEnabled = IsSaveEnabled();
            SelectOrganization.SelectedIndex = -1;
            PreviousButton.IsEnabled = docId >= 0;
            PreviousButton.Content = $"<< Previous {(docId < 0 ? "(0)" : $"({docId})")}";
            NextButton.IsEnabled = docId < controller.Documents.Count - 1;
            NextButton.Content = $"({controller.Documents.Count - docId - 1}) Next >>";
            if (docId < 0)
            {
                ALastName.TextBox.Text = string.Empty;
                AFirstName.TextBox.Text = string.Empty;
                Wos.TextBox.Text = string.Empty;
                SeqNo.TextBox.Text = string.Empty;
                Address.TextBox.Text = string.Empty;
                OrganizationNames.Text = string.Empty;
                SubOrganizationNames.Text = string.Empty;
                lvSimilarAuthors.ItemsSource = new List<string[]>();
            }
            else
            {
                ALastName.TextBox.Text = docArray[17];
                AFirstName.TextBox.Text = docArray[20];
                Wos.TextBox.Text = docArray[0];
                SeqNo.TextBox.Text = docArray[14];
                Address.TextBox.Text = docArray[7];
                StringBuilder sb = new StringBuilder()
                    .AppendLine(docArray[8])
                    .AppendLine(docArray[9])
                    .AppendLine(docArray[10])
                    .AppendLine(docArray[11])
                    .AppendLine(docArray[12]);
                OrganizationNames.Text = sb.ToString().TrimEnd();
                SubOrganizationNames.Text = docArray[13];
                lvSimilarAuthors.ItemsSource = controller.Documents
                    .Where(d => d.Ut != docArray[0] && d.LastName == docArray[17])
                    .Select(d => d.ToArray());
            }
        }

        private void SelectOrganization_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveButton.IsEnabled = IsSaveEnabled();
        }

        private bool IsSaveEnabled()
        {
            return SelectOrganization.SelectedIndex >= 0 && !string.IsNullOrEmpty(Wos.TextBox.Text) && !string.IsNullOrEmpty(ALastName.TextBox.Text);
        }
    }
}