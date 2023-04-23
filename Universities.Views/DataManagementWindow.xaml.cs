﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Universities.Controller;
using Universities.Data.Models;
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
            controller.Documents = controller.Context.Documents.ToList();
            UpdateDocumentsView();
            lvOrganizations.ItemsSource = controller.Organizations.Select(o => o.ToArray());
            lvPeople.ItemsSource = controller.People.Select(o => o.ToArray());
            lvDuplicateDocuments.ItemsSource = controller.DuplicateDocuments.Select(d => d.ToArray());
            lvIncompleteDocuments.ItemsSource = controller.IncompleteDocuments.Select(d => d.ToArray());
            Users.ItemsSource = SqlCommands.GetUsers().Where(u => u != "root");
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
                    lvDocuments.ItemsSource = controller.Documents.Where(d => d.AssignedToUser == controller.CurrentUser).Select(d => d.ToArray());
                }
                else
                {
                    lvDocuments.ItemsSource = controller.Documents
                        .Where(d => d.AssignedToUser == controller.CurrentUser)
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
                lvOrganizations.ItemsSource = CollectionSorter.GetOrganizationsOrderedBy(sortBy, direction).Select(d => d.ToArray());
            }
            else if (PeopleTab.IsSelected)
            {
                lvPeople.ItemsSource = CollectionSorter.GetPeopleOrderedBy(sortBy, direction).Select(d => d.ToArray());
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentsTab.IsSelected) ImportExport.ImportDocuments();
            if (OrganizationsTab.IsSelected) ImportExport.ImportOrganizations();
            if (PeopleTab.IsSelected) ImportExport.ImportPeople();
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

        private void ShiftId_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShiftIds.IsEnabled = !string.IsNullOrEmpty(NewStartId.Text) && int.TryParse(NewStartId.Text, out newStartId);
        }

        private void ShiftIdsButton_Click(object sender, RoutedEventArgs e)
        {
            List<Person> shiftedPeople = new List<Person>();
            foreach (Person person in controller.People)
            {
                Person? findPerson = shiftedPeople.Find(p => p.LastPersonId == person.PersonId);
                person.LastPersonId = person.PersonId;
                person.PersonId = findPerson?.PersonId ?? (shiftedPeople.Count == 0 ? newStartId : shiftedPeople.Last().PersonId + 1);
                shiftedPeople.Add(new Person(person.ToArray()) { LastPersonId = person.LastPersonId });
            }
            MessageBox.Show("All done!");
        }

        private void Numbers_OnKeyDown(object sender, KeyEventArgs e) => e.Handled = KeyPressHandler.NotNumbers(e);

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
            selectedItems.ForEach(i => controller.UpdateDocument(i, (string)Users.SelectedItem));
            lvDocuments.SelectedItems.Clear();
            //UpdateDocumentsView();
        }

        private void DataManageWindow_Closing(object sender, CancelEventArgs e)
        {
            controller.UpdateDocuments();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            UpdateDocumentsView();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string senderButton = ((Button)sender).Content.ToString() ?? string.Empty;
            bool processed = senderButton == "Processed";
            List<string[]> selectedItems = new List<string[]>();
            foreach (string[] item in lvDocuments.SelectedItems) selectedItems.Add(item);
            selectedItems.ForEach(i => controller.UpdateDocument(i, processed));
            lvDocuments.SelectedItems.Clear();
            //UpdateDocumentsView();
        }
    }
}