using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Universities.Utils;

namespace Universities.Handlers
{
    public static class PhpHandler
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly bool useTestUrl = false;

        public static async Task<bool> CreateCommonTables()
        {
            string response = await GetAsync($"create_common_tables.php");
            return response == "success";
        }

        public static async Task<string> CreateOrVerifyUserAsync(string mode, string username = "", string password = "", string role = "user")
        {
            Dictionary<string, string> values = new Dictionary<string, string> { { "mode", mode } };
            if (mode == "verify")
            {
                if (string.IsNullOrEmpty(username)) username = Settings.Instance.Username;
                if (string.IsNullOrEmpty(password)) password = Settings.Instance.DecryptedPassword;
            }
            values.Add("username", username);
            values.Add("password", password);
            if (mode == "create")
            {
                values.Add("role", role);
            }
            string response = await PostAsync($"create_verify_user.php", values);
            return response;
        }

        public static async Task<string> AddMainOrgAsync(string newMainOrg, string newMainPreff)
        {
            Dictionary<string, string> values = new Dictionary<string, string> { { "new_main_org", newMainOrg }, { "new_main_preff", newMainPreff } };
            string response = await PostAsync($"add_main_org.php", values);
            return response;
        }

        public static async Task<string[]> GetColumnsAsync(string table)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "table", table },
                { "type", "query_columns" }
            };
            string result = string.Empty;
            string response = await PostAsync($"get_from_table.php", values);
            foreach(string line in response.Split("<br>", StringSplitOptions.RemoveEmptyEntries))
            {
                result += $"{line.Split(";")[0]};";
            }
            return result.Split(";", StringSplitOptions.RemoveEmptyEntries);
        }

        public static async Task<string[]> GetFromTableAsync(string table, string assignedToUser = "", int id = 0, int processed = 0, string firstName = "", string lastName = "", int orgId = 0, string ut = "", string firstLast = "", string column = "")
        {
            Dictionary<string, string> values = new Dictionary<string, string> { { "table", table } };
            if (!string.IsNullOrEmpty(assignedToUser)) values.Add("assigned_to_user", assignedToUser);
            if (id > 0) values.Add("id", $"{id}");
            if (table.Contains("_documents")) values.Add("processed", $"{processed}");
            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && orgId > 0)
            {
                values.Add("first_name", firstName);
                values.Add("last_name", lastName);
                values.Add("org_id", $"{orgId}");
            }
            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && !string.IsNullOrEmpty(ut))
            {
                values.Add("ut", ut);
                values.Add("first_name", firstName);
                values.Add("last_name", lastName);
            }
            if (!string.IsNullOrEmpty(firstLast)) values.Add("first_last", firstLast);
            if (!string.IsNullOrEmpty(column)) values.Add("column", column);
            string response = await PostAsync($"get_from_table.php", values);
            return response.Split("<br>", StringSplitOptions.RemoveEmptyEntries);
        }

        public static async Task<bool> AddToTable(string table, string type, string data)
        {
            data = data.Replace("&", "and");
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "table", table },
                { "type", type },
                { "data", data }
            };
            string response = await PostAsync($"add_to_table.php", values);
            return response == "success";
        }

        public static async Task<bool> UpdateInTable(string table, string column, string value, int id = 0, int personId = 0, string firstName = "", string lastName = "", string ut = "")
        {
            value = value.Replace("&", "and");
            Dictionary<string, string> values = new Dictionary<string, string> { { "table", table }, { "column", column }, { "value", value } };
            if (id > 0) values.Add("id", $"{id}");
            if (personId > 0) values.Add("person_id", $"{personId}");
            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && !string.IsNullOrEmpty(ut))
            {
                values.Add("first_name", $"{firstName}");
                values.Add("last_name", $"{lastName}");
                values.Add("ut", $"{ut}");
            }
            string response = await PostAsync($"update_in_table.php", values);
            return response == "success";
        }

        public static async Task<bool> DeleteFromTable(string table, int id)
        {
            Dictionary<string, string> values = new Dictionary<string, string> { { "table", table }, { "id", $"{id}" } };
            string response = await PostAsync($"delete_from_table.php", values);
            if (response != "success") PromptBox.Error(response);
            return response == "success";
        }

        public static async Task<string> DeleteMainOrg(string mainOrgPreff)
        {
            Dictionary<string, string> values = new Dictionary<string, string> { { "main_org_preff", mainOrgPreff } };
            string response = await PostAsync($"delete_main_org.php", values);
            return response;
        }

        public static async Task<string> ClearTable(string table)
        {
            Dictionary<string, string> values = new Dictionary<string, string> { { "table", table } };
            string response = await PostAsync($"clear_table.php", values);
            return response;
        }

        public static async Task<string[]> GetInfoSchemaAsync(string table)
        {
            Dictionary<string, string> values = new Dictionary<string, string> { { "table", table } };
            string response = await PostAsync($"get_info_schema.php", values);
            return response.Split("<br>", StringSplitOptions.RemoveEmptyEntries);
        }

        public static async Task<bool> AddColumnsToTable(string table)
        {
            Dictionary<string, string> values = new Dictionary<string, string> { { "table", table }, { "mode", "add" } };
            string response = await PostAsync($"alter_table.php", values);
            if (response != "success") PromptBox.Error(response);
            return response == "success";
        }

        public static async Task<bool> DeleteColumnFromTable(string table, string column)
        {
            Dictionary<string, string> values = new Dictionary<string, string> { { "table", table }, { "column", column }, { "mode", "delete" } };
            string response = await PostAsync($"alter_table.php", values);
            if (response != "success") PromptBox.Error(response);
            return response == "success";
        }

        public static async Task<string> ResetAutoIncrement(string table)
        {
            Dictionary<string, string> values = new Dictionary<string, string> { { "table", table } };
            string response = await PostAsync($"reset_auto_increment.php", values);
            return response;
        }

        private static async Task<string> GetAsync(string requestPage)
        {
            string responseString = string.Empty;
            try
            {
                string urlPreffix = Settings.Instance.Server.StartsWith("http") ? string.Empty : "https://";
                string requestUrl = urlPreffix + Settings.Instance.Server + "/api/" + requestPage;
                if (useTestUrl) requestUrl = "http://localhost/universities-web" + "/api/" + requestPage;
                responseString = await client.GetStringAsync(requestUrl);
                return responseString;
            }
            catch
            {
                return responseString;
            }
        }

        private static async Task<string> PostAsync(string requestPage, Dictionary<string, string> values)
        {
            string responseString = string.Empty;
            try
            {
                string urlPreffix = Settings.Instance.Server.StartsWith("http") ? string.Empty : "https://";
                string requestUrl = urlPreffix + Settings.Instance.Server + "/api/" + requestPage;
                if (useTestUrl) requestUrl = "http://localhost/universities-web" + "/api/" + requestPage;
                FormUrlEncodedContent content = new FormUrlEncodedContent(values);
                HttpResponseMessage response = await client.PostAsync(requestUrl, content);
                responseString = await response.Content.ReadAsStringAsync();
                string logMsg = $"POST: {requestPage}/{string.Join(", ", values)}/";
                if (requestPage.Contains("get_from_table")) logMsg += $"Results found: {responseString.Split("<br>", StringSplitOptions.RemoveEmptyEntries).Length}";
                else if (requestPage.Contains("create_verify_user")) logMsg = $"POST: {requestPage}/Mode: {values["mode"]}, Name: {values["username"]}/Response: {responseString.Split("<br>").Length > 0}";
                else logMsg += $"Response: {responseString}";
                Logging.Instance.WriteLine(logMsg);
                return responseString;
            }
            catch
            {
                return responseString;
            }
        }
    }
}