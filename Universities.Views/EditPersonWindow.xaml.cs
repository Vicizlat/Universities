using System.Linq;
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
        private MainController Controller { get; }
        public string[] Person { get; }
        private int selectedOrgIndex { get; }

        public EditPersonWindow(string[] person, MainController controller)
        {
            InitializeComponent();
            DataContext = this;
            Person = person;
            Controller = controller;
            Organizations.ItemsSource = Controller.Organizations.Select(o => o.ToString());
            Organizations.SelectedItem = Controller.Organizations.FirstOrDefault(o => int.Parse(Person[4]) == o.OrganizationId).ToString();
            PersonId.Text = Person[1];
            selectedOrgIndex = Organizations.SelectedIndex;
            Save.IsEnabled = false;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Save.IsEnabled) Save_Click(this, e);
            if (e.Key == Key.Escape) Cancel_Click(this, e);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => IsSaveEnabled();

        private void Organizations_SelectionChanged(object sender, SelectionChangedEventArgs e) => IsSaveEnabled();

        private void IsSaveEnabled()
        {
            Save.IsEnabled = PersonId.Text != Person[0] || Organizations.SelectedIndex != selectedOrgIndex;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            string msg = $"Do you want to apply these changes to every person with this PersonID?";
            string question = "YES = Apply to all\tNO = Apply to this entry only";
            bool applyToAll = PromptBox.Question(msg, question);
            int idToApply = applyToAll ? 0 : int.Parse(Person[0]);
            int personIdToApply = applyToAll ? int.Parse(Person[1]) : 0;
            if (PersonId.Text != Person[1])
            {
                DialogResult = await PhpHandler.UpdateInTable($"{MainOrg.Preffix}_people", "PersonID", PersonId.Text, idToApply, personIdToApply);
            }
            if (Organizations.SelectedIndex != selectedOrgIndex)
            {
                string orgId = Organizations.SelectedItem.ToString().Split(";")[0];
                DialogResult = await PhpHandler.UpdateInTable($"{MainOrg.Preffix}_people", "OrganizationID", orgId, idToApply, personIdToApply);
            }
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        private void PersonId_KeyDown(object sender, KeyEventArgs e) => e.Handled = !KeyPressHandler.Numbers(e.Key);
    }
}