using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Universities.Controller;
using Universities.Utils;

namespace Universities.Views
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
            ParentOrganization.ItemsSource = controller.OrganizationsDisplayNames;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Save.IsEnabled)
            {
                Save_Click(this, e);
            }
            if (e.Key == Key.Escape)
            {
                Cancel_Click(this, e);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ParentOrganization.SelectedItem == null)
            {
                if (!PromptBox.Question("Are you sure you want to save without a Parent Organization?")) return;
            }
            controller.UpdateOrganizations();
            int orgId = controller.Organizations.Last()?.OrganizationId + 1 ?? Settings.Instance.OrgaStartId;
            int? parOrgId = controller.GetOrganizationByIndex(ParentOrganization.SelectedIndex)?.OrganizationId;
            controller.AddOrganization(new string[] { $"{orgId}", OrganizationName.Text, $"{parOrgId}" });
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}