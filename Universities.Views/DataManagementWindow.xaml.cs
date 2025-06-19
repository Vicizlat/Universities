using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Universities.Controller;
using Universities.Models;
using Universities.Handlers;
using Universities.Utils;
using System.Threading.Tasks;

namespace Universities.Views
{
    public partial class DataManagementWindow
    {
        public WaitWindow WaitWindow { get; set; }
        private readonly MainController controller;
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        private TabItem lastSelectedTab = null;
        private IEnumerable<Dictionary<string, string>> DuplicateDocs { get; set; }
        private IEnumerable<Dictionary<string, string>> IncompleteDocs { get; set; }

        public DataManagementWindow(MainController controller)
        {
            InitializeComponent();
            DataContext = this;
            this.controller = controller;
        }

        private async void DataManageWindow_LoadedAsync(object sender, RoutedEventArgs e)
        {
            await UpdateDocumentsView();
            await UpdateOrganizationsViewAsync();
            await UpdatePeopleView();
            await UpdateAcadPersonnelView();
            List<string> list = new List<string>() { "Unassign (No User)" };
            string[] users = await PhpHandler.GetFromTableAsync("users");
            list.AddRange(users.Select(u => u.Split(";")).Select(us => us[1]));
            Users.ItemsSource = list;
        }

        private async Task UpdateDocumentsView()
        {
            string[] docs = await PhpHandler.GetFromTableAsync($"{MainOrg.Preffix}_documents", processed: 1);
            controller.Documents = docs.Select(d => new Document(d.Split(";"))).ToList();
            if (cbAssDocs.IsChecked == true)
            {
                if (cbPrDocs.IsChecked == true)
                {
                    lvDocuments.ItemsSource = controller.Documents.Select(d => d.ToDict());
                }
                else
                {
                    lvDocuments.ItemsSource = controller.Documents.Where(d => !d.Processed).Select(d => d.ToDict());
                }
            }
            else
            {
                if (cbPrDocs.IsChecked == true)
                {
                    lvDocuments.ItemsSource = controller.Documents.Where(d => d.AssignedToUser == User.Username).Select(d => d.ToDict());
                }
                else
                {
                    lvDocuments.ItemsSource = controller.Documents
                        .Where(d => d.AssignedToUser == User.Username)
                        .Where(d => !d.Processed)
                        .Select(d => d.ToDict());
                }
            }
            string[] ddocs = await PhpHandler.GetFromTableAsync($"{MainOrg.Preffix}_duplicate_documents", processed: 1);
            DuplicateDocs = ddocs.Select(dd => new Document(dd.Split(";")).ToDict());
            lvDuplicateDocuments.ItemsSource = DuplicateDocs;
            string[] idocs = await PhpHandler.GetFromTableAsync($"{MainOrg.Preffix}_incomplete_documents", processed: 1);
            IncompleteDocs = idocs.Select(id => new Document(id.Split(";")).ToDict());
            lvIncompleteDocuments.ItemsSource = IncompleteDocs;
            UpdateDocumentsCount();
        }

        private async Task UpdateOrganizationsViewAsync()
        {
            await controller.UpdateOrganizations();
            lvOrganizations.ItemsSource = controller.Organizations.Select(o => o.ToArray());
            UpdateOrganizationsCount();
        }

        private async Task UpdatePeopleView()
        {
            await controller.UpdatePeopleAsync();
            lvPeople.ItemsSource = controller.People.Select(p => p.ToArray());
            UpdatePeopleCount();
        }

        private async Task UpdateAcadPersonnelView()
        {
            await controller.UpdateAcadPersonnelAsync();
            lvAcadPersonnel.ItemsSource = controller.AcadPersonnel.Select(ap => ap.ToArray());
            UpdateAcadPersonnelCount();
        }

