using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Universities.Controller;

namespace Universities
{
    public partial class AddOrganization
    {
        private readonly MainController controller;

        public AddOrganization(MainController controller)
        {
            InitializeComponent();
            this.controller = controller;
        }

        private void OrganizationName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Save.IsEnabled = !string.IsNullOrEmpty(OrganizationName.Text);
        }

        private void ParentOrganization_OnLoaded(object sender, RoutedEventArgs e)
        {
            ParentOrganization.ItemsSource = controller.Organizations.Select(controller.GetOrganizationName);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ParentOrganization.SelectedItem == null)
            {
                string message = "Are you sure you want to save without a Parent Organization?";
                if (!controller.QuestionBox(message, "Question")) return;
            }
            int organizationId = controller.Organizations.Last().OrganizationId + 1;
            int parentOrganizationId = ParentOrganization.SelectedIndex + 1;
            string organizationName = OrganizationName.Text;
            DialogResult = controller.AddOrganization(organizationId, organizationName, parentOrganizationId);
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}