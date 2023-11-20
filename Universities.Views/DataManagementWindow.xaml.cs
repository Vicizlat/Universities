using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Universities.Controller;
using Universities.Utils;

namespace Universities.Views
{
    public partial class DataManagementWindow
    {
        public WaitWindow WaitWindow { get; set; }
        private readonly MainController controller;
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        private TabItem lastSelectedTab = null;

        public DataManagementWindow(MainController controller)
        {
            InitializeComponent();
            DataContext = this;
            this.controller = controller;
            controller.Documents = DBAccess.GetContext().Documents.ToList();
            //controller.UpdateDocuments();
            UpdateDocumentsView();
            //UpdateOrganizationsView();
            //UpdatePeopleView();
            UpdateAcadPersonnelView();
            lvDuplicateDocuments.ItemsSource = controller.DuplicateDocuments.Select(dd => dd.ToArray());
            lvIncompleteDocuments.ItemsSource = controller.IncompleteDocuments.Select(id => id.ToArray());
            List<string> list = new List<string>() { "Unassign (No User)" };
            list.AddRange(SqlCommands.GetUsers().Select(t => t.Item1));
            Users.ItemsSource = list;
            controller.OnDocumentsChanged += Controller_OnDocumentsChanged;
            controller.OnOrganizationsChanged += Controller_OnOrganizationsChanged;
            controller.OnPeopleChanged += Controller_OnPeopleChanged;
            controller.OnAcadPersonnelChanged += Controller_OnAcadPersonnelChanged;
            controller.UpdateOrganizations();
            controller.UpdatePeople();
        }

        private void UpdateDocumentsView()
        {
            if (cbAssDocs.IsChecked == true)
            {
                if (cbPrDocs.IsChecked == true)
                {
                    lvDocuments.ItemsSource = controller.Documents.Select(d => d.ToArray());
                }
                else
                {
                    lvDocuments.ItemsSource = controller.Documents.Where(d => !d.Processed).Select(d => d.ToArray());
                }
            }
            else
            {
                if (cbPrDocs.IsChecked == true)
                {
                    lvDocuments.ItemsSource = controller.Documents.Where(d => d.AssignedToUser == SqlCommands.CurrentUser.Item1).Select(d => d.ToArray());
                }
                else
                {
                    lvDocuments.ItemsSource = controller.Documents
                        .Where(d => d.AssignedToUser == SqlCommands.CurrentUser.Item1)
                        .Where(d => !d.Processed)
                        .Select(d => d.ToArray());
                }
            }
            UpdateDocumentsCount();
        }

        private void UpdateOrganizationsView()
        {
            lvOrganizations.ItemsSource = controller.Organizations.Select(o => o.ToArray());
            UpdateOrganizationsCount();
        }

        private void UpdatePeopleView()
        {
            lvPeople.ItemsSource = controller.People.Select(p => p.ToArray());
            UpdatePeopleCount();
        }

        private void UpdateAcadPersonnelView()
        {
            lvAcadPersonnel.ItemsSource = DBAccess.GetContext().AcadPersonnel.ToList().Select(p => p.ToArray());
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
                controller.Documents = CollectionSorter.GetDocumentsOrderedBy(sortBy, direction).ToList();
                UpdateDocumentsView();
            }
            else if (OrganizationsTab.IsSelected)
            {
                lvOrganizations.ItemsSource = CollectionSorter.GetOrganizationsOrderedBy(sortBy, direction).Select(o => o.ToArray());
            }
            else if (PeopleTab.IsSelected)
            {
                lvPeople.ItemsSource = CollectionSorter.GetPeopleOrderedBy(sortBy, direction).Select(p => p.ToArray());
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            ShowWaitWindow();
            if (DocumentsTab.IsSelected) ImportExport.ImportDocuments();
            if (OrganizationsTab.IsSelected) ImportExport.ImportOrganizations();
            if (PeopleTab.IsSelected) ImportExport.ImportPeople();
            if (AcadPersonnelTab.IsSelected) ImportExport.ImportAcadPersonnel();
            CloseWaitWindow();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            UpdateDocumentsView();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            string message = string.Empty;
            if (DocumentsTab.IsSelected) ImportExport.ExportDocuments(out message);
            if (OrganizationsTab.IsSelected) ImportExport.ExportOrganizations(out message);
            if (PeopleTab.IsSelected) ImportExport.ExportPeople(out message);
            if (AcadPersonnelTab.IsSelected) ImportExport.ExportAcadPersonnel(out message);
            if (IncompleteDocumentsTab.IsSelected) ImportExport.ExportDocuments(out message, isIncomplete: true);
            if (DuplicateDocumentsTab.IsSelected) ImportExport.ExportDocuments(out message, isDuplicate: true);
            Logging.Instance.WriteLine(message);
            MessageBox.Show(message);
        }