        private void ListViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (GridViewColumnHeader)sender;
            string sortBy = column?.Content.ToString() ?? string.Empty;
            if (column == null) return;
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            string direction = newDir.ToString();
            if (DocumentsTab.IsSelected)
            {
                if (direction == "Ascending") lvDocuments.ItemsSource = controller.Documents.OrderBy(d => d.GetType().GetProperty(sortBy)?.GetValue(d)).Select(d => d.ToDict());
                else lvDocuments.ItemsSource = controller.Documents.OrderByDescending(d => d.GetType().GetProperty(sortBy)?.GetValue(d)).Select(d => d.ToDict());
            }
            else if (OrganizationsTab.IsSelected)
            {
                if (direction == "Ascending") lvOrganizations.ItemsSource = controller.Organizations.OrderBy(o => o.GetType().GetProperty(sortBy)?.GetValue(o)).Select(o => o.ToArray());
                else lvOrganizations.ItemsSource = controller.Organizations.OrderByDescending(o => o.GetType().GetProperty(sortBy)?.GetValue(o)).Select(o => o.ToArray());
            }
            else if (PeopleTab.IsSelected)
            {
                if (direction == "Ascending") lvPeople.ItemsSource = controller.People.OrderBy(p => p.GetType().GetProperty(sortBy)?.GetValue(p)).Select(p => p.ToArray());
                else lvPeople.ItemsSource = controller.People.OrderByDescending(p => p.GetType().GetProperty(sortBy)?.GetValue(p)).Select(p => p.ToArray());
            }
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedTabName = $"{(Tabs.SelectedItem as TabItem).Header}";
            ShowWaitWindow($"Importing {selectedTabName}...");
            if (FileDialogHandler.ShowOpenFileDialog($"Open {selectedTabName} file", out string filePath))
            {
                if (FileHandler.ReadAllLines(filePath, out string[] lines) && controller.GetSeparator(lines[0], out string separator))
                {
                    int counter = 0;
                    int count = lines.Length - 1;
                    WaitWindow.UpdateProgressBar(count);
                    if (DocumentsTab.IsSelected)
                    {
                        string[] fileHeaders = lines[0].Split(separator);
                        FileToDbWindow fileToDbWindow = new FileToDbWindow(fileHeaders, Constants.ExportDocumentsHeader);
                        if (fileToDbWindow.ShowDialog() == true)
                        {
                            Dictionary<string, int> lineDict = fileToDbWindow.LineDict;
                            foreach (string[] lineArr in lines.Skip(1).Select(l => l.Split(separator, StringSplitOptions.TrimEntries)))
                            {
                                if (lineArr.Length != fileHeaders.Length)
                                {
                                    if (PromptBox.Question(string.Format(Constants.InfDifferentColums, counter + 2), Constants.QuestionImportContinue)) continue;
                                    else break;
                                }
                                counter++;
                                if (WaitWindow.IsCanceled) break;
                                WaitWindow.ChangeText($"Importing: {counter} / {count}");
                                WaitWindow.ProgBar.Value = counter;
                                Document doc = new Document(lineDict, lineArr);
                                string tableName = $"{MainOrg.Preffix}_documents";
                                string[] matchingDoc = await PhpHandler.GetFromTableAsync(tableName, firstName: doc.FirstName, lastName: doc.LastName, ut: doc.Ut);
                                if (matchingDoc.Length == 1) tableName = $"{MainOrg.Preffix}_duplicate_documents";
                                else if (string.IsNullOrEmpty(doc.Ut) || string.IsNullOrEmpty(doc.LastName)) tableName = $"{MainOrg.Preffix}_incomplete_documents";
                                await PhpHandler.AddToTable(tableName, "document", doc.ToString());
                            }
                            await UpdateDocumentsView();
                        }
                    }
                    if (OrganizationsTab.IsSelected)
                    {
                        if (lines[0].Split(separator).Length == Constants.ExportOrganizationsHeader.Length)
                        {
                            foreach (string[] lineArr in lines.Skip(1).Select(l => l.Split(separator, StringSplitOptions.TrimEntries)))
                            {
                                counter++;
                                if (WaitWindow.IsCanceled) break;
                                WaitWindow.ChangeText($"Importing: {counter} / {count}");
                                WaitWindow.ProgBar.Value = counter;
                                await PhpHandler.AddToTable($"{MainOrg.Preffix}_organizations", "organization", string.Join(";", lineArr));
                            }
                            await UpdateOrganizationsViewAsync();
                        }
                        else PromptBox.Error(Constants.ErrorWrongFile);
                    }
                    if (PeopleTab.IsSelected)
                    {
                        if (lines[0].Split(separator).Length == Constants.ExportPeopleHeader.Length)
                        {
                            foreach (string[] lineArr in lines.Skip(1).Select(l => l.Split(separator, StringSplitOptions.TrimEntries)))
                            {
                                counter++;
                                if (WaitWindow.IsCanceled) break;
                                WaitWindow.ChangeText($"Importing: {counter} / {count}");
                                WaitWindow.ProgBar.Value = counter;
                                await PhpHandler.AddToTable($"{MainOrg.Preffix}_people", "person", string.Join(";", lineArr));
                            }
                            await UpdatePeopleView();
                        }
                        else PromptBox.Error(Constants.ErrorWrongFile);
                    }
                    if (AcadPersonnelTab.IsSelected)
                    {
                        foreach (string[] lineArr in lines.Skip(1).Select(l => l.Split(separator, StringSplitOptions.TrimEntries)))
                        {
                            counter++;
                            if (WaitWindow.IsCanceled) break;
                            WaitWindow.ChangeText($"Importing: {counter} / {count}");
                            WaitWindow.ProgBar.Value = counter;
                            if (lines[0].Split(separator).Length != Constants.ExportAcadPersonnelHeader.Length)
                            {
                                string[] acadPersonArr = new string[] { lineArr[2], lineArr[3], lineArr[4], lineArr[5], $"{lineArr[7]} | {lineArr[8]}", lineArr[9], lineArr[10] };
                                string[] lastPerson = await PhpHandler.GetFromTableAsync($"{MainOrg.Preffix}_academic_personnel", firstLast: "last");
                                if (string.IsNullOrEmpty(lineArr[0]) && lastPerson.Any())
                                {
                                    string[] lastPersonArray = lastPerson[0].Split(";");
                                    int lastPersonId = int.Parse(lastPersonArray[0]);
                                    if (!string.IsNullOrEmpty(lineArr[2]))
                                    {
                                        await PhpHandler.UpdateInTable($"{MainOrg.Preffix}_academic_personnel", "FirstNames", lastPersonArray[1] + $" | {lineArr[2]}", lastPersonId);
                                    }
                                    if (!string.IsNullOrEmpty(lineArr[3]))
                                    {
                                        await PhpHandler.UpdateInTable($"{MainOrg.Preffix}_academic_personnel", "LastNames", lastPersonArray[2] + $" | {lineArr[3]}", lastPersonId);
                                    }
                                    if (!string.IsNullOrEmpty(lineArr[9]))
                                    {
                                        await PhpHandler.UpdateInTable($"{MainOrg.Preffix}_academic_personnel", "Notes", lastPersonArray[6] + $" | {lineArr[9]}", lastPersonId);
                                    }
                                    if (!string.IsNullOrEmpty(lineArr[10]))
                                    {
                                        await PhpHandler.UpdateInTable($"{MainOrg.Preffix}_academic_personnel", "Comments", lastPersonArray[7] + $" | {lineArr[10]}", lastPersonId);
                                    }
                                }
                                else await PhpHandler.AddToTable($"{MainOrg.Preffix}_academic_personnel", "acad_person", string.Join(";", acadPersonArr));
                            }
                            else await PhpHandler.AddToTable($"{MainOrg.Preffix}_academic_personnel", "acad_person", string.Join(";", lineArr));
                        }
                        await UpdateAcadPersonnelView();
                    }
                    if (counter > 0) PromptBox.Information(string.Format(Constants.SuccessImport, counter, selectedTabName, filePath));
                }
                else PromptBox.Error(string.Format(Constants.ErrorFileRead, filePath));
            }
            WaitWindow?.Close();
        }

