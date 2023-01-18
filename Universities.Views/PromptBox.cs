using System.Windows;

namespace Universities.Views
{
    public static class PromptBox
    {
        public static bool Question(string message)
        {
            return MessageBox.Show(message, "Attention!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public static bool Warning(string message)
        {
            return MessageBox.Show(message, "Attention!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }

        public static bool Stop(string message)
        {
            return MessageBox.Show(message, "Attention!", MessageBoxButton.YesNo, MessageBoxImage.Stop) == MessageBoxResult.Yes;
        }
    }
}