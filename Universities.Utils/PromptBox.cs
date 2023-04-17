using System.Windows;

namespace Universities.Utils
{
    public static class PromptBox
    {
        public static bool Information(string message, string caption = "Information!")
        {
            return MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation) == MessageBoxResult.Yes;
        }

        public static bool Question(string message)
        {
            return MessageBox.Show(message, "Attention!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public static bool Warning(string message)
        {
            return MessageBox.Show(message, "Attention!", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }

        public static bool Error(string message, string caption = "Error!")
        {
            return MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.Yes;
        }
    }
}