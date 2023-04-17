using Microsoft.Win32;
using Universities.Utils;

namespace Universities.Handlers
{
    public static class FileDialogHandler
    {
        public static bool ShowOpenFileDialog(string title, out string filePath)
        {
            filePath = string.Empty;
            OpenFileDialog openDlg = new OpenFileDialog
            {
                Filter = "Comma Separated Value | *.csv",
                Title = title
            };
            if (!openDlg.ShowDialog() ?? false) return false;
            filePath = openDlg.FileName;
            return true;
        }

        public static bool ShowSaveFileDialog(string title, out string filePath)
        {
            filePath = string.Empty;
            SaveFileDialog saveDlg = new SaveFileDialog
            {
                Filter = "Comma Separated Value | *.csv",
                AddExtension = true,
                InitialDirectory = Settings.Instance.DataSetFilePath,
                Title = title
            };
            if (!saveDlg.ShowDialog() ?? false) return false;
            filePath = saveDlg.FileName;
            return true;
        }
    }
}