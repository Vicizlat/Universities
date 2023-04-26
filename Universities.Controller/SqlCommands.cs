using MySqlConnector;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using Universities.Utils;

namespace Universities.Controller
{
    public static class SqlCommands
    {
        public static MainController? Controller;

        public static string GetCurrentUser(out bool isAdmin)
        {
            isAdmin = false;
            try
            {
                string? currentUser;
                using (MySqlConnection Connection = new MySqlConnection(Settings.Instance.GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand("SELECT CURRENT_USER;", Connection))
                {
                    Connection.Open();
                    currentUser = (string?)Command.ExecuteScalar();
                }
                using (MySqlConnection Connection = new MySqlConnection(Settings.Instance.GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand("SHOW GRANTS;", Connection))
                {
                    Connection.Open();
                    MySqlDataReader reader = Command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (((string)reader.GetValue(0)).StartsWith("GRANT ALL"))
                        {
                            isAdmin = true;
                            break;
                        }
                    }
                }
                return currentUser?.Remove(currentUser.Length - 2) ?? string.Empty;
            }
            catch
            {
                return "Not connected to DB";
            }
        }

        public static void AddUser(string username, string password, bool isAdmin)
        {
            if (!string.IsNullOrEmpty(Controller?.CurrentUser) && Controller.IsAdmin)
            {
                string command = $"CREATE USER '{username}' IDENTIFIED BY '{password}';";
                if (isAdmin) command += $"GRANT ALL ON *.* TO '{username}';";
                else command += $"GRANT SELECT, INSERT, UPDATE ON *.* TO '{username}';";
                using (MySqlConnection Connection = new MySqlConnection(Settings.Instance.GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand(command, Connection))
                {
                    Connection.Open();
                    Command.ExecuteNonQuery();
                }
                MessageBox.Show($"User '{username}' added successfully!", Controller.CurrentUser, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else MessageBox.Show("You don't have permission to add users!", Controller.CurrentUser, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void UpdateUserPassword(string username, string password)
        {
            try
            {
                if (Controller.IsAdmin || Controller.CurrentUser == username)
                {
                    string command = $"SET PASSWORD FOR '{username}'@'%' = PASSWORD('{password}');";
                    using (MySqlConnection Connection = new MySqlConnection(Settings.Instance.GetConnectionString()))
                    using (MySqlCommand Command = new MySqlCommand(command, Connection))
                    {
                        Connection.Open();
                        Command.ExecuteNonQuery();
                    }
                    MessageBox.Show($"Password for User '{username}' changed successfully!", Controller.CurrentUser, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else MessageBox.Show($"Unable to change password for User '{username}'!", Controller.CurrentUser, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                PromptBox.Error("Can't change password when you are not logged in.");
            }
        }

        public static List<string> GetUsers()
        {
            try
            {
                List<string> users = new List<string>();
                using (MySqlConnection Connection = new MySqlConnection(Settings.Instance.GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand("SELECT * FROM mysql.user;", Connection))
                {
                    Connection.Open();
                    MySqlDataReader reader = Command.ExecuteReader();
                    while (reader.Read())
                    {
                        string user = (string)reader.GetValue("User");
                        if (user == "root") continue;
                        users.Add(user);
                    }
                }
                return users;
            }
            catch
            {
                return new List<string>() { "Unable to get Users" };
            }
        }

        public static List<string> GetDatabases()
        {
            try
            {
                List<string> databases = new List<string>();
                using (MySqlConnection Connection = new MySqlConnection(Settings.Instance.GetConnectionString()))
                using (MySqlCommand Command = new MySqlCommand("SHOW SCHEMAS", Connection))
                {
                    Connection.Open();
                    MySqlDataReader reader = Command.ExecuteReader();
                    while (reader.Read())
                    {
                        string database = (string)reader.GetValue("Database");
                        if (!database.Contains("univ")) continue;
                        databases.Add(database);
                    }
                }
                return databases;
            }
            catch
            {
                return new List<string>() { "Unable to get Databases" };
            }
        }
    }
}