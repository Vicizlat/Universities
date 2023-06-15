using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Universities.Controller;
using Universities.Utils;

namespace Universities.Views
{
    public partial class MainWindow
    {
        private readonly MainController controller;
        private WaitWindow WaitWindow;
        private string[] docArray = Array.Empty<string>();
        private int docId = -1;

        public MainWindow(MainController controller)
        {
            InitializeComponent();
            DataContext = this;
            this.controller = controller;
            if (!SqlCommands.CurrentUser.Item2)
            {
                AddUser.Visibility = Visibility.Hidden;
                DataManage.Visibility = Visibility.Hidden;
            }
            if (controller.Organizations.Count > 0)
            {
                SelectOrganization.ItemsSource = controller.OrganizationsDisplayNames;
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
            SelectOrganization.ItemsSource = controller.OrganizationsDisplayNames;
            SelectOrganization.SelectedIndex = -1;
            SaveButton.IsEnabled = IsSaveEnabled();
        }

        private void AddUserIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            new AddUserWindow().ShowDialog();
        }

        private void ImportExportIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            new DataManagementWindow(controller).ShowDialog();
        }

        private void SettingsIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            new SettingsWindow().ShowDialog();
        }

        private void AddOrganization_OnClick(object sender, RoutedEventArgs e)
        {
            new AddOrganization(controller).ShowDialog();
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
            ShowWaitWindow();
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
                controller.AddPerson(new string[] { $"{personId}", doc[20], doc[17], $"{orgId}", doc[0], doc[14], "", "", "" });
                controller.UpdateDocument(doc, true);
            }
            controller.UpdateDocuments();
            CloseWaitWindow();
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
                Address.Text = string.Empty;
                SubOrganizationName.Text = string.Empty;
                lvSimilarPendingAuthors.ItemsSource = new List<string[]>();
                lvSimilarProcessedAuthors.ItemsSource = new List<string[]>();
                lvAcadPersonnel.ItemsSource = new List<string[]>();
                lvSimilarProcessedAuthors.Background = Brushes.Transparent;
            }
            else
            {
                ALastName.TextBox.Text = docArray[17];
                AFirstName.TextBox.Text = docArray[20];
                Wos.TextBox.Text = docArray[0];
                Address.Text = docArray[7];
                SubOrganizationName.Text = docArray[13];
                lvSimilarPendingAuthors.ItemsSource = controller.Documents.Where(d => d.Ut != docArray[0] && d.LastName == docArray[17]).Select(d => d.ToArray());
                List<string[]> similarProcessedAuthors = new List<string[]>();
                foreach (string[] author in controller.People.Where(p => p.LastName == docArray[17]).Select(p => p.ToArray()))
                {
                    if (similarProcessedAuthors.Any(a => a[0] == author[0]))
                    {
                        string[] a = similarProcessedAuthors.FirstOrDefault(a => a[0] == author[0]);
                        if (a[1] != author[1] && !a[11].Contains(author[1])) a[11] += " | " + author[1];
                        continue;
                    }
                    string orgDisplayName = controller.Organizations.FirstOrDefault(o => o.OrganizationId == int.Parse(author[3])).GetDisplayName(controller.Organizations);
                    similarProcessedAuthors.Add(author.Append(orgDisplayName).Append(author[1]).ToArray());
                }
                lvSimilarProcessedAuthors.ItemsSource = similarProcessedAuthors.OrderBy(a => a[2]).ThenBy(a => a[1]);
                List<string[]> acadPersonnel = new List<string[]>();
                foreach (string[] author in controller.AcadPersonnel.Where(p => p.LastNames.Contains(docArray[17])).Select(p => p.ToArray()))
                {
                    acadPersonnel.Add(author);
                }
                lvAcadPersonnel.ItemsSource = acadPersonnel.OrderBy(a => a[1]).ThenBy(a => a[0]);
                if (lvSimilarProcessedAuthors.Items.Count > 0)
                {
                    lvSimilarProcessedAuthors.Background = Brushes.LightGreen;
                }
                else
                {
                    lvSimilarProcessedAuthors.Background = Brushes.Transparent;
                }
            }
        }

        private void SelectOrganization_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveButton.IsEnabled = IsSaveEnabled();
            if (sender == lvSimilarProcessedAuthors && lvSimilarProcessedAuthors.SelectedItem != null)
            {
                SelectOrganization.SelectedItem = ((string[])lvSimilarProcessedAuthors.SelectedItem)[10];
            }
            if (sender == lvAcadPersonnel && lvAcadPersonnel.SelectedItem != null)
            {
                string acadPersFac = ((string[])lvAcadPersonnel.SelectedItem)[2];
                string acadPersDep = ((string[])lvAcadPersonnel.SelectedItem)[3];
                string orgName = string.IsNullOrEmpty(acadPersDep) ? acadPersFac : acadPersDep;
                for (int i = 0; i < SelectOrganization.Items.Count; i++)
                {
                    if (SelectOrganization.Items[i].ToString().Contains(orgName)) SelectOrganization.SelectedIndex = i;
                }
            }
        }

        private bool IsSaveEnabled()
        {
            if (lvSimilarProcessedAuthors.SelectedItem != null) return true;
            return SelectOrganization.SelectedIndex >= 0;
        }

        public void ShowWaitWindow(string text = null)
        {
            WaitWindow = new WaitWindow(text);
            WaitWindow.Show();
        }

        public void CloseWaitWindow()
        {
            if (WaitWindow != null) WaitWindow.Close();
        }
    }
}