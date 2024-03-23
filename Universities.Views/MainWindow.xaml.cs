using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Universities.Controller;
using Universities.Handlers;
using Universities.Utils;

namespace Universities.Views
{
    public partial class MainWindow
    {
        public MainController Controller;
        private readonly string version;
        private WaitWindow WaitWindow;
        private string[] docArray = Array.Empty<string>();
        private int docId = User.LastDocId;
        private int selectedOrgIndex = -1;

        public MainWindow(string version)
        {
            InitializeComponent();
            DataContext = this;
            this.version = version;
            DataManage.Visibility = User.Role.Contains("admin") ? Visibility.Visible : Visibility.Hidden;
            SelectOrganization.OnSelectionChanged += SelectOrganization_SetSelectedIndex;
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Controller.Organizations.Any())
            {
                if (Settings.Instance.ShowParentOrganization) SelectOrganization.AutoSuggestionList = Controller.Organizations.Select(o => o.OrgaNameWithParent);
                else SelectOrganization.AutoSuggestionList = Controller.Organizations.Select(o => o.OrganizationName);
            }
            Controller.OnDocumentsChanged += OnDocumentsChanged;
            Controller.OnOrganizationsChanged += OnOrganizationsChanged;
            Controller.OnMainOrgChanged += OnMainOrgChanged;
            CloseWaitWindow();
        }

        private void OnMainOrgChanged(object sender, EventArgs e)
        {
            Title = $"Universities\tv.{version}\t\tUser: {User.Username}\t\tMain organization: {MainOrg.Name}";
            docId = User.LastDocId;
            PopulateFields();
        }

        private void OnDocumentsChanged(object sender, EventArgs e)
        {
            PopulateFields();
        }

        private void OnOrganizationsChanged(object sender, EventArgs e)
        {
            if (Settings.Instance.ShowParentOrganization)
            {
                SelectOrganization.AutoSuggestionList = Controller.Organizations.Select(o => o.OrgaNameWithParent);
                ParentOrganization.ItemsSource = Controller.Organizations.Select(o => o.OrgaNameWithParent);
            }
            else
            {
                SelectOrganization.AutoSuggestionList = Controller.Organizations.Select(o => o.OrganizationName);
                ParentOrganization.ItemsSource = Controller.Organizations.Select(o => o.OrganizationName);
            }
            selectedOrgIndex = -1;
            SaveButton.IsEnabled = SelectOrganization.Validate();
        }

