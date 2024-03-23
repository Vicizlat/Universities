using System;
using System.Windows;

namespace Universities.Utils
{
    /// <summary>
    /// Selection of Message boxes to show to the user in diferent scenarios.
    /// </summary>
    public static class PromptBox
    {
        /// <summary>
        /// Displays a Message Box with an OK Button, an Exclamation image and a message string. Always returns TRUE.
        /// </summary>
        /// <param name="message">String that will be shown in the Message Box.</param>
        /// <param name="caption">Optional: String that will be shown in the top bar of the Message Box window. Default = "Information!"</param>
        /// <returns>Bool that is always TRUE.</returns>
        public static bool Information(string message, string caption = "Information!", bool noPrompt = false)
        {
            Logging.Instance.WriteLine($"{caption}: {message}");
            return noPrompt || MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation) == MessageBoxResult.OK;
        }

        /// <summary>
        /// Displays a Message Box with a message string presenting some information and a question string that must be replied to with a YES or NO Button.
        /// Returns TRUE for YES and FALSE for NO. The message and question are clearly separated by an empty line.
        /// </summary>
        /// <param name="message">First string that will be shown in the Message Box. Usually presenting some information about the question that follows.</param>
        /// <param name="question">Second string that will be shown in the Message Box. Usually some kind of confirm or deny question.</param>
        /// <param name="caption">Optional: String that will be shown in the top bar of the Message Box window. Default = "Information!"</param>
        /// <returns>Bool - TRUE for YES and FALSE for NO.</returns>
        public static bool Question(string message, string question, string caption = "Attention!")
        {
            Logging.Instance.WriteLine($"{caption}: {message}");
            string displayMessage = message + Environment.NewLine + Environment.NewLine + question;
            MessageBoxResult result = MessageBox.Show(displayMessage, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
            Logging.Instance.WriteLine($"{question}: {result}", true);
            return result == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Displays a Message Box with an OK Button, an Error image and a message string. Always returns FALSE.
        /// </summary>
        /// <param name="message">String that will be shown in the Message Box.</param>
        /// <param name="caption">Optional: String that will be shown in the top bar of the Message Box window. Default = "Error!"</param>
        /// <returns>Bool that is always FALSE.</returns>
        public static bool Error(string message, string caption = "Error!")
        {
            Logging.Instance.WriteLine($"{caption}: {message}");
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }
}