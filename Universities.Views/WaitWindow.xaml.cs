using System.Windows;

namespace Universities.Views
{
    public partial class WaitWindow : Window
    {
        public WaitWindow(string text = null)
        {
            InitializeComponent();
            TextBlock.Text = text;
            Show();
        }
    }
}