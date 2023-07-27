using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using Universities.Handlers;
using Universities.Utils;

namespace Universities.Controller
{
    public static class ImportExport
    {
        public static MainController? Controller;
        public static event EventHandler? OnDocumentsChanged;
        public static event EventHandler? OnOrganizationsChanged;
        public static event EventHandler? OnPeopleChanged;

        public static bool ImportDocuments()
        {
            if (Controller == null) return false;
            if (FileDialogHandler.ShowOpenFileDialog("Open DataSet file", out string filePath))
            {
                if (DataReader.ReadDataSetFile(filePath, out string message))
                {
                    OnDocumentsChanged?.Invoke(null, EventArgs.Empty);
                    message = $"Successfully imported all Documents from {filePath}.";
                    MessageBox.Show(message);
                    return true;
                }
                MessageBox.Show(message);
            }
            return false;
        }

        public static bool ImportOrganizations()
        {
            if (Controller == null) return false;
            if (FileDialogHandler.ShowOpenFileDialog("Open Organizations file", out string filePath))
            {
                if (DataReader.ReadOrganizationsFile(filePath, out string message))
                {
                    OnOrganizationsChanged?.Invoke(null, EventArgs.Empty);
                    message = $"Successfully imported all Organizations from {filePath}.";
                    MessageBox.Show(message);
                    return true;
                }
                MessageBox.Show(message);
            }
            return false;
        }

        public static bool ImportPeople()
        {
            if (Controller == null) return false;
            if (FileDialogHandler.ShowOpenFileDialog("Open People file", out string filePath))
            {
                if (DataReader.ReadPeopleFile(filePath, out string message))
                {
                    OnPeopleChanged?.Invoke(null, EventArgs.Empty);
                    message = $"Successfully imported all People from {filePath}.";
                    MessageBox.Show(message);
                    return true;
                }
                MessageBox.Show(message);
            }
            return false;
        }

        public static bool ImportAcadPersonnel()
        {
            if (Controller == null) return false;
            if (FileDialogHandler.ShowOpenFileDialog("Open Academic Personnel file", out string filePath))
            {
                if (DataReader.ReadAcadPersonnelFile(filePath, out string message))
                {
                    message = $"Successfully imported all Academic Personnel from {filePath}.";
                    MessageBox.Show(message);
                    return true;
                }
                MessageBox.Show(message);
            }
            return false;
        }

        public static bool ExportDocuments(out string message, bool isIncomplete = false, bool isDuplicate = false, string? filePath = null)
        {
            if (Controller != null && (filePath != null || FileDialogHandler.ShowSaveFileDialog("Export Documents", out filePath)))
            {
                List<string> exportDocuments = new List<string> { string.Join(Settings.Instance.Separator, Constants.ExportDocumentsHeader) };
                if (isDuplicate) exportDocuments.AddRange(DBAccess.GetContext().DuplicateDocuments.Select(d => d.ToString()));
                else if (isIncomplete) exportDocuments.AddRange(DBAccess.GetContext().IncompleteDocuments.Select(d => d.ToString()));
                else exportDocuments.AddRange(DBAccess.GetContext().Documents.Select(d => d.ToString()));
                if (FileHandler.WriteAllLines(filePath, exportDocuments))
                {
                    message = $"Successfully exported all Documents to {filePath}.";
                    return true;
                }
            }
            message = $"Failed to export Documents to {filePath}.";
            return false;
        }

        public static bool ExportOrganizations(out string message, string? filePath = null)
        {
            if (Controller != null && (filePath != null || FileDialogHandler.ShowSaveFileDialog("Export Organizations", out filePath)))
            {
                List<string> exportOrganizations = new List<string> { string.Join(Settings.Instance.Separator, Constants.ExportOrganizationsHeader) };
                exportOrganizations.AddRange(DBAccess.GetContext().Organizations.Select(o => o.ToExportString()));
                if (FileHandler.WriteAllLines(filePath, exportOrganizations))
                {
                    message = $"Successfully exported all Organizations to {filePath}.";
                    return true;
                }
            }
            message = $"Failed to export Organizations to {filePath}.";
            return false;
        }

        public static bool ExportPeople(out string message, string? filePath = null)
        {
            if (Controller != null && (filePath != null || FileDialogHandler.ShowSaveFileDialog("Export People", out filePath)))
            {
                List<string> exportPeople = new List<string> { string.Join(Settings.Instance.Separator, Constants.ExportPeopleHeader) };
                exportPeople.AddRange(DBAccess.GetContext().People.Select(p => p.ToExportString()));
                if (FileHandler.WriteAllLines(filePath, exportPeople))
                {
                    message = $"Successfully exported all People to {filePath}.";
                    return true;
                }
            }
            message = $"Failed to export People to {filePath}.";
            return false;
        }

        public static bool ExportAcadPersonnel(out string message, string? filePath = null)
        {
            if (Controller != null && (filePath != null || FileDialogHandler.ShowSaveFileDialog("Export Academic Personnel", out filePath)))
            {
                List<string> exportAcadPersonnel = new List<string> { string.Join(Settings.Instance.Separator, Constants.ExportAcadPersonnelHeader) };
                exportAcadPersonnel.AddRange(DBAccess.GetContext().AcadPersonnel.Select(p => p.ToString()));
                if (FileHandler.WriteAllLines(filePath, exportAcadPersonnel))
                {
                    message = $"Successfully exported all Academic Personnel to {filePath}.";
                    return true;
                }
            }
            message = $"Failed to export Academic Personnel to {filePath}.";
            return false;
        }

        public static void ExportBackupFiles()
        {
            ExportPeople(out string message, $"{Constants.NowPath}\\{Settings.Instance.Database}_People.csv");
            Logging.Instance.WriteLine(message);
            ExportOrganizations(out message, $"{Constants.NowPath}\\{Settings.Instance.Database}_Organizations.csv");
            Logging.Instance.WriteLine(message);
            ExportDocuments(out message, filePath: $"{Constants.NowPath}\\{Settings.Instance.Database}_Documents.csv");
            Logging.Instance.WriteLine(message);
            ExportAcadPersonnel(out message, $"{Constants.NowPath}\\{Settings.Instance.Database}_AcadPersonnel.csv");
            Logging.Instance.WriteLine(message);
        }
    }
}