using System;
using System.Windows;

namespace Universities.Views.UserControls
{
    public partial class PasswordBoxLabeled
    {
        public string Label { get; set; }
        public event EventHandler<RoutedEventArgs> PasswordChanged;

        public PasswordBoxLabeled()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordChanged?.Invoke(this, e);
        }
    }
}