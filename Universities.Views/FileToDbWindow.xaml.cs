using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Universities.Views
{
    public partial class FileToDbWindow : Window
    {
        public string[] DbHeaders { get; set; }
        public string[] FHeaders { get; set; }
        public Dictionary<string, int> LineDict { get; set; }
        public FileToDbWindow(string[] fileHeaders, string[] dbHeaders)
        {
            InitializeComponent();
            DataContext = this;
            DbHeaders = dbHeaders;
            FHeaders = new string[fileHeaders.Length + 1];
            FHeaders[0] = "\t<CLEAR>";
            fileHeaders.CopyTo(FHeaders, 1);
            LineDict = dbHeaders.ToDictionary(x => x, x => -1);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string comboBoxName = (sender as ComboBox).Name;
            string textBoxName = comboBoxName.Replace("f", "db");
            LineDict[(FindName(textBoxName) as TextBox).Text] = (sender as ComboBox).SelectedIndex - 1;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (ComboBox cb in FileHeaders.Children)
            {
                string textBoxName = cb.Name.Replace("f", "db");
                string tbText = (FindName(textBoxName) as TextBox).Text;
                string tbTextCompare = tbText.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("address", "addr").Replace("names", "name").Replace("organisation", "orga").Replace("author", "avtor");
                string exactMatch = cb.Items.Cast<string>().FirstOrDefault(item => item.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("address", "addr").Replace("names", "name").Replace("organisation", "orga").Replace("author", "avtor") == tbTextCompare);
                if (!string.IsNullOrEmpty(exactMatch))
                {
                    cb.SelectedItem = exactMatch;
                    continue;
                }
                foreach (string cbItem in cb.Items)
                {
                    string cbTextCompare = cbItem.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("address", "addr").Replace("names", "name").Replace("organisation", "orga").Replace("author", "avtor");
                    if (tbTextCompare.Contains(cbTextCompare) || cbTextCompare.Contains(tbTextCompare))
                    {
                        cb.SelectedItem = cbItem;
                        break;
                    }
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}