using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Universities.Controller;
using Universities.Models;
using Universities.Templates.Icons;

namespace Universities
{
    public partial class MainWindow
    {
        private readonly MainController controller;
        private int docId = -1;

        public MainWindow(MainController controller)
        {
            InitializeComponent();
            this.controller = controller;
            IconsDockPanel.Children.Add(new SettingsImage(controller));
            if (controller.Organizations.Count > 0) SelectOrganization.ItemsSource = controller.Organizations.Select(controller.GetOrganizationName);
            PopulateFields(GetDocument());
            controller.OnDocumentsChanged += OnDocumentsChanged;
            controller.OnOrganizationsChanged += OnOrganizationsChanged;
        }

        private void OnDocumentsChanged(object sender, EventArgs e)
        {
            PopulateFields(GetDocument());
        }

        private void OnOrganizationsChanged(object? sender, EventArgs e)
        {
            SelectOrganization.ItemsSource = controller.Organizations.Select(controller.GetOrganizationName);
            SelectOrganization.SelectedIndex = -1;
            SaveButton.IsEnabled = IsSaveEnabled();
        }

        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            docId--;
            PopulateFields(GetDocument());
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            docId++;
            PopulateFields(GetDocument());
        }

        private DocumentModel GetDocument()
        {
            if (docId < 0 || docId >= controller.Documents.Count) return null;
            return controller.Documents[docId];
        }

        private void PopulateFields(DocumentModel doc)
        {
            SaveButton.IsEnabled = IsSaveEnabled();
            SelectOrganization.SelectedIndex = -1;
            PreviousButton.IsEnabled = docId >= 0;
            PreviousButton.Content = $"<< Previous {(docId < 0 ? "(0)" : $"({docId})")}";
            NextButton.IsEnabled = docId < controller.Documents.Count - 1;
            NextButton.Content = $"({controller.Documents.Count - docId - 1}) Next >>";
            if (doc == null)
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
                Wos.Text = doc.Ut;
                SeqNo.Text = $"{doc.SeqNo}";
                Author.Text = $"{doc.FirstName} {doc.LastName}";
                Address.Text = doc.FullAddress;
                StringBuilder sb = new StringBuilder()
                    .AppendLine(doc.OrgaName)
                    .AppendLine(doc.OrgaName1)
                    .AppendLine(doc.OrgaName2)
                    .AppendLine(doc.OrgaName3)
                    .AppendLine(doc.OrgaName4);
                OrganizationNames.Text = sb.ToString().TrimEnd();
                SubOrganizationNames.Text = doc.SubOrgaName;
            }
        }

        private void AddOrganization_OnClick(object sender, RoutedEventArgs e)
        {
            new AddOrganization(controller).ShowDialog();
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            DocumentModel document = GetDocument();
            string firstName = document.FirstName;
            string lastName = document.LastName;
            string documentId = document.Ut;
            int seqNo = document.SeqNo;
            int orgId = controller.Organizations[SelectOrganization.SelectedIndex].OrganizationId;
            if (controller.AddPerson(firstName, lastName, orgId, documentId, seqNo))
            {
                controller.Documents.Remove(document);
                controller.SaveDocuments();
                PopulateFields(GetDocument());
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