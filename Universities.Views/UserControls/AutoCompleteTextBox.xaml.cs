using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Universities.Views.UserControls
{
    public partial class AutoCompleteTextBox
    {
        public event EventHandler OnSelectionChanged;
        public event EventHandler OnTextChanged;
        public IEnumerable<string> AutoSuggestionList { get; set; }

        public AutoCompleteTextBox()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void AutoSuggestionPopup(bool openPopup)
        {
            AutoListPopup.IsOpen = openPopup;
            AutoListPopup.Visibility = openPopup ? Visibility.Visible : Visibility.Collapsed;
            AutoList.Width = AutoTextBox.ActualWidth;
        }

        public bool Validate()
        {
            bool isValid = !string.IsNullOrEmpty(AutoTextBox.Text) && AutoSuggestionList.Any(s => s == AutoTextBox.Text);
            AutoTextBox.Background = isValid ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Bisque);
            return isValid;
        }

        private void AutoTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                AutoSuggestionPopup(!string.IsNullOrEmpty(AutoTextBox.Text));
                AutoList.ItemsSource = AutoSuggestionList.Where(s => s.ToLower().Contains(AutoTextBox.Text.ToLower()));
                OnTextChanged?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in AutoTextBox_TextChanged", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AutoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                AutoSuggestionPopup(false);
                if (AutoList.SelectedIndex < 0) return;
                AutoTextBox.Text = $"{AutoList.SelectedItem}";
                OnSelectionChanged?.Invoke(AutoSuggestionList.ToList().IndexOf(AutoTextBox.Text), null);
                AutoList.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in AutoList_SelectionChanged", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AutoTextBox_GotMouseCapture(object sender, RoutedEventArgs e)
        {
            AutoTextBox.Text = string.Empty;
            AutoSuggestionPopup(true);
            AutoList.ItemsSource = AutoSuggestionList;
        }

        private void AutoList_MouseLeave(object sender, MouseEventArgs e)
        {
            AutoSuggestionPopup(false);
        }

        private void AutoTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && AutoList.Items.Count == 1)
            {
                AutoTextBox.Text = $"{AutoList.Items[0]}";
                AutoSuggestionPopup(false);
                OnSelectionChanged?.Invoke(AutoSuggestionList.ToList().IndexOf(AutoTextBox.Text), null);
            }
        }
    }
}