        private void Controller_OnDocumentsChanged(object? sender, EventArgs e)
        {
            UpdateDocumentsView();
        }

        private void Controller_OnOrganizationsChanged(object? sender, EventArgs e)
        {
            UpdateOrganizationsView();
        }

        private void Controller_OnPeopleChanged(object? sender, EventArgs e)
        {
            UpdatePeopleView();
        }

        private void Controller_OnAcadPersonnelChanged(object? sender, EventArgs e)
        {
            UpdateAcadPersonnelView();
        }

        private void EditSelected_Click(object sender, RoutedEventArgs e)
        {
            string[] person = lvPeople.SelectedItem as string[];
            new EditPersonWindow(person, controller).ShowDialog();
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (!PromptBox.Question("Are you sure you want to permanently remove all selected items?")) return;
            if (DocumentsTab.IsSelected)
            {
                List<string[]> selectedDocuments = lvDocuments.SelectedItems.Cast<string[]>().ToList();
                Logging.Instance.WriteLine("Documents removed from DB:");
                selectedDocuments.ForEach(DBAccess.DeleteDocument);
                controller.Documents = DBAccess.GetContext().Documents.ToList();
                UpdateDocumentsView();
            }
            if (OrganizationsTab.IsSelected)
            {
                List<string[]> selectedOrganizations = lvOrganizations.SelectedItems.Cast<string[]>().ToList();
                Logging.Instance.WriteLine("Organizations removed from DB:");
                selectedOrganizations.ForEach(DBAccess.DeleteOrganization);
                controller.Organizations = DBAccess.GetContext().Organizations.ToList();
                UpdateOrganizationsView();
            }
            if (PeopleTab.IsSelected)
            {
                List<string[]> selectedPeople = lvPeople.SelectedItems.Cast<string[]>().ToList();
                Logging.Instance.WriteLine("People removed from DB:");
                selectedPeople.ForEach(DeletePersonAndRestoreDocument);
                controller.People = DBAccess.GetContext().People.ToList();
                UpdatePeopleView();
            }
            MessageBox.Show("All done!");
        }

        private void DeletePersonAndRestoreDocument(string[] personArr)
        {
            string[] doc = controller.Documents.FirstOrDefault(d => d.Ut == personArr[4] && d.FirstName == personArr[1] && d.LastName == personArr[2]).ToArray();
            if (controller.UpdateDocument(doc, false)) DBAccess.DeletePerson(personArr);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }

        private void Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Assign.IsEnabled = !((string)Users.SelectedItem).Contains("Unable");
        }

        private void Assign_Click(object sender, RoutedEventArgs e)
        {
            List<string[]> selectedItems = new List<string[]>();
            foreach (string[] item in lvDocuments.SelectedItems) selectedItems.Add(item);
            string selectedUser = Users.SelectedItem.ToString().StartsWith("Unassign") ? string.Empty : Users.SelectedItem.ToString();
            selectedItems.ForEach(d => controller.UpdateDocument(d, selectedUser));
            lvDocuments.SelectedItems.Clear();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string senderButton = ((Button)sender).Content.ToString() ?? string.Empty;
            bool processed = senderButton == "Processed";
            List<string[]> selectedItems = new List<string[]>();
            foreach (string[] item in lvDocuments.SelectedItems) selectedItems.Add(item);
            selectedItems.ForEach(d => controller.UpdateDocument(d, processed));
            lvDocuments.SelectedItems.Clear();
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
                lvDocuments.ItemsSource = controller.Documents.Where(d => d.ToString().ToLower().Contains(SearchBox.Text.ToLower())).Select(d => d.ToArray());
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
        }

        private void DataManageWindow_Closing(object sender, CancelEventArgs e)
        {
            controller.UpdateDocuments();
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

        public void CloseWaitWindow()
        {
            if (WaitWindow != null) WaitWindow.Close();
        }
    }
}