using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Universities.Handlers;
using Universities.Models;
using Universities.Utils;

namespace Universities.Controller
{
    public class MainController
    {
        public event EventHandler? OnDocumentsChanged;
        public event EventHandler? OnOrganizationsChanged;
        public event EventHandler? OnPeopleChanged;
        public event EventHandler? OnAcadPersonnelChanged;
        public event EventHandler? OnMainOrgChanged;
        public IEnumerable<Document> Documents { get; set; } = new List<Document>();
        public IEnumerable<Organization> Organizations { get; set; } = new List<Organization>();
        public IEnumerable<Person> People { get; set; } = new List<Person>();
        public IEnumerable<AcadPerson> AcadPersonnel { get; set; } = new List<AcadPerson>();

        public MainController()
        {
            GetRegex();
            MainOrgChangedAsync();
        }

        private static async void GetRegex()
        {
            string[] regex = await PhpHandler.GetFromTableAsync("regex_patterns");
            Settings.Instance.RegexPattern = string.Join("|", regex.Select(rp => rp.Split(";")[1]));
            Settings.Instance.WriteSettingsFile();
        }

        public async void MainOrgChangedAsync()
        {
            await UpdateDocuments();
            await UpdateOrganizations();
            await UpdatePeopleAsync();
            await UpdateAcadPersonnelAsync();
            OnMainOrgChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task UpdateDocuments()
        {
            string assignedToUser = User.Role == "superadmin" ? "" : User.Username;
            string[] docs = await PhpHandler.GetFromTableAsync($"{MainOrg.Preffix}_documents", assignedToUser: assignedToUser);
            Documents = docs.Select(d => new Document(d.Split(";")));
            OnDocumentsChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task UpdateOrganizations()
        {
            string[] orgs = await PhpHandler.GetFromTableAsync($"{MainOrg.Preffix}_organizations");
            List<Organization> orgsList = new List<Organization>();
            foreach (string[] org in orgs.Select(o => o.Split(";")))
            {
                string parent = orgs.Select(o => o.Split(";")).FirstOrDefault(o => o[1] == org[3])?[2] ?? string.Empty;
                string parentName = string.IsNullOrEmpty(parent) ? string.Empty : parent;
                string nameWithParent = $"{org[2]} ({parentName})";
                orgsList.Add(new Organization(org, nameWithParent));
            }
            Organizations = orgsList.ToArray();
            OnOrganizationsChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task UpdatePeopleAsync()
        {
            string[] people = await PhpHandler.GetFromTableAsync($"{MainOrg.Preffix}_people");
            People = people.Select(p => new Person(p.Split(";")));
            OnPeopleChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task UpdateAcadPersonnelAsync()
        {
            string[] acadPeople = await PhpHandler.GetFromTableAsync($"{MainOrg.Preffix}_academic_personnel");
            AcadPersonnel = acadPeople.Select(ap => new AcadPerson(ap.Split(";")));
            OnAcadPersonnelChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task<int> GetPersonId(string firstName, string lastName, int orgId)
        {
            string[] findPerson = await PhpHandler.GetFromTableAsync($"{MainOrg.Preffix} _people", firstName: firstName, lastName: lastName, orgId: orgId);
            string msg = "A person with the same First Name, Last Name and Organization already exists but was not selected.";
            string question = "Do you want to save this person with a new PersonID?";
            if (findPerson.Any() && !PromptBox.Question(msg, question)) return 0;
            string[] lastPerson = await PhpHandler.GetFromTableAsync($"{MainOrg.Preffix}_people", firstLast: "last", column: "PersonID");
            return lastPerson.Any() ? int.Parse(lastPerson[0].Split(";")[1]) + 1 : MainOrg.PeopleStartId;
        }

        public bool GetSeparator(string line, out string separator)
        {
            separator = string.Empty;
            if (line.Contains(',') && line.Contains(';'))
            {
                return PromptBox.Error(Constants.ErrorManySeparators);
            }
            else if (line.Contains(',') && !line.Contains(';'))
            {
                separator = ",";
                return true;
            }
            else if (!line.Contains(',') && line.Contains(';'))
            {
                separator = ";";
                return true;
            }
            else return PromptBox.Error(Constants.ErrorNoSeparator); ;
        }

        public async Task<bool> ExportAsync(string type, bool isIncomplete = false, bool isDuplicate = false, string? filePath = null, bool noPrompt = false)
        {
            if (filePath != null || FileDialogHandler.ShowSaveFileDialog($"Export {type}", out filePath))
            {
                List<string> exportLines = new List<string>();
                string table = MainOrg.Preffix;
                if (type == "Documents")
                {
                    exportLines.Add(string.Join(Settings.Instance.Separator, Constants.ExportDocumentsHeader));
                    table += "_documents";
                    if (isDuplicate) table += "_duplicate_documents";
                    else if (isIncomplete) table += "_incomplete_documents";
                }
                else if (type == "Organizations")
                {
                    exportLines.Add(string.Join(Settings.Instance.Separator, Constants.ExportOrganizationsHeader));
                    table += "_organizations";
                }
                else if (type == "People")
                {
                    exportLines.Add(string.Join(Settings.Instance.Separator, Constants.ExportPeopleHeader));
                    table += "_people";
                }
                else if (type == "Academic Personnel")
                {
                    exportLines.Add(string.Join(Settings.Instance.Separator, Constants.ExportAcadPersonnelHeader));
                    table += "_academic_personnel";
                }
                string[] lines = await PhpHandler.GetFromTableAsync(table, processed: 1);
                exportLines.AddRange(lines.Select(l => string.Join(Settings.Instance.Separator, l.Split(";").Skip(1))));
                if (FileHandler.WriteAllLines(filePath, exportLines))
                {
                    return PromptBox.Information(string.Format(Constants.SuccessExport, type, filePath), noPrompt: noPrompt);
                }
            }
            return PromptBox.Error(string.Format(Constants.FailExport, type, filePath));
        }

        public bool RegexMatch(string text1, string text2)
        {
            Regex regex = new Regex(Settings.Instance.RegexPattern, RegexOptions.IgnoreCase);
            text1 = regex.Replace(text1, "");
            text2 = regex.Replace(text2, "");
            return text1 == text2;
        }
    }
}