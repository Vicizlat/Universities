using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Universities.Controller;
using Universities.Handlers;
using Universities.Utils;

namespace Universities
{
    public partial class SettingsWindow
    {
        private readonly MainController controller;

        public SettingsWindow(MainController controller)
        {
            InitializeComponent();
            this.controller = controller;
            DataSetFilePath.Text = Settings.Instance.DataSetFilePath;
            OrganizationsFilePath.Text = Settings.Instance.OrganizationsFilePath;
            PeopleFilePath.Text = Settings.Instance.PeopleFilePath;
        }

        private void GenericText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Save.IsEnabled = !string.IsNullOrEmpty(DataSetFilePath.Text) && !string.IsNullOrEmpty(OrganizationsFilePath.Text);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.WriteSettingsFile();
            controller.LoadFiles();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BrowseDataSetButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileDialogHandler.OpenFileDialog("Open DataSet file", out string filePath))
            {
                Settings.Instance.DataSetFilePath = filePath;
                DataSetFilePath.Text = Settings.Instance.DataSetFilePath;
            }
        }

        private void BrowseOrganizationsButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileDialogHandler.OpenFileDialog("Open Organizations file", out string filePath))
            {
                Settings.Instance.OrganizationsFilePath = filePath;
                OrganizationsFilePath.Text = Settings.Instance.OrganizationsFilePath;
            }
        }

        private void BrowsePeopleButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileDialogHandler.OpenFileDialog("Open People file", out string filePath))
            {
                Settings.Instance.PeopleFilePath = filePath;
                PeopleFilePath.Text = Settings.Instance.PeopleFilePath;
            }
        }

        private void Numbers_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.D0 && e.Key != Key.D1 && e.Key != Key.D2 && e.Key != Key.D3 && e.Key != Key.D4 &&
                e.Key != Key.D5 && e.Key != Key.D6 && e.Key != Key.D7 && e.Key != Key.D8 && e.Key != Key.D9 &&
                e.Key != Key.NumPad0 && e.Key != Key.NumPad1 && e.Key != Key.NumPad2 && e.Key != Key.NumPad3 &&
                e.Key != Key.NumPad4 && e.Key != Key.NumPad5 && e.Key != Key.NumPad6 && e.Key != Key.NumPad7 &&
                e.Key != Key.NumPad8 && e.Key != Key.NumPad9) e.Handled = true;
        }

        private void NewStartId_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShiftIds.IsEnabled = !string.IsNullOrEmpty(NewStartId.Text) && int.TryParse(NewStartId.Text, out int result);
        }

        private void ShiftIdsButton_Click(object sender, RoutedEventArgs e)
        {
            if (controller.ShiftPeopleIds(int.Parse(NewStartId.Text))) MessageBox.Show("All done!");
        }

        private void Separator_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSeparator.IsEnabled = !string.IsNullOrEmpty(Separator.Text);
        }

        private void ChangeSeparatorButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.Separator = Separator.Text[0];
            if (controller.SaveDocuments() && controller.SaveOrganizations() && controller.SavePeople())
            {
                MessageBox.Show("All done!");
            }
        }
    }
}