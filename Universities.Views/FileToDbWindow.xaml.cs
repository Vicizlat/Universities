using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            LineDict = DbHeaders.ToDictionary(x => x, x => -1);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string comboBoxName = (sender as ComboBox).Name;
            string textBoxName = comboBoxName.Replace("f", "db");
            LineDict[DatabaseHeaders.Children.Cast<TextBox>().FirstOrDefault(tb => tb.Name == textBoxName).Text] = (sender as ComboBox).SelectedIndex - 1;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < DbHeaders.Length; i++)
            {
                DatabaseHeaders.Children.Add(GetTextBox(i));
                FileHeaders.Children.Add(GetComboBox(i));
            }
            for (int i = 0; i < FileHeaders.Children.Count; i++)
            {
                ComboBox cb = (ComboBox)FileHeaders.Children[i];
                string tbTextCompare = StringPrepForCompare((DatabaseHeaders.Children[i] as TextBox).Text);
                string exactMatch = cb.Items.Cast<string>().FirstOrDefault(item => StringPrepForCompare(item) == tbTextCompare);
                if (!string.IsNullOrEmpty(exactMatch))
                {
                    cb.SelectedItem = exactMatch;
                    continue;
                }
                foreach (string cbItem in cb.Items)
                {
                    string cbTextCompare = StringPrepForCompare(cbItem);
                    if (tbTextCompare.Contains(cbTextCompare) || cbTextCompare.Contains(tbTextCompare))
                    {
                        cb.SelectedItem = cbItem;
                        break;
                    }
                }
            }
        }

        private TextBox GetTextBox(int i)
        {
            return new TextBox
            {
                Name = $"dbHeader{i}",
                Text = DbHeaders[i],
                Padding =  new Thickness(2),
                Margin = new Thickness(2),
                FontSize = 14,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                Cursor = Cursors.Arrow,
                IsReadOnly = true
            };
        }

        private ComboBox GetComboBox(int i)
        {
            ComboBox cb = new ComboBox
            {
                Name = $"fHeader{i}",
                Text = DbHeaders[i],
                Margin = new Thickness(2),
                FontSize = 14,
                ItemsSource = FHeaders
            };
            cb.SelectionChanged += ComboBox_SelectionChanged;
            return cb;
        }

        private static string StringPrepForCompare(string text)
        {
            string result = text.ToLower();
            result = result.Replace(" ", "");
            result = result.Replace("_", "");
            result = result.Replace("-", "");
            result = result.Replace("(", "");
            result = result.Replace(")", "");
            result = result.Replace("address", "addr");
            result = result.Replace("names", "name");
            result = result.Replace("organisation", "orga");
            result = result.Replace("author", "avtor");
            return result;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}