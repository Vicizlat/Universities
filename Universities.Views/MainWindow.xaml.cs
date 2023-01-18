using System;
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

        public MainWindow(MainController controller)
        {
            InitializeComponent();
            this.controller = controller;
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

        private void SettingsImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => new SettingsWindow(controller).ShowDialog();

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
            string[] document = controller.GetDocumentArray(docId);
            string firstName = document[20];
            string lastName = document[17];
            string documentId = document[0];
            int seqNo = int.Parse(document[14]);
            int orgId = controller.Organizations[SelectOrganization.SelectedIndex].OrganizationId;
            if (controller.AddPerson(firstName, lastName, orgId, documentId, seqNo))
            {
                if (controller.RemoveDocument(docId))
                {
                    if (controller.SaveDocuments())
                    {
                        PopulateFields();
                        return;
                    }
                    controller.AddDocument(docId, document);
                }
            }
            controller.FindAndRemovePerson(firstName, lastName, documentId, seqNo);
            MessageBox.Show("Failed to save Person. No changes were made.");
        }

        private void PopulateFields()
        {
            string[] document = controller.GetDocumentArray(docId);
            SaveButton.IsEnabled = IsSaveEnabled();
            SelectOrganization.SelectedIndex = -1;
            PreviousButton.IsEnabled = docId >= 0;
            PreviousButton.Content = $"<< Previous {(docId < 0 ? "(0)" : $"({docId})")}";
            NextButton.IsEnabled = docId < controller.Documents.Count - 1;
            NextButton.Content = $"({controller.Documents.Count - docId - 1}) Next >>";
            if (document.Length <= 0)
            {
                Wos.Text = string.Empty;
                SeqNo.Text = string.Empty;
                Author.Text = string.Empty;
                Address.Text = string.Empty;
                OrganizationNames.Text = string.Empty;
                SubOrganizationNames.Text = string.Empty;
            }
            else
            {
                Wos.Text = document[0];
                SeqNo.Text = document[14];
                Author.Text = $"{document[20]} {document[17]}";
                Address.Text = document[7];
                StringBuilder sb = new StringBuilder()
                    .AppendLine(document[8])
                    .AppendLine(document[9])
                    .AppendLine(document[10])
                    .AppendLine(document[11])
                    .AppendLine(document[12]);
                OrganizationNames.Text = sb.ToString().TrimEnd();
                SubOrganizationNames.Text = document[13];
            }
        }

        private void SelectOrganization_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveButton.IsEnabled = IsSaveEnabled();
        }

        private bool IsSaveEnabled()
        {
            return SelectOrganization.SelectedIndex >= 0 && !string.IsNullOrEmpty(Wos.Text) && !string.IsNullOrEmpty(Author.Text);
        }
    }
}