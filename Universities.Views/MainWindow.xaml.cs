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
            if (controller.Organizations.Count > 0)
            {
                SelectOrganization.ItemsSource = controller.Organizations.Select(o => controller.GetOrganizationDisplayName(o.OrganizationId));
            }
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
            SelectOrganization.ItemsSource = controller.Organizations.Select(o => controller.GetOrganizationDisplayName(o.OrganizationId));
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

        private void AddOrganization_OnClick(object sender, RoutedEventArgs e)
        {
            AddOrganization addOrganizationWindow = new AddOrganization(controller);
            addOrganizationWindow.ShowDialog();
        }

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
            int orgId;
            int personId;
            if (lvSimilarProcessedAuthors.SelectedItem != null)
            {
                orgId = int.Parse(((string[])lvSimilarProcessedAuthors.SelectedItem)[3]);
                personId = int.Parse(((string[])lvSimilarProcessedAuthors.SelectedItem)[0]);
            }
            else
            {
                orgId = controller.Organizations[SelectOrganization.SelectedIndex].OrganizationId;
                personId = controller.GetPersonId(docArray[20], docArray[17], orgId);
            }
            List<string[]> documents = new List<string[]>() { docArray };
            documents.AddRange(lvSimilarPendingAuthors.SelectedItems.OfType<string[]>());
            foreach (string[] doc in documents)
            {
                controller.AddPerson(new string[] { $"{personId}", doc[20], doc[17], $"{orgId}", doc[0], doc[14], "", "", "", "" });
                controller.UpdateDocument(doc, true);
            }
            controller.UpdateDocuments();
        }

        private void PopulateFields()
        {
            docArray = docId >= 0 && docId < controller.Documents.Count ? controller.Documents[docId].ToArray() : Array.Empty<string>();
            if (docArray.Length < 0) docId = -1;
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
                OrganizationNames.TextBox.Text = string.Empty;
                SubOrganizationName.TextBox.Text = string.Empty;
                lvSimilarPendingAuthors.ItemsSource = new List<string[]>();
                lvSimilarProcessedAuthors.ItemsSource = new List<string[]>();
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
                OrganizationNames.TextBox.Text = sb.ToString().TrimEnd();
                SubOrganizationName.TextBox.Text = docArray[13];
                lvSimilarPendingAuthors.ItemsSource = controller.Documents
                    .Where(d => d.Ut != docArray[0] && d.LastName == docArray[17])
                    .Select(d => d.ToArray());
                List<string[]> similarProcessedAuthors = new List<string[]>();
                foreach (string[] author in controller.People.Where(p => p.LastName == docArray[17]).Select(p => p.ToArray()))
                {
                    similarProcessedAuthors.Add(author.Append(controller.GetOrganizationDisplayName(int.Parse(author[3]))).ToArray());
                }
                lvSimilarProcessedAuthors.ItemsSource = similarProcessedAuthors;
            }
        }

        private void SelectOrganization_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveButton.IsEnabled = IsSaveEnabled();
            if (sender is ListView && lvSimilarProcessedAuthors.SelectedItem != null)
            {
                SelectOrganization.SelectedItem = ((string[])lvSimilarProcessedAuthors.SelectedItem)[10];
            }
        }

        private bool IsSaveEnabled()
        {
            if (lvSimilarProcessedAuthors.SelectedItem != null) return true;
            return SelectOrganization.SelectedIndex >= 0;
        }
    }
}