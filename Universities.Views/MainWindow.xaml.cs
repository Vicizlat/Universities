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
        private readonly string version;
        private WaitWindow WaitWindow;
        private string[] docArray = Array.Empty<string>();
        private int docId = Settings.Instance.LastDocNo;
        private int selectedOrgIndex = -1;

        public MainWindow(MainController controller, string version)
        {
            InitializeComponent();
            DataContext = this;
            this.version = version;
            Title = $"Universities     v.{version}     User: {SqlCommands.CurrentUser.Item1}     Database: {Settings.Instance.Database}";
            this.controller = controller;
            if (!SqlCommands.CurrentUser.Item2)
            {
                AddUser.Visibility = Visibility.Hidden;
                DataManage.Visibility = Visibility.Hidden;
            }
            if (controller.Organizations.Count > 0)
            {
                SelectOrganization.AutoSuggestionList = controller.OrganizationsDisplayNames;
            }
            PopulateFields();
            SelectOrganization.OnSelectionChanged += SelectOrganization_SetSelectedIndex;
            controller.OnDocumentsChanged += OnDocumentsChanged;
            controller.OnOrganizationsChanged += OnOrganizationsChanged;
            DBAccess.OnDBChanged += DBAccess_OnDBChanged;
        }

        private void DBAccess_OnDBChanged(object sender, EventArgs e)
        {
            Title = $"Universities     v.{version}     User: {SqlCommands.CurrentUser.Item1}     Database: {Settings.Instance.Database}";
        }

        private void OnDocumentsChanged(object sender, EventArgs e)
        {
            PopulateFields();
        }

        private void OnOrganizationsChanged(object sender, EventArgs e)
        {
            SelectOrganization.AutoSuggestionList = controller.OrganizationsDisplayNames;
            selectedOrgIndex = -1;
            SaveButton.IsEnabled = SelectOrganization.Validate();
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
            int? personId = null;
            if (lvSimilarProcessedAuthors.SelectedItem != null)
            {
                orgId = int.Parse(((string[])lvSimilarProcessedAuthors.SelectedItem)[3]);
                personId = int.Parse(((string[])lvSimilarProcessedAuthors.SelectedItem)[0]);
            }
            else
            {
                orgId = controller.Organizations[selectedOrgIndex].OrganizationId;
                personId = controller.GetPersonId(docArray[20], docArray[17], orgId);
            }
            if (personId == null)
            {
                CloseWaitWindow();
                return;
            }
            List<string[]> documents = new List<string[]>() { docArray };
            documents.AddRange(lvSimilarPendingAuthors.SelectedItems.OfType<string[]>());
            foreach (string[] doc in documents)
            {
                controller.AddPerson(new string[] { $"{personId}", doc[20], doc[17], $"{orgId}", doc[0], "", "", "", "" });
                controller.UpdateDocument(doc, true);
            }
            controller.UpdateDocuments();
            CloseWaitWindow();
        }

        private void PopulateFields()
        {
            if (controller.Documents.Count == 0) docId = -1;
            if (controller.Documents.Count < docId) docId = controller.Documents.Count - 1;
            docArray = docId >= 0 && docId < controller.Documents.Count ? controller.Documents[docId].ToArray() : Array.Empty<string>();
            SaveButton.IsEnabled = SelectOrganization.Validate();
            selectedOrgIndex = -1;
            SelectOrganization.AutoTextBox.Text = string.Empty;
            PreviousButton.IsEnabled = docId >= 0;
            PreviousButton.Content = $"<< Previous {(docId < 0 ? "(0)" : $"({docId})")}";
            NextButton.IsEnabled = docId < controller.Documents.Count - 1;
            NextButton.Content = $"({controller.Documents.Count - docId - 1}) Next >>";
            if (docId < 0)
            {
                ALastName.Text = string.Empty;
                AFirstName.Text = string.Empty;
                Wos.Text = string.Empty;
                Address.Text = string.Empty;
                SubOrganizationName.Text = string.Empty;
                lvSimilarPendingAuthors.ItemsSource = new List<string[]>();
                lvSimilarProcessedAuthors.ItemsSource = new List<string[]>();
                lvAcadPersonnel.ItemsSource = new List<string[]>();
                lvSimilarProcessedAuthors.Background = Brushes.Transparent;
            }
            else
            {
                ALastName.Text = docArray[17];
                AFirstName.Text = docArray[20];
                Wos.Text = docArray[0];
                Address.Text = docArray[7];
                SubOrganizationName.Text = docArray[13];
                lvSimilarPendingAuthors.ItemsSource = GetSimilarPendingAuthors();
                lvSimilarProcessedAuthors.ItemsSource = GetSimilarProcessedAuthors();
                lvSimilarProcessedAuthors.Background = lvSimilarProcessedAuthors.Items.Count > 0 ? Brushes.LightGreen : Brushes.Transparent;
                lvAcadPersonnel.ItemsSource = GetMatchingAcadPersonnel();
            }
        }

        private IEnumerable<string[]> GetSimilarPendingAuthors()
        {
            return controller.Documents
                .Where(d => d.Ut != docArray[0] && controller.RegexMatch(d.LastName, docArray[17]))
                .OrderBy(d => d.LastName).ThenBy(d => d.FirstName)
                .Select(d => d.ToArray());
        }

        private List<string[]> GetSimilarProcessedAuthors()
        {
            List<string[]> similarProcessedAuthors = new List<string[]>();
            foreach (string[] author in controller.People.Where(p => controller.RegexMatch(p.LastName, docArray[17])).Select(p => p.ToArray()))
            {
                if (similarProcessedAuthors.Any(pa => controller.RegexMatch(pa[0], author[0])))
                {
                    string[] a = similarProcessedAuthors.FirstOrDefault(pa => controller.RegexMatch(pa[0], author[0]));
                    if (a[1] != author[1] && !a[11].Contains(author[1])) a[11] += " | " + author[1];
                    continue;
                }
                string orgDisplayName = controller.Organizations.FirstOrDefault(o => o.OrganizationId == int.Parse(author[3])).GetDisplayName(controller.Organizations);
                similarProcessedAuthors.Add(author.Append(orgDisplayName).Append(author[1]).ToArray());
            }
            return similarProcessedAuthors.OrderBy(a => a[2]).ThenBy(a => a[1]).ToList();
        }

        private IEnumerable<string[]> GetMatchingAcadPersonnel()
        {
            foreach (string[] author in controller.AcadPersonnel.Where(p => controller.RegexMatch(p.LastNames, docArray[17])).Select(p => p.ToArray()))
            {
                yield return author;
            }
        }

        private void SelectOrganization_SetSelectedIndex(object sender, EventArgs e)
        {
            selectedOrgIndex = (int)sender;
        }

        private void SelectOrganization_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == lvSimilarProcessedAuthors && lvSimilarProcessedAuthors.SelectedItem != null)
            {
                SelectOrganization.AutoTextBox.Text = ((string[])lvSimilarProcessedAuthors.SelectedItem)[10];
            }
            if (sender == lvAcadPersonnel && lvAcadPersonnel.SelectedItem != null)
            {
                string acadPersFac = ((string[])lvAcadPersonnel.SelectedItem)[2];
                string acadPersDep = ((string[])lvAcadPersonnel.SelectedItem)[3];
                string orgName = string.IsNullOrEmpty(acadPersDep) ? acadPersFac : acadPersDep;
                for (int i = 0; i < SelectOrganization.AutoSuggestionList.Count; i++)
                {
                    if (SelectOrganization.AutoSuggestionList[i].StartsWith(orgName))
                    {
                        SelectOrganization.AutoTextBox.Text = SelectOrganization.AutoSuggestionList[i];
                        selectedOrgIndex = i;
                    }
                }
            }
        }

        public void AutoTextBox_TextChanged(object sender, EventArgs e)
        {
            SaveButton.IsEnabled = SelectOrganization.Validate();
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

        private void Window_Closed(object sender, EventArgs e)
        {
            Settings.Instance.LastDocNo = docId;
            Settings.Instance.WriteSettingsFile();
            ImportExport.ExportBackupFiles();
            Application.Current.Shutdown(0);
        }
    }
}