        private async void CheckBox_ClickAsync(object sender, RoutedEventArgs e)
        {
            await UpdateDocumentsView();
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentsTab.IsSelected) await controller.ExportAsync("Documents");
            if (OrganizationsTab.IsSelected) await controller.ExportAsync("Organizations");
            if (PeopleTab.IsSelected) await controller.ExportAsync("People");
            if (AcadPersonnelTab.IsSelected) await controller.ExportAsync("Academic Personnel");
            if (IncompleteDocumentsTab.IsSelected) await controller.ExportAsync("Documents", isIncomplete: true);
            if (DuplicateDocumentsTab.IsSelected) await controller.ExportAsync("Documents", isDuplicate: true);
        }

        private async void EditSelected_ClickAsync(object sender, RoutedEventArgs e)
        {
            string[] person = lvPeople.SelectedItem as string[];
            bool result = (bool)new EditPersonWindow(person, controller).ShowDialog();
            if (result)
            {
                await controller.UpdatePeopleAsync();
                await UpdatePeopleView();
            }
        }

        private async void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            string selectedTabName = $"{(Tabs.SelectedItem as TabItem).Header}";
            if (!PromptBox.Question(string.Format(Constants.InfDeleteSelected, selectedTabName, MainOrg.Name), Constants.QuestionAreYouSure)) return;
            ShowWaitWindow($"Deleting selected {(Tabs.SelectedItem as TabItem).Header}...");
            if (DocumentsTab.IsSelected)
            {
                int counter = 0;
                int count = lvDocuments.SelectedItems.Count;
                WaitWindow.UpdateProgressBar(count);
                foreach (Dictionary<string, string> doc in lvDocuments.SelectedItems.Cast<Dictionary<string, string>>())
                {
                    counter++;
                    if (WaitWindow.IsCanceled) break;
                    WaitWindow.ChangeText($"Deleting: {counter} / {count}");
                    WaitWindow.ProgBar.Value = counter;
                    if (!await PhpHandler.DeleteFromTable($"{MainOrg.Preffix}_documents", int.Parse(doc["Id"])))
                    {
                        PromptBox.Error($"There was a problem deleting {string.Join(";", doc)}!");
                        continue;
                    }
                }
                await UpdateDocumentsView();
            }
            if (OrganizationsTab.IsSelected)
            {
                int counter = 0;
                int count = lvOrganizations.SelectedItems.Count;
                WaitWindow.UpdateProgressBar(count);
                foreach (string[] org in lvOrganizations.SelectedItems.Cast<string[]>())
                {
                    counter++;
                    if (WaitWindow.IsCanceled) break;
                    WaitWindow.ChangeText($"Deleting: {counter} / {count}");
                    WaitWindow.ProgBar.Value = counter;
                    if (!await PhpHandler.DeleteFromTable($"{MainOrg.Preffix}_organizations", int.Parse(org[0])))
                    {
                        PromptBox.Error($"There was a problem deleting {org[2]}!");
                        continue;
                    }
                }
                await UpdateOrganizationsViewAsync();
            }
            if (PeopleTab.IsSelected)
            {
                int counter = 0;
                int count = lvPeople.SelectedItems.Count;
                WaitWindow.UpdateProgressBar(count);
                foreach (string[] person in lvPeople.SelectedItems.Cast<string[]>())
                {
                    counter++;
                    if (WaitWindow.IsCanceled) break;
                    WaitWindow.ChangeText($"Deleting: {counter} / {count}");
                    WaitWindow.ProgBar.Value = counter;
                    if (!await PhpHandler.DeleteFromTable($"{MainOrg.Preffix}_people", int.Parse(person[0])))
                    {
                        PromptBox.Error($"There was a problem deleting {person[2]} {person[3]} with PersonId: {person[1]}!");
                        continue;
                    }
                    await PhpHandler.UpdateInTable($"{MainOrg.Preffix}_documents", "Processed", "0", firstName: person[2], lastName: person[3], ut: person[5]);
                }
                await UpdatePeopleView();
                await UpdateDocumentsView();
            }
            WaitWindow?.Close();
            MessageBox.Show("All done!");
        }

        private async void ClearTable_Click(object sender, RoutedEventArgs e)
        {
            string selectedTabName = $"{(Tabs.SelectedItem as TabItem).Header}";
            if (!PromptBox.Question(string.Format(Constants.InfClearTable, selectedTabName, MainOrg.Name), Constants.QuestionAreYouSure)) return;
            if (DocumentsTab.IsSelected) PromptBox.Information(await PhpHandler.ClearTable($"{MainOrg.Preffix}_documents"));
            if (OrganizationsTab.IsSelected) PromptBox.Information(await PhpHandler.ClearTable($"{MainOrg.Preffix}_organizations"));
            if (PeopleTab.IsSelected) PromptBox.Information(await PhpHandler.ClearTable($"{MainOrg.Preffix}_people"));
            if (AcadPersonnelTab.IsSelected) PromptBox.Information(await PhpHandler.ClearTable($"{MainOrg.Preffix}_academic_personnel"));
            if (DuplicateDocumentsTab.IsSelected) PromptBox.Information(await PhpHandler.ClearTable($"{MainOrg.Preffix}_duplicate_documents"));
            if (IncompleteDocumentsTab.IsSelected) PromptBox.Information(await PhpHandler.ClearTable($"{MainOrg.Preffix}_incomplete_documents"));
            DataManageWindow_LoadedAsync(sender, e);
        }

        private void Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Assign.IsEnabled = lvDocuments.SelectedItems.Count > 0;
        }

        private async void Assign_ClickAsync(object sender, RoutedEventArgs e)
        {
            ShowWaitWindow($"Assigning {Users.SelectedItem} to selected Documents...");
            string selectedUser = Users.SelectedIndex == 0 ? string.Empty : $"{Users.SelectedItem}";
            int counter = 0;
            int count = lvDocuments.SelectedItems.Count;
            WaitWindow.UpdateProgressBar(count);
            foreach (Dictionary<string, string> doc in lvDocuments.SelectedItems.Cast<Dictionary<string, string>>())
            {
                counter++;
                if (WaitWindow.IsCanceled) break;
                WaitWindow.ChangeText($"Assigning: {counter} / {count}");
                WaitWindow.ProgBar.Value = counter;
                await PhpHandler.UpdateInTable($"{MainOrg.Preffix}_documents", "AssignedToUser", selectedUser, id: int.Parse(doc["Id"]));
            }
            lvDocuments.SelectedItems.Clear();
            Users.SelectedItem = null;
            await UpdateDocumentsView();
            WaitWindow?.Close();
            PromptBox.Information("All assignments done!");
        }

        private void lvDocuments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteSelected.IsEnabled = lvDocuments.SelectedItems.Count > 0;
            UpdateDocumentsCount();
        }

        private void lvOrganizations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteSelected.IsEnabled = lvOrganizations.SelectedItems.Count > 0;
            UpdateOrganizationsCount();
        }

        private void lvPeople_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteSelected.IsEnabled = lvPeople.SelectedItems.Count > 0;
            EditSelected.IsEnabled = lvPeople.SelectedItems.Count == 1;
            UpdatePeopleCount();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DocumentsTab.IsSelected)
            {
                lvDocuments.ItemsSource = controller.Documents.Where(d => d.ToString().ToLower().Contains(SearchBox.Text.ToLower())).Select(d => d.ToDict());
                UpdateDocumentsCount();
            }
            if (OrganizationsTab.IsSelected)
            {
                lvOrganizations.ItemsSource = controller.Organizations.Where(o => o.ToString().ToLower().Contains(SearchBox.Text.ToLower())).Select(o => o.ToArray());
                UpdateOrganizationsCount();
            }
            if (PeopleTab.IsSelected)
            {
                lvPeople.ItemsSource = controller.People.Where(p => p.ToString().ToLower().Contains(SearchBox.Text.ToLower())).Select(p => p.ToArray());
                UpdatePeopleCount();
            }
            if (AcadPersonnelTab.IsSelected)
            {
                lvAcadPersonnel.ItemsSource = controller.AcadPersonnel.Where(ap => ap.ToString().ToLower().Contains(SearchBox.Text.ToLower())).Select(ap => ap.ToArray());
                UpdateAcadPersonnelCount();
            }
            if (DuplicateDocumentsTab.IsSelected)
            {
                lvDuplicateDocuments.ItemsSource = DuplicateDocs.Where(dd => string.Join(';', dd.Values).ToLower().Contains(SearchBox.Text.ToLower())).Select(dd => dd);
                UpdateDocumentsCount();
            }
            if (IncompleteDocumentsTab.IsSelected)
            {
                lvIncompleteDocuments.ItemsSource = IncompleteDocs.Where(id => string.Join(';', id.Values).ToLower().Contains(SearchBox.Text.ToLower())).Select(id => id);
                UpdateDocumentsCount();
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl)
            {
                if (lastSelectedTab != null && (tabControl.SelectedItem as TabItem) != lastSelectedTab)
                    SearchBox.Clear();
                lastSelectedTab = tabControl.SelectedItem as TabItem;
            }

            UpdateDocumentsCount();
            UpdateOrganizationsCount();
            UpdatePeopleCount();
            UpdateAcadPersonnelCount();
        }

        private void UpdateDocumentsCount()
        {
            if (DocumentsTab.IsSelected)
            {
                TotalCount.Text = $"{lvDocuments.Items.Count}";
                SelectedCount.Text = $"{lvDocuments.SelectedItems.Count}";
            }
            if (DuplicateDocumentsTab.IsSelected)
            {
                TotalCount.Text = $"{lvDuplicateDocuments.Items.Count}";
                SelectedCount.Text = $"{lvDuplicateDocuments.SelectedItems.Count}";
            }
            if (IncompleteDocumentsTab.IsSelected)
            {
                TotalCount.Text = $"{lvIncompleteDocuments.Items.Count}";
                SelectedCount.Text = $"{lvIncompleteDocuments.SelectedItems.Count}";
            }
        }

        private void UpdateOrganizationsCount()
        {
            if (OrganizationsTab.IsSelected)
            {
                TotalCount.Text = $"{lvOrganizations.Items.Count}";
                SelectedCount.Text = $"{lvOrganizations.SelectedItems.Count}";
            }
        }

        private void UpdatePeopleCount()
        {
            if (PeopleTab.IsSelected)
            {
                TotalCount.Text = $"{lvPeople.Items.Count}";
                SelectedCount.Text = $"{lvPeople.SelectedItems.Count}";
            }
        }

        private void UpdateAcadPersonnelCount()
        {
            if (AcadPersonnelTab.IsSelected)
            {
                TotalCount.Text = $"{lvAcadPersonnel.Items.Count}";
                SelectedCount.Text = $"{lvAcadPersonnel.SelectedItems.Count}";
            }
        }

        public void ShowWaitWindow(string text = null)
        {
            WaitWindow = new WaitWindow(text);
            WaitWindow.Show();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (((Button)sender) == ImportButton) ImportButtonImage.Source = new BitmapImage(new Uri(@"Images/UploadIconC.png", UriKind.Relative));
            if (((Button)sender) == ExportButton) ExportButtonImage.Source = new BitmapImage(new Uri(@"Images/DownloadIconC.png", UriKind.Relative));
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (((Button)sender) == ImportButton) ImportButtonImage.Source = new BitmapImage(new Uri(@"Images/UploadIconBW.png", UriKind.Relative));
            if (((Button)sender) == ExportButton) ExportButtonImage.Source = new BitmapImage(new Uri(@"Images/DownloadIconBW.png", UriKind.Relative));
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }

        private async void DataManageWindow_Closing(object sender, CancelEventArgs e)
        {
            await controller.UpdateDocuments();
        }
    }
}