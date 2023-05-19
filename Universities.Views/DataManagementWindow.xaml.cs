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
        private readonly MainController controller;
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        public DataManagementWindow(MainController controller)
        {
            InitializeComponent();
            DataContext = this;
            this.controller = controller;
            controller.Documents = DBAccess.Context.Documents.ToList();
            UpdateDocumentsView();
            lvOrganizations.ItemsSource = controller.Organizations.Select(o => o.ToArray());
            lvPeople.ItemsSource = controller.People.Select(p => p.ToArray());
            lvDuplicateDocuments.ItemsSource = controller.DuplicateDocuments.Select(dd => dd.ToArray());
            lvIncompleteDocuments.ItemsSource = controller.IncompleteDocuments.Select(id => id.ToArray());
            List<string> list = new List<string>(){ "Unassign (No User)" };
            list.AddRange(SqlCommands.GetUsers().Select(t => t.Item1));
            Users.ItemsSource = list;
            controller.OnDocumentsChanged += Controller_OnDocumentsChanged;
            controller.OnOrganizationsChanged += Controller_OnOrganizationsChanged;
            controller.OnPeopleChanged += Controller_OnPeopleChanged;
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
            if (DocumentsTab.IsSelected) ImportExport.ImportDocuments();
            if (OrganizationsTab.IsSelected) ImportExport.ImportOrganizations();
            if (PeopleTab.IsSelected) ImportExport.ImportPeople();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            UpdateDocumentsView();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentsTab.IsSelected) ImportExport.ExportDocuments();
            if (OrganizationsTab.IsSelected) ImportExport.ExportOrganizations();
            if (PeopleTab.IsSelected) ImportExport.ExportPeople();
        }

        private void Controller_OnDocumentsChanged(object? sender, EventArgs e)
        {
            UpdateDocumentsView();
        }

        private void Controller_OnOrganizationsChanged(object? sender, EventArgs e)
        {
            lvOrganizations.ItemsSource = controller.Organizations.Select(d => d.ToArray());
        }

        private void Controller_OnPeopleChanged(object? sender, EventArgs e)
        {
            lvPeople.ItemsSource = controller.People.Select(o => o.ToArray());
        }

        private void SetIdsInOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!PromptBox.Question("Warning! This will take a long time. Are you sure you want to start the process now?")) return;
            List<string[]> sortedPeople = lvPeople.Items.Cast<string[]>().ToList();
            string lastProcessedId = "0";
            int currentId = Settings.Instance.PeopleStartId;
            foreach (string[] person in sortedPeople)
            {
                if (person[0] != lastProcessedId)
                {
                    lastProcessedId = person[0];
                    currentId++;
                }
                DBAccess.EditPersonId(person, currentId);
            }
            MessageBox.Show("All done!");
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
                controller.Documents = DBAccess.Context.Documents.ToList();
                UpdateDocumentsView();
            }
            if (PeopleTab.IsSelected)
            {
                List<string[]> selectedPeople = lvPeople.SelectedItems.Cast<string[]>().ToList();
                Logging.Instance.WriteLine("People removed from DB:");
                selectedPeople.ForEach(DBAccess.DeletePerson);
            }
            MessageBox.Show("All done!");
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
            selectedItems.ForEach(i => controller.UpdateDocument(i, selectedUser));
            lvDocuments.SelectedItems.Clear();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string senderButton = ((Button)sender).Content.ToString() ?? string.Empty;
            bool processed = senderButton == "Processed";
            List<string[]> selectedItems = new List<string[]>();
            foreach (string[] item in lvDocuments.SelectedItems) selectedItems.Add(item);
            selectedItems.ForEach(i => controller.UpdateDocument(i, processed));
            lvDocuments.SelectedItems.Clear();
        }

        private void lvPeople_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetIdsInOrder.IsEnabled = lvPeople.SelectedItems.Count == 0;
            DeleteSelected.IsEnabled = lvPeople.SelectedItems.Count > 0;
            EditSelected.IsEnabled = lvPeople.SelectedItems.Count == 1;
        }

        private void lvDocuments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteSelected.IsEnabled = lvDocuments.SelectedItems.Count > 0;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            lvPeople.ItemsSource = controller.People.Where(p => p.ToString().ToLower().Contains(SearchBox.Text.ToLower())).Select(p => p.ToArray());
        }

        private void DataManageWindow_Closing(object sender, CancelEventArgs e)
        {
            controller.UpdateDocuments();
        }
    }
}