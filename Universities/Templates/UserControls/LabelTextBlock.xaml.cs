namespace Universities.Templates.UserControls
{
    public partial class LabelTextBlock
    {
        public string Label { get; set; }

        public LabelTextBlock()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}