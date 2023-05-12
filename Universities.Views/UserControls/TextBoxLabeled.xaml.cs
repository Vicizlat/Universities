using System;
using System.Windows.Controls;

namespace Universities.Views.UserControls
{
    public partial class TextBoxLabeled
    {
        public string Label { get; set; }
        public string CustomText { get; set; }
        public event EventHandler<TextChangedEventArgs> TextChanged;

        public TextBoxLabeled()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged?.Invoke(this, e);
        }
    }
}