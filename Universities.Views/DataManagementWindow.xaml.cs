using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Universities.Controller;
using Universities.Handlers;

namespace Universities.Views
{
    public partial class DataManagementWindow
    {
        private readonly MainController controller;
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        private int newStartId;
        public event EventHandler<int>? OnShiftIdsClicked;

        public DataManagementWindow(MainController controller)
        {
            InitializeComponent();
            DataContext = this;
            this.controller = controller;
            lvDocuments.ItemsSource = GetDocuments(controller.Documents.Select(d => d.ToArray()));
            lvOrganizations.ItemsSource = controller.Organizations.Select(o => o.ToArray());
            lvPeople.ItemsSource = controller.People.Select(o => o.ToArray());
            lvDuplicateDocuments.ItemsSource = controller.DuplicateDocuments.Select(d => d.ToArray());
            lvIncompleteDocuments.ItemsSource = controller.IncompleteDocuments.Select(d => d.ToArray());
            Users.ItemsSource = controller.GetUsers().Where(u => u != "root");
            controller.OnDocumentsChanged += Controller_OnDocumentsChanged;
            controller.OnOrganizationsChanged += Controller_OnOrganizationsChanged;
            controller.OnPeopleChanged += Controller_OnPeopleChanged;
        }

        private IEnumerable<string[]> GetDocuments(IEnumerable<string[]> documents)
        {
            foreach (string[] item in documents)
            {
                yield return new[]
                {
                    item[0], item[4], item[5], item[6], item[8], item[9], item[13], item[14], item[17],
                    string.IsNullOrEmpty(item[20]) ? item[19].Split(',')[1].Trim() : item[20].Trim()
                };
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
                lvDocuments.ItemsSource = GetDocuments(controller.GetDocumentsOrderedBy(sortBy, direction).Select(d => d.ToArray()));
            }
            else if (OrganizationsTab.IsSelected)
            {
                lvOrganizations.ItemsSource = controller.GetOrganizationsOrderedBy(sortBy, direction).Select(d => d.ToArray());
            }
            else if (PeopleTab.IsSelected)
            {
                lvPeople.ItemsSource = controller.GetPeopleOrderedBy(sortBy, direction).Select(d => d.ToArray());
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentsTab.IsSelected)
            {
                if (FileDialogHandler.ShowOpenFileDialog("Open DataSet file", out string filePath))
                {
                    controller.ImportDataset(filePath);
                }
            }
            if (OrganizationsTab.IsSelected)
            {
                if (FileDialogHandler.ShowOpenFileDialog("Open Organizations file", out string filePath))
                {
                    controller.ImportOrganizations(filePath);
                }
            }
            if (PeopleTab.IsSelected)
            {
                if (FileDialogHandler.ShowOpenFileDialog("Open People file", out string filePath))
                {
                    controller.ImportPeople(filePath);
                }
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Controller_OnDocumentsChanged(object? sender, EventArgs e)
        {
            lvDocuments.ItemsSource = GetDocuments(controller.Documents.Select(d => d.ToArray()));
        }

        private void Controller_OnOrganizationsChanged(object? sender, EventArgs e)
        {
            lvOrganizations.ItemsSource = controller.Organizations.Select(d => d.ToArray());
        }

        private void Controller_OnPeopleChanged(object? sender, EventArgs e)
        {
            lvPeople.ItemsSource = controller.People.Select(o => o.ToArray());
        }

        private void ShiftId_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShiftIds.IsEnabled = !string.IsNullOrEmpty(NewStartId.Text) && int.TryParse(NewStartId.Text, out newStartId);
        }

        private void ShiftIdsButton_Click(object sender, RoutedEventArgs e) => OnShiftIdsClicked?.Invoke(sender, newStartId);

        private void Numbers_OnKeyDown(object sender, KeyEventArgs e) => e.Handled = KeyPressHandler.NotNumbers(e);

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }

        private void Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Assign.IsEnabled = true;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView selectedList = sender as ListView;
            object items = selectedList.SelectedItems;
        }
    }
}