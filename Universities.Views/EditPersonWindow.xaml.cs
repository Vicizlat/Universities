using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Universities.Controller;
using Universities.Handlers;
using Universities.Utils;

namespace Universities.Views
{
    public partial class EditPersonWindow
    {
        public string[] Person { get; }
        public MainController Controller { get; }
        private int selectedOrgIndex;

        public EditPersonWindow(string[] person, MainController controller)
        {
            InitializeComponent();
            DataContext = this;
            Person = person;
            Controller = controller;
            Organizations.ItemsSource = Controller.OrganizationsDisplayNames;
            Organizations.SelectedItem = DBAccess.GetOrganization(int.Parse(Person[3]))?.GetDisplayName(controller.Organizations);
            PersonId.Text = Person[0];
            selectedOrgIndex = Organizations.SelectedIndex;
            Save.IsEnabled = false;
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Save.IsEnabled = PersonId.Text != Person[0] || Organizations.SelectedIndex != selectedOrgIndex;
        }

        private void Organizations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Save.IsEnabled = PersonId.Text != Person[0] || Organizations.SelectedIndex != selectedOrgIndex;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (PersonId.Text != Person[0])
            {
                if (DBAccess.EditPersonId(Person, int.Parse(PersonId.Text))) PromptBox.Information("PersonId changed successfully!");
                else PromptBox.Error("Failed to change PersonId!");
            }
            if (Organizations.SelectedIndex != selectedOrgIndex)
            {
                int orgId = Controller.Organizations[Organizations.SelectedIndex].OrganizationId;
                if (DBAccess.EditPersonOrgId(Person, orgId)) PromptBox.Information("Organization changed successfully!");
                else PromptBox.Error("Failed to change Organization!");
            }
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PersonId_KeyDown(object sender, KeyEventArgs e) => e.Handled = KeyPressHandler.NotNumbers(e);
    }
}