        private void ImportExportIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            new DataManagementWindow(Controller).ShowDialog();
        }

        private void SettingsIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool result = (bool)new SettingsWindow().ShowDialog();
            if (result)
            {
                Controller.MainOrgChangedAsync();
                DataManage.Visibility = User.Role.Contains("admin") ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void AddOrganization_OnClick(object sender, RoutedEventArgs e)
        {
            AddNewOrg.Visibility = Visibility.Visible;
        }

        private void NavButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender == PreviousButton) docId--;
            else if (sender == NextButton) docId++;
            PopulateFields();
        }

        private async void PopulateFields()
        {
            ShowWaitWindow("Loading Data...");
            if (Controller.Documents.Count() == 0) docId = -1;
            if (Controller.Documents.Count() < docId) docId = Controller.Documents.Count() - 1;
            docArray = docId >= 0 && docId < Controller.Documents.Count() ? Controller.Documents.ElementAtOrDefault(docId).ToArray() : Array.Empty<string>();
            SaveButton.IsEnabled = SelectOrganization.Validate();
            selectedOrgIndex = -1;
            SelectOrganization.AutoTextBox.Text = string.Empty;
            PreviousButton.IsEnabled = docId >= 0;
            PreviousButton.Content = $"<< Previous {(docId < 0 ? "(0)" : $"({docId})")}";
            NextButton.IsEnabled = docId < Controller.Documents.Count() - 1;
            NextButton.Content = $"({Controller.Documents.Count() - docId - 1}) Next >>";
            await PhpHandler.UpdateInTable("users", "LastDocId", $"{docId}", id: User.Id);
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
                ALastName.Text = docArray[21];
                AFirstName.Text = docArray[25];
                Wos.Text = docArray[1];
                Address.Text = docArray[8];
                SubOrganizationName.Text = docArray[14];
                lvSimilarPendingAuthors.ItemsSource = GetSimilarPendingAuthors();
                lvSimilarProcessedAuthors.ItemsSource = GetSimilarProcessedAuthors();
                lvSimilarProcessedAuthors.Background = lvSimilarProcessedAuthors.Items.Count > 0 ? Brushes.LightGreen : Brushes.Transparent;
                lvAcadPersonnel.ItemsSource = GetMatchingAcadPersonnel();
            }
            CloseWaitWindow();
        }

        private async void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowWaitWindow("Saving Person...");
            string[] simAuthor = (string[])lvSimilarProcessedAuthors.SelectedItem ?? new string[4];
            int orgId = int.TryParse(simAuthor[3], out int selOrgId) ? selOrgId : Controller.Organizations.ElementAtOrDefault(selectedOrgIndex).OrganizationId;
            int personId = int.TryParse(simAuthor[0], out int selPerId) ? selPerId : await Controller.GetPersonId(docArray[21], docArray[18], orgId);
            if (personId == 0)
            {
                CloseWaitWindow();
                return;
            }
            List<string[]> documents = new List<string[]>() { docArray };
            documents.AddRange(lvSimilarPendingAuthors.SelectedItems.OfType<string[]>());
            foreach (string[] doc in documents)
            {
                string newPerson = $"{personId};{doc[21]};{doc[18]};{orgId};{doc[1]};;;;";
                if (await PhpHandler.AddToTable($"{MainOrg.Preffix}_people", "person", newPerson))
                {
                    await PhpHandler.UpdateInTable($"{MainOrg.Preffix}_documents", "Processed", "1", id: int.Parse(doc[0]));
                }
            }
            await Controller.UpdatePeopleAsync();
            await Controller.UpdateDocuments();
            CloseWaitWindow();
        }

        private IEnumerable<string[]> GetSimilarPendingAuthors()
        {
            return Controller.Documents
                .Where(d => d.Ut != docArray[1] && Controller.RegexMatch(d.LastName, docArray[21]))
                .OrderBy(d => d.LastName).ThenBy(d => d.FirstName)
                .Select(d => d.ToArray());
        }

        private List<string[]> GetSimilarProcessedAuthors()
        {
            List<string[]> similarProcessedAuthors = new List<string[]>();
            foreach (string[] author in Controller.People.Where(p => Controller.RegexMatch(p.LastName, docArray[21])).Select(p => p.ToArray()))
            {
                if (similarProcessedAuthors.Any(pa => Controller.RegexMatch(pa[1], author[1])))
                {
                    string[] pa = similarProcessedAuthors.FirstOrDefault(pa => Controller.RegexMatch(pa[1], author[1]));
                    if (pa[2] != author[2] && !pa[11].Contains(author[2])) pa[11] += " | " + author[2];
                    continue;
                }
                string orgDisplayName;
                if (Settings.Instance.ShowParentOrganization)
                {
                    orgDisplayName = Controller.Organizations.FirstOrDefault(o => o.OrganizationId == int.Parse(author[3])).OrgaNameWithParent;
                }
                else orgDisplayName = Controller.Organizations.FirstOrDefault(o => o.OrganizationId == int.Parse(author[4])).OrganizationName;
                similarProcessedAuthors.Add(author.Append(orgDisplayName).Append(author[2]).ToArray());
            }
            return similarProcessedAuthors.OrderBy(a => a[3]).ThenBy(a => a[2]).ToList();
        }

        private IEnumerable<string[]> GetMatchingAcadPersonnel()
        {
            foreach (string[] author in Controller.AcadPersonnel.Where(p => Controller.RegexMatch(p.LastNames, docArray[21])).Select(p => p.ToArray()))
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
                SelectOrganization.AutoTextBox.Text = ((string[])lvSimilarProcessedAuthors.SelectedItem)[9];
            }
            if (sender == lvAcadPersonnel && lvAcadPersonnel.SelectedItem != null)
            {
                string acadPersFac = ((string[])lvAcadPersonnel.SelectedItem)[2].ToLower().Replace(",", "");
                string acadPersDep = ((string[])lvAcadPersonnel.SelectedItem)[3].ToLower().Replace(",", "");
                string orgName = string.IsNullOrEmpty(acadPersDep) ? acadPersFac : acadPersDep;
                for (int i = 0; i < SelectOrganization.AutoSuggestionList.Count(); i++)
                {
                    if (SelectOrganization.AutoSuggestionList.ElementAt(i).ToLower().Replace(",", "") == orgName)
                    {
                        SelectOrganization.AutoTextBox.Text = SelectOrganization.AutoSuggestionList.ElementAt(i);
                        selectedOrgIndex = i;
                        break;
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
            WaitWindow?.Close();
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            string workingFolder = string.Join('\\', Environment.CurrentDirectory.Split('\\').ToList().SkipLast(1));
            string backupsPath = Directory.CreateDirectory(Path.Combine(workingFolder, "Backups")).FullName;
            string todayPath = Directory.CreateDirectory(Path.Combine(backupsPath, $"{DateTime.Now:yyyy.MM.dd}")).FullName;
            string nowPath = Directory.CreateDirectory(Path.Combine(todayPath, $"{DateTime.Now:HH..mm}")).FullName;
            await Controller.ExportAsync("People", filePath: $"{nowPath}\\{MainOrg.Preffix}_People.csv", noPrompt: true);
            await Controller.ExportAsync("Organizations", filePath: $"{nowPath}\\{MainOrg.Preffix}_Organizations.csv", noPrompt: true);
            await Controller.ExportAsync("Documents", filePath: $"{nowPath}\\{MainOrg.Preffix}_Documents.csv", noPrompt: true);
            FileHandler.ManageBackupFiles(backupsPath);
            Application.Current.Shutdown(0);
        }

        private void NewOrgName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            AddOrgSave.IsEnabled = !string.IsNullOrEmpty(NewOrgName.Text);
        }

        private void NewOrgName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && AddOrgSave.IsEnabled)
            {
                AddOrgSave_Click(this, e);
            }
            if (e.Key == Key.Escape)
            {
                AddOrgCancel_Click(this, e);
            }
        }

        private async void AddOrgSave_Click(object sender, RoutedEventArgs e)
        {
            int? parOrgId = null;
            if (ParentOrganization.SelectedItem == null)
            {
                string msg = "You are adding a new organization without a Parent Organization.";
                string question = "Are you sure you want to save without a Parent Organization?";
                if (!PromptBox.Question(msg, question)) return;
            }
            else parOrgId = Controller.Organizations.ElementAtOrDefault(ParentOrganization.SelectedIndex).OrganizationId;
            string[] lastOrg = await PhpHandler.GetFromTableAsync($"{MainOrg.Preffix}_organizations", firstLast: "last");
            int orgId = lastOrg.Any() ? int.Parse(lastOrg[0].Split(";")[1]) + 1 : MainOrg.OrgStartId;
            await PhpHandler.AddToTable($"{MainOrg.Preffix}_organizations", "organization", $"{orgId};{NewOrgName.Text};{parOrgId}");
            await Controller.UpdateOrganizations();
            AddNewOrg.Visibility = Visibility.Hidden;
        }

        private void AddOrgCancel_Click(object sender, RoutedEventArgs e)
        {
            AddNewOrg.Visibility = Visibility.Hidden;
        }
    }
}