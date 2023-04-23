using Microsoft.Win32;
using Universities.Utils;

namespace Universities.Handlers
{
    public static class FileDialogHandler
    {
        public static bool ShowOpenFileDialog(string title, out string filePath)
        {
            OpenFileDialog openDlg = new OpenFileDialog
            {
                Filter = "Comma Separated Value | *.csv",
                Title = title
            };
            if (openDlg.ShowDialog() == true)
            {
                filePath = openDlg.FileName;
                return true;
            }
            filePath = string.Empty;
            return false;
        }

        public static bool ShowSaveFileDialog(string title, out string filePath)
        {
            SaveFileDialog saveDlg = new SaveFileDialog
            {
                Filter = "Comma Separated Value | *.csv",
                AddExtension = true,
                InitialDirectory = Constants.DocumentsFolder,
                Title = title
            };
            if (saveDlg.ShowDialog() == true)
            {
                filePath = saveDlg.FileName;
                return true;
            }
            filePath = string.Empty;
            return false;
        }
    }
}