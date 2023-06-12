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
            if (FileDialogHandler.ShowOpenFileDialog("Open Acadademic Personnel file", out string filePath))
            {
                if (DataReader.ReadAcadPersonnelFile(filePath, out string message))
                {
                    //OnPeopleChanged?.Invoke(null, EventArgs.Empty);
                    message = $"Successfully imported all Acadademic Personnel from {filePath}.";
                    MessageBox.Show(message);
                    return true;
                }
                MessageBox.Show(message);
            }
            return false;
        }

        public static bool ExportDocuments(bool isIncomplete = false, bool isDuplicate = false)
        {
            if (Controller == null) return false;
            if (FileDialogHandler.ShowSaveFileDialog("Export Documents", out string filePath))
            {
                List<string> exportDocuments = new List<string> { string.Join(Settings.Instance.Separator, Constants.ExportDocumentsHeader) };
                if (isDuplicate) exportDocuments.AddRange(DBAccess.Context.DuplicateDocuments.Select(d => d.ToString()));
                else if (isIncomplete) exportDocuments.AddRange(DBAccess.Context.IncompleteDocuments.Select(d => d.ToString()));
                else exportDocuments.AddRange(DBAccess.Context.Documents.Select(d => d.ToString()));
                if (FileHandler.WriteAllLines(filePath, exportDocuments))
                {
                    string message = $"Successfully exported all Documents to {filePath}.";
                    MessageBox.Show(message);
                    return true;
                }
            }
            return false;
        }

        public static bool ExportOrganizations()
        {
            if (Controller == null) return false;
            if (FileDialogHandler.ShowSaveFileDialog("Export Organizations", out string filePath))
            {
                List<string> exportOrganizations = new List<string> { string.Join(Settings.Instance.Separator, Constants.ExportOrganizationsHeader) };
                exportOrganizations.AddRange(DBAccess.Context.Organizations.Select(o => o.ToString()));
                if (FileHandler.WriteAllLines(filePath, exportOrganizations))
                {
                    string message = $"Successfully exported all Organizations to {filePath}.";
                    MessageBox.Show(message);
                    return true;
                }
            }
            return false;
        }

        public static bool ExportPeople()
        {
            if (Controller == null) return false;
            if (FileDialogHandler.ShowSaveFileDialog("Export People", out string filePath))
            {
                List<string> exportPeople = new List<string> { string.Join(Settings.Instance.Separator, Constants.ExportPeopleHeader) };
                exportPeople.AddRange(DBAccess.Context.People.Select(p => p.ToExportString()));
                if (FileHandler.WriteAllLines(filePath, exportPeople))
                {
                    string message = $"Successfully exported all People to {filePath}.";
                    MessageBox.Show(message);
                    return true;
                }
            }
            return false;
        }

        public static bool ExportAcadPersonnel()
        {
            if (Controller == null) return false;
            if (FileDialogHandler.ShowSaveFileDialog("Export Academic Personnel", out string filePath))
            {
                List<string> exportAcadPersonnel = new List<string> { string.Join(Settings.Instance.Separator, Constants.ExportAcadPersonnelHeader) };
                exportAcadPersonnel.AddRange(DBAccess.Context.AcadPersonnel.Select(p => p.ToString()));
                if (FileHandler.WriteAllLines(filePath, exportAcadPersonnel))
                {
                    string message = $"Successfully exported all Academic Personnel to {filePath}.";
                    MessageBox.Show(message);
                    return true;
                }
            }
            return false;
        }
    }
}