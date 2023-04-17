using System.Windows.Controls;

namespace Universities.Views.UserControls
{
    public partial class TextBlockBordered
    {
        public string Text { get; set; }

        public TextBlockBordered()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}