using System;
using System.Windows;

namespace Universities.Views
{
    public partial class WaitWindow : Window
    {
        public bool IsCanceled { get; set; }

        public WaitWindow(string text = null)
        {
            InitializeComponent();
            TextBlock.Text = text;
            IsCanceled = false;
            CancelButton.Visibility = Visibility.Hidden;
        }

        public void ChangeText(string text)
        {
            TextBlock.Text = text;
        }

        public void UpdateProgressBar(double maxValue)
        {
            ProgBar.Visibility = Visibility.Visible;
            ProgBar.Maximum = maxValue;
            CancelButton.Visibility = Visibility.Visible;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;
            Close();
        }
    